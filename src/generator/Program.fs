
namespace Generator

open System
open System.IO
open Spectre.Console
open GiraffeDslConverter
open Verification

module Program =
    let printHelp () =
        AnsiConsole.WriteLine()
        AnsiConsole.Write(Rule("[blue]HTML to Giraffe Converter[/]").RuleStyle("grey"))
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[yellow]Usage:[/]")
        AnsiConsole.MarkupLine("  html2giraffe [[command]] [[options]]")
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[yellow]Commands:[/]")
        AnsiConsole.MarkupLine("  [green]convert[/] <input> [[output]]     Convert HTML file to F# Giraffe DSL")
        AnsiConsole.MarkupLine("  [green]batch[/] <input-dir> <output>  Batch convert HTML files")
        AnsiConsole.MarkupLine("  [green]verify[/]                      Verify conversion integrity")
        AnsiConsole.MarkupLine("  [green]help[/]                        Show this help message")
        AnsiConsole.MarkupLine("  [green]version[/]                     Show version information")
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[yellow]Options for convert:[/]")
        AnsiConsole.MarkupLine("  [green]--dsl[/]                       Use proper Giraffe DSL (default)")
        AnsiConsole.MarkupLine("  [green]--literal[/]                   Use triple-quoted literal mode")
        AnsiConsole.MarkupLine("  [green]--lines[/]                     Use line-by-line variable mode")
        AnsiConsole.WriteLine()

    let printVersion () =
        AnsiConsole.MarkupLine("[blue]html2giraffe[/] version [yellow]1.0.0[/]")
        AnsiConsole.MarkupLine("HTML to F# Giraffe.ViewEngine Converter")

    /// <summary>
    /// Converts HTML to F# using the new proper Giraffe DSL converter.
    /// </summary>
    let convertToGiraffeDsl (inputPath: string) (outputPath: string) =
        let html = File.ReadAllText(inputPath)
        let fsharpCode = GiraffeDslConverter.convert html
        File.WriteAllText(outputPath, fsharpCode)

    /// <summary>
    /// Converts HTML to F# using triple-quoted literal mode.
    /// </summary>
    let convertToLiteral (inputPath: string) (outputPath: string) =
        let html = File.ReadAllText(inputPath)
        let fsharpCode = EnhancedConverter.convertQuick html
        File.WriteAllText(outputPath, fsharpCode)

    /// <summary>
    /// Converts HTML to F# using line-by-line variable mode.
    /// </summary>
    let convertToLines (inputPath: string) (outputPath: string) =
        let html = File.ReadAllText(inputPath)
        let moduleName = Path.GetFileNameWithoutExtension(inputPath) |> fun s -> s.[0..0].ToUpper() + s.[1..]
        let fsharpCode = EnhancedConverter.convertWithLines html moduleName
        File.WriteAllText(outputPath, fsharpCode)

    let handleConvert (args: string[]) =
        if args.Length < 2 then
            AnsiConsole.MarkupLine("[red]Error:[/] Input file required")
            1
        else
            let inputPath = args.[1]
            
            // Check for mode flags
            let mode = 
                if args |> Array.contains "--literal" then "literal"
                elif args |> Array.contains "--lines" then "lines"
                else "dsl"
            
            // Find output path (skip mode flags)
            let outputPath = 
                let nonFlagArgs = args |> Array.filter (fun a -> not (a.StartsWith("--")))
                if nonFlagArgs.Length > 2 then nonFlagArgs.[2]
                else Path.ChangeExtension(inputPath, ".fs")
            
            if not (File.Exists(inputPath)) then
                AnsiConsole.MarkupLine(sprintf "[red]Error:[/] File not found: %s" inputPath)
                1
            else
                try
                    AnsiConsole.Status().Start(
                        sprintf "Converting [blue]%s[/] using [green]%s[/] mode..." (Path.GetFileName(inputPath)) mode,
                        fun ctx ->
                            match mode with
                            | "literal" -> convertToLiteral inputPath outputPath
                            | "lines" -> convertToLines inputPath outputPath
                            | _ -> convertToGiraffeDsl inputPath outputPath
                    )
                    AnsiConsole.MarkupLine(sprintf "[green]Success:[/] Converted to [blue]%s[/]" outputPath)
                    0
                with ex ->
                    AnsiConsole.MarkupLine(sprintf "[red]Error:[/] %s" ex.Message)
                    1

    let handleBatch (args: string[]) =
        if args.Length < 3 then
            AnsiConsole.MarkupLine("[red]Error:[/] Input and output directories required")
            1
        else
            let inputDir = args.[1]
            let outputDir = args.[2]
            
            // Check for mode flags
            let mode = 
                if args |> Array.contains "--literal" then "literal"
                elif args |> Array.contains "--lines" then "lines"
                else "dsl"
            
            if not (Directory.Exists(inputDir)) then
                AnsiConsole.MarkupLine(sprintf "[red]Error:[/] Directory not found: %s" inputDir)
                1
            else
                try
                    let htmlFiles = Directory.GetFiles(inputDir, "*.html", SearchOption.AllDirectories)
                    
                    AnsiConsole.Status().Start(
                        sprintf "Converting [blue]%d[/] files using [green]%s[/] mode..." htmlFiles.Length mode,
                        fun ctx ->
                            for htmlFile in htmlFiles do
                                let relativePath = htmlFile.Substring(inputDir.Length).TrimStart('/', '\\')
                                let outputFile = Path.Combine(outputDir, Path.ChangeExtension(relativePath, ".fs"))
                                Directory.CreateDirectory(Path.GetDirectoryName(outputFile)) |> ignore
                                
                                match mode with
                                | "literal" -> convertToLiteral htmlFile outputFile
                                | "lines" -> convertToLines htmlFile outputFile
                                | _ -> convertToGiraffeDsl htmlFile outputFile
                    )
                    
                    AnsiConsole.MarkupLine(sprintf "[green]Success:[/] Converted [blue]%d[/] files" htmlFiles.Length)
                    0
                with ex ->
                    AnsiConsole.MarkupLine(sprintf "[red]Error:[/] %s" ex.Message)
                    1

    let handleVerify (args: string[]) =
        try
            let report = runAllChecks()
            printReport report
            if List.isEmpty report.OrphanFiles then 0 else 2
        with ex ->
            AnsiConsole.MarkupLine(sprintf "[red]Error:[/] %s" ex.Message)
            1

    [<EntryPoint>]
    let main (args: string[]) : int =
        try
            if args.Length = 0 then
                printHelp()
                0
            else
                match args.[0].ToLowerInvariant() with
                | "convert" | "c" -> handleConvert args
                | "batch" | "b" -> handleBatch args
                | "verify" | "v" -> handleVerify args
                | "help" | "h" | "--help" | "-h" -> printHelp(); 0
                | "version" | "--version" | "-v" -> printVersion(); 0
                | _ ->
                    AnsiConsole.MarkupLine(sprintf "[red]Unknown command:[/] %s" args.[0])
                    printHelp()
                    1
        with ex ->
            AnsiConsole.MarkupLine(sprintf "[red]Fatal error:[/] %s" ex.Message)
            1


