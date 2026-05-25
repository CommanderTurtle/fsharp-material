
module Config.Workflows.PrCheck

let content = """# =============================================================================
# PR Validation Workflow
# =============================================================================
# Validates pull requests before merging.
# =============================================================================

name: PR Validation

on:
  pull_request:
    branches: [main, master]

permissions:
  contents: read

env:
  DOTNET_VERSION: '10.0.x'
  UV_VERSION: '0.5.x'

jobs:
  build:
    name: Build and Test
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Repository
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet Packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.fsproj') }}

      - name: Restore Dependencies
        run: dotnet restore

      - name: Build Solution
        run: dotnet build --configuration Release --no-restore

      - name: Setup uv
        uses: astral-sh/setup-uv@v5
        with:
          version: ${{ env.UV_VERSION }}

      - name: Install MkDocs
        run: uv pip install --system mkdocs mkdocs-material

      - name: Verify Build Output
        run: |
          echo "✓ Build successful"
"""

let render() = content


