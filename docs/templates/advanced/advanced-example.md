---
title: Advanced Giraffe-in-Markdown Patterns
nav_order: 6
---

# Advanced Giraffe-in-Markdown Patterns

This page demonstrates advanced patterns using the `Components.fs` library.

## Using the Component Library

First, reference the components module in your `indexmd.fs`:

```fsharp
module Docs.AdvancedExample

open Giraffe.ViewEngine
open Docs.GiraffeInMarkdown.Components  // Import the component library

// Now use any component:
let myCard = card (Some "Card Title") (Some "Footer text") "<p>Body content</p>"
```

## Available Components

### Admonitions (Callouts)

```fsharp
let note = admonition "note" (Some "Custom Title") "<p>Your content here...</p>"
let warning = admonition "warning" None "<p>This is a warning!</p>"
```

Supported types: `note`, `info`, `tip`, `warning`, `danger`, `success`

### Tab Containers

```fsharp
let tabs = tabContainer [
    ("F#", "<p>F# content...</p>")
    ("C#", "<p>C# content...</p>")
    ("Python", "<p>Python content...</p>")
]
```

### Feature Grids

```fsharp
let features = featureGrid [
    ("&#x1F680;", "Fast", "Lightning fast performance")
    ("&#x1F512;", "Secure", "Built-in security features")
    ("&#x2699;", "Configurable", "Highly customizable")
]
```

### Code Blocks with Copy Button

```fsharp
let code = codeBlockWithCopy "fsharp" """
let hello() = printfn "Hello, World!"
"""
```

### Buttons

```fsharp
let primaryBtn = button "primary" "Get Started" "/start/"
let secondaryBtn = button "secondary" "Learn More" "/docs/"
```

### Badges

```fsharp
let stable = badge "success" "stable"
let beta = badge "warning" "beta"
let deprecated = badge "danger" "deprecated"
```

### Hero Sections

```fsharp
let heroSection = hero 
    "My Awesome Project"
    "Build amazing things with F#"
    "Get Started"
    "/getting-started/"
```

## Complete Example

Here's a complete `indexmd.fs` file:

```fsharp
module Docs.MyLandingPage

open Giraffe.ViewEngine
open Docs.GiraffeInMarkdown.Components

// Hero section
let heroSection = hero
    "Html2Giraffe"
    "Convert HTML to F# Giraffe.ViewEngine DSL with ease"
    "Try it Now"
    "/demo/"

// Feature grid
let features = featureGrid [
    ("&#x1F9E9;", "Type-Safe", "F# compiler catches errors before runtime")
    ("&#x1F4E6;", "Composable", "Build complex UIs from simple components")
    ("&#x26A1;", "Fast", "Generate HTML at compile time, not runtime")
]

// Installation card
let installCard = card 
    (Some "Installation") 
    None
    """
    <p>Install via NuGet:</p>
    <pre><code>dotnet add package Giraffe.ViewEngine</code></pre>
    """

// Warning admonition
let warningBox = admonition "warning" (Some "Prerequisites") """
    <p>Make sure you have:</p>
    <ul>
        <li>.NET 6.0 or higher</li>
        <li>Basic knowledge of F#</li>
    </ul>
"""

// Code example with copy button
let exampleCode = codeBlockWithCopy "fsharp" """
open Giraffe.ViewEngine

let view =
    html [] [
        head [] [ title [] [ str "My Page" ] ]
        body [] [
            h1 [] [ str "Hello, World!" ]
        ]
    ]
"""

// CTA buttons
let primaryCta = button "primary" "Read Documentation" "/docs/"
let secondaryCta = button "secondary" "View on GitHub" "https://github.com/"

// Build the page
let content = $"""
---
title: Html2Giraffe
nav_order: 1
---

{heroSection}

## Features

{features}

## Quick Start

{installCard}

{warningBox}

## Example

{exampleCode}

## Next Steps

{primaryCta} {secondaryCta}
"""

let render() = content
```

## Best Practices

1. **Keep components pure** - Don't mutate state in components
2. **Use type-safe parameters** - Leverage F#'s type system
3. **Compose small components** - Build complex UIs from simple pieces
4. **Use triple-quoted strings** - For multi-line HTML content in components
5. **Document your components** - Add XML docs for IntelliSense support

## Tips

- Components return `string` (HTML), so you can use them anywhere in Markdown
- Use `rawText` for HTML that shouldn't be escaped
- Use `str` for text content that should be HTML-escaped
- The component library is just F# - extend it however you need!


***