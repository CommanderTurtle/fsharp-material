module Config.Workflows.Sharpendabot

let content = """# =============================================================================
# Sharpendabot - Authoritative F# File Translator & Workflow Controller
# =============================================================================
# This workflow is the SINGLE POINT OF CONTROL for all F#-generated artifacts.
#
# BIDIRECTIONAL SYNC:
#   - .fs -> YML: Normal generation from F# sources
#   - YML -> .fs: Encode current YML state back to .fs (for recovery/migration)
#
# TRANSLATION MODES:
#   MODE 1: Pattern-Based (steamedyam-*, sharp*)
#   MODE 2: Inline Generation (.github/config/*.fs)
#
# STATE MACHINE (controlled by 'bool' file):
#   STATE 1 (bool missing): Generate all files -> create bool -> STOP
#   STATE 2 (bool exists): Execute workflows -> cleanup -> delete bool
#
# ESSENTIAL YMLS (NEVER deleted in State 2):
#   - sharpendabot.yml (this workflow - the authority)
#   - deploy-*.yml (subdomain deployers)
#   - pr-check.yml
#   - dependency-update.yml
#
# NON-ESSENTIAL YMLs (deleted in State 2, regenerated from .fs sources):
#   - All steamedyam-*.yml files
#   - build-and-deploy.yml (throw conversion only)
#   - dependabot.yml
#
# GITHUB PAGES BEHAVIOR:
#   Deleting workflows does NOT unpublish your site. GitHub Pages serves
#   the last deployed snapshot indefinitely until you explicitly disable
#   Pages or push a new deployment.
# =============================================================================

name: Sharpendabot

on:
  push:
    branches: [main, master]
    paths:
      - '.github/sources/*.fs'
      - '.github/config/*.fs'
      - '**/sharp*.fs'
      - '**/steamedyam-*.fs'
      - 'bool'
      - 'throw/**'
  workflow_dispatch:
    inputs:
      mode:
        description: 'Execution mode'
        required: false
        default: 'auto'
        type: choice
        options:
          - auto
          - regenerate
          - sync
          - clean

permissions:
  contents: write

env:
  DOTNET_VERSION: '10.0.x'

jobs:
  sharpendabot:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Repository
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Determine State
        id: state
        run: |
          MANUAL_MODE="${{ github.event.inputs.mode }}"
          if [ -n "$MANUAL_MODE" ] && [ "$MANUAL_MODE" != "auto" ]; then
            echo "mode=$MANUAL_MODE" >> $GITHUB_OUTPUT
            echo "Manual mode: $MANUAL_MODE"
            exit 0
          fi

          if [ -f "bool" ]; then
            BOOL_CONTENT=$(cat bool 2>/dev/null | tr -d '\n' || echo "")
            echo "exists=true" >> $GITHUB_OUTPUT
            echo "content=$BOOL_CONTENT" >> $GITHUB_OUTPUT
            echo "STATE: bool exists -> State 2 (Execute + Cleanup)"
          else
            echo "exists=false" >> $GITHUB_OUTPUT
            echo "STATE: bool missing -> State 1 (Generate + Create bool)"
          fi

      - name: Sync YML to .fs
        if: steps.state.outputs.exists == 'false' || steps.state.outputs.mode == 'sync' || steps.state.outputs.mode == 'regenerate'
        run: |
          if [ -f "GenerateConfig.fsx" ]; then
            echo "=== Running YML -> .fs Sync ==="
            dotnet fsi GenerateConfig.fsx sync
          else
            echo "GenerateConfig.fsx not found, skipping sync"
          fi

      - name: Inline Generation - Config Workflows
        if: steps.state.outputs.exists == 'false' || steps.state.outputs.mode == 'regenerate' || steps.state.outputs.mode == 'sync'
        run: |
          echo "========================================"
          echo "MODE 2: Inline Generation (.github/config/*.fs)"
          echo "========================================"
          
          mkdir -p .github/workflows
          generated=0
          
          for fs_file in .github/config/*.fs; do
            [ -f "$fs_file" ] || continue
            name=$(basename "$fs_file" .fs)
            
            module_line=$(grep "^module " "$fs_file" | head -1)
            if [ -n "$module_line" ]; then
              module_path=$(echo "$module_line" | sed 's/^module //')
              
              echo "Generating: $name.yml"
              dotnet fsi -e "
                #load \"$fs_file\"
                open $(echo "$module_path" | sed 's/\././g')
                System.IO.File.WriteAllText(\".github/workflows/$name.yml\", render())
                printfn \"Generated: $name.yml\"
              "
              generated=$((generated + 1))
            fi
          done
          
          echo "Generated $generated workflow(s)"
          echo "========================================"

      - name: Pattern-Based Translation
        if: steps.state.outputs.exists == 'false' || steps.state.outputs.mode == 'regenerate' || steps.state.outputs.mode == 'sync'
        run: |
          dotnet fsi <<'FSSCRIPT'
          open System
          open System.IO
          open System.Text.RegularExpressions

          let steamedyamPattern = Regex(@"^steamedyam-(.+)\.fs$", RegexOptions.IgnoreCase)
          let sharpPattern = Regex(@"^sharp([a-zA-Z]+)-(.+)\.fs$", RegexOptions.IgnoreCase)
          
          let workflowDir = ".github/workflows"
          let sourcesDir = ".github/sources"
          let mutable generatedWorkflows = []
          let mutable generatedOther = []
          let mutable errors = []
          
          Directory.CreateDirectory(workflowDir) |> ignore
          
          let excludedDirs = [".git"; "_site"; "site"; "bin"; "obj"; "node_modules"]
          let allFsFiles = 
              Directory.GetFiles(".", "*.fs", SearchOption.AllDirectories)
              |> Array.filter (fun f -> 
                  let dir = Path.GetDirectoryName(f)
                  not (excludedDirs |> List.exists (fun ex -> dir.Contains(ex)))
              )
          
          printfn "========================================"
          printfn "MODE 1: Pattern-Based Translation"
          printfn "========================================"
          printfn "Scanning %d F# file(s)..." allFsFiles.Length
          printfn ""
          
          for fsFile in allFsFiles do
              let fileName = Path.GetFileName(fsFile)
              let dir = Path.GetDirectoryName(fsFile)
              
              if dir.Contains(".github/config") then ()
              else
                  let steamedyamMatch = steamedyamPattern.Match(fileName)
                  if steamedyamMatch.Success then
                      if not (dir.Contains(sourcesDir)) then
                          printfn "⚠️  SKIPPED (not in %s): %s" sourcesDir fsFile
                          errors <- (fsFile, "Not in .github/sources/") :: errors
                      else
                          let name = steamedyamMatch.Groups.[1].Value
                          let outFile = Path.Combine(workflowDir, name + ".yml")
                          
                          printfn "Translating: %s -> %s" fileName (Path.GetFileName(outFile))
                          
                          try
                              let fileContent = File.ReadAllText(fsFile)
                              let moduleMatch = Regex.Match(fileContent, @"^module\s+([A-Za-z0-9_.]+)", RegexOptions.Multiline)
                              let openStmt = if moduleMatch.Success then sprintf "open %s\n" moduleMatch.Groups.[1].Value else ""
                              
                              let tempScript = Path.GetTempFileName() + ".fsx"
                              let scriptCode = sprintf """#load @"%s"
