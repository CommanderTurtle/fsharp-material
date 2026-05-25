module GiraffeDslConverter

open System
open System.Text
open System.Text.RegularExpressions
open System.Net

// =============================================================================
// Configuration
// =============================================================================

type ConversionConfig = {
    UseRawTextForScript: bool
    UseRawTextForStyle: bool
    EscapeTripleQuotes: bool
    IndentSize: int
    Namespace: string
    ModuleName: string
}

let defaultConfig = {
    UseRawTextForScript = true
    UseRawTextForStyle = true
    EscapeTripleQuotes = true
    IndentSize = 4
    Namespace = "Generated"
    ModuleName = "Views"
}

// =============================================================================
// HTML AST Types
// =============================================================================

type HtmlAttribute = {
    Name: string
    Value: string option
}

type HtmlElement =
    | Element of tag: string * attrs: HtmlAttribute list * children: HtmlElement list
    | VoidElement of tag: string * attrs: HtmlAttribute list
    | TextNode of text: string
    | Script of content: string * attrs: HtmlAttribute list
    | Style of content: string * attrs: HtmlAttribute list
    | Comment of text: string
    | Doctype of value: string

// =============================================================================
// HTML Parser
// =============================================================================

let voidElements : Set<string> = 
    Set ["area"; "base"; "br"; "col"; "embed"; "hr"; "img"; "input"; 
         "link"; "meta"; "param"; "source"; "track"; "wbr"]

let isVoidElement (tag: string) = voidElements.Contains(tag.ToLower())

let isRawContentElement (tag: string) =
    let t = tag.ToLower()
    t = "script" || t = "style"

let parseAttributes (attrString: string) : HtmlAttribute list =
    let attrs = ResizeArray<HtmlAttribute>()
    let mutable i = 0
    let len = attrString.Length
    
    while i < len do
        // Skip whitespace
        while i < len && Char.IsWhiteSpace(attrString.[i]) do
            i <- i + 1
        
        if i >= len then ()
        else
            // Read attribute name
            let nameStart = i
            while i < len && attrString.[i] <> '=' && not (Char.IsWhiteSpace(attrString.[i])) do
                i <- i + 1
            let name = attrString.Substring(nameStart, i - nameStart)
            
            if name = "" then ()
            elif i < len && attrString.[i] = '=' then
                i <- i + 1 // skip '='
                // Skip whitespace after =
                while i < len && Char.IsWhiteSpace(attrString.[i]) do
                    i <- i + 1
                
                if i < len && (attrString.[i] = '"' || attrString.[i] = '\'') then
                    let quote = attrString.[i]
                    i <- i + 1 // skip opening quote
                    let valueStart = i
                    while i < len && attrString.[i] <> quote do
                        i <- i + 1
                    let value = attrString.Substring(valueStart, i - valueStart)
                    if i < len && attrString.[i] = quote then
                        i <- i + 1 // skip closing quote
                    attrs.Add({ Name = name; Value = Some value })
                else
                    // Unquoted value
                    let valueStart = i
                    while i < len && not (Char.IsWhiteSpace(attrString.[i])) do
                        i <- i + 1
                    let value = attrString.Substring(valueStart, i - valueStart)
                    attrs.Add({ Name = name; Value = Some value })
            else
                // Boolean attribute (no value)
                attrs.Add({ Name = name; Value = None })
    
    attrs |> Seq.toList

let findNextTagEnd (html: string) (pos: int) : int =
    let mutable quoteChar = '\u0000'
    let mutable result = -1
    let mutable i = pos
    while i < html.Length && result < 0 do
        if quoteChar = '\u0000' then
            if html.[i] = '>' then result <- i
            elif html.[i] = '"' || html.[i] = '\'' then quoteChar <- html.[i]
        else
            if html.[i] = quoteChar then quoteChar <- '\u0000'
        i <- i + 1
    result

let findClosingTag (html: string) (pos: int) (tagName: string) : int =
    let closeTag = $"</{tagName}>"
    html.IndexOf(closeTag, pos, StringComparison.OrdinalIgnoreCase)

