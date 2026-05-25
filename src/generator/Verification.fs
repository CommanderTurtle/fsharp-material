
namespace Generator

open System
open System.IO
open System.Text.RegularExpressions

/// <summary>
/// Verification module for HTML to F# conversion.
/// </summary>
module Verification =

    /// <summary>
    /// Configuration for HTML verification.
    /// </summary>
    type VerificationConfig = {
        SkipComments: bool
        PreserveWhitespace: bool
        CriticalElements: string list
    }

    /// <summary>
    /// Result of a verification check.
    /// </summary>
    type VerificationResult =
        | Pass of message: string
        | Fail of message: string
        | Warning of message: string

    /// <summary>
    /// Result of comparing two HTML documents.
    /// </summary>
    type ComparisonResult = {
        OriginalPath: string
        GeneratedPath: string
        IsMatch: bool
        Differences: string list
        Warnings: string list
        Stats: Map<string, int>
    }

    /// <summary>
    /// Complete verification report.
    /// </summary>
    type VerificationReport = {
        Timestamp: DateTime
        TotalChecks: int
        Passed: int
        Failed: int
        Warnings: int
        OrphanFiles: string list
        RoundTripResults: ComparisonResult list
        Messages: string list
    }

    /// <summary>
    /// Default verification configuration.
    /// </summary>
    let defaultConfig = {
        SkipComments = true
        PreserveWhitespace = false
        CriticalElements = ["title"; "style"; "script"; "meta"]
    }

    /// <summary>
    /// Normalizes HTML for comparison.
    /// </summary>
    let normalizeHtml (config: VerificationConfig) (html: string) : string =
        let mutable result = html
        if not config.PreserveWhitespace then
            result <- Regex.Replace(result, @"\s+", " ")
        if config.SkipComments then
            result <- Regex.Replace(result, @"<!--.*?-->", "")
        result.Trim()

    /// <summary>
    /// Extracts content from a specific HTML tag.
    /// </summary>
    let extractTagContent (tagName: string) (html: string) : string list =
        let pattern = sprintf @"<%s[^>]*>(.*?)</%s>" tagName tagName
        let matches = Regex.Matches(html, pattern, RegexOptions.Singleline ||| RegexOptions.IgnoreCase)
        matches
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Groups.[1].Value.Trim())
        |> Seq.filter (not << String.IsNullOrWhiteSpace)
        |> Seq.toList

    /// <summary>
    /// Checks if an HTML file has been converted to F#.
    /// </summary>
    let hasBeenConverted (htmlPath: string) : bool =
        let directory = Path.GetDirectoryName(htmlPath)
        let fsPath = Path.Combine(directory, "index.fs")
        File.Exists(fsPath)

    /// <summary>
    /// Finds all orphan HTML files (HTML files without corresponding F# files).
    /// </summary>
    let findOrphanHtmlFiles (rootPath: string) : string list =
        let rec searchDirectory (dir: string) : string list =
            try
                let htmlPath = Path.Combine(dir, "index.html")
                let orphanFiles =
                    if File.Exists(htmlPath) && not (hasBeenConverted htmlPath) then
                        [htmlPath]
                    else
                        []
                let subdirs =
                    Directory.GetDirectories(dir)
                    |> Array.collect (fun subdir -> searchDirectory subdir |> List.toArray)
                    |> Array.toList
                orphanFiles @ subdirs
            with ex ->
                printfn "[Verification] Warning: Could not access %s: %s" dir ex.Message
                []
        if Directory.Exists(rootPath) then
            searchDirectory rootPath
        else
            []

    /// <summary>
    /// Compares two HTML strings for equivalence.
    /// </summary>
    let compareHtml (config: VerificationConfig) (originalHtml: string) (generatedHtml: string) : ComparisonResult =
        let normalizedOriginal = normalizeHtml config originalHtml
        let normalizedGenerated = normalizeHtml config generatedHtml
        let mutable differences = []
        let mutable warnings = []
        let mutable stats = Map.empty<string, int>
        let isExactMatch = normalizedOriginal = normalizedGenerated
        if not isExactMatch then
            for element in config.CriticalElements do
                let originalContent = extractTagContent element originalHtml
                let generatedContent = extractTagContent element generatedHtml
                if originalContent <> generatedContent then
                    if element = "style" || element = "script" then
                        differences <- sprintf "%s content differs" element :: differences
                    else
                        warnings <- sprintf "%s content differs (may be expected)" element :: warnings
            let originalTitle = extractTagContent "title" originalHtml
            let generatedTitle = extractTagContent "title" generatedHtml
            if originalTitle <> generatedTitle then
                differences <- "Title mismatch" :: differences
            let lengthDiff = Math.Abs(originalHtml.Length - generatedHtml.Length)
            stats <- stats.Add("lengthDifference", lengthDiff)
        stats <- stats.Add("originalLength", originalHtml.Length)
        stats <- stats.Add("generatedLength", generatedHtml.Length)
        {
            OriginalPath = ""
            GeneratedPath = ""
            IsMatch = isExactMatch && differences.IsEmpty
            Differences = differences |> List.rev
            Warnings = warnings |> List.rev
            Stats = stats
        }

    /// <summary>
    /// Verifies roundtrip fidelity of HTML conversion.
    /// </summary>
    let verifyRoundTrip (originalHtml: string) (generatedHtml: string) : VerificationResult =
        let result = compareHtml defaultConfig originalHtml generatedHtml
        if result.IsMatch then
            Pass "Round-trip verification passed - HTML matches"
        elif result.Differences.IsEmpty then
            Warning (sprintf "Round-trip verification passed with warnings: %s" (String.concat ", " result.Warnings))
        else
            Fail (sprintf "Round-trip verification failed: %s" (String.concat ", " result.Differences))

    /// <summary>
    /// Runs roundtrip test on files.
    /// </summary>
    let runRoundTripTest (htmlPath: string) (generatedPath: string) : ComparisonResult =
        try
            let originalHtml = File.ReadAllText(htmlPath, Text.Encoding.UTF8)
            let generatedHtml = File.ReadAllText(generatedPath, Text.Encoding.UTF8)
            let result = compareHtml defaultConfig originalHtml generatedHtml
            { result with OriginalPath = htmlPath; GeneratedPath = generatedPath }
        with ex ->
            {
                OriginalPath = htmlPath
                GeneratedPath = generatedPath
                IsMatch = false
                Differences = [sprintf "Error reading files: %s" ex.Message]
                Warnings = []
                Stats = Map.empty
            }

    /// <summary>
    /// Checks content preservation between HTML and F# files.
    /// </summary>
    let checkContentPreservation (htmlPath: string) (fsPath: string) : VerificationResult list =
        try
            let html = File.ReadAllText(htmlPath, Text.Encoding.UTF8)
            let fs = File.ReadAllText(fsPath, Text.Encoding.UTF8)
            let results = ResizeArray<VerificationResult>()
            let htmlTitle = extractTagContent "title" html |> List.tryHead |> Option.defaultValue ""
            if not (String.IsNullOrWhiteSpace(htmlTitle)) then
                if fs.Contains(htmlTitle) then
                    results.Add(Pass "Title preserved")
                else
                    results.Add(Warning "Title may not be preserved in F# file")
            let htmlStyles = extractTagContent "style" html
            if not htmlStyles.IsEmpty then
                if fs.Contains("style") then
                    results.Add(Pass "Styles preserved")
                else
                    results.Add(Warning "Styles may not be preserved")
            results |> Seq.toList
        with ex ->
            [Fail (sprintf "Error checking content preservation: %s" ex.Message)]

    /// <summary>
    /// Runs all verification checks.
    /// </summary>
    let runAllChecks () : VerificationReport =
        let timestamp = DateTime.Now
        let messages = ResizeArray<string>()
        let mutable passed = 0
        let mutable failed = 0
        let mutable warnings = 0
        messages.Add("Starting comprehensive verification...")
        let orphanFiles = findOrphanHtmlFiles "."
        if orphanFiles.IsEmpty then
            messages.Add("No orphan HTML files found")
            passed <- passed + 1
        else
            messages.Add(sprintf "Found %d orphan HTML files" orphanFiles.Length)
            failed <- failed + 1
        let roundTripResults = ResizeArray<ComparisonResult>()
        let testDir = Path.Combine(".", "tests", "samples")
        if Directory.Exists(testDir) then
            let sampleDirs = Directory.GetDirectories(testDir)
            for sampleDir in sampleDirs do
                let htmlPath = Path.Combine(sampleDir, "index.html")
                let generatedPath = Path.Combine(sampleDir, "generated.html")
                if File.Exists(htmlPath) && File.Exists(generatedPath) then
                    messages.Add(sprintf "Running round-trip test on %s" sampleDir)
                    let result = runRoundTripTest htmlPath generatedPath
                    roundTripResults.Add(result)
                    if result.IsMatch then
                        passed <- passed + 1
                    else
                        failed <- failed + 1
        messages.Add("Verification complete")
        {
            Timestamp = timestamp
            TotalChecks = passed + failed + warnings
            Passed = passed
            Failed = failed
            Warnings = warnings
            OrphanFiles = orphanFiles
            RoundTripResults = roundTripResults |> Seq.toList
            Messages = messages |> Seq.toList
        }

    /// <summary>
    /// Formats a verification report for display.
    /// </summary>
    let formatReport (report: VerificationReport) : string =
        let sb = System.Text.StringBuilder()
        sb.AppendLine("========================================") |> ignore
        sb.AppendLine("Verification Report") |> ignore
        sb.AppendLine("========================================") |> ignore
        sb.AppendLine(sprintf "Timestamp: %s" (report.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))) |> ignore
        sb.AppendLine("") |> ignore
        sb.AppendLine("Summary:") |> ignore
        sb.AppendLine(sprintf "  Total Checks: %d" report.TotalChecks) |> ignore
        sb.AppendLine(sprintf "  Passed: %d" report.Passed) |> ignore
        sb.AppendLine(sprintf "  Failed: %d" report.Failed) |> ignore
        sb.AppendLine(sprintf "  Warnings: %d" report.Warnings) |> ignore
        if not report.OrphanFiles.IsEmpty then
            sb.AppendLine("") |> ignore
            sb.AppendLine("Orphan HTML Files:") |> ignore
            for file in report.OrphanFiles do
                sb.AppendLine(sprintf "  - %s" file) |> ignore
        sb.ToString()

    /// <summary>
    /// Prints a verification report to the console.
    /// </summary>
    let printReport (report: VerificationReport) : unit =
        printfn "%s" (formatReport report)


