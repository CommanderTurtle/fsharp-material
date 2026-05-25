# F# in Material: The Complete Guide

## Overview

This repository introduces a powerful pattern: **using Giraffe.ViewEngine DSL inside Markdown files** via F# string interpolation, as well as transition to F# with Material by literal translation.

DSL additions to markdown give you:

- ✅ Type-safe HTML components
- ✅ F# logic in documentation
- ✅ Material for MkDocs features
- ✅ Reusable component libraries
- ✅ Compile-time error checking
- ✅ No runtime dependencies
## The Key Insight

`indexmd.fs` files are **F# code**, not just Markdown strings. This means:

```fsharp
module Docs.MyPage

open Giraffe.ViewEngine

// 1. Define components using Giraffe DSL (type-safe!)
let myCard =
    div [ _class "card" ] [
        h2 [] [ str "Hello" ]
        p [] [ str "From Giraffe DSL!" ]
    ]
    |> RenderView.AsString.htmlNode  // Convert to HTML string

// 2. Interpolate into Markdown using $"""
let content = $"""
# My Page

Some Markdown content.

{myCard}  <-- HTML component inserted here!

More Markdown content.
"""

let render() = content
```
## How It Works

```
┌─────────────────────────────────────────────────────────────────┐
│  indexmd.fs (F# Code)                                           │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  1. Define Giraffe DSL components                       │   │
│  │     div [] [ h1 [] [ str "Title" ] ]                    │   │
│  │     |> RenderView.AsString.htmlNode                     │   │
│  │                                                         │   │
│  │  2. Interpolate into Markdown string                    │   │
│  │     let content = $"""...{component}..."""             │   │
│  └─────────────────────────────────────────────────────────┘   │
│                          ↓                                      │
│  F# Compiler → String with embedded HTML                        │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│  index.md (Generated)                                           │
│  ---                                                            │
│  title: My Page                                                 │
│  ---                                                            │
│  # My Page                                                      │
│                                                                 │
│  Some Markdown content.                                         │
│                                                                 │
│  <div class="card"><h2>Hello</h2><p>From Giraffe!</p></div>    │
│                                                                 │
│  More Markdown content.                                         │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│  MkDocs Material                                                │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  • Renders Markdown to HTML                             │   │
│  │  • Passes through embedded HTML                         │   │
│  │  • Applies Material theme styling                       │   │
│  │  • Processes admonitions, tabs, Mermaid                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│  Final HTML (GitHub Pages)                                      │
│  Beautiful documentation with type-safe F# components!          │
└─────────────────────────────────────────────────────────────────┘
```

## Benefits

| Benefit | Description |
|---------|-------------|
| **Type-Safe** | F# compiler catches HTML errors at build time |
| **Composable** | Build complex UIs from simple, reusable components |
| **No Escaping** | Triple-quoted strings handle any content safely |
| **Dynamic** | Use F# logic (loops, conditionals) in documentation |
| **Familiar** | Use Material for MkDocs features you already know |

## File Structure

```shell
# Example docs directory (-> docs.mysite.tld)
docs/
├── mkdocs.fs                           # (⇢ mkdocs.yaml)
├── pyproject.fs                        # (⇢ pyproject.toml)
├── .nojekyll                           # Disable default jekyll
├── .gitignore                          # Filetype blacklist for commits
├── docs/indexmd.fs                     # Home page (mkdocs nested docs/ structure)
|
├── templates/                          # Showcase examples
│   ├── basic/
│   |   └── stylesheets/                # Common .css (sharpcss-extra.fs ⇢ extra.css)
│   ├── intermediate/
│   ├── advanced/                       # Utilizes "open Giraffe.ViewEngine"
│       ├── indexmd.fs                  # Example 1, Giraffe
│       ├── Components.fs               # Reusable component library
│       ├── advanced-example.md         # Example 2, Giraffe + Components (Documentation)
```

## Component Library

Located at `Components.fs`:

