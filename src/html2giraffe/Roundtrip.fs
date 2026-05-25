

namespace Html2Giraffe

open System
open System.Text
open Ast

module Roundtrip =
    type ValidationResult =
        | Success
        | Failure of ValidationError list

    and ValidationError = {
        Path: string
        Expected: string
        Actual: string
        Message: string
    }

    type ValidationOptions = {
        IgnoreWhitespace: bool
        IgnoreAttributeOrder: bool
    }

    let defaultValidationOptions = {
        IgnoreWhitespace = true
        IgnoreAttributeOrder = true
    }

    let normalizeHtml (options: ValidationOptions) (html: string) : string =
        let mutable result = html
        if options.IgnoreWhitespace then
            result <- result.Trim()
            result <- result.Replace("\r\n", "\n")
            while result.Contains("  ") do
                result <- result.Replace("  ", " ")
        result <- result.Replace("'", "\"")
        result <- result.Replace("/>", ">")
        result <- result.ToLowerInvariant()
        result

    let rec renderNodeToHtml (node: HtmlNode) : string =
        match node with
        | Element el -> renderElementToHtml el
        | Text text ->
            if text.IsRaw then text.Content
            else text.Content.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")
        | Comment comment -> sprintf "<!--%s-->" comment
        | DocType docType -> sprintf "<!DOCTYPE %s>" docType
        | Raw raw -> raw

    and renderElementToHtml (el: ElementNode) : string =
        let sb = StringBuilder()
        sb.Append(sprintf "<%s" el.TagName) |> ignore
        el.Attributes
        |> List.sortBy (fun a -> a.Name)
        |> List.iter (fun attr ->
            if attr.IsBoolean then
                sb.Append(sprintf " %s" attr.Name) |> ignore
            else
                match attr.Value with
                | Some value ->
                    let escaped = value.Replace("\"", "&quot;")
                    sb.Append(sprintf " %s=\"%s\"" attr.Name escaped) |> ignore
                | None -> sb.Append(sprintf " %s" attr.Name) |> ignore
        )
        if el.IsVoid then
            sb.Append(">") |> ignore
        else
            sb.Append(">") |> ignore
            el.Children
            |> List.map renderNodeToHtml
            |> String.concat ""
            |> sb.Append |> ignore
            sb.Append(sprintf "</%s>" el.TagName) |> ignore
        sb.ToString()

    let renderDocumentToHtml (document: HtmlDocument) : string =
        let sb = StringBuilder()
        document.DocType |> Option.iter (fun dt -> sb.AppendLine(sprintf "<!DOCTYPE %s>" dt) |> ignore)
        document.Children
        |> List.map renderNodeToHtml
        |> String.concat "\n"
        |> sb.Append |> ignore
        sb.ToString()

    let validateWithOptions (options: ValidationOptions) (originalHtml: string) (reconstructedHtml: string) : ValidationResult =
        let normalizedOriginal = normalizeHtml options originalHtml
        let normalizedReconstructed = normalizeHtml options reconstructedHtml
        if normalizedOriginal = normalizedReconstructed then
            Success
        else
            let error = {
                Path = "/"
                Expected = normalizedOriginal
                Actual = normalizedReconstructed
                Message = "HTML does not match after roundtrip"
            }
            Failure [error]

    let validate (originalHtml: string) (reconstructedHtml: string) : ValidationResult =
        validateWithOptions defaultValidationOptions originalHtml reconstructedHtml

    let testRoundtrip (html: string) : ValidationResult =
        let document = Parser.parse html
        let reconstructed = renderDocumentToHtml document
        validate html reconstructed

    let generateDiffReport (result: ValidationResult) : string =
        match result with
        | Success -> "Roundtrip validation passed!"
        | Failure errors ->
            let sb = StringBuilder()
            sb.AppendLine("Roundtrip Validation Failed") |> ignore
            sb.AppendLine("===========================") |> ignore
            errors |> List.iteri (fun i error ->
                sb.AppendLine(sprintf "Error %d:" (i + 1)) |> ignore
                sb.AppendLine(sprintf "  Path: %s" error.Path) |> ignore
                sb.AppendLine(sprintf "  Message: %s" error.Message) |> ignore
            )
            sb.ToString()






