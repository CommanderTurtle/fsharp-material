
module Config.Dependabot

let content = """# =============================================================================
# Dependabot Configuration
# =============================================================================

version: 2

updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"

  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
"""

let render() = content


