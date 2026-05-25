
module Main.MkDocs

/// <summary>
/// MkDocs configuration for main site (shel.sh)
/// Deploys to 'website' branch
/// </summary>
let content = """site_name: shel.sh
site_url: https://shel.sh
docs_dir: docs
site_dir: site

theme:
  name: material
  palette:
    - scheme: default
      primary: indigo
      accent: indigo
  features:
    - navigation.tabs
    - navigation.top
    - search.suggest

plugins:
  - search
  - minify:
      minify_html: true

markdown_extensions:
  - admonition
  - tables
  - toc:
      permalink: true
  - pymdownx.details
  - pymdownx.superfences
  - pymdownx.highlight
  - pymdownx.tabbed:
      alternate_style: true

extra_javascript:
  - https://unpkg.com/mermaid@10/dist/mermaid.min.js

nav:
  - Home: index.md
  - About: about.md
"""

let render() = content


