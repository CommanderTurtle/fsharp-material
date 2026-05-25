
namespace Generator

open System
open System.Text

/// <summary>
/// XML-style literal handling inspired by the TurtleProtect XML project.
/// Implements CMD-style tokenization and substring operations in F#.
/// </summary>
module XmlStyleLiteral =

    /// <summary>
    /// Represents a tokenized line from the XML project approach.
    /// </summary>
    type TokenizedLine = {
        Index: int
        Content: string
        Tokens: string list
    }

    /// <summary>
    /// CMD-style FOR /F tokenization in F#.
    /// Splits a line into tokens by delimiters.
    /// </summary>
    let tokenizeLine (line: string) (delimiters: string list) : string list =
        let delimArray = delimiters |> List.toArray
        line.Split(delimArray, StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
        |> List.map (fun s -> s.Trim())

    /// <summary>
    /// CMD-style %var:~start,length% substring.
    /// </summary>
    let cmdSubstring (content: string) (start: int) (length: int option) : string =
        match length with
        | Some len ->
            DelimiterExtractor.slice content start len
        | None ->
            DelimiterExtractor.sliceToEnd content start

    /// <summary>
    /// CMD-style string replacement: %var:old=new%
    /// </summary>
    let cmdReplace (content: string) (oldValue: string) (newValue: string) : string =
        content.Replace(oldValue, newValue)

    /// <summary>
    /// Wraps content in XML-style CDATA-like safe container.
    /// Uses F# triple-quoted strings as the equivalent.
    /// </summary>
    let toCData (content: string) : string =
        // In F#, triple-quoted strings are safer than CDATA
        SafeStringBuilder.toTripleQuoted content

    /// <summary>
    /// Creates an XML-style variable array from lines.
    /// Similar to: set line[0]=... set line[1]=...
    /// </summary>
    let toVariableArray (lines: string list) (arrayName: string) : string =
        let sb = StringBuilder()
        
        for i, line in List.indexed lines do
            let escaped = SafeStringBuilder.toTripleQuoted line
            sb.AppendLine($"let {arrayName}_{i} = {escaped}") |> ignore
        
        sb.AppendLine() |> ignore
        sb.AppendLine($"let {arrayName} = [|") |> ignore
        
        for i = 0 to lines.Length - 1 do
            sb.AppendLine($"    {arrayName}_{i}") |> ignore
        
        sb.AppendLine("|]") |> ignore
        sb.ToString()

    /// <summary>
    /// Echo-style output: writes each line with optional prefix/suffix.
    /// </summary>
    let echoLines (lines: string list) (prefix: string) (suffix: string) : string list =
        lines |> List.map (fun line -> prefix + line + suffix)

    /// <summary>
    /// Boolean condition evaluation similar to CMD IF statements.
    /// </summary>
    let evalCondition (content: string) (condition: string) : bool =
        match condition.ToLower() with
        | "empty" -> String.IsNullOrEmpty(content)
        | "notempty" -> not (String.IsNullOrEmpty(content))
        | "contains<script" -> content.Contains("<script")
        | "contains<style" -> content.Contains("<style")
        | "contains<body" -> content.Contains("<body")
        | _ -> false

    /// <summary>
    /// Processes content with conditional logic.
    /// </summary>
    let processConditional (content: string) (condition: string) (ifTrue: string -> string) (ifFalse: string -> string) : string =
        if evalCondition content condition then
            ifTrue content
        else
            ifFalse content

    /// <summary>
    /// Extracts content between XML-style tags.
    /// </summary>
    let extractTag (content: string) (tagName: string) : string option =
        let startTag = $"<{tagName}"
        let endTag = $"</{tagName}>"
        
        let start = content.IndexOf(startTag)
        if start < 0 then None
        else
            let tagEnd = content.IndexOf(">", start)
            if tagEnd < 0 then None
            else
                let contentStart = tagEnd + 1
                let endPos = content.IndexOf(endTag, contentStart)
                if endPos < 0 then None
                else
                    Some (content.Substring(contentStart, endPos - contentStart))

    /// <summary>
    /// Extracts attribute value from XML/HTML tag.
    /// </summary>
    let extractAttribute (tag: string) (attrName: string) : string option =
        let pattern = $"{attrName}=\\\""
        let start = tag.IndexOf(pattern)
        if start < 0 then
            // Try single quotes
            let pattern2 = $"{attrName}='"
            let start2 = tag.IndexOf(pattern2)
            if start2 < 0 then None
            else
                let valueStart = start2 + pattern2.Length
                let valueEnd = tag.IndexOf("'", valueStart)
                if valueEnd < 0 then None
                else Some (tag.Substring(valueStart, valueEnd - valueStart))
        else
            let valueStart = start + pattern.Length
            let valueEnd = tag.IndexOf("\"", valueStart)
            if valueEnd < 0 then None
            else Some (tag.Substring(valueStart, valueEnd - valueStart))

    /// <summary>
    /// Creates a complete XML-style literal module.
    /// </summary>
    let toLiteralModule (content: string) (moduleName: string) : string =
        let lines = 
            content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
            |> Array.toList
        
        let sb = StringBuilder()
        
        sb.AppendLine($"module {moduleName}") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine("open System") |> ignore
        sb.AppendLine() |> ignore
        
        // Line count constant
        sb.AppendLine($"let lineCount = {lines.Length}") |> ignore
        sb.AppendLine() |> ignore
        
        // Individual line variables (XML-style)
        for i, line in List.indexed lines do
            let varName = $"line{i}"
            let escaped = SafeStringBuilder.toTripleQuoted line
            sb.AppendLine($"let {varName} = {escaped}") |> ignore
        
        sb.AppendLine() |> ignore
        
        // Array of all lines
        sb.AppendLine("let lines = [|") |> ignore
        for i = 0 to lines.Length - 1 do
            sb.AppendLine($"    line{i}") |> ignore
        sb.AppendLine("|]") |> ignore
        sb.AppendLine() |> ignore
        
        // Render function (concatenates all lines)
        sb.AppendLine("/// <summary>") |> ignore
        sb.AppendLine("/// Renders the complete content by concatenating all lines.") |> ignore
        sb.AppendLine("/// </summary>") |> ignore
        sb.AppendLine("let render() =") |> ignore
        
        if lines.Length = 0 then
            sb.AppendLine("    \"\"") |> ignore
        elif lines.Length = 1 then
            sb.AppendLine("    line0") |> ignore
        else
            let varList = [0..lines.Length-1] |> List.map (fun i -> $"line{i}")
            let concat = String.Join(" + \"\\n\" + ", varList)
            sb.AppendLine($"    {concat}") |> ignore
        
        sb.AppendLine() |> ignore
        
        // Utility functions (XML-style access)
        sb.AppendLine("/// <summary>") |> ignore
        sb.AppendLine("/// Gets a specific line by index (0-based).") |> ignore
        sb.AppendLine("/// </summary>") |> ignore
        sb.AppendLine("let getLine (index: int) =") |> ignore
        sb.AppendLine("    if index >= 0 && index < lineCount then") |> ignore
        sb.AppendLine("        lines.[index]") |> ignore
        sb.AppendLine("    else") |> ignore
        sb.AppendLine("        \"\"") |> ignore
        sb.AppendLine() |> ignore
        
        sb.AppendLine("/// <summary>") |> ignore
        sb.AppendLine("/// Gets a range of lines (CMD-style slicing).") |> ignore
        sb.AppendLine("/// </summary>") |> ignore
        sb.AppendLine("let getRange (start: int) (count: int) =") |> ignore
        sb.AppendLine("    lines") |> ignore
        sb.AppendLine("    |> Array.skip start") |> ignore
        sb.AppendLine("    |> Array.truncate count") |> ignore
        sb.AppendLine("    |> String.concat \"\\n\"") |> ignore
        sb.AppendLine() |> ignore
        
        sb.AppendLine("/// <summary>") |> ignore
        sb.AppendLine("/// Gets lines from start to end.") |> ignore
        sb.AppendLine("/// </summary>") |> ignore
        sb.AppendLine("let getSlice (start: int) (finish: int) =") |> ignore
        sb.AppendLine("    lines") |> ignore
        sb.AppendLine("    |> Array.skip start") |> ignore
        sb.AppendLine("    |> Array.take (finish - start + 1)") |> ignore
        sb.AppendLine("    |> String.concat \"\\n\"") |> ignore
        
        sb.ToString()

    /// <summary>
    /// Example usage demonstrating XML-style literal approach.
    /// </summary>
    let exampleUsage () : string =
        let html = """<div class="test">Hello World</div>"""
        toLiteralModule html "ExampleModule"


