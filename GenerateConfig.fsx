
#!/usr/bin/env dotnet fsi
// =============================================================================
// Multi-Subdomain Config Generator & YML Sync Tool
// =============================================================================
// Generates mkdocs.yml for each subdomain from F# sources
// Also provides bidirectional YML ↔ .fs synchronization
//
// Usage:
//   dotnet fsi GenerateConfig.fsx all        # Generate all configs
//   dotnet fsi GenerateConfig.fsx main       # Generate main only
//   dotnet fsi GenerateConfig.fsx docs       # Generate docs only
//   dotnet fsi GenerateConfig.fsx app        # Generate app only
//   dotnet fsi GenerateConfig.fsx blog       # Generate blog only
//   dotnet fsi GenerateConfig.fsx sync       # Sync YML → .fs
// =============================================================================

open System
open System.IO
open System.Text.RegularExpressions

// =============================================================================
// Config Generation Functions
// =============================================================================

let generate (folder: string) (moduleName: string) (outputName: string) =
    let fsPath = Path.Combine(folder, "mkdocs.fs")
    if not (File.Exists(fsPath)) then
        printfn "❌ %s not found" fsPath
        false
    else
        let code = sprintf """
#load "%s"
open %s
System.IO.File.WriteAllText("%s", render())
"""         fsPath moduleName (Path.Combine(folder, outputName))
        
        let tempFile = Path.GetTempFileName() + ".fsx"
        File.WriteAllText(tempFile, code)
        
        let psi = new System.Diagnostics.ProcessStartInfo("dotnet", sprintf "fsi %s" tempFile)
        psi.WorkingDirectory <- Environment.CurrentDirectory
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.UseShellExecute <- false
        
        use proc = System.Diagnostics.Process.Start(psi)
        proc.WaitForExit()
        
        File.Delete(tempFile)
        
        if proc.ExitCode = 0 then
            printfn "  ✓ %s/%s" folder outputName
            true
        else
            printfn "  ❌ Failed to generate %s/%s" folder outputName
            printfn "     %s" (proc.StandardError.ReadToEnd())
            false

let generateAll() =
    printfn ""
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║     Generating MkDocs Configurations                       ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    printfn ""
    
    let results = [
        generate "main" "Main.MkDocs" "mkdocs.yml"
        generate "docs" "Docs.MkDocs" "mkdocs.yml"
        generate "app" "App.MkDocs" "mkdocs.yml"
        generate "blog" "Blog.MkDocs" "mkdocs.yml"
    ]
    
    printfn ""
    if List.forall id results then
        printfn "✅ All configurations generated successfully!"
    else
        printfn "⚠️  Some configurations failed to generate."
    printfn ""

// =============================================================================
// YML → .fs Sync Functions
// =============================================================================

