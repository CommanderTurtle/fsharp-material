// =============================================================================
// Safe String Builder
// =============================================================================
// Handles triple-quoted string safety for F# code generation.
// Ensures content containing """ is properly escaped in generated F#.
// Empty string returns """""" (6 quotes = open """ + close """)
// =============================================================================

module SafeStringBuilder

open System
open System.Text

let containsTripleQuote (content: string) : bool =
    content.Contains("\"\"\"")

let escapeForTripleQuote (content: string) : string =
    if not (containsTripleQuote content) then
        content
    else
        let parts = content.Split([|"\"\"\""|], StringSplitOptions.None)
        if parts.Length <= 1 then
            content
        else
            let sb = StringBuilder()
            sb.Append("\"\"\"") |> ignore
            for i = 0 to parts.Length - 1 do
                sb.Append(parts.[i]) |> ignore
                if i < parts.Length - 1 then
                    sb.Append("\"\"\"") |> ignore
            sb.Append("\"\"\"") |> ignore
            sb.ToString()

let toTripleQuoted (content: string) : string =
    if String.IsNullOrEmpty(content) then
        "\"\"\"\"\"\""
    elif containsTripleQuote content then
        escapeForTripleQuote content
    else
        sprintf "\"\"\"%s\"\"\"" content

let breakTripleQuote (content: string) : string list =
    if String.IsNullOrEmpty(content) then [""]
    elif not (containsTripleQuote content) then [content]
    else
        content.Split([|"\"\"\""|], StringSplitOptions.None)
        |> Array.toList

let fromLines (lines: string list) : string =
    String.concat "\n" lines

let indent (level: int) (content: string) : string =
    let prefix = String.replicate (level * 4) " "
    content.Split('\n')
    |> Array.map (fun line -> if String.IsNullOrWhiteSpace(line) then line else prefix + line)
    |> String.concat "\n"

let toFSharpStringLiteral (content: string) : string =
    let escaped = content.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")
    sprintf "\"%s\"" escaped

let toMultilineFSharpString (content: string) : string =
    if content.Contains("\n") then
        let lines = content.Split('\n')
        let indented = 
            lines
            |> Array.mapi (fun i line -> if i = 0 then line else "            " + line)
        sprintf "\"\"\"%s\"\"\"" (String.concat "\n" indented)
    else
        toTripleQuoted content
