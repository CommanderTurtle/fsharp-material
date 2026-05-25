

namespace Html2Giraffe

open System
open System.Text
open Ast
open Attributes

module Converter =
    let convertAttribute (attr: Attribute) : string =
        let attrName = getGiraffeAttrName attr.Name
        if attr.IsBoolean then
            sprintf "%s" attrName
        else
            match attr.Value with
            | Some value ->
                let escaped = escapeForFsharp value
                sprintf "%s \"%s\"" attrName escaped
            | None -> sprintf "%s \"\"" attrName

    let convertAttributes (attrs: Attribute list) : string =
        match attrs with
        | [] -> "[]"
        | _ ->
            attrs
            |> List.map convertAttribute
            |> String.concat "; "
            |> sprintf "[ %s ]"

    let rec convertNode (node: HtmlNode) : string =
        match node with
        | Element el -> convertElement el
        | Text text ->
            if text.IsRaw then
                sprintf "rawText \"%s\"" (escapeForFsharp text.Content)
            else
                sprintf "text \"%s\"" (escapeForFsharp text.Content)
        | Comment comment -> sprintf "// %s" comment
        | DocType dt -> sprintf "DocType \"%s\"" dt
        | Raw raw -> sprintf "Raw \"%s\"" (escapeForFsharp raw)

    and convertElement (el: ElementNode) : string =
        let tagName = el.TagName
        let attrsCode = convertAttributes el.Attributes
        if el.IsVoid then
            sprintf "%s %s []" tagName attrsCode
        elif List.isEmpty el.Children then
            sprintf "%s %s []" tagName attrsCode
        else
            let childrenCode =
                el.Children
                |> List.map convertNode
                |> String.concat "\n                "
            sprintf "%s %s [\n                %s\n            ]" tagName attrsCode childrenCode

    let convertToModule (namespaceName: string) (moduleName: string) (viewName: string) (doc: HtmlDocument) : string =
        let sb = StringBuilder()
        sb.AppendLine(sprintf "namespace %s" namespaceName) |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine("open Giraffe.ViewEngine") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine(sprintf "module %s =" moduleName) |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine(sprintf "    let %s =" viewName) |> ignore
        match doc.Children with
        | [] -> sb.AppendLine("        div [] []") |> ignore
        | [single] ->
            sb.AppendLine(sprintf "        %s" (convertNode single)) |> ignore
        | children ->
            sb.AppendLine("        [") |> ignore
            for child in children do
                sb.AppendLine(sprintf "            %s" (convertNode child)) |> ignore
            sb.AppendLine("        ]") |> ignore
        sb.ToString()

    let convertToString (doc: HtmlDocument) : string =
        convertToModule "Generated" "Views" "page" doc






