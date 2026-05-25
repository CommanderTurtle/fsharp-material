
module Docs.GiraffeInMarkdown

open Giraffe.ViewEngine

// =============================================================================
// Example: Using Giraffe DSL inside Markdown
// =============================================================================
// Since indexmd.fs is F# code, we can use Giraffe.ViewEngine DSL
// and interpolate the HTML output into the Markdown string!
// =============================================================================

/// <summary>
/// A reusable card component using Giraffe DSL.
/// </summary>
let card (title: string) (content: string) =
    div [ _class "md-card"; _style "border: 1px solid #e2e8f0; border-radius: 8px; padding: 1rem; margin: 1rem 0;" ] [
        h3 [ _style "margin-top: 0; color: #4f46e5;" ] [ str title ]
        p [ _style "margin-bottom: 0;" ] [ str content ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// A feature grid component.
/// </summary>
let featureGrid (features: (string * string) list) =
    div [ _class "feature-grid"; _style "display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 1rem;" ] [
        for (title, desc) in features ->
            div [ _class "feature-item"; _style "padding: 1rem; background: #f8fafc; border-radius: 8px;" ] [
                h4 [ _style "margin-top: 0;" ] [ str title ]
                p [ _style "margin-bottom: 0; font-size: 0.9rem;" ] [ str desc ]
            ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// A code block component with syntax highlighting classes.
/// </summary>
let codeBlock (language: string) (code: string) =
    pre [ _class $"language-{language}"; _style "background: #1e293b; color: #e2e8f0; padding: 1rem; border-radius: 8px; overflow-x: auto;" ] [
        code [ _class $"language-{language}" ] [ str code ]
    ]
    |> RenderView.AsString.htmlNode

/// <summary>
/// A button component.
/// </summary>
let button (text: string) (href: string) =
    a [ 
        _href href; 
        _class "md-button";
        _style "display: inline-block; padding: 0.75rem 1.5rem; background: #6366f1; color: white; text-decoration: none; border-radius: 6px; font-weight: 500;"
    ] [ str text ]
    |> RenderView.AsString.htmlNode

// =============================================================================
// Generate components using Giraffe DSL
// =============================================================================

let welcomeCard = card "Welcome!" "This card was generated using Giraffe.ViewEngine DSL inside an indexmd.fs file."

let features = [
    ("Type Safe", "F# compiler ensures your HTML is valid")
    ("Composable", "Build reusable components with functions")
    ("No Escaping", "Triple-quoted strings handle any content")
]

let featureSection = featureGrid features

let exampleCode = codeBlock "fsharp" """let card title content =
    div [ _class "card" ] [
        h3 [] [ str title ]
        p [] [ str content ]
    ]
    |> RenderView.AsString.htmlNode"""

let ctaButton = button "Get Started" "/getting-started/"

// =============================================================================
// Build the Markdown content with interpolated HTML
// =============================================================================

let content = $"""---
title: Giraffe DSL in Markdown
description: Using Giraffe.ViewEngine DSL inside indexmd.fs files
nav_order: 5
---

# Giraffe DSL in Markdown

This page demonstrates how to use **Giraffe.ViewEngine DSL** inside `indexmd.fs` files.

Since `indexmd.fs` is just F# code, we can:

1. Define components using `div [] []`, `span [] []`, etc.
2. Use `RenderView.AsString.htmlNode` to convert to HTML string
3. Interpolate the HTML into the Markdown using `$"""...{component}..."""`

---

## Example Card Component

Here's a card component generated with Giraffe DSL:

{welcomeCard}

The card above was created with:

```fsharp
let card title content =
    div [ _class "md-card" ] [
        h3 [] [ str title ]
        p [] [ str content ]
    ]
    |> RenderView.AsString.htmlNode

// Then interpolated into Markdown:
let content = $"""...{{card "Title" "Content"}}..."""
```

---

## Feature Grid

Here's a grid of features, also generated with Giraffe DSL:

{featureSection}

---

## Code Block Component

Custom code block with styling:

{exampleCode}

---

## Call to Action Button

{ctaButton}

---

## How It Works

The key insight is that `indexmd.fs` files are **F# code**, not just Markdown strings. This means:

```fsharp
module Docs.MyPage

open Giraffe.ViewEngine

// 1. Define a component using Giraffe DSL
let myComponent =
    div [ _class "my-class" ] [
        h2 [] [ str "Hello" ]
        p [] [ str "World" ]
    ]
    |> RenderView.AsString.htmlNode  // Convert to HTML string

// 2. Interpolate into Markdown
let content = $"""
# My Page

Some Markdown content.

{{myComponent}}  <-- HTML inserted here!

More Markdown content.
"""

let render() = content
```

---

## Benefits

| Approach | Benefits |
|----------|----------|
| **Pure Markdown** | Simple, works with MkDocs features |
| **Giraffe DSL in Markdown** | Type-safe, composable, reusable components |
| **Hybrid** | Best of both worlds! |

---

## When to Use This

Use Giraffe DSL in `indexmd.fs` when you need:

- ✅ Reusable components across documentation pages
- ✅ Type-safe HTML structure
- ✅ Dynamic content based on F# logic
- ✅ Complex layouts that Markdown can't handle
- ✅ Custom styling without CSS files

---

*This page is proof that Giraffe.ViewEngine DSL works inside Markdown files!*
"""

let render() = content


