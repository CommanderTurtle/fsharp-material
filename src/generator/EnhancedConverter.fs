
namespace Generator

open System
open System.Text
open SafeStringBuilder
open DelimiterExtractor

/// <summary>
/// Enhanced HTML to F# converter using safe string techniques.
/// Leverages F#'s triple-quoted strings for literal HTML embedding.
/// Allows walking lines in html for literal extraction.
/// This project creates a running linecount and classifies them.
/// </summary>
module EnhancedConverter =

    /// <summary>
    /// Conversion options.
    /// </summary>
    type ConversionOptions = {
        /// Use line-by-line variable approach
        UseLineVariables: bool
        /// Variable prefix for line variables
        VariablePrefix: string
        /// Module name for generated code
        ModuleName: string
        /// Extract and separate JS/CSS
        SeparateScripts: bool
        /// Use interpolation for dynamic content
        UseInterpolation: bool
    }

    /// <summary>
    /// Default conversion options.
    /// </summary>
    let defaultOptions = {
        UseLineVariables = false
        VariablePrefix = "line"
        ModuleName = "GeneratedPage"
        SeparateScripts = true
        UseInterpolation = false
    }

    /// <summary>
    /// Represents a page component.
    /// </summary>
    type PageComponent =
        | HtmlHead of content: string
        | HtmlBody of content: string
        | JavaScript of content: string
        | CSS of content: string
        | RawContent of content: string

    /// <summary>
    /// Parses HTML into components.
    /// </summary>
    let parseComponents (html: string) (options: ConversionOptions) : PageComponent list =
        let components = ResizeArray<PageComponent>()
        
        // Extract head
        match extractHead html with
        | Some head -> components.Add(HtmlHead head)
        | None -> ()
        
        // Extract body
        match extractBody html with
        | Some body -> components.Add(HtmlBody body)
        | None -> ()
        
        // Extract scripts if requested
        if options.SeparateScripts then
            let scripts = extractJavaScript html
            for script in scripts do
                components.Add(JavaScript script.Content)
            
            let styles = extractCSS html
            for style in styles do
                components.Add(CSS style.Content)
        
        components |> Seq.toList

    /// <summary>
    /// Converts content to a line-based module with numbered variables.
    /// </summary>
    let toLineModule (content: string) (modName: string) (linePrefix: string) : string =
        let lines = content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
        let sb = StringBuilder()
        sb.AppendLine($"module {modName} =") |> ignore
        for i, line in lines |> Array.indexed do
            let varName = $"{linePrefix}{i}"
            let escaped = line.Replace("\\", "\\\\").Replace("\"", "\\\"")
            sb.AppendLine($"    let {varName} = \"{escaped}\"") |> ignore
        // Combined variable
        sb.AppendLine($"    let combined =") |> ignore
        let vars = lines |> Array.mapi (fun i _ -> $"{linePrefix}{i}") |> String.concat " + \"\\n\" + \n        "
        sb.AppendLine($"        {vars}") |> ignore
        sb.ToString()

    /// <summary>
    /// Converts a single component to F# code.
    /// </summary>
    let convertComponent (component': PageComponent) (options: ConversionOptions) : string =
        match component' with
        | HtmlHead content ->
            if options.UseLineVariables then
                toLineModule content "HeadContent" "headLine"
            else
                let escaped = toTripleQuoted content
                $"let headContent = {escaped}"
        
        | HtmlBody content ->
            if options.UseLineVariables then
                toLineModule content "BodyContent" "bodyLine"
            else
                let escaped = toTripleQuoted content
                $"let bodyContent = {escaped}"
        
        | JavaScript content ->
            if options.UseLineVariables then
                toLineModule content "ScriptContent" "scriptLine"
            else
                let escaped = toTripleQuoted content
                $"let scriptContent = {escaped}"
        
        | CSS content ->
            if options.UseLineVariables then
                toLineModule content "StyleContent" "styleLine"
            else
                let escaped = toTripleQuoted content
                $"let styleContent = {escaped}"
        
        | RawContent content ->
            if options.UseLineVariables then
                toLineModule content "RawContent" "rawLine"
            else
                let escaped = toTripleQuoted content
                $"let rawContent = {escaped}"

    /// <summary>
    /// Builds the complete F# module from components.
    /// </summary>
    let buildModule (components: PageComponent list) (options: ConversionOptions) : string =
        let sb = StringBuilder()
        
        // Module header
        sb.AppendLine($"namespace {options.ModuleName}") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine($"module Page =") |> ignore
        sb.AppendLine() |> ignore
        
        // Convert each component
        for component' in components do
            let code = convertComponent component' options
            sb.AppendLine(code) |> ignore
            sb.AppendLine() |> ignore
        
        // Build render function
        sb.AppendLine("    /// <summary>") |> ignore
        sb.AppendLine("    /// Renders the complete page.") |> ignore
        sb.AppendLine("    /// </summary>") |> ignore
        sb.AppendLine("    let render() =") |> ignore
        
        // Concatenate all components
        let componentNames = 
            components 
            |> List.mapi (fun i _ -> 
                match components.[i] with
                | HtmlHead _ -> "headContent"
                | HtmlBody _ -> "bodyContent"
                | JavaScript _ -> "scriptContent"
                | CSS _ -> "styleContent"
                | RawContent _ -> "rawContent"
            )
        
        if componentNames.IsEmpty then
            sb.AppendLine("        \"\"") |> ignore
        else
            let concat = String.Join(" + \"\\n\\n\" + ", componentNames)
            sb.AppendLine($"        {concat}") |> ignore
        
        sb.ToString()

    /// <summary>
    /// Converts HTML to F# using the enhanced safe string approach.
    /// </summary>
    let convert (html: string) (options: ConversionOptions) : string =
        let components = parseComponents html options
        buildModule components options

    /// <summary>
    /// Quick convert with default options.
    /// </summary>
    let convertQuick (html: string) : string =
        convert html defaultOptions

    /// <summary>
    /// Converts HTML with line-by-line variable approach.
    /// Best for very large files.
    /// </summary>
    let convertWithLines (html: string) (moduleName: string) : string =
        let options = { 
            defaultOptions with 
                UseLineVariables = true
                ModuleName = moduleName 
        }
        convert html options

    /// <summary>
    /// Validates that the generated F# code is safe.
    /// </summary>
    let validate (fsharpCode: string) : Result<unit, string list> =
        let errors = ResizeArray<string>()
        
        // Check for unmatched triple quotes
        let tripleQuoteCount = 
            fsharpCode.Split([|"\"\"\""|], StringSplitOptions.None).Length - 1
        
        if tripleQuoteCount % 2 <> 0 then
            errors.Add("Unmatched triple quotes detected")
        
        // Check for common issues
        if fsharpCode.Contains("let let ") then
            errors.Add("Duplicate 'let' keywords detected")
        
        if fsharpCode.Contains("module module ") then
            errors.Add("Duplicate 'module' keywords detected")
        
        if errors.Count = 0 then
            Ok ()
        else
            Error (errors |> Seq.toList)

    /// <summary>
    /// Example: Convert a simple HTML page.
    /// </summary>
    let exampleConversion () : string =
        let html = """
<!DOCTYPE html>
<html>
<head>
    <title>Test</title>
    <style>
        body { color: red; }
    </style>
</head>
<body>
    <h1>Hello World</h1>
    <script>
        console.log("Hello!");
    </script>
</body>
</html>
"""
        convertQuick html


