
module Main.Docs.Index

/// <summary>
/// Main site homepage (shel.sh)
/// Generates: index.md
/// </summary>
let content = """---
title: Welcome
---

# Welcome to shel.sh

This is the main site for **shel.sh** — the apex domain.

## Subdomains

| Site | URL | Description |
|------|-----|-------------|
| Docs | [docs.shel.sh](https://docs.shel.sh) | Documentation and templates |
| App | [app.shel.sh](https://app.shel.sh) | Application dashboard |
| Blog | [blog.shel.sh](https://blog.shel.sh) | Latest updates |

## About

This multi-subdomain architecture uses:

- **GitHub Pages** with branch-based deployments
- **MkDocs Material** for documentation
- **F#-first configuration** — type-safe config generation

---

*Built with F#*
"""

let render() = content


