
module Blog.Docs.Index

/// <summary>
/// Blog site homepage (blog.shel.sh)
/// Generates: index.md
/// </summary>
let content = """---
title: Blog
description: Latest updates from shel.sh
---

# shel.sh Blog

Welcome to the blog for **shel.sh**.

## Latest Posts

Stay tuned for updates on:

- Multi-subdomain architecture patterns
- F#-first configuration management
- MkDocs Material best practices
- Static site deployment strategies

## About

This blog is built with:

- **MkDocs Material** with blog plugin
- **GitHub Pages** branch-based deployments
- **F#** for type-safe configuration

---

*[View Archive](archive.md)*
"""

let render() = content


