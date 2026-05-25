
module Docs.Docs.Index

/// <summary>
/// Docs site homepage (docs.shel.sh)
/// Generates: index.md
/// </summary>
let content = """---
title: Documentation
---

# shel.sh Documentation

Welcome to the documentation for **shel.sh**.

## Quick Start

This site is deployed from the `docs/` folder to **docs.shel.sh**.

## Architecture

```
repo/
├── main/          → shel.sh (apex)
├── docs/          → docs.shel.sh (this site)
├── app/           → app.shel.sh
└── blog/          → blog.shel.sh
```

## Templates

Explore our template collection:

- [Basic Templates](templates/basic/) — Simple components
- [Intermediate Templates](templates/intermediate/) — Showcase examples
- [Advanced Templates](templates/advanced/) — Full Giraffe DSL integration

## F#-First Configuration

Each subdomain has its own `mkdocs.fs`:

```fsharp
module Docs.MkDocs
let content = """
site_name: shel.sh Docs
site_url: https://docs.shel.sh
...
"""
let render() = content
```

Generate: `dotnet fsi GenerateConfig.fsx docs`
"""

let render() = content


