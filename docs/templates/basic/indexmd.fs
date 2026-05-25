
module Docs.Home

let content = """---
title: Documentation
nav_order: 1
description: Html2Giraffe documentation and guides
---

# Html2Giraffe Documentation

Welcome! Html2Giraffe is a complete workflow for building GitHub Pages sites with F#.

## Quick Links

- [Getting Started](getting-started/)
- [API Reference](api/)
- [Examples](examples/)

## Features

| Feature | Description |
|---------|-------------|
| Type-Safe | F# compile-time checking |
| Automatic CI/CD | GitHub Actions deployment |
| Material Design | MkDocs Material theme |
| Standalone Pages | HTML output from F# |
| Documentation | Markdown from F# |

## Example

```fsharp
module MyPage
open Giraffe.ViewEngine

let render() =
    html [] [
        head [] [ title [] [ str "My Page" ] ]
        body [] [ h1 [] [ str "Hello!" ] ]
    ]
    |> RenderView.AsString.htmlDocument
```

!!! tip "Getting Started"
    Check out the [Getting Started guide](getting-started/) to create your first page.
"""

let render() = content


