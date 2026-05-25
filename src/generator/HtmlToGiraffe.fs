

namespace Generator

open System
open System.IO
open System.Text
open HtmlAgilityPack
open DomRepresentation

type ParserConfig = {
    PreserveWhitespace: bool
    IncludeComments: bool
}

type CodeGenConfig = {
    Namespace: string
    ModuleName: string
    FunctionName: string
}

module HtmlParser =
    let defaultParserConfig = {
        PreserveWhitespace = false
        IncludeComments = true
    }

    let parseAttribute (attr: HtmlAttribute) : DomAttribute option =
        let name = attr.Name.ToLowerInvariant()
        let value = attr.Value
        if String.IsNullOrEmpty(value) then
            Some (Boolean name)
        elif name = "style" then
            let styles =
                value.Split(';')
                |> Array.choose (fun s ->
                    let parts = s.Split(':')
                    if parts.Length = 2 then
                        Some (parts.[0].Trim(), parts.[1].Trim())
                    else None
                )
                |> Array.toList
            Some (Style styles)
        elif name.StartsWith("data-") then
            Some (Data(name.Substring(5), value))
        else
            Some (KeyValue(name, value))

    let rec parseNode (node: HtmlNode) (config: ParserConfig) : DomNode option =
        match node.NodeType with
        | HtmlNodeType.Element ->
            let tagName = node.Name.ToLowerInvariant()
            if VoidElements.isVoid tagName then
                let attrs = node.Attributes |> Seq.choose parseAttribute |> Seq.toList
                Some (VoidElement(tagName, attrs))
            elif RawContentElements.isRawContent tagName then
                Some (RawContent(node.InnerHtml))
            else
                let attrs = node.Attributes |> Seq.choose parseAttribute |> Seq.toList
                let children =
                    node.ChildNodes
                    |> Seq.choose (fun n -> parseNode n config)
                    |> Seq.toList
                Some (Element(tagName, attrs, children))
        | HtmlNodeType.Text ->
            let text = node.InnerText
            if not config.PreserveWhitespace && String.IsNullOrWhiteSpace(text) then
                None
            else
                Some (Text text)
        | HtmlNodeType.Comment ->
            if config.IncludeComments then
                Some (Comment(node.InnerText))
            else None
        | _ -> None

    let parseHtml (config: ParserConfig) (html: string) : DomDocument =
        let doc = HtmlDocument()
        doc.LoadHtml(html)
        let headNodes =
            doc.DocumentNode.SelectNodes("//head/*")
            |> Option.ofObj
            |> Option.map (fun nodes -> nodes |> Seq.choose (fun n -> parseNode n config) |> Seq.toList)
            |> Option.defaultValue []
        let bodyNodes =
            doc.DocumentNode.SelectNodes("//body/*")
            |> Option.ofObj
            |> Option.map (fun nodes -> nodes |> Seq.choose (fun n -> parseNode n config) |> Seq.toList)
            |> Option.defaultValue []
        let doctype =
            doc.DocumentNode.SelectSingleNode("//comment()[contains(.,'DOCTYPE')]")
            |> Option.ofObj
            |> Option.map (fun n -> "html")
        {
            DocType = doctype
            Head = headNodes
            Body = bodyNodes
            CustomStyles = []
            CustomScripts = []
        }

    let parseHtmlFile (config: ParserConfig) (filePath: string) : DomDocument =
        let html = File.ReadAllText(filePath)
        parseHtml config html

