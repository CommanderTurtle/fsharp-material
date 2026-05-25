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
