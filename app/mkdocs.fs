
module App.MkDocs

/// <summary>
/// MkDocs configuration for app site (app.shel.sh)
/// Deploys to 'applications' branch
/// </summary>
let content = """site_name: shel.sh App
site_url: https://app.shel.sh
docs_dir: docs
site_dir: site

theme:
  name: material
  palette:
    - scheme: slate
      primary: deep purple
      accent: deep purple
  features:
    - navigation.tabs
    - navigation.top
    - search.suggest
    - content.code.copy

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

extra_css:
  - stylesheets/app.css

nav:
  - Dashboard: index.md
  - Features: features.md
  - Settings: settings.md
"""

let render() = content


