module Config.Workflows.DeployWebsite

let content = """name: Deploy Website

on:
  push:
    branches: [ main ]
    paths:
      - 'main/**'
      - '.github/config/deploy-website.fs'
  workflow_dispatch:

permissions:
  contents: read

concurrency:
  group: "deploy-main"
  cancel-in-progress: false

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - uses: astral-sh/setup-uv@v3
      - uses: actions/setup-python@v5
        with:
          python-version: '3.x'

      - name: Generate pyproject.toml
        working-directory: main
        run: |
          if [ -f "pyproject.fs" ]; then
            dotnet fsi -e "#load \"pyproject.fs\"; open Main.PyProject; System.IO.File.WriteAllText(\"pyproject.toml\", render())"
          fi

      - name: Generate mkdocs.yml
        working-directory: main
        run: |
          if [ -f "mkdocs.fs" ]; then
            dotnet fsi -e "#load \"mkdocs.fs\"; open Main.MkDocs; System.IO.File.WriteAllText(\"mkdocs.yml\", render())"
          fi

      - name: Generate index.md from indexmd.fs
        working-directory: main
        run: |
          find . -name "indexmd.fs" -type f | while read f; do
            dir=$(dirname "$f"); name=$(basename "$f" .fs)
            cd "$dir"
            dotnet fsi -e "#r \"nuget: Giraffe.ViewEngine\"; #load \"$name.fs\"; open $(grep '^module ' $name.fs | head -1 | sed 's/^module //'); System.IO.File.WriteAllText(\"index.md\", render())"
            cd - > /dev/null
          done

      - name: Render index.fs to index.html
        working-directory: main
        run: |
          find . -name "index.fs" -type f | while read f; do
            dir=$(dirname "$f"); cd "$dir"
            modPath=$(grep -E '^(module|namespace) ' index.fs | head -1 | sed -E 's/^(module|namespace) //; s/ =$//')
            innerMod=$(grep '^module ' index.fs | grep '=' | sed -E 's/^module //; s/ =$//' | head -1)
            [ -n "$innerMod" ] && modPath="$modPath.$innerMod"
            dotnet fsi -e "#r \"nuget: Giraffe.ViewEngine\"; #load \"index.fs\"; open $modPath; System.IO.File.WriteAllText(\"index.html\", render())"
            cd - > /dev/null
          done

      - name: Install and build
        working-directory: main
        run: |
          uv pip install -e . --system || true
          if [ -f "mkdocs.yml" ]; then mkdocs build -f mkdocs.yml; echo "BUILD_OUTPUT=site" >> "$GITHUB_ENV"
          else echo "BUILD_OUTPUT=." >> "$GITHUB_ENV"; fi

      - name: Deploy to apex repo
        env:
          GH_PAGES_TOKEN: ${{ secrets.GH_PAGES_TOKEN }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          OWNER="${{ github.repository_owner }}"
          REPO="${OWNER}.github.io"
          TOKEN="${GH_PAGES_TOKEN:-${GITHUB_TOKEN}}"
          cd "main/${BUILD_OUTPUT:-.}"
          git init
          git config user.name "github-actions[bot]"
          git config user.email "github-actions[bot]@users.noreply.github.com"
          git remote add target "https://x-access-token:${TOKEN}@github.com/${OWNER}/${REPO}.git" 2>/dev/null || true
          echo "shel.sh" > CNAME
          git add . && git commit -m "Deploy main [skip ci]" || true
          git push target HEAD:main --force
"""

let render() = content