/// Convert a kebab-case name to PascalCase module name
let toPascalCase (name: string) =
    name.Split([|'-'; '_'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.map (fun part ->
        if String.IsNullOrEmpty(part) then ""
        else Char.ToUpper(part.[0]).ToString() + part.[1..])
    |> String.concat ""

/// Determine the module path based on file name
let getModulePath (fileName: string) =
    match fileName with
    | "dependabot" -> "Config.Dependabot"
    | name when name.StartsWith("deploy-") -> 
        let suffix = name.Substring(7) |> toPascalCase
        sprintf "Config.Workflows.Deploy%s" suffix
    | name when name.StartsWith("pr-") ->
        let suffix = name.Substring(3) |> toPascalCase
        sprintf "Config.Workflows.Pr%s" suffix
    | name when name.Contains("dependency") ->
        "Config.Workflows.DependencyUpdate"
    | name when name.StartsWith("build-and-deploy") ->
        "Config.Workflows.BuildAndDeploy"
    | name when name.StartsWith("hello-world") ->
        "Workflows.HelloWorld"
    | _ ->
        sprintf "Config.Workflows.%s" (toPascalCase fileName)

/// Check if a YML file should be synced (not sharpendabot)
let shouldSync (fileName: string) =
    let name = Path.GetFileNameWithoutExtension(fileName)
    name <> "sharpendabot"

/// Escape content for F# triple-quoted string
let escapeForTripleQuoted (content: string) =
    if content.Contains("""") then
        let parts = content.Split([|""""|], StringSplitOptions.None)
        parts |> String.concat """ + "\"\"\"" + """
    else
        content

/// Generate the F# source file content
let generateFsContent (modulePath: string) (ymlContent: string) (sourceFile: string) =
    let escapedContent = escapeForTripleQuoted ymlContent
    
    sprintf """module %s

/// <summary>
/// Auto-generated from %s
/// This workflow was encoded back from its YML form.
/// </summary>
let content = """
%s
"""

let render() = content
"""         modulePath sourceFile escapedContent

/// Sync a single YML file to its .fs equivalent
let syncYmlFile (ymlPath: string) (configDir: string) =
    let fileName = Path.GetFileNameWithoutExtension(ymlPath)
    let fsPath = Path.Combine(configDir, fileName + ".fs")
    
    printfn "  Syncing: %s → %s" (Path.GetFileName(ymlPath)) (Path.GetFileName(fsPath))
    
    let ymlContent = File.ReadAllText(ymlPath)
    let modulePath = getModulePath fileName
    let fsContent = generateFsContent modulePath ymlContent (Path.GetFileName(ymlPath))
    
    File.WriteAllText(fsPath, fsContent)
    printfn "    ✓ Encoded: %s" fsPath

/// Sync all YML files to .fs
let syncAllYml() =
    printfn ""
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║     Syncing YML → .fs                                      ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    printfn ""
    
    let workflowsDir = ".github/workflows"
    let configDir = ".github/config"
    
    if not (Directory.Exists(workflowsDir)) then
        printfn "❌ Workflows directory not found: %s" workflowsDir
        ()
    else
        Directory.CreateDirectory(configDir) |> ignore
        
        let ymlFiles = Directory.GetFiles(workflowsDir, "*.yml")
        let mutable syncedCount = 0
        
        for ymlFile in ymlFiles do
            if shouldSync ymlFile then
                try
                    syncYmlFile ymlFile configDir
                    syncedCount <- syncedCount + 1
                with
                | ex ->
                    printfn "  ✗ Failed to sync %s: %s" (Path.GetFileName(ymlFile)) ex.Message
            else
                printfn "  ⏭ Skipped (authority): %s" (Path.GetFileName(ymlFile))
        
        printfn ""
        printfn "✅ Synced %d workflow(s)" syncedCount
        printfn ""

// =============================================================================
// Entry Point
// =============================================================================

match Environment.GetCommandLineArgs() |> Array.tryLast with
| Some "all" | None -> generateAll()
| Some "main" -> generate "main" "Main.MkDocs" "mkdocs.yml" |> ignore
| Some "docs" -> generate "docs" "Docs.MkDocs" "mkdocs.yml" |> ignore
| Some "app" -> generate "app" "App.MkDocs" "mkdocs.yml" |> ignore
| Some "blog" -> generate "blog" "Blog.MkDocs" "mkdocs.yml" |> ignore
| Some "sync" -> syncAllYml()
| Some arg ->
    printfn "Unknown argument: %s" arg
    printfn ""
    printfn "Usage:"
    printfn "  dotnet fsi GenerateConfig.fsx all        # Generate all configs"
    printfn "  dotnet fsi GenerateConfig.fsx main       # Generate main only"
    printfn "  dotnet fsi GenerateConfig.fsx docs       # Generate docs only"
    printfn "  dotnet fsi GenerateConfig.fsx app        # Generate app only"
    printfn "  dotnet fsi GenerateConfig.fsx blog       # Generate blog only"
    printfn "  dotnet fsi GenerateConfig.fsx sync       # Sync YML → .fs"