%slet output = render()
System.IO.File.WriteAllText(@"%s", output)
printfn "OK: %s"
"""                                 fsFile openStmt outFile (Path.GetFileName(outFile))
                              
                              File.WriteAllText(tempScript, scriptCode)
                              
                              let psi = new System.Diagnostics.ProcessStartInfo("dotnet", sprintf "fsi %s" tempScript)
                              psi.WorkingDirectory <- Directory.GetCurrentDirectory()
                              psi.RedirectStandardOutput <- true
                              psi.RedirectStandardError <- true
                              psi.UseShellExecute <- false
                              
                              use proc = System.Diagnostics.Process.Start(psi)
                              proc.WaitForExit()
                              let output = proc.StandardOutput.ReadToEnd()
                              let error = proc.StandardError.ReadToEnd()
                              
                              File.Delete(tempScript)
                              
                              if proc.ExitCode = 0 && File.Exists(outFile) then
                                  printfn "   ✓ Generated: %s" (Path.GetFileName(outFile))
                                  generatedWorkflows <- outFile :: generatedWorkflows
                              else
                                  printfn "   ✗ Failed: %s" error
                                  errors <- (fsFile, error) :: errors
                          with
                          | ex ->
                              printfn "   ✗ Exception: %s" ex.Message
                              errors <- (fsFile, ex.Message) :: errors
                  
                  let sharpMatch = sharpPattern.Match(fileName)
                  if sharpMatch.Success then
                      let ext = sharpMatch.Groups.[1].Value.ToLower()
                      let name = sharpMatch.Groups.[2].Value
                      
                      let outExt =
                          match ext with
                          | "css" -> ".css" | "py" -> ".py" | "js" -> ".js"
                          | "ts" -> ".ts" | "md" -> ".md" | "html" -> ".html"
                          | "json" -> ".json" | "oml" | "toml" -> ".toml"
                          | "yaml" | "yml" -> ".yml" | "xml" -> ".xml"
                          | "sql" -> ".sql" | "sh" -> ".sh" | "ps1" -> ".ps1"
                          | "docker" | "dockerfile" -> ""
                          | _ -> "." + ext
                      
                      let outFile = 
                          if ext = "docker" || ext = "dockerfile" then
                              Path.Combine(dir, "Dockerfile")
                          else
                              Path.Combine(dir, name + outExt)
                      
                      printfn "Translating: %s -> %s" fileName (Path.GetFileName(outFile))
                      
                      try
                          let fileContent = File.ReadAllText(fsFile)
                          let moduleMatch = Regex.Match(fileContent, @"^module\s+([A-Za-z0-9_.]+)", RegexOptions.Multiline)
                          let openStmt = if moduleMatch.Success then sprintf "open %s\n" moduleMatch.Groups.[1].Value else ""
                          
                          let tempScript = Path.GetTempFileName() + ".fsx"
                          let scriptCode = sprintf """#load @"%s"