```fsharp
open Docs.GiraffeInMarkdown.Components

// Admonitions (callouts)
admonition "tip" (Some "Pro Tip") "<p>Your content</p>"

// Tabs
tabContainer [
    ("Tab 1", "<p>Content 1</p>")
    ("Tab 2", "<p>Content 2</p>")
]

// Cards
card (Some "Title") (Some "Footer") "<p>Body</p>"

// Feature grids
featureGrid [
    ("&#x1F680;", "Fast", "Description")
]

// Code blocks with copy button
codeBlockWithCopy "fsharp" "let x = 42"

// Buttons
button "primary" "Click Me" "/action/"

// Badges
badge "success" "stable"

// Hero sections
hero "Title" "Subtitle" "CTA" "/link/"
```

## Patterns

### Pattern 1: Simple Component

```fsharp
let greeting name =
    div [ _class "greeting" ] [
        h2 [] [ str $"Hello, {name}!" ]
    ]
    |> RenderView.AsString.htmlNode

let content = $"""
# Welcome

{greeting "World"}
"""
```

### Pattern 2: Dynamic List

```fsharp
let items = ["Apple"; "Banana"; "Cherry"]

let fruitList =
    ul [] [ for item in items -> li [] [ str item ] ]
    |> RenderView.AsString.htmlNode

let content = $"""
# Fruits

{fruitList}
"""
```

### Pattern 3: Conditional Content

```fsharp
let isBeta = true

let versionBadge =
    if isBeta then
        badge "warning" "beta"
    else
        badge "success" "stable"

let content = $"""
# My Project {versionBadge}
"""
```

### Pattern 4: Reusable Components

```fsharp
// Define once
let infoBox (title: string) (content: string) =
    div [ _class "info-box" ] [
        h3 [] [ str title ]
        p [] [ str content ]
    ]
    |> RenderView.AsString.htmlNode

// Use many times
let box1 = infoBox "Feature 1" "Description 1"
let box2 = infoBox "Feature 2" "Description 2"
let box3 = infoBox "Feature 3" "Description 3"

let content = $"""
# Features

{box1}

{box2}

{box3}
"""
```

## Combining with Material Features

The hybrid approach lets you use BOTH Giraffe DSL AND Material features:

``````fsharp
let content = $"""
# My Page

{myGiraffeComponent}  <-- Giraffe DSL

!!! tip "Material Admonition"
    This works too!

=== "Tab 1"
    Tab content

=== "Tab 2"
    More content

```mermaid
graph LR
    A --> B
```
"""
``````

## When to Use

| Use Case | Recommendation |
|----------|----------------|
| Documentation with F# logic | ✅ Use `indexmd.fs` with Giraffe interpolation |
| Complex standalone pages | ✅ Use `index.fs` (pure Giraffe) |
| Simple documentation | ✅ Use `indexmd.fs` (pure Markdown) |
| Reusable components across docs | ✅ Use `Components.fs` library |

## Examples

### Basic Example

See `docs/giraffe-in-markdown/indexmd.fs` for a simple demonstration.

### Advanced Example

See `docs/showcase/indexmd.fs` for a comprehensive example with:
- Dynamic F# logic
- Component library usage
- Stats grids
- Changelogs
- Mermaid diagrams

### Component Library

See `docs/giraffe-in-markdown/Components.fs` for reusable components.

## Tips

1. **Use `RenderView.AsString.htmlNode`** for components (not `htmlDocument`)
2. **Use `$"""..."""`** for string interpolation with embedded components
3. **Use `rawText`** for HTML that shouldn't be escaped
4. **Use `str`** for text content that should be HTML-escaped
5. **Keep components pure** - no side effects

## Migration Guide

### From Pure Markdown

Before:
```markdown
# My Page

<div class="card">
    <h2>Title</h2>
    <p>Content</p>
</div>
```

After:
```fsharp
let myCard =
    div [ _class "card" ] [
        h2 [] [ str "Title" ]
        p [] [ str "Content" ]
    ]
    |> RenderView.AsString.htmlNode

let content = $"""
# My Page

{myCard}
"""
```

### From Pure Giraffe

Before:
```fsharp
let render() =
    html [] [
        body [] [
            h1 [] [ str "Title" ]
        ]
    ]
    |> RenderView.AsString.htmlDocument
```

After (for documentation):
```fsharp
let title =
    h1 [] [ str "Title" ]
    |> RenderView.AsString.htmlNode

let content = $"""
---
title: My Page
---

{title}

!!! tip "Now you can use Material features!"
"""

let render() = content
```

