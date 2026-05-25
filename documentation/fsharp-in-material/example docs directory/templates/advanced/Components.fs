
module Docs.GiraffeInMarkdown.Components

open Giraffe.ViewEngine
open System

// =============================================================================
// Material for MkDocs Component Library
// =============================================================================
// Reusable components that follow Material for MkDocs styling conventions.
// Use these in your indexmd.fs files for consistent documentation.
// =============================================================================

/// <summary>
/// Admonition/Callout component (equivalent to MkDocs !!! syntax)
/// </summary>
/// <param name="type">Type: note, info, tip, warning, danger, success</param>
/// <param name="title">Optional title (null for default)</param>
/// <param name="content">HTML content string</param>
let admonition (``type``: string) (title: string option) (content: string) =
    let icon =
        match ``type``.ToLower() with
        | "note" -> "&#x1F4DD;"  // 📝
        | "info" -> "&#x2139;"   // ℹ️
        | "tip" -> "&#x1F4A1;"   // 💡
        | "warning" -> "&#x26A0;" // ⚠️
        | "danger" -> "&#x2620;"  // ☠️
        | "success" -> "&#x2705;" // ✅
        | _ -> "&#x1F4CC;"        // 📌
    
    let displayTitle = 
        match title with
        | Some t -> t
        | None -> ``type``.[0].ToString().ToUpper() + ``type``.[1..].ToLower()
    
    div [ 
        _class $"admonition {``type``}"; 
        _style $"border-left: 4px solid var(--md-admonition-{``type``}-color, #6366f1); background: var(--md-admonition-bg, #f8fafc); padding: 1rem; margin: 1rem 0; border-radius: 0 8px 8px 0;" 
    ] [
        p [ _class "admonition-title"; _style "font-weight: 600; margin: 0 0 0.5rem 0; display: flex; align-items: center; gap: 0.5rem;" ] [
            rawText icon
            str displayTitle
        ]
        div [ _class "admonition-content"; _style "margin: 0;" ] [
            rawText content
        ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Tab container component for creating tabbed content
/// </summary>
let tabContainer (tabs: (string * string) list) =
    let tabId = Guid.NewGuid().ToString("n")[..7]
    
    let tabButtons =
        tabs
        |> List.mapi (fun i (label, _) ->
            button [
                _class $"tab-button-{tabId}"
                _style (if i = 0 then "padding: 0.5rem 1rem; border: none; background: #6366f1; color: white; cursor: pointer; border-radius: 4px 4px 0 0;" else "padding: 0.5rem 1rem; border: none; background: #e2e8f0; cursor: pointer; border-radius: 4px 4px 0 0;")
                attr "onclick" $"showTab('{tabId}', {i})"
            ] [ str label ]
        )
    
    let tabContents =
        tabs
        |> List.mapi (fun i (_, content) ->
            div [
                _class $"tab-content-{tabId}"
                _style (if i = 0 then "display: block; padding: 1rem; border: 1px solid #e2e8f0; border-radius: 0 8px 8px 8px;" else "display: none; padding: 1rem; border: 1px solid #e2e8f0; border-radius: 0 8px 8px 8px;")
            ] [ rawText content ]
        )
    
    let script =
        $"""
        <script>
        function showTab(tabId, index) {{
            document.querySelectorAll('.tab-content-' + tabId).forEach((el, i) => {{
                el.style.display = i === index ? 'block' : 'none';
            }});
            document.querySelectorAll('.tab-button-' + tabId).forEach((el, i) => {{
                el.style.background = i === index ? '#6366f1' : '#e2e8f0';
                el.style.color = i === index ? 'white' : 'inherit';
            }});
        }}
        </script>
        """
    
    div [ _class "tab-container"; _style "margin: 1rem 0;" ] [
        div [ _class "tab-buttons"; _style "display: flex; gap: 0.25rem;" ] tabButtons
        div [ _class "tab-contents" ] tabContents
        rawText script
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Card component with optional header, body, and footer
/// </summary>
let card ?(header: string option) ?(footer: string option) (body: string) =
    div [ 
        _class "md-card"; 
        _style "border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; margin: 1rem 0; box-shadow: 0 1px 3px rgba(0,0,0,0.1);" 
    ] [
        match header with
        | Some h ->
            div [ _class "card-header"; _style "background: #f1f5f9; padding: 0.75rem 1rem; border-bottom: 1px solid #e2e8f0; font-weight: 600;" ] [
                str h
            ]
        | None -> ()
        
        div [ _class "card-body"; _style "padding: 1rem;" ] [
            rawText body
        ]
        
        match footer with
        | Some f ->
            div [ _class "card-footer"; _style "background: #f8fafc; padding: 0.75rem 1rem; border-top: 1px solid #e2e8f0; font-size: 0.875rem; color: #64748b;" ] [
                rawText f
            ]
        | None -> ()
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Feature grid with icon, title, and description
/// </summary>
let featureGrid (features: (string * string * string) list) =
    div [ _class "feature-grid"; _style "display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1.5rem; margin: 1.5rem 0;" ] [
        for (icon, title, description) in features ->
            div [ _class "feature-item"; _style "display: flex; gap: 1rem; padding: 1rem;" ] [
                div [ _class "feature-icon"; _style "font-size: 2rem; flex-shrink: 0;" ] [
                    rawText icon
                ]
                div [ _class "feature-content" ] [
                    h4 [ _style "margin: 0 0 0.5rem 0; color: #1e293b;" ] [ str title ]
                    p [ _style "margin: 0; color: #64748b; line-height: 1.5;" ] [ str description ]
                ]
            ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Code block with copy button
/// </summary>
let codeBlockWithCopy (language: string) (code: string) =
    let escapedCode = code.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
    let blockId = Guid.NewGuid().ToString("n")[..7]
    
    div [ _class "code-block-wrapper"; _style "position: relative; margin: 1rem 0;" ] [
        button [
            _class "copy-button"
            _style "position: absolute; top: 0.5rem; right: 0.5rem; padding: 0.25rem 0.75rem; background: #475569; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 0.875rem;"
            attr "onclick" $"copyCode('{blockId}')"
        ] [ str "Copy" ]
        pre [ 
            _id blockId
            _class $"language-{language}"; 
            _style "background: #1e293b; color: #e2e8f0; padding: 1rem; border-radius: 8px; overflow-x: auto; margin: 0;" 
        ] [
            code [ _class $"language-{language}" ] [
                rawText escapedCode
            ]
        ]
        rawText """
        <script>
        function copyCode(id) {
            const code = document.getElementById(id).innerText;
            navigator.clipboard.writeText(code).then(() => {
                const btn = document.querySelector(`button[onclick="copyCode('${id}')"]`);
                const original = btn.innerText;
                btn.innerText = 'Copied!';
                setTimeout(() => btn.innerText = original, 2000);
            });
        }
        </script>
        """
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Button component with variants
/// </summary>
let button (variant: string) (text: string) (href: string) =
    let (bg, hoverBg) =
        match variant.ToLower() with
        | "primary" -> ("#6366f1", "#4f46e5")
        | "secondary" -> ("#64748b", "#475569")
        | "success" -> ("#22c55e", "#16a34a")
        | "danger" -> ("#ef4444", "#dc2626")
        | _ -> ("#6366f1", "#4f46e5")
    
    a [ 
        _href href; 
        _class $"md-button md-button--{variant}";
        _style $"display: inline-block; padding: 0.75rem 1.5rem; background: {bg}; color: white; text-decoration: none; border-radius: 6px; font-weight: 500; transition: background 0.2s;"
        attr "onmouseover" $"this.style.background='{hoverBg}'"
        attr "onmouseout" $"this.style.background='{bg}'"
    ] [ str text ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Badge/Pill component
/// </summary>
let badge (variant: string) (text: string) =
    let (bg, color) =
        match variant.ToLower() with
        | "info" -> ("#dbeafe", "#1e40af")
        | "success" -> ("#dcfce7", "#166534")
        | "warning" -> ("#fef3c7", "#92400e")
        | "danger" -> ("#fee2e2", "#991b1b")
        | _ -> ("#f3f4f6", "#374151")
    
    span [
        _class $"badge badge--{variant}"
        _style $"display: inline-block; padding: 0.25rem 0.75rem; background: {bg}; color: {color}; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em;"
    ] [ str text ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Two-column layout
/// </summary>
let twoColumn (left: string) (right: string) =
    div [ _class "two-column"; _style "display: grid; grid-template-columns: 1fr 1fr; gap: 2rem; margin: 1rem 0;" ] [
        div [ _class "column-left" ] [ rawText left ]
        div [ _class "column-right" ] [ rawText right ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Mermaid diagram container
/// </summary>
let mermaid (diagram: string) =
    div [ _class "mermaid"; _style "text-align: center; margin: 1rem 0;" ] [
        rawText $"```mermaid\n{diagram}\n```"
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Table of contents for current page
/// </summary>
let tableOfContents (items: (string * string) list) =
    div [ _class "toc"; _style "background: #f8fafc; padding: 1rem; border-radius: 8px; margin: 1rem 0;" ] [
        p [ _style "font-weight: 600; margin: 0 0 0.5rem 0;" ] [ str "On this page:" ]
        ul [ _style "margin: 0; padding-left: 1.25rem;" ] [
            for (text, href) in items ->
                li [ _style "margin: 0.25rem 0;" ] [
                    a [ _href href; _style "color: #6366f1; text-decoration: none;" ] [ str text ]
                ]
        ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// Hero section for landing pages
/// </summary>
let hero (title: string) (subtitle: string) (ctaText: string) (ctaHref: string) =
    div [ 
        _class "hero"; 
        _style "text-align: center; padding: 4rem 2rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 12px; margin: 2rem 0;" 
    ] [
        h1 [ _style "font-size: 3rem; margin: 0 0 1rem 0; font-weight: 800;" ] [ str title ]
        p [ _style "font-size: 1.25rem; margin: 0 0 2rem 0; opacity: 0.9;" ] [ str subtitle ]
        a [ 
            _href ctaHref; 
            _class "hero-cta";
            _style "display: inline-block; padding: 1rem 2rem; background: white; color: #667eea; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 1.125rem;"
        ] [ str ctaText ]
    ]
    |> RenderView.AsString.htmlNode


// =============================================================================
// HTML AST Manipulation Utilities
// =============================================================================
// These functions enable programmatic traversal and transformation of HTML
// represented as Giraffe ViewEngine XmlNode trees.
// =============================================================================

/// <summary>
/// Module for HTML AST manipulation and traversal.
/// </summary>
module HtmlAst =
    open Giraffe.ViewEngine
    
    /// <summary>
    /// Find all text nodes containing a substring.
    /// </summary>
    let rec findTextContaining (substring: string) (node: XmlNode) : string list =
        match node with
        | Text t when t.Contains(substring) -> [t]
        | Element(_, _, children) ->
            children |> List.collect (findTextContaining substring)
        | _ -> []
    
    /// <summary>
    /// Find all elements of a given tag name.
    /// </summary>
    let rec findElements (tagName: string) (node: XmlNode) : XmlNode list =
        match node with
        | Element(t, _, _) when t = tagName -> [node]
        | Element(_, _, children) ->
            children |> List.collect (findElements tagName)
        | _ -> []
    
    /// <summary>
    /// Add a class to all elements matching a tag name.
    /// </summary>
    let rec addClass (cls: string) (tagName: string) (node: XmlNode) : XmlNode =
        match node with
        | Element(t, attrs, children) when t = tagName ->
            Element(t, _class cls :: attrs, children |> List.map (addClass cls tagName))
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (addClass cls tagName))
        | _ -> node
    
    /// <summary>
    /// Add a class to every <li> element.
    /// </summary>
    let rec addClassToLis (cls: string) (node: XmlNode) : XmlNode =
        match node with
        | Element("li", attrs, children) ->
            Element("li", _class cls :: attrs, children |> List.map (addClassToLis cls))
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (addClassToLis cls))
        | _ -> node
    
    /// <summary>
    /// Replace text content in the tree.
    /// </summary>
    let rec replaceText (oldValue: string) (newValue: string) (node: XmlNode) : XmlNode =
        match node with
        | Text t when t = oldValue -> Text newValue
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (replaceText oldValue newValue))
        | _ -> node
    
    /// <summary>
    /// Append content to elements matching a tag name.
    /// </summary>
    let rec appendToElements (tagName: string) (extra: XmlNode) (node: XmlNode) : XmlNode =
        match node with
        | Element(t, attrs, children) when t = tagName ->
            Element(t, attrs, children @ [extra])
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (appendToElements tagName extra))
        | _ -> node
    
    /// <summary>
    /// Append text to all <p> elements.
    /// </summary>
    let rec appendToParagraphs (extra: string) (node: XmlNode) : XmlNode =
        appendToElements "p" (Text extra) node
    
    /// <summary>
    /// Insert a <br> element after every element matching a tag name.
    /// </summary>
    let rec insertBreaks (tagName: string) (node: XmlNode) : XmlNode =
        match node with
        | Element(t, attrs, children) when t = tagName ->
            Element(t, attrs, children @ [br [] []])
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (insertBreaks tagName))
        | _ -> node
    
    /// <summary>
    /// Generic tree walker that applies a function to each node.
    /// </summary>
    let rec walk (f: XmlNode -> unit) (node: XmlNode) : unit =
        f node
        match node with
        | Element(_, _, children) -> children |> List.iter (walk f)
        | _ -> ()
    
    /// <summary>
    /// Extract the nth child of an element.
    /// </summary>
    let nthChild (n: int) (node: XmlNode) : XmlNode option =
        match node with
        | Element(_, _, children) when n < children.Length ->
            Some children.[n]
        | _ -> None
    
    /// <summary>
    /// Extract the nth attribute of an element.
    /// </summary>
    let nthAttribute (n: int) (node: XmlNode) : HtmlAttribute option =
        match node with
        | Element(_, attrs, _) when n < attrs.Length ->
            Some attrs.[n]
        | _ -> None
    
    /// <summary>
    /// Collect all text content from a node and its children.
    /// </summary>
    let rec collectText (node: XmlNode) : string list =
        match node with
        | Text t -> [t]
        | Element(_, _, children) ->
            children |> List.collect collectText
        | _ -> []
    
    /// <summary>
    /// Extract the nth text token from a node.
    /// </summary>
    let nthTextToken (n: int) (node: XmlNode) : string option =
        let tokens = collectText node
        if n < tokens.Length then Some tokens.[n] else None
    
    /// <summary>
    /// Extract a slice of children from an element.
    /// </summary>
    let sliceChildren (start: int) (count: int) (node: XmlNode) : XmlNode =
        match node with
        | Element(tag, attrs, children) ->
            let subset = children |> List.skip start |> List.take (min count (children.Length - start))
            Element(tag, attrs, subset)
        | _ -> node
    
    /// <summary>
    /// Map a function over all nodes in the tree.
    /// </summary>
    let rec map (f: XmlNode -> XmlNode) (node: XmlNode) : XmlNode =
        let mapped = f node
        match mapped with
        | Element(tag, attrs, children) ->
            Element(tag, attrs, children |> List.map (map f))
        | _ -> mapped
    
    /// <summary>
    /// Filter elements by a predicate.
    /// </summary>
    let rec filter (predicate: XmlNode -> bool) (node: XmlNode) : XmlNode list =
        let results =
            match node with
            | Element(_, _, children) ->
                children |> List.collect (filter predicate)
            | _ -> []
        if predicate node then node :: results else results
    
    /// <summary>
    /// Count elements matching a tag name.
    /// </summary>
    let rec countElements (tagName: string) (node: XmlNode) : int =
        match node with
        | Element(t, _, children) when t = tagName ->
            1 + (children |> List.sumBy (countElements tagName))
        | Element(_, _, children) ->
            children |> List.sumBy (countElements tagName)
        | _ -> 0


// =============================================================================
// Safe String Utilities
// =============================================================================
// Functions for safely handling strings that may contain triple-quotes
// or other special characters that could break F# string literals.
// =============================================================================

/// <summary>
/// Module for safe string handling, especially for embedded content.
/// </summary>
module SafeString =
    /// <summary>
    /// A triple-quote sequence that can be safely concatenated.
    /// </summary>
    let tripleQuote = "\"\"\""
    
    /// <summary>
    /// Break a triple-quoted string by concatenating with a safe sequence.
    /// Use this when embedding content that contains triple-quotes.
    /// </summary>
    let breakTripleQuote (content: string) : string =
        content.Replace("\"\"\"", "\"\"\" + \"\\\"\\\"\\\" + \"\"\"")
    
    /// <summary>
    /// Create a string from a list of lines.
    /// This is the safest method for embedding arbitrary content.
    /// </summary>
    let fromLines (lines: string list) : string =
        String.concat "\n" lines
    
    /// <summary>
    /// Create a string from a list of lines with a specific separator.
    /// </summary>
    let fromLinesWith (separator: string) (lines: string list) : string =
        String.concat separator lines
    
    /// <summary>
    /// Escape content for use in a triple-quoted string by breaking delimiters.
    /// </summary>
    let escapeForTripleQuote (content: string) : string =
        if content.Contains("\"\"\"") then
            breakTripleQuote content
        else
            content
    
    /// <summary>
    /// Combine literal and interpolated sections safely.
    /// </summary>
    let combine (sections: string list) : string =
        String.concat "" sections
    
    /// <summary>
    /// Create an interpolated section from a value.
    /// </summary>
    let interpolated (value: 'a) : string =
        $"{{value}}"
    
    /// <summary>
    /// Wrap content in literal triple-quotes.
    /// </summary>
    let literal (content: string) : string =
        $"\"\"\"{content}\"\"\""


// =============================================================================
// Component Composition Helpers
// =============================================================================
// Higher-order functions for composing and transforming components.
// =============================================================================

/// <summary>
/// Module for component composition patterns.
/// </summary>
module Composition =
    /// <summary>
    /// Compose multiple components into a single container.
    /// </summary>
    let compose (container: XmlNode list -> XmlNode) (components: XmlNode list) : XmlNode =
        container components
    
    /// <summary>
    /// Wrap a component with a div and class.
    /// </summary>
    let wrap (className: string) (component: XmlNode) : XmlNode =
        div [ _class className ] [ component ]
    
    /// <summary>
    /// Apply a transformation to a component.
    /// </summary>
    let transform (f: XmlNode -> XmlNode) (component: XmlNode) : XmlNode =
        f component
    
    /// <summary>
    /// Conditionally render a component.
    /// </summary>
    let when' (condition: bool) (component: XmlNode) : XmlNode list =
        if condition then [component] else []
    
    /// <summary>
    /// Render a list of items using a component function.
    /// </summary>
    let forEach (items: 'a list) (render: 'a -> XmlNode) : XmlNode list =
        items |> List.map render
    
    /// <summary>
    /// Render a component with a fallback if empty.
    /// </summary>
    let orElse (fallback: XmlNode) (component: XmlNode option) : XmlNode =
        match component with
        | Some c -> c
        | None -> fallback
    
    /// <summary>
    /// Chain multiple transformations on a component.
    /// </summary>
    let pipe (component: XmlNode) (transformations: (XmlNode -> XmlNode) list) : XmlNode =
        transformations |> List.fold (|>) component