%slet output = render()
System.IO.File.WriteAllText(@"%s", output)
printfn "OK: %s"
"""                             fsFile openStmt outFile (Path.GetFileName(outFile))
                          
                          File.WriteAllText(tempScript, scriptCode)
                          
                          let psi = new System.Diagnostics.ProcessStartInfo("dotnet", sprintf "fsi %s" tempScript)
                          psi.WorkingDirectory <- Directory.GetCurrentDirectory()
                          psi.RedirectStandardOutput <- true
                          psi.RedirectStandardError <- true
                          psi.UseShellExecute <- false
                          
                          use proc = System.Diagnostics.Process.Start(psi)
                          proc.WaitForExit()
                          let output = proc.StandardOutput.ReadToEnd()
                          let error = proc.StandardError.ReadToEnd()
                          
                          File.Delete(tempScript)
                          
                          if proc.ExitCode = 0 && File.Exists(outFile) then
                              printfn "   ✓ Generated: %s" (Path.GetFileName(outFile))
                              generatedOther <- outFile :: generatedOther
                          else
                              printfn "   ✗ Failed: %s" error
                              errors <- (fsFile, error) :: errors
                      with
                      | ex ->
                          printfn "   ✗ Exception: %s" ex.Message
                          errors <- (fsFile, ex.Message) :: errors
          
          printfn ""
          printfn "Summary"
          printfn "========================================"
          printfn "Workflows generated: %d" generatedWorkflows.Length
          generatedWorkflows |> List.iter (fun f -> printfn "   - %s" (Path.GetFileName(f)))
          printfn "Other files generated: %d" generatedOther.Length
          generatedOther |> List.iter (fun f -> printfn "   - %s" f)
          if errors.Length > 0 then
              printfn "Errors: %d" errors.Length
              errors |> List.iter (fun (f, e) -> printfn "   ✗ %s: %s" f e)
          printfn "========================================"
          
          if errors.Length > 0 then Environment.Exit(1)
          FSSCRIPT

      - name: Create bool file
        if: steps.state.outputs.exists == 'false' || steps.state.outputs.mode == 'regenerate' || steps.state.outputs.mode == 'sync'
        run: |
          TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
          COMMIT=$(git rev-parse --short HEAD)
          
          cat > bool <<EOF
# Sharpendabot State File
# Created: $TIMESTAMP
# Commit: $COMMIT
# 
# Trigger commands:
# - "throw" : Trigger HTML->DSL conversion
# - "regenerate" : Force regeneration
# - "sync" : Encode YML state back to .fs
# - "clean" : Skip execution, just cleanup
EOF
          
          echo "Created bool file -> Next push triggers State 2"

      - name: Commit Generated Files
        if: steps.state.outputs.exists == 'false' || steps.state.outputs.mode == 'regenerate' || steps.state.outputs.mode == 'sync'
        run: |
          git config user.name "sharpendabot[bot]"
          git config user.email "sharpendabot[bot]@users.noreply.github.com"
          
          git add -A
          
          if git diff --cached --quiet; then
            echo "No changes to commit"
          else
            git commit -m "🔧 Generate files from F# sources [skip ci]"
            git push
            echo "Committed generated files + bool"
            echo ""
            echo "⚠️  Push again (or modify bool file) to trigger State 2"
          fi

      - name: HTML to Giraffe DSL Conversion
        if: steps.state.outputs.exists == 'true'
        run: |
          BOOL_CONTENT="${{ steps.state.outputs.content }}"
          
          SHOULD_THROW=false
          if [ "$BOOL_CONTENT" = "throw" ]; then
            SHOULD_THROW=true
            echo "Bool content is 'throw' -> Running conversion"
          elif [ -d "throw" ] && [ "$(find throw -name '*.html' -type f 2>/dev/null | wc -l)" -gt 0 ]; then
            SHOULD_THROW=true
            echo "HTML files found in /throw -> Running conversion"
          fi
          
          if [ "$SHOULD_THROW" = "true" ]; then
            echo "=== HTML -> Giraffe DSL Conversion ==="
            HTML_COUNT=$(find throw -name "*.html" -type f 2>/dev/null | wc -l)
            echo "Found $HTML_COUNT HTML file(s)"
          else
            echo "No HTML conversion needed"
          fi

      - name: Note Generated Workflows
        if: steps.state.outputs.exists == 'true'
        run: |
          echo "=== Workflows before cleanup ==="
          for yml in .github/workflows/*.yml; do
            [ -f "$yml" ] || continue
            name=$(basename "$yml")
            if [ "$name" = "sharpendabot.yml" ]; then
              echo "  ♻️  Authority (never touched): $name"
            elif [[ "$name" == deploy-*.yml ]] || \
                 [ "$name" = "pr-check.yml" ] || \
                 [ "$name" = "dependency-update.yml" ]; then
              echo "  ♻️  Essential (kept): $name"
            else
              echo "  🗑️  Will delete: $name"
            fi
          done

      - name: Delete Non-Essential YMLs
        if: steps.state.outputs.exists == 'true' && steps.state.outputs.mode != 'clean'
        run: |
          echo "=== Cleaning up generated workflows ==="
          deleted=0
          for yml in .github/workflows/*.yml; do
            [ -f "$yml" ] || continue
            name=$(basename "$yml")
            
            # Essential YMLs: NEVER delete these
            if [ "$name" = "sharpendabot.yml" ] || \
               [[ "$name" == deploy-*.yml ]] || \
               [ "$name" = "pr-check.yml" ] || \
               [ "$name" = "dependency-update.yml" ]; then
              echo "  Preserving essential: $name"
              continue
            fi
            
            # Everything else can be regenerated from .fs
            echo "  Deleting: $name"
            rm -f "$yml"
            git rm "$yml" 2>/dev/null || true
            deleted=$((deleted + 1))
          done
          echo "Deleted $deleted workflow(s)"

      - name: Delete bool file
        if: steps.state.outputs.exists == 'true'
        run: |
          rm -f bool
          git rm bool 2>/dev/null || true
          echo "Deleted bool file -> State machine reset"

      - name: Commit Cleanup
        if: steps.state.outputs.exists == 'true'
        run: |
          git config user.name "sharpendabot[bot]"
          git config user.email "sharpendabot[bot]@users.noreply.github.com"
          
          git add -A
          
          if git diff --cached --quiet; then
            echo "No changes to commit"
          else
            git commit -m "🧹 Cleanup generated workflows [skip ci]"
            git push
            echo "Committed cleanup"
          fi
"""

let render() = content
