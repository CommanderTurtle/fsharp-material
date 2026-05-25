module Config.Workflows.BuildAndDeploy

let content = """name: Build and Deploy

on:
  push:
    branches: [main, master]
    paths:
      - 'throw/**'
      - 'src/**'
      - '.github/config/build-and-deploy.fs'
  workflow_dispatch:

permissions:
  contents: read

jobs:
  convert:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore and build generator
        run: |
          dotnet restore src/generator/Generator.fsproj
          dotnet build src/generator/Generator.fsproj --no-restore

      - name: HTML to Giraffe DSL Conversion
        run: |
          if [ ! -d "throw" ] || [ ! "$(ls throw/*.html 2>/dev/null)" ]; then
            echo "No HTML files to convert"
            exit 0
          fi
          for f in throw/*.html; do
            name=$(basename "$f" .html)
            mkdir -p "throw/$name"
            echo "Converting: $f -> throw/$name/index.fs"
            dotnet run --project src/generator/Generator.fsproj -- convert "$f" "throw/$name/index.fs"
          done
          echo "Converted files are in throw/<filename>/index.fs"
          echo "Move them manually to your target site directory."
"""

let render() = content
