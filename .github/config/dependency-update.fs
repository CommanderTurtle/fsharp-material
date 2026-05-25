
module Config.Workflows.DependencyUpdate

let content = """# =============================================================================
# Dependency Update Workflow
# =============================================================================
# Automatically checks for and updates dependencies weekly.
# =============================================================================

name: Dependency Updates

on:
  schedule:
    - cron: '0 9 * * 1'  # Mondays at 9:00 UTC
  workflow_dispatch:

permissions:
  contents: write
  pull-requests: write

jobs:
  update:
    name: Check for Updates
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Repository
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Check .NET Packages
        run: |
          dotnet list package --outdated --include-transitive 2>/dev/null || true

      - name: Create Update PR
        uses: peter-evans/create-pull-request@v6
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: 'chore: update dependencies'
          title: 'Automated Dependency Updates'
          body: |
            This PR contains automated dependency updates.
            
            Please review and merge if all checks pass.
          branch: automated/dependency-updates
          delete-branch: true
"""

let render() = content


