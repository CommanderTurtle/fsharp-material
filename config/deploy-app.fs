module Config.Workflows.DeployApp

let content = """name: Deploy App

on:
  push:
    branches: [ main ]
    paths:
      - 'app/**'
      - '.github/config/deploy-app.fs'
  workflow_dispatch:

permissions:
  contents: read

concurrency:
  group: "deploy-app"
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

      - name: Generate configs
        working-directory: app
        run: |
          if [ -f "pyproject.fs" ]; then dotnet fsi -e "#load \"pyproject.fs\"; open App.PyProject; System.IO.File.WriteAllText(\"pyproject.toml\", render())"; fi
          if [ -f "mkdocs.fs" ]; then dotnet fsi -e "#load \"mkdocs.fs\"; open App.MkDocs; System.IO.File.WriteAllText(\"mkdocs.yml\", render())"; fi

      - name: Generate index.md and index.html
        working-directory: app
        run: |
          find . -name "indexmd.fs" -type f | while read f; do
            dir=$(dirname "$f"); name=$(basename "$f" .fs)
            cd "$dir"; dotnet fsi -e "#r \"nuget: Giraffe.ViewEngine\"; #load \"$name.fs\"; open $(grep '^module ' $name.fs | head -1 | sed 's/^module //'); System.IO.File.WriteAllText(\"index.md\", render())"; cd - > /dev/null
          done
          find . -name "index.fs" -type f | while read f; do
            dir=$(dirname "$f"); cd "$dir"
            modPath=$(grep -E '^(module|namespace) ' index.fs | head -1 | sed -E 's/^(module|namespace) //; s/ =$//')
            innerMod=$(grep '^module ' index.fs | grep '=' | sed -E 's/^module //; s/ =$//' | head -1)
            [ -n "$innerMod" ] && modPath="$modPath.$innerMod"
            dotnet fsi -e "#r \"nuget: Giraffe.ViewEngine\"; #load \"index.fs\"; open $modPath; System.IO.File.WriteAllText(\"index.html\", render())"; cd - > /dev/null
          done

      - name: Build
        working-directory: app
        run: |
          uv pip install -e . --system || true
          if [ -f "mkdocs.yml" ]; then mkdocs build -f mkdocs.yml; echo "BUILD_OUTPUT=site" >> "$GITHUB_ENV"
          else echo "BUILD_OUTPUT=." >> "$GITHUB_ENV"; fi

      - name: Deploy to app repo
        env:
          GH_PAGES_TOKEN: ${{ secrets.GH_PAGES_TOKEN }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          OWNER="${{ github.repository_owner }}"
          REPO="app-pages"
          TOKEN="${GH_PAGES_TOKEN:-${GITHUB_TOKEN}}"
          cd "app/${BUILD_OUTPUT:-.}"
          git init && git config user.name "github-actions[bot]" && git config user.email "github-actions[bot]@users.noreply.github.com"
          git remote add target "https://x-access-token:${TOKEN}@github.com/${OWNER}/${REPO}.git" 2>/dev/null || true
          echo "app.shel.sh" > CNAME
          git add . && git commit -m "Deploy app [skip ci]" || true
          git push target HEAD:main --force
"""

let render() = content