let rec parseHtml (html: string) (pos: int) : HtmlElement list * int =
    let elements = ResizeArray<HtmlElement>()
    let mutable i = pos
    let mutable inText = false
    let textStart = ref pos
    let mutable closingTagPos = -1
    
    while i < html.Length && closingTagPos < 0 do
        if html.[i] = '<' then
            // Save any pending text
            if inText && i > !textStart then
                let text = html.Substring(!textStart, i - !textStart)
                if not (String.IsNullOrWhiteSpace(text)) then
                    elements.Add(TextNode(text.Trim()))
                inText <- false
            
            // Check what kind of tag
            if i + 1 < html.Length && html.[i + 1] = '!' then
                if i + 3 < html.Length && html.Substring(i, 3) = "<!--" then
                    let commentEnd = html.IndexOf("-->", i)
                    if commentEnd > i then
                        let comment = html.Substring(i + 4, commentEnd - i - 4)
                        elements.Add(Comment(comment))
                        i <- commentEnd + 3
                    else
                        i <- i + 1
                elif i + 9 < html.Length && html.Substring(i, 9).ToUpper() = "<!DOCTYPE" then
                    let dtEnd = findNextTagEnd html i
                    if dtEnd > i then
                        let dt = html.Substring(i + 9, dtEnd - i - 9).Trim()
                        elements.Add(Doctype(dt))
                        i <- dtEnd + 1
                    else
                        i <- i + 1
                else
                    i <- i + 1
            elif i + 1 < html.Length && html.[i + 1] = '/' then
                let closeEnd = findNextTagEnd html i
                i <- if closeEnd > i then closeEnd + 1 else i + 1
                closingTagPos <- i
            else
                let tagStart = i + 1
                let tagEnd = findNextTagEnd html i
                
                if tagEnd > tagStart then
                    let fullTag = html.Substring(tagStart, tagEnd - tagStart)
                    let spaceIdx = fullTag.IndexOf(' ')
                    
                    let tagName = 
                        if spaceIdx > 0 then fullTag.Substring(0, spaceIdx).ToLower()
                        else fullTag.ToLower()
                    
                    let attrs =
                        if spaceIdx > 0 then parseAttributes (fullTag.Substring(spaceIdx))
                        else []
                    
                    i <- tagEnd + 1
                    
                    if isVoidElement tagName then
                        elements.Add(VoidElement(tagName, attrs))
                    elif isRawContentElement tagName then
                        let closePos = findClosingTag html i tagName
                        
                        if closePos > i then
                            let content = html.Substring(i, closePos - i)
                            if tagName = "script" then
                                elements.Add(Script(content, attrs))
                            elif tagName = "style" then
                                elements.Add(Style(content, attrs))
                            else
                                elements.Add(TextNode(content))
                            i <- closePos + (String.length tagName) + 3
                        else
                            i <- closePos + 1
                    else
                        let (children, newPos) = parseHtml html i
                        elements.Add(Element(tagName, attrs, children))
                        i <- newPos
                else
                    i <- i + 1
        else
            if not inText then
                textStart := i
                inText <- true
            i <- i + 1
    
    if inText && i > !textStart then
        let text = html.Substring(!textStart, i - !textStart)
        if not (String.IsNullOrWhiteSpace(text)) then
            elements.Add(TextNode(text.Trim()))
    
    (elements |> Seq.toList, i)

// =============================================================================
// F# Code Generation
// =============================================================================

let indent level size = String.replicate (level * size) " "

let generateAttr (attr: HtmlAttribute) : string =
    match attr.Name.ToLower(), attr.Value with
    | "class", Some v -> sprintf "_class \"%s\"" (WebUtility.HtmlDecode v)
    | "id", Some v -> sprintf "_id \"%s\"" (WebUtility.HtmlDecode v)
    | "href", Some v -> sprintf "_href \"%s\"" (WebUtility.HtmlDecode v)
    | "src", Some v -> sprintf "_src \"%s\"" (WebUtility.HtmlDecode v)
    | "alt", Some v -> sprintf "_alt \"%s\"" (WebUtility.HtmlDecode v)
    | "type", Some v -> sprintf "_type \"%s\"" (WebUtility.HtmlDecode v)
    | "lang", Some v -> sprintf "_lang \"%s\"" (WebUtility.HtmlDecode v)
    | name, Some v -> 
        if name.StartsWith("data-") then
            sprintf "attr \"%s\" \"%s\"" name (WebUtility.HtmlDecode v)
        else
            sprintf "attr \"%s\" \"%s\"" name (WebUtility.HtmlDecode v)
    | name, None -> sprintf "attr \"%s\" \"\"" name

let generateAttrs (attrs: HtmlAttribute list) : string =
    match attrs with
    | [] -> "[]"
    | [a] -> sprintf "[ %s ]" (generateAttr a)
    | _ -> 
        let attrStrs = attrs |> List.map generateAttr
        sprintf "[ %s ]" (String.concat "; " attrStrs)

