
module Shared.Components

open Giraffe.ViewEngine
open System

// =============================================================================
// Material for MkDocs Component Library
// =============================================================================

let admonition (``type``: string) (title: string option) (content: string) =
    let icon =
        match ``type``.ToLower() with
        | "note" -> "&#x1F4DD;"
        | "info" -> "&#x2139;"
        | "tip" -> "&#x1F4A1;"
        | "warning" -> "&#x26A0;"
        | "danger" -> "&#x2620;"
        | "success" -> "&#x2705;"
        | _ -> "&#x1F4CC;"
    
    let displayTitle = 
        match title with
        | Some t -> t
        | None -> ``type``.[0].ToString().ToUpper() + ``type``.[1..].ToLower()
    
    div [ _class $"admonition {``type``}"; _style $"border-left: 4px solid #6366f1; background: #f8fafc; padding: 1rem; margin: 1rem 0; border-radius: 0 8px 8px 0;" ] [
        p [ _style "font-weight: 600; margin: 0 0 0.5rem 0; display: flex; align-items: center; gap: 0.5rem;" ] [
            rawText icon
            str displayTitle
        ]
        div [ _style "margin: 0;" ] [ rawText content ]
    ]
    |> RenderView.AsString.htmlNode

let card ?(header: string option) ?(footer: string option) (body: string) =
    div [ _class "md-card"; _style "border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; margin: 1rem 0; box-shadow: 0 1px 3px rgba(0,0,0,0.1);" ] [
        match header with
        | Some h -> div [ _style "background: #f1f5f9; padding: 0.75rem 1rem; border-bottom: 1px solid #e2e8f0; font-weight: 600;" ] [ str h ]
        | None -> ()
        div [ _style "padding: 1rem;" ] [ rawText body ]
        match footer with
        | Some f -> div [ _style "background: #f8fafc; padding: 0.75rem 1rem; border-top: 1px solid #e2e8f0; font-size: 0.875rem; color: #64748b;" ] [ rawText f ]
        | None -> ()
    ]
    |> RenderView.AsString.htmlNode

let featureGrid (features: (string * string * string) list) =
    div [ _style "display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1.5rem; margin: 1.5rem 0;" ] [
        for (icon, title, description) in features ->
            div [ _style "display: flex; gap: 1rem; padding: 1rem;" ] [
                div [ _style "font-size: 2rem; flex-shrink: 0;" ] [ rawText icon ]
                div [] [
                    h4 [ _style "margin: 0 0 0.5rem 0; color: #1e293b;" ] [ str title ]
                    p [ _style "margin: 0; color: #64748b; line-height: 1.5;" ] [ str description ]
                ]
            ]
    ]
    |> RenderView.AsString.htmlNode

let button (variant: string) (text: string) (href: string) =
    let bg =
        match variant.ToLower() with
        | "primary" -> "#6366f1"
        | "secondary" -> "#64748b"
        | "success" -> "#22c55e"
        | "danger" -> "#ef4444"
        | _ -> "#6366f1"
    
    a [ _href href; _style $"display: inline-block; padding: 0.75rem 1.5rem; background: {bg}; color: white; text-decoration: none; border-radius: 6px; font-weight: 500;" ] [ str text ]
    |> RenderView.AsString.htmlNode

let badge (variant: string) (text: string) =
    let (bg, color) =
        match variant.ToLower() with
        | "info" -> ("#dbeafe", "#1e40af")
        | "success" -> ("#dcfce7", "#166534")
        | "warning" -> ("#fef3c7", "#92400e")
        | "danger" -> ("#fee2e2", "#991b1b")
        | _ -> ("#f3f4f6", "#374151")
    
    span [ _style $"display: inline-block; padding: 0.25rem 0.75rem; background: {bg}; color: {color}; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em;" ] [ str text ]
    |> RenderView.AsString.htmlNode

let hero (title: string) (subtitle: string) (ctaText: string) (ctaHref: string) =
    div [ _style "text-align: center; padding: 4rem 2rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 12px; margin: 2rem 0;" ] [
        h1 [ _style "font-size: 3rem; margin: 0 0 1rem 0; font-weight: 800;" ] [ str title ]
        p [ _style "font-size: 1.25rem; margin: 0 0 2rem 0; opacity: 0.9;" ] [ str subtitle ]
        a [ _href ctaHref; _style "display: inline-block; padding: 1rem 2rem; background: white; color: #667eea; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 1.125rem;" ] [ str ctaText ]
    ]
    |> RenderView.AsString.htmlNode


// =============================================================================
// HTML AST Manipulation
// =============================================================================

module HtmlAst =
    open Giraffe.ViewEngine
    
    let rec findTextContaining (substring: string) (node: XmlNode) : string list =
        match node with
        | Text t when t.Contains(substring) -> [t]
        | Element(_, _, children) -> children |> List.collect (findTextContaining substring)
        | _ -> []
    
    let rec findElements (tagName: string) (node: XmlNode) : XmlNode list =
        match node with
        | Element(t, _, _) when t = tagName -> [node]
        | Element(_, _, children) -> children |> List.collect (findElements tagName)
        | _ -> []
    
    let rec addClassToLis (cls: string) (node: XmlNode) : XmlNode =
        match node with
        | Element("li", attrs, children) -> Element("li", _class cls :: attrs, children |> List.map (addClassToLis cls))
        | Element(tag, attrs, children) -> Element(tag, attrs, children |> List.map (addClassToLis cls))
        | _ -> node
    
    let rec replaceText (oldValue: string) (newValue: string) (node: XmlNode) : XmlNode =
        match node with
        | Text t when t = oldValue -> Text newValue
        | Element(tag, attrs, children) -> Element(tag, attrs, children |> List.map (replaceText oldValue newValue))
        | _ -> node
    
    let rec walk (f: XmlNode -> unit) (node: XmlNode) : unit =
        f node
        match node with
        | Element(_, _, children) -> children |> List.iter (walk f)
        | _ -> ()
    
    let nthChild (n: int) (node: XmlNode) : XmlNode option =
        match node with
        | Element(_, _, children) when n < children.Length -> Some children.[n]
        | _ -> None
    
    let rec collectText (node: XmlNode) : string list =
        match node with
        | Text t -> [t]
        | Element(_, _, children) -> children |> List.collect collectText
        | _ -> []


// =============================================================================
// Safe String Utilities
// =============================================================================

module SafeString =
    let tripleQuote = "\"\"\""
    
    let breakTripleQuote (content: string) : string =
        content.Replace("\"\"\"", "\"\"\" + \"\\\"\\\"\\\" + \"\"\"")
    
    let fromLines (lines: string list) : string =
        String.concat "\n" lines
    
    let escapeForTripleQuote (content: string) : string =
        if content.Contains("\"\"\"") then breakTripleQuote content else content


