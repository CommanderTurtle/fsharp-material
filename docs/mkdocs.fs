
module Docs.MkDocs

/// <summary>
/// MkDocs configuration for docs site (docs.shel.sh)
/// Deploys to 'docs' branch
/// </summary>
let content = """site_name: shel.sh Docs
site_url: https://docs.shel.sh
docs_dir: docs
site_dir: site

theme:
  name: material
  palette:
    - scheme: default
      primary: teal
      accent: teal
  features:
    - navigation.tabs
    - navigation.sections
    - navigation.expand
    - navigation.top
    - search.suggest
    - content.code.copy
    - content.tooltips

plugins:
  - search
  - minify:
      minify_html: true

markdown_extensions:
  - admonition
  - tables
  - attr_list
  - toc:
      permalink: true
  - pymdownx.details
  - pymdownx.superfences:
      custom_fences:
        - name: mermaid
          class: mermaid
          format: !!python/name:pymdownx.superfences.fence_code_format
  - pymdownx.highlight
  - pymdownx.tabbed:
      alternate_style: true
  - pymdownx.emoji:
      emoji_index: !!python/name:material.extensions.emoji.twemoji
      emoji_generator: !!python/name:material.extensions.emoji.to_svg

extra_javascript:
  - https://unpkg.com/mermaid@10/dist/mermaid.min.js

nav:
  - Home: index.md
  - Getting Started: getting-started.md
  - Templates:
    - Basic: templates/basic/index.md
    - Intermediate: templates/intermediate/index.md
    - Advanced: templates/advanced/index.md
  - API: api.md
"""

let render() = content