### But what about..?

the *only* genuinely tricky corner of “F# module as your literal file container”:  
**What do you do when the *embedded* content itself contains triple‑quotes?**

This example is perfect:

``````fsharp
```python
# So I was walking the other day and
"""
# jesus christ!
```
```

If you drop that directly into:

```fsharp
let content = """
...markdown...
"""
``````

It will break the F# parser the moment it sees the inner `"""`.

So the real question becomes:

**How does one safely embed a file that itself contains triple‑quoted strings?**

There are **three reliable strategies** in F#:

---

#### Strategy 1: Concatenation (used by the engine)
This is the most idiomatic solution.

Intentionally *interrupt* the `"""` sequence so the F# parser never sees it as a delimiter.

Example:

``````fsharp
let content =
    """
# This is some markdown with an arbitrary codeblock (python uses triple quotes infrequently)

```python
# So I was walking the other day and
""" + "\"\"\"" + """
# I stubbed my toe!
```

## This concludes why wrappers are difficult
"""
``````

Inside the markdown, the Python triple‑quote becomes:

```fsharp
""" + "\"\"\"" + """
```

Which evaluates to:

```fsharp
"""
```

…but the F# parser never sees the three quotes together inside a literal.

In the engine, it uses `new string('"', 3)` (F#) to construct the triple-quote marker without ever writing `"""` as a literal in generated source:

```fsharp
let tq = new string('"', 3)  // produces """
let parts = safeContent.Split([|tq|], StringSplitOptions.None)
let concat = 
    parts
    |> Array.mapi (fun i p -> tq + p + tq)
    |> String.concat " + "
```

---

#### Strategy 2: List of lines and join
This is extremely robust for documentation pipelines.

``````fsharp
let content =
    [
        "# This is some markdown"
        ""
        "```python"
        "# So I was walking the other day and"
        "\"\"\""   // safe
        "# jesus christ!"
        "```"
        ""
        "## This concludes why wrappers are difficult in other languages"
    ]
    |> String.concat "\n"
``````

Advantages:

- No escaping issues  
- No delimiter issues  
- No parser surprises  
- Easy to generate programmatically  
- Works with any content  

This is the most robust method for fully automated ingestion.

---
### Strategy 3: Use a base64 or encoded representation (rarely needed)**  
You *can* encode the entire markdown file:

```fsharp
let encoded = Convert.ToBase64String(File.ReadAllBytes "file.md")
```

Then decode at runtime.

This is overkill unless you’re embedding binary assets.

---

#### Let devs break their files!!1!!!1!

**The generator handles `"""` safely, but it doesn't pre-emptively "fix" developer content.** 

If you write `indexmd.fs` with a triple-quoted Markdown string that contains `"""`, the F# compiler will tell you. Fix it with one of the strategies above -- explicit and controllable. 

What the engine does automatically:

When `EscapeTripleQuotes = true` (default), and script/style content contains `"""`, the engine splits and concatenates:

**Input:**
```html
<script>
console.log("""hello""");
</script>
```

**Output:**
```fsharp
script [] [
    rawText ("""console.log(""" + "\"\"\"" + """hello""" + "\"\"\"" + """);
""")
]
```

---

Giraffe-in-Markdown gives you:

- ✅ Type-safe HTML components
- ✅ F# logic in documentation
- ✅ Material for MkDocs features
- ✅ Reusable component libraries
- ✅ Compile-time error checking
- ✅ No runtime dependencies

**This is type-safe documentation powered by F#!**

---

# Extra:

Below is an updated, professional README section with an additional chapter dedicated to **searching**, **matching**, **rewriting**, **tokenizing**, **walking**, and **augmenting** HTML represented in the Giraffe ViewEngine DSL.  
This expands the “practical commands” section into a full toolkit for structural manipulation.

---

# **Why Use F# for Web Development**  
### A Structured, Typed, and Maintainable Approach to Building Modern Websites

F# provides a disciplined, composable, and type‑safe foundation for building web applications. When paired with Giraffe ViewEngine, HTML is no longer treated as an unstructured text blob but as a structured, typed tree. This enables reliable dynamic content, reusable components, safe literal embedding, and programmatic transformations that are difficult or impossible in template‑based systems.

This document outlines the advantages of using F# for web development and provides practical examples of how to work with structured HTML, dynamic content, and AST‑level manipulation.

---

## **1. HTML as Structured Data**

Giraffe ViewEngine represents HTML as a typed AST:

```fsharp
html [] [
    head [] [ title [] [ str "My Page" ] ]
    body [] [ h1 [] [ str "Welcome" ] ]
]
```

Each element is a value of type `XmlNode`, enabling:

- compile‑time validation  
- safe composition  
- structural traversal  
- programmatic rewriting  
- reliable reuse  

This eliminates the fragility of string‑based HTML.

---

## **2. Dynamic Content Without Templates**

Dynamic content is expressed directly in F#:

### Loops
```fsharp
ul [] [
    for item in items ->
        li [] [ str item ]
]
```

### Conditionals
```fsharp
if model.ShowBanner then
    div [ _class "banner" ] [ str "Hello" ]
```

There is no template engine and no escaping rules to manage.

---

## **3. Reusable Components**

F# functions naturally become UI components:

```fsharp
let navBar user =
    nav [] [
        a [ _href "/" ] [ str "Home" ]
        if user.IsAdmin then
            a [ _href "/admin" ] [ str "Admin" ]
    ]
```

This encourages modular, maintainable design.

---

## **4. Literal‑Safe Embedding of HTML, CSS, and JavaScript**

Triple‑quoted strings allow literal embedding without escaping:

```fsharp
style [] [
    rawText """
    body { color: red; }
    .highlight { background: yellow; }
    """
]
```

Triple‑quoted strings do not interpret:

- `{}`  
- `\`  
- `<` or `>`  
- `%`, `!`, `^`  

This makes them ideal for inline CSS, JS, SVG, JSON, and complex HTML fragments.

When dynamic content is needed:

```fsharp
""" literal """ +
$""" dynamic {value} """ +
""" literal """
```

---

## **5. Programmatic HTML Editing (AST Rewriting)**

Because HTML is represented as a typed AST, you can traverse and transform it programmatically.

Example: add a class to every `<li>`:

```fsharp
let rec addClassToLis node =
    match node with
    | Element("li", attrs, children) ->
        Element("li", _class "highlight" :: attrs, children)
    | Element(tag, attrs, children) ->
        Element(tag, attrs, children |> List.map addClassToLis)
    | _ -> node
```

This enables automated refactoring, sanitization, and structural transformations.

---

# **6. Practical F# Commands and Patterns for Parsing & Manipulation**

What do you get for all this bulky code? 

Modularity is astonishing, below is a showcase of some tooling in this site engine:

---

## **6.1 Searching for Literal Text in the HTML DSL**

### Find all text nodes containing a substring
```fsharp
let rec findTextContaining substring node =
    match node with
    | Text t when t.Contains substring -> [t]
    | Element(_, _, children) ->
        children |> List.collect (findTextContaining substring)
    | _ -> []
```

### Find all elements of a given tag
```fsharp
let rec findElements tagName node =
    match node with
    | Element(t, _, _) when t = tagName -> [node]
    | Element(_, _, children) ->
        children |> List.collect (findElements tagName)
    | _ -> []
```

---

## **6.2 Appending Arbitrary Data to Matching Nodes**

Example: append text to every `<p>` element.

```fsharp
let rec appendToParagraphs extra node =
    match node with
    | Element("p", attrs, children) ->
        Element("p", attrs, children @ [ Text extra ])
    | Element(tag, attrs, children) ->
        Element(tag, attrs, children |> List.map (appendToParagraphs extra))
    | _ -> node
```

---

## **6.3 Inserting Newlines or `<br>` Elements**

### Insert `<br>` after every `<li>`
```fsharp
let rec insertBreaks node =
    match node with
    | Element("li", attrs, children) ->
        Element("li", attrs, children @ [ br [] [] ])
    | Element(tag, attrs, children) ->
        Element(tag, attrs, children |> List.map insertBreaks)
    | _ -> node
```

### Insert newline text nodes
```fsharp
let newline = RawText "\n"
```

---

## **6.4 Walking the HTML Tree**

### Generic walker
```fsharp
let rec walk f node =
    f node
    match node with
    | Element(_, _, children) -> children |> List.iter (walk f)
    | _ -> ()
```

Use case: logging, validation, indexing, etc.

---

## **6.5 Tokenizing and Extracting the Nth “Token” of HTML DSL Nodes**

### Extract the nth child of a node
```fsharp
let nthChild n node =
    match node with
    | Element(_, _, children) when n < children.Length ->
        Some children.[n]
    | _ -> None
```

### Extract the nth attribute
```fsharp
let nthAttribute n node =
    match node with
    | Element(_, attrs, _) when n < attrs.Length ->
        Some attrs.[n]
    | _ -> None
```

### Extract the nth text token inside a node
```fsharp
let rec collectText node =
    match node with
    | Text t -> [t]
    | Element(_, _, children) ->
        children |> List.collect collectText
    | _ -> []

let nthTextToken n node =
    let tokens = collectText node
    if n < tokens.Length then Some tokens.[n] else None
```

---

## **6.6 Delimiting and Extracting Subtrees**

### Extract all children between two indices
```fsharp
let sliceChildren start count node =
    match node with
    | Element(tag, attrs, children) ->
        let subset = children |> List.skip start |> List.take count
        Element(tag, attrs, subset)
    | _ -> node
```

---

# **Conclusion**

F# provides a structured, typed, and maintainable approach to building web applications.  
By treating HTML as a composable AST rather than a string, you gain:

- safe dynamic content  
- reusable components  
- literal‑safe embedding  
- programmatic transformations  
- reliable long‑term maintainability  

The parsing and manipulation tools above demonstrate how F# enables precise, structural control over your HTML—something that is difficult to achieve in template‑based systems.

***

***



## File: .\documentation\how-to-throw\README.md

# How to Throw HTML for Conversion

The `/throw/` directory is for **legacy HTML conversion** — drop HTML files here and they'll be automatically converted to proper F# Giraffe ViewEngine DSL.

## Quick Start

```bash
# 1. Drop HTML in throw/
cp my-old-page.html throw/

# 2. Push to trigger conversion
#    (or run locally:)
dotnet run --project src/generator -- convert throw/my-old-page.html throw/my-old-page.fs

# 3. Converted file is now at:
#    throw/my-old-page.fs

# 4. Manually move to target site:
cp throw/my-old-page.fs app/docs/widgets/my-page/index.fs

# 5. The module name inside index.fs should match the target:
#    module Docs.Widgets.MyPage  (or whatever your site's namespace is)
```

## What the Converter Produces

The `GiraffeDslConverter` parses HTML **element-by-element** and generates proper Giraffe ViewEngine DSL:

**Input HTML:**
```html
<div class="container">
    <h1>Hello World</h1>
    <p>Welcome to my site</p>
</div>
```

**Output F#:**
```fsharp
module Views
open Giraffe.ViewEngine

let page =
    div [ _class "container" ] [
        h1 [] [ str "Hello World" ]
        p [] [ str "Welcome to my site" ]
    ]
```

## F# String Literals: The Power of Triple Quotes

F# provides powerful string handling that makes embedding HTML/JS/CSS effortless:

### Literal Triple-Quoted Strings (`"""`)

```fsharp
let literal = """
<div class="test">
    <script>alert("Hello! %^<weird>");</script>
</div>
"""
```

**Triple-quoted strings do NOT interpret:**
- `{}` - curly braces are just characters
- `\` - backslashes are literal
- `"` - double quotes don't end the string
- `<`, `>` - angle brackets are fine
- `%`, `!`, `^` - shell metacharacters are safe
- Only `"""` ends the string

This is the fundamental property that makes the entirety of F# work in edge cases, still somehow allowing everything to be "type-safe". 
Triple-quoted strings are "formatting-transparent" -- they pass every character through literally except the triple-quote delimiter sequence itself.

### Interpolated Triple-Quoted Strings (`$"""`)

```fsharp
let name = "World"
let interpolated = $"""
<div class="greeting">
    <h1>Hello, {name}!</h1>
</div>
"""
```

### The Magic: Seamless Switching

F# lets you **seamlessly switch** between literal and interpolated modes:

```fsharp
let dynamicContent = "Dynamic"

let mixed = """
Static content here
""" + $"""
And {dynamicContent} content here
""" + """
More static content
"""
```

This is incredibly powerful for HTML generation:

```fsharp
let generatePage (title: string) (items: string list) =
    let header = """
<!DOCTYPE html>
<html>
<head>
    <title>""" + $"""{title}""" + """
    </title>
</head>
<body>
"""
    
    let itemList =
        items
        |> List.map (fun item -> $"""    <li>{item}</li>""")
        |> String.concat "\n"
    
    let footer = """
</body>
</html>
"""
    
    header + "\n<ul>\n" + itemList + "\n</ul>\n" + footer
```

## Why F# is Perfect for Low-Level Code Generation

F# is a **high-level language** that excels at **low-level code generation**:

| Feature | Benefit |
|---------|---------|
| Type safety | Catch errors at compile time |
| Discriminated unions | Model AST nodes precisely |
| Pattern matching | Transform trees elegantly |
| Triple-quoted strings | Embed any content safely |
| Functional composition | Build complex pipelines |

### Example: AST-Based HTML Manipulation

```fsharp
// Define the HTML AST
type HtmlElement =
    | Element of tag: string * attrs: (string * string) list * children: HtmlElement list
    | TextNode of text: string
    | Script of content: string

// Walk the tree
let rec walk f element =
    f element
    match element with
    | Element(_, _, children) -> children |> List.iter (walk f)
    | _ -> ()

// Transform the tree
let rec addClassToDivs cls element =
    match element with
    | Element("div", attrs, children) ->
        Element("div", ("class", cls) :: attrs, children |> List.map (addClassToDivs cls))
    | Element(tag, attrs, children) ->
        Element(tag, attrs, children |> List.map (addClassToDivs cls))
    | _ -> element
```

## Manual Conversion

You can also convert HTML manually:

```bash
# Build the generator
dotnet build src/generator

# Convert a file
dotnet run --project src/generator -- convert throw/my-page.html pages/my-page/index.fs
```

## Example: Complex Page Conversion

**Input:** `throw/dashboard.html`

```html
<!DOCTYPE html>
<html>
<head>
    <title>Dashboard</title>
    <style>
        .card { border: 1px solid #ccc; padding: 1rem; }
    </style>
</head>
<body>
    <div class="dashboard">
        <h1>Welcome</h1>
        <script>
            console.log("Dashboard loaded! %^&*()");
            const data = { name: "Test" };
        </script>
    </div>
</body>
</html>
```

**Output:** `pages/dashboard/index.fs`

```fsharp
module Pages.Dashboard
open Giraffe.ViewEngine

let render() =
    html [] [
        head [] [
            title [] [ str "Dashboard" ]
            style [] [
                rawText """
        .card { border: 1px solid #ccc; padding: 1rem; }
        """
            ]
        ]
        body [] [
            div [ _class "dashboard" ] [
                h1 [] [ str "Welcome" ]
                script [] [
                    rawText """
            console.log("Dashboard loaded! %^&*()");
            const data = { name: "Test" };
            """
                ]
            ]
        ]
    ]
    |> RenderView.AsString.htmlDocument
```

## Source Code Location

The conversion engine is in `/src/generator/`:

| File | Purpose |
|------|---------|
| `GiraffeDslConverter.fs` | Main converter - element-by-element parsing |
| `SafeStringBuilder.fs` | Triple-quoted string utilities |
| `DelimiterExtractor.fs` | CMD-style tokenization |
| `XmlStyleLiteral.fs` | XML-style literal handling |
| `EnhancedConverter.fs` | High-level conversion API |

See /documentation/ARCHITECTURE.md for more detailed information.

## Why use F# for HTML?

F#'s type system make it the perfect language for:
- **Performing high-level embedding** of stateful html
- **AST-based transformations** with pattern matching, string walking, and tree climbing
- **Composable code generation** pipelines (save a brick of arbitrary data, and execute mid-html, call it any time)
- **Low-level code extraction** from high-level abstractions (tokenize and delemit elements, splice and search)