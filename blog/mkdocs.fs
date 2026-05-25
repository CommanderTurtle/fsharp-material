
module Blog.MkDocs

/// <summary>
/// MkDocs configuration for blog site (blog.shel.sh)
/// Deploys to 'blog' branch
/// </summary>
let content = """site_name: shel.sh Blog
site_url: https://blog.shel.sh
docs_dir: docs
site_dir: site

theme:
  name: material
  palette:
    - scheme: default
      primary: orange
      accent: orange
  features:
    - navigation.top
    - search.suggest
    - content.code.copy

plugins:
  - search
  - blog:
      blog_dir: .
      post_dir: posts
      post_url_format: "{date}/{slug}"
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
  - pymdownx.emoji:
      emoji_index: !!python/name:material.extensions.emoji.twemoji
      emoji_generator: !!python/name:material.extensions.emoji.to_svg

extra_css:
  - stylesheets/blog.css

nav:
  - Home: index.md
  - Archive: archive.md
"""

let render() = content


