

namespace Html2Giraffe

open System
open AngleSharp
open AngleSharp.Html.Parser
open AngleSharp.Dom
open Ast
open Attributes

module Parser =
    type ParserOptions = {
        SkipComments: bool
        PreserveWhitespace: bool
        IncludeDocType: bool
    }

    let defaultOptions = {
        SkipComments = false
        PreserveWhitespace = false
        IncludeDocType = false
    }

    let private parseAttribute (attr: IAttr) : Attribute =
        let name = attr.Name.ToLowerInvariant()
        let value = if String.IsNullOrEmpty(attr.Value) then None else Some attr.Value
        let isBool = isBooleanAttribute name || (value = Some "" || value = Some name)
        { Name = name; Value = (if isBool then None else value); IsBoolean = isBool }

    let private parseTextNode (text: string) (options: ParserOptions) : HtmlNode option =
        if String.IsNullOrEmpty(text) then None
        elif not options.PreserveWhitespace && String.IsNullOrWhiteSpace(text) then None
        else Some (Text { Content = (if options.PreserveWhitespace then text else text.Trim()); IsRaw = false })

    let private parseComment (comment: IComment) (options: ParserOptions) : HtmlNode option =
        if options.SkipComments then None else Some (Comment comment.Data)

    let rec private parseElement (element: IElement) (options: ParserOptions) : HtmlNode =
        let tagName = element.TagName.ToLowerInvariant()
        let isVoid = isVoidElement tagName || element.Flags.HasFlag(NodeFlags.SelfClosing)
        let attributes = element.Attributes |> Seq.map parseAttribute |> Seq.toList
        let children =
            if isVoid then []
            else element.ChildNodes |> Seq.choose (fun node -> parseNode node options) |> Seq.toList
        Element { TagName = tagName; Attributes = attributes; Children = children; IsVoid = isVoid }

    and private parseNode (node: INode) (options: ParserOptions) : HtmlNode option =
        match node.NodeType with
        | NodeType.Element -> Some (parseElement (node :?> IElement) options)
        | NodeType.Text -> parseTextNode node.TextContent options
        | NodeType.Comment -> parseComment (node :?> IComment) options
        | NodeType.DocumentType ->
            if options.IncludeDocType then
                let docType = node :?> IDocumentType
                Some (DocType docType.Name)
            else None
        | _ -> None

    let parseWithOptions (options: ParserOptions) (html: string) : HtmlDocument =
        if String.IsNullOrWhiteSpace(html) then emptyDocument
        else
            let parser = HtmlParser()
            let document = parser.ParseDocument(html)
            let docType = if options.IncludeDocType then document.Doctype |> Option.ofObj |> Option.map (fun dt -> dt.Name) else None
            let children = document.Body.ChildNodes |> Seq.choose (fun node -> parseNode node options) |> Seq.toList
            { DocType = docType; Children = children }

    let parse (html: string) : HtmlDocument =
        parseWithOptions defaultOptions html

    let parseFile (filePath: string) : HtmlDocument =
        let html = System.IO.File.ReadAllText(filePath)
        parse html