module FSharpCodeGenerator =
    let defaultCodeGenConfig = {
        Namespace = "Generated"
        ModuleName = "Views"
        FunctionName = "page"
    }

    let escapeString (s: string) : string =
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")

    let rec generateNodeCode (node: DomNode) (level: int) (config: CodeGenConfig) : string =
        let ind = String.replicate (level * 4) " "
        match node with
        | Element(tag, attrs, children) ->
            let attrsCode =
                match attrs with
                | [] -> "[]"
                | _ ->
                    attrs
                    |> List.map (fun a ->
                        match a with
                        | KeyValue(name, value) -> sprintf "attr \"%s\" \"%s\"" name value
                        | Boolean name -> sprintf "attr \"%s\" \"\"" name
                        | Style styles ->
                            let styleStr = styles |> List.map (fun (k, v) -> sprintf "%s:%s" k v) |> String.concat ";"
                            sprintf "attr \"style\" \"%s\"" styleStr
                        | Data(key, value) -> sprintf "data \"%s\" \"%s\"" key value
                    )
                    |> String.concat "; "
                    |> sprintf "[ %s ]"
            if List.isEmpty children then
                sprintf "%s%s %s []" ind tag attrsCode
            else
                let childrenCode = children |> List.map (fun c -> generateNodeCode c (level + 1) config) |> String.concat "\n"
                sprintf "%s%s %s [\n%s\n%s]" ind tag attrsCode childrenCode ind
        | VoidElement(tag, attrs) ->
            let attrsCode =
                match attrs with
                | [] -> "[]"
                | _ ->
                    attrs
                    |> List.map (fun a ->
                        match a with
                        | KeyValue(name, value) -> sprintf "attr \"%s\" \"%s\"" name value
                        | Boolean name -> sprintf "attr \"%s\" \"\"" name
                        | Style styles ->
                            let styleStr = styles |> List.map (fun (k, v) -> sprintf "%s:%s" k v) |> String.concat ";"
                            sprintf "attr \"style\" \"%s\"" styleStr
                        | Data(key, value) -> sprintf "data \"%s\" \"%s\"" key value
                    )
                    |> String.concat "; "
                    |> sprintf "[ %s ]"
            sprintf "%s%s %s" ind tag attrsCode
        | Text text -> sprintf "%sstr \"%s\"" ind (escapeString text)
        | RawContent content -> sprintf "%srawText \"%s\"" ind (escapeString content)
        | Comment text -> sprintf "%s// %s" ind text
        | DocType dt -> sprintf "%srawText \"<!DOCTYPE %s>\"" ind dt

    let generateModuleCode (doc: DomDocument) (config: CodeGenConfig) : string =
        let sb = StringBuilder()
        sb.AppendLine(sprintf "namespace %s" config.Namespace) |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine("open Giraffe.ViewEngine") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine(sprintf "module %s =" config.ModuleName) |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine(sprintf "    let %s =" config.FunctionName) |> ignore
        let allNodes = doc.Head @ doc.Body
        match allNodes with
        | [] -> sb.AppendLine("        div [] []") |> ignore
        | [single] ->
            let code = generateNodeCode single 2 config
            sb.AppendLine(code) |> ignore
        | nodes ->
            sb.AppendLine("        [") |> ignore
            for node in nodes do
                let code = generateNodeCode node 3 config
                sb.AppendLine(code) |> ignore
            sb.AppendLine("        ]") |> ignore
        sb.ToString()

module HtmlToGiraffe =
    open HtmlParser
    open FSharpCodeGenerator

    let convertHtmlString (html: string) : string =
        let doc = parseHtml defaultParserConfig html
        generateModuleCode doc defaultCodeGenConfig

    let convertHtmlFile (filePath: string) : string =
        let doc = parseHtmlFile defaultParserConfig filePath
        generateModuleCode doc defaultCodeGenConfig

    let convertAndWriteFile (inputPath: string) (outputPath: string) : unit =
        let code = convertHtmlFile inputPath
        File.WriteAllText(outputPath, code)

    let batchConvertFiles (inputDir: string) (outputDir: string) (pattern: string) : int =
        Directory.CreateDirectory(outputDir) |> ignore
        let files = Directory.GetFiles(inputDir, pattern)
        files
        |> Array.filter (fun f -> f.EndsWith(".html"))
        |> Array.map (fun f ->
            let fileName = Path.GetFileNameWithoutExtension(f)
            let outputPath = Path.Combine(outputDir, fileName + ".fs")
            convertAndWriteFile f outputPath
            1
        )
        |> Array.sum






