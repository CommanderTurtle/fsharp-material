
module Docs.Showcase

open Giraffe.ViewEngine
open Docs.GiraffeInMarkdown.Components
open System

// =============================================================================
// Showcase: All Patterns in One Page
// =============================================================================
// This page demonstrates every pattern available in the Html2Giraffe workflow:
// 1. Pure Markdown content
// 2. Giraffe DSL components interpolated into Markdown
// 3. Component library usage
// 4. Dynamic F# logic in documentation
// =============================================================================

// -----------------------------------------------------------------------------
// Dynamic Content with F# Logic
// -----------------------------------------------------------------------------

let generateVersionBadge() =
    let version = "1.0.0"  // Could be read from a file
    badge "info" $"v{version}"

let generateBuildDate() =
    let now = DateTime.Now
    $"Last updated: {now:yyyy-MM-dd}"

let featureList = [
    ("&#x1F3AF;", "Type-Safe", "F# compiler catches HTML errors at build time")
    ("&#x1F4E6;", "Composable", "Build complex UIs from simple, reusable components")
    ("&#x26A1;", "Fast", "HTML generated at compile time, not runtime")
    ("&#x1F511;", "No Escaping", "Triple-quoted strings handle any content safely")
]

let dynamicFeatures = featureGrid featureList

// -----------------------------------------------------------------------------
// Custom Components
// -----------------------------------------------------------------------------

let changelog =
    let entries = [
        ("1.0.0", "2024-03-15", "Initial release")
        ("0.9.0", "2024-03-01", "Beta release")
        ("0.5.0", "2024-02-15", "Alpha release")
    ]
    
    div [ _class "changelog"; _style "margin: 1rem 0;" ] [
        h3 [] [ str "Changelog" ]
        ul [ _style "list-style: none; padding: 0;" ] [
            for (version, date, description) in entries ->
                li [ _style "padding: 0.5rem 0; border-bottom: 1px solid #e2e8f0;" ] [
                    span [ _style "font-weight: 600; color: #6366f1;" ] [ str version ]
                    span [ _style "color: #94a3b8; margin: 0 0.5rem;" ] [ str "•" ]
                    span [ _style "color: #64748b;" ] [ str date ]
                    span [ _style "margin: 0 0.5rem;" ] [ str "—" ]
                    span [] [ str description ]
                ]
        ]
    ]
    |> RenderView.AsString.htmlNode

let statsGrid =
    let stats = [
        ("1,234", "Stars")
        ("567", "Forks")
        ("89", "Contributors")
        ("42", "Releases")
    ]
    
    div [ _class "stats-grid"; _style "display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; margin: 1.5rem 0; text-align: center;" ] [
        for (value, label) in stats ->
            div [ _class "stat-item"; _style "padding: 1rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 8px;" ] [
                div [ _style "font-size: 2rem; font-weight: 700;" ] [ str value ]
                div [ _style "font-size: 0.875rem; opacity: 0.9;" ] [ str label ]
            ]
    ]
    |> RenderView.AsString.htmlNode

// -----------------------------------------------------------------------------
// Component Library Usage
// -----------------------------------------------------------------------------

let welcomeAdmonition = admonition "success" (Some "Welcome to the Showcase!") """
    <p>This page demonstrates the <strong>hybrid approach</strong>:</p>
    <ul>
        <li>Material for MkDocs features (admonitions, tabs, Mermaid)</li>
        <li>Giraffe.ViewEngine DSL components (type-safe, composable)</li>
        <li>F# logic (loops, conditionals, dynamic content)</li>
    </ul>
"""

let installTabs = tabContainer [
    (".NET CLI", """
        <pre><code class="language-bash">dotnet add package Giraffe.ViewEngine</code></pre>
    """)
    ("Paket", """
        <pre><code class="language-bash">paket add Giraffe.ViewEngine</code></pre>
    """)
    ("NuGet", """
        <pre><code class="language-xml">&lt;PackageReference Include="Giraffe.ViewEngine" /&gt;</code></pre>
    """)
]

let exampleCard = card 
    (Some "Quick Example") 
    (Some "Copy and paste into your project")
    """
    <pre><code class="language-fsharp">open Giraffe.ViewEngine

let view =
    html [] [
        head [] [ title [] [ str "My Page" ] ]
        body [] [ h1 [] [ str "Hello!" ] ]
    ]</code></pre>
    """

let ctaButtons =
    div [ _style "display: flex; gap: 1rem; margin: 1rem 0;" ] [
        rawText (button "primary" "Get Started" "/getting-started/")
        rawText (button "secondary" "View on GitHub" "https://github.com/")
    ]
    |> RenderView.AsString.htmlNode

let codeWithCopy = codeBlockWithCopy "fsharp" """
module MyPage

open Giraffe.ViewEngine

let card title content =
    div [ _class "card" ] [
        h3 [] [ str title ]
        p [] [ str content ]
    ]
    |> RenderView.AsString.htmlNode

let content = $"""
# My Page

{card "Hello" "World"}
"""
"""

// -----------------------------------------------------------------------------
// Build the Page
// -----------------------------------------------------------------------------

let content = $"""---
title: Showcase
description: Demonstrating all Html2Giraffe patterns
nav_order: 2
---

# Showcase

{generateVersionBadge()} {badge "success" "stable"}

{welcomeAdmonition}

---

## Stats

{statsGrid}

---

## Features

{dynamicFeatures}

---

## Installation

{installTabs}

---

## Example

{exampleCard}

---

## Code with Copy Button

{codeWithCopy}

---

## Changelog

{changelog}

---

## Mermaid Diagram

```mermaid
graph TD
    A[indexmd.fs] -->|F# Compiler| B[F# Logic]
    B --> C[Giraffe DSL Components]
    C --> D[HTML Strings]
    D --> E[Markdown with Interpolation]
    E --> F[MkDocs Material]
    F --> G[Final HTML]
    
    style A fill:#6366f1,color:#fff
    style G fill:#22c55e,color:#fff
```

---

## Material Features

!!! tip "Admonitions Work"
    This admonition is rendered by MkDocs Material, not Giraffe DSL.

!!! warning "Remember"
    You can mix Material features with Giraffe components!

!!! info "Dynamic Content"
    The stats above were generated with F# logic at compile time.

=== "F#"
    F# is a functional-first language with strong typing.

=== "C#"
    C# is a multi-paradigm language with object-oriented features.

=== "Python"
    Python is a dynamically-typed language with simple syntax.

---

## Call to Action

{ctaButtons}

---

## Dynamic Footer

{generateBuildDate()}

---

*This page was generated from `docs/showcase/indexmd.fs` using the hybrid approach.*
"""

let render() = content


