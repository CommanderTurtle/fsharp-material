
module Blog.PyProject

let content = """[project]
name = "shel-sh-blog"
version = "1.0.0"
description = "Blog for shel.sh"
requires-python = ">=3.9"
dependencies = [
    "mkdocs>=1.6.1",
    "mkdocs-material>=9.7.6",
    "mkdocs-minify-plugin>=0.7.0",
    "mkdocs-blog-plugin>=1.0.0",
]

[build-system]
requires = ["hatchling"]
build-backend = "hatchling.build"
"""

let render() = content