let rec generateDsl (element: HtmlElement) (level: int) (config: ConversionConfig) : string =
    let ind = indent level config.IndentSize
    
    match element with
    | Element(tag, attrs, children) ->
        let attrsCode = generateAttrs attrs
        let childrenCode = 
            children 
            |> List.map (fun c -> generateDsl c (level + 1) config)
            |> String.concat "\n"
        
        if String.IsNullOrWhiteSpace(childrenCode) then
            sprintf "%s%s %s []" ind tag attrsCode
        else
            sprintf "%s%s %s [\n%s\n%s]" ind tag attrsCode childrenCode ind
    
    | VoidElement(tag, attrs) ->
        let attrsCode = generateAttrs attrs
        sprintf "%s%s %s" ind tag attrsCode
    
    | TextNode(text) ->
        let dq = string '"'
        let esc = "\\" + string '"'
        let safeText = WebUtility.HtmlDecode(text).Replace(dq, esc)
        sprintf "%sstr \"%s\"" ind safeText
    
    | Script(content, attrs) ->
        let attrsCode = generateAttrs attrs
        let safeContent = content.Trim()
        let childInd = indent (level + 1) config.IndentSize
        let tq = new string('"', 3)
        
        let rawTextExpr =
            if config.EscapeTripleQuotes && safeContent.Contains(tq) then
                let parts = safeContent.Split([|tq|], StringSplitOptions.None)
                let concat = 
                    parts
                    |> Array.mapi (fun i p -> tq + p + tq)
                    |> String.concat " + "
                sprintf "rawText (%s)" concat
            else
                sprintf "rawText (%s%s%s)" tq safeContent tq
        
        if safeContent.Contains("\n") then
            sprintf "%sscript %s [\n%s    %s\n%s]" ind attrsCode childInd rawTextExpr ind
        else
            sprintf "%sscript %s [ %s ]" ind attrsCode rawTextExpr
    
    | Style(content, attrs) ->
        let attrsCode = generateAttrs attrs
        let safeContent = content.Trim()
        let childInd = indent (level + 1) config.IndentSize
        let tq = new string('"', 3)
        
        let rawTextExpr =
            if config.EscapeTripleQuotes && safeContent.Contains(tq) then
                let parts = safeContent.Split([|tq|], StringSplitOptions.None)
                let concat = 
                    parts
                    |> Array.mapi (fun i p -> tq + p + tq)
                    |> String.concat " + "
                sprintf "rawText (%s)" concat
            else
                sprintf "rawText (%s%s%s)" tq safeContent tq
        
        if safeContent.Contains("\n") then
            sprintf "%sstyle %s [\n%s    %s\n%s]" ind attrsCode childInd rawTextExpr ind
        else
            sprintf "%sstyle %s [ %s ]" ind attrsCode rawTextExpr
    
    | Comment(text) ->
        let dq = string '"'
        let esc = "\\" + string '"'
        let safe = text.Replace(dq, esc).Replace("--", "- -")
        sprintf "%srawText (\"<!-- %s -->\")" ind safe
    
    | Doctype(value) ->
        ""

// =============================================================================
// Module Generation
// =============================================================================

let generateModule (elements: HtmlElement list) (config: ConversionConfig) : string =
    let sb = StringBuilder()
    
    // Flat module style - no namespace block, no equals sign
    if not (String.IsNullOrEmpty(config.Namespace)) then
        sb.AppendLine(sprintf "module %s.%s" config.Namespace config.ModuleName) |> ignore
    else
        sb.AppendLine(sprintf "module %s" config.ModuleName) |> ignore
    
    sb.AppendLine() |> ignore
    sb.AppendLine("open Giraffe.ViewEngine") |> ignore
    sb.AppendLine() |> ignore
    sb.AppendLine("let page =") |> ignore
    
    match elements with
    | [] ->
        sb.AppendLine("    html [] []") |> ignore
    
    | [single] ->
        let code = generateDsl single 1 config
        if not (String.IsNullOrWhiteSpace(code)) then sb.AppendLine(code) |> ignore
    
    | elements ->
        let hasHtml = elements |> List.exists (function | Element("html", _, _) -> true | _ -> false)
        
        if hasHtml then
            for element in elements do
                let code = generateDsl element 1 config
                if not (String.IsNullOrWhiteSpace(code)) then sb.AppendLine(code) |> ignore
        else
            sb.AppendLine("    html [] [") |> ignore
            sb.AppendLine("        head [] []") |> ignore
            sb.AppendLine("        body [] [") |> ignore
            
            for element in elements do
                let code = generateDsl element 3 config
                if not (String.IsNullOrWhiteSpace(code)) then sb.AppendLine(code) |> ignore
            
            sb.AppendLine("        ]") |> ignore
            sb.AppendLine("    ]") |> ignore
    
    sb.AppendLine() |> ignore
    sb.AppendLine("let render() =") |> ignore
    sb.AppendLine("    page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument") |> ignore
    
    sb.ToString()

let convert (html: string) : string =
    let (elements, _) = parseHtml html 0
    generateModule elements defaultConfig
