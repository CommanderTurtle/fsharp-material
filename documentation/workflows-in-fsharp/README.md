# Workflows in F# (Sharpendabot)

## Overview

Sharpendabot is a two-phase workflow system that allows you to write GitHub Actions workflows (and other files) in F#.

## The Problem

GitHub Actions only sees workflow files that exist at push time. If we generate workflows DURING a build, GitHub won't know about them until the NEXT push.

## The Solution: Two-Phase State Machines

Sharpendabot uses a simple state machine mechanic controlled by a `bool` file:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         BOOL STATE MACHINE                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐              ┌─────────────────┐                   │
│  │  STATE 1        │              │  STATE 2        │                   │
│  │  bool = false   │ ──create──▶ │  bool = true    │                   │
│  │  (or missing)   │    bool    │                 │                   │
│  └─────────────────┘              └─────────────────┘                   │
│          ▲                                │                              │
│          │                                │                              │
│          │         delete bool            │                              │
│          └────────────────────────────────┘                              │
│                                                                          │
│  STATE 1: Translate F# → target files + create bool + STOP               │
│  STATE 2: Note workflows + delete non-essential YMLs + delete bool       │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

And a SEPARATE state machine (`deploy-state`) to control cross-repo deployment:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       DEPLOY ORCHESTRATOR STATE MACHINE                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐    build-all    ┌─────────────┐    deploy-all   ┌────────┐ │
│  │   IDLE      │ ──────────────▶ │   BUILT     │ ──────────────▶ │ DEPLOY │ │
│  │ (no marker) │                 │ (built.all) │                 │ (done) │ │
│  └─────────────┘                 └─────────────┘                 └────────┘ │
│                                                                             │
│  Trigger: push to main/ docs/ app/ blog/ pages/ throw/                     │
│  Action:  1. Run mkdocs build / F# render for all changed folders            │
│           2. Mark built.all                                                  │
│           3. Deploy orchestrator pushes each site/ folder to target repo    │
│           4. Mark deploy.done, clean markers                                 │
└─────────────────────────────────────────────────────────────────────────────┘
```

**In simpler terms:** Sharpendabot is a two-push dance. Push 1 generates YAML workflow files from F# sources and drops a marker (the `bool` file). Push 2 cleans up the generated YAMLs, leaving only the F# sources as the permanent truth. This works around a GitHub limitation: you can't generate a workflow AND have GitHub run it in the same push.

**The `bool` file** is literally just an empty file named `.sharpendabot-bool` in the repo root. Its existence is the signal. Its absence triggers generation. This is elegant in its simplicity -- no databases, no state servers, just a file in git.
## Source of Truth

### `.github/config/` - Inline Generation (Essential Workflows)

Essential workflows that are regenerated each cycle but kept during operation:

```
.github/
├── config/
│   ├── deploy-website.fs    # → deploy-website.yml (mysite.tld)
│   ├── deploy-docs.fs       # → deploy-docs.yml (docs.mysite.tld)
│   ├── deploy-app.fs        # → deploy-app.yml (app.mysite.tld)
│   ├── deploy-blog.fs       # → deploy-blog.yml (blog.mysite.tld)
│   ├── pr-check.fs          # → pr-check.yml
│   ├── dependency-update.fs # → dependency-update.yml
│   └── dependabot.fs        # → dependabot.yml
```

### `.github/sources/` - Pattern-Based (Transient Workflows)

Transient workflows that are generated then cleaned up:

```
.github/
├── sources/
│   ├── steamedyam-build-and-deploy.fs   # → build-and-deploy.yml (transient)
│   └── steamedyam-hello-world.fs        # → hello-world.yml (transient)
```

## Dual Translation Modes

### MODE 1: Pattern-Based Translation

| Source Pattern | Output | Location |
|----------------|--------|----------|
| `steamedyam-<name>.fs` | `<name>.yml` | `.github/workflows/` (from `.github/sources/`) |
| `sharp<ext>-<name>.fs` | `<name>.<ext>` | Same directory as source |

### MODE 2: Inline Generation

Uses `dotnet fsi -e "#load ... open ... render()"` pattern:

```bash
dotnet fsi -e "
  #load \".github/config/deploy-website.fs\"
  open Config.Workflows.DeployWebsite
  System.IO.File.WriteAllText(\".github/workflows/deploy-website.yml\", render())
"
```

## Bidirectional Sync

Sharpendabot supports bidirectional synchronization using F# itself:

- **.fs → YML**: Normal generation from F# sources
- **YML → .fs**: Encode current YML state back to .fs (for recovery/migration)

### F#-Based Sync

The sync operation uses `GenerateConfig.fsx` - the same F# script that generates configs:

```bash
dotnet fsi GenerateConfig.fsx sync
```

This:
1. Reads all YML files in `.github/workflows/`
2. Generates proper F# source files in `.github/config/`
3. Uses **triple-quoted strings** for safe YML content embedding

### Why F# for Sync?

YML files are too complex for shell tools like `echo` or `sed`:
- Quotes, backslashes, and special characters need proper handling
- Triple-quoted strings in F# (`"""..."""`) don't interpret escape sequences
- Perfect for embedding arbitrary YML content without escaping hell

Example F# output:
```fsharp
module Config.Workflows.DeployWebsite

let content = """
name: Deploy Website

on:
  push:
    branches: [main]
"""

let render() = content
```

### Using Sync

Use the `sync` command to encode existing YMLs back to .fs:

```bash
echo "sync" > bool
git add bool && git commit -m "Sync YML to .fs" && git push
```

The F# script (`GenerateConfig.fsx`) will run during State 1 and encode all non-sharpendabot YMLs to their .fs equivalents.

## Bool File Commands

The `bool` file content can trigger specific actions:

| Command | Action |
|---------|--------|
| (empty) | Normal two-phase operation |
| `regenerate` | Force regeneration |
| `sync` | Encode YML state back to .fs |
| `throw` | Trigger HTML→DSL conversion |
| `clean` | Skip execution, just cleanup |

## How It Works

### Example: Creating a Workflow

**Step 1:** Create `.github/sources/steamedyam-deploy-staging.fs`:

```fsharp
module Workflows.DeployStaging

let content = """
name: Deploy Staging

on:
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: echo "Deploying to staging..."
"""

let render() = content
```

**Step 2:** Push to main

```bash
git add .github/sources/steamedyam-deploy-staging.fs
git commit -m "Add staging deployment workflow"
git push
```

**Step 3:** Sharpendabot (State 1) runs:
- Translates `steamedyam-deploy-staging.fs` → `.github/workflows/deploy-staging.yml`
- Creates `bool` file
- Commits both
- Stops

**Step 4:** Push again (or any trigger)

Sharpendabot (State 2) runs:
- Notes that workflows exist
- Deletes `.github/workflows/deploy-staging.yml`
- Deletes `bool`
- Commits cleanup
- Stops

**The workflow exists only transiently**. The F# source is the permanent source of truth.

## Important Notes

### Workflows Are Transient

Generated workflow YMLs are deleted in State 2. This is by design:
- The F# source is the permanent source of truth
- The YML is generated, used, then cleaned up
- This keeps the repo clean

### Why Two Pushes?

GitHub Actions decides which workflows to run BEFORE any workflow executes. So:
- Push 1: Generate YML + create bool
- Push 2: YML exists in repo, GitHub can run it

This is unavoidable due to GitHub Actions architecture.

### Essential vs Non-Essential Workflows

| Type | Kept | Deleted in State 2 |
|------|------|-------------------|
| `sharpendabot.yml` | ✅ Yes | ❌ Never (the authority) |
| `deploy-*.yml` | ❌ No | ✅ Yes (regenerated from config/) |
| `pr-check.yml` | ❌ No | ✅ Yes (regenerated from config/) |
| `dependency-update.yml` | ❌ No | ✅ Yes (regenerated from config/) |
| `dependabot.yml` | ❌ No | ✅ Yes (regenerated from config/) |
| `steamedyam-*.yml` | ❌ No | ✅ Yes (transient) |
| `build-and-deploy.yml` | ❌ No | ✅ Yes (transient) |

**Note:** Only `sharpendabot.yml` is truly essential and never deleted. All other workflows can be regenerated from their `.fs` sources.

`deploy-*.yml`, `pr-check.yml`, and `dependency-update.yml` are **protected** in the sharpendabot Step 9 essential list. They are still deleted in State 2 (transient), but sharpendabot will never accidentally delete them during its own operation. Only `sharpendabot.yml` is truly permanent.
### Skipped Directories

Sharpendabot skips these directories (handled by deploy workflows):
- `/src/` - Source code
- `/docs/` - Documentation site
- `/main/` - Main site
- `/app/` - App site
- `/blog/` - Blog site

## Manual State Reset

If something goes wrong:

```bash
rm bool
git rm bool 2>/dev/null
git commit -m "Reset sharpendabot state"
git push
```

## Troubleshooting

### "My workflow didn't run!"

Expected. Sharpendabot is a two-push system:
- Push 1: Generate YML + create bool
- Push 2: Note workflows + cleanup

### "Generated file has wrong extension!"

Check your prefix:
- `sharpcss-` → `.css`
- `sharppy-` → `.py`
- `sharpjs-` → `.js`
- etc.

### "Sharpendabot keeps running!"

Make sure `[skip ci]` is in the commit message.

## .NET Version

Sharpendabot uses the latest .NET version:

```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
```

This ensures F# files can use the latest language features. This matches the `net10.0` target framework in all `.fsproj` files.

---

## Workflow Execution Order & Priorities

The system is designed so workflows don't conflict - each has specific triggers and responsibilities:

### Workflow Trigger Matrix

| Workflow | Triggers On | Purpose | Priority |
|----------|-------------|---------|----------|
| **Sharpendabot** | `.github/config/*.fs`, `.github/sources/*.fs`, `bool` | Generate YMLs from F# | Highest (runs first) |
| **Deploy Website** | `main/**` | Deploy shel.sh | Independent |
| **Deploy Docs** | `docs/**` | Deploy docs.shel.sh | Independent |
| **Deploy App** | `app/**` | Deploy app.shel.sh | Independent |
| **Deploy Blog** | `blog/**` | Deploy blog.shel.sh | Independent |
| **Build & Deploy** | `throw/**`, `pages/**`, `src/**` | HTML→DSL conversion | Independent |
| **PR Check** | Pull requests | Validate changes | Independent |
| **Dependency Update** | Schedule (weekly) | Update dependencies | Independent |

### Why No Conflicts?

Each workflow has **path-specific triggers**:

```yaml
# Sharpendabot triggers on F# sources
paths:
  - '.github/config/*.fs'
  - '.github/sources/*.fs'
  - 'bool'

# Deploy Website triggers on main/ changes
paths:
  - 'main/**'

# Build & Deploy triggers on throw/ changes  
paths:
  - 'throw/**'
  - 'pages/**'
```

### Execution Sequence Example

**Scenario: You edit `main/mkdocs.fs`**

1. **Push 1**: `main/mkdocs.fs` changes
   - Sharpendabot does NOT run (not in its paths)
   - Deploy Website runs → Deploys updated site to `website` branch

**Scenario: You edit `.github/config/deploy-website.fs`**

1. **Push 1**: `deploy-website.fs` changes
   - Sharpendabot runs (State 1) → Generates `deploy-website.yml` + creates `bool`
   - Deploy Website does NOT run yet (YML didn't exist at push time)

2. **Push 2**: `bool` exists
   - Sharpendabot runs (State 2) → Deletes `deploy-website.yml` + deletes `bool`
   - Deploy Website does NOT run (YML was deleted)

3. **Push 3**: Edit anything in `main/`
   - Sharpendabot runs (State 1) → Regenerates `deploy-website.yml` + creates `bool`
   - Deploy Website runs → Deploys site

4. **Push 4**: `bool` exists
   - Sharpendabot runs (State 2) → Deletes `deploy-website.yml` + deletes `bool`

### Key Insight

The two-phase system ensures:
- **YMLs exist transiently** - just long enough to be seen by GitHub Actions
- **F# sources are permanent** - the true source of truth
- **Deployments happen** - when content changes, not when workflows regenerate

### Dependency Chain

```
F# Source (.github/config/*.fs)
    ↓
Sharpendabot (generates YML)
    ↓
YML exists in repo
    ↓
Content push triggers deploy workflow
    ↓
Site deploys to Pages branch
```

No circular dependencies. Each layer is independent.

---

## GitHub Pages Behavior Note

**Deleting workflows does NOT unpublish your site.** GitHub Pages serves the last deployed snapshot indefinitely until you explicitly disable Pages or push a new deployment.

Because Pages serves the last deployed snapshot, you can safely delete all workflow YAMLs, regenerate them from F# sources, and your site stays live the entire time. No downtime.

### Key Points:

- ✔️ The site stays live even if you delete all workflows
- ✔️ The site stays live even if you delete `.github/workflows/*`
- ✔️ The site stays live even if you delete `.nojekyll` from source
- ✔️ The site stays live even if you delete the entire source branch

### Why This Works

GitHub Pages deployment is a **snapshot** model:

1. When a workflow deploys a site, GitHub stores the built static files in the **Pages environment**
2. The snapshot is **completely independent** of your repo, branches, workflows, or source files
3. Once deployed, the site is "frozen" until you deploy again

### What Actually Unpublishes a Site?

Only these actions:
- ❌ Disabling GitHub Pages in Settings
- ❌ Changing the Pages source branch/folder
- ❌ Pushing a new deployment that overwrites the old one
- ❌ Deleting the Pages environment (rare, manual action)

### Why This Is Useful

You can safely:
1. Delete all workflows
2. Restructure your repo
3. Move to a multi-branch architecture
4. Regenerate everything from F# sources

And during this entire transition:
- ✔️ Your existing site stays live
- ✔️ No downtime
- ✔️ No broken domains
- ✔️ No SSL issues

This is exactly what sharpendabot leverages - the YMLs can be deleted and regenerated at will, while your deployed sites remain live.

---

# Module Specifications

### 1. `.github-repo-config.fs`

**Purpose:** Central F# configuration mapping source folders to target repositories.

**Interface:**
```fsharp
module RepoConfig

/// Repository target for a site
-type RepoTarget = {
    Owner: string
    Repo: string
    Branch: string
    Cname: string option          // Optional custom domain
    TokenName: string             // "GH_PAGES_TOKEN" or "GITHUB_TOKEN"
}

/// Mapping from source folder to deployment target
-type SiteMapping = {
    SourceFolder: string          // e.g., "main", "docs", "app", "blog", "pages"
    BuildCommand: string          // e.g., "mkdocs build", "dotnet fsi render"
    OutputFolder: string          // e.g., "site", "dist", "_site"
    Target: RepoTarget
}

/// All configured sites
let sites : SiteMapping list = [
    {
        SourceFolder = "main"
        BuildCommand = "mkdocs build -f {folder}/mkdocs.yml -d {folder}/site"
        OutputFolder = "site"
        Target = {
            Owner = "{{OWNER}}"
            Repo = "{{OWNER}}.github.io"
            Branch = "main"
            Cname = Some "shel.sh"
            TokenName = "GH_PAGES_TOKEN"
        }
    }
    {
        SourceFolder = "docs"
        BuildCommand = "mkdocs build -f {folder}/mkdocs.yml -d {folder}/site"
        OutputFolder = "site"
        Target = {
            Owner = "{{OWNER}}"
            Repo = "docs-pages"
            Branch = "main"
            Cname = Some "docs.shel.sh"
            TokenName = "GH_PAGES_TOKEN"
        }
    }
    // ... app, blog
]

/// Render deployment script for a site
let renderDeployScript (mapping: SiteMapping) : string = ...
```

**Replacement Rule:** The `.github-repo-config.fs` is evaluated by `GenerateConfig.fsx` to produce shell scripts in `.github/deploy-scripts/`.

### 2. `GenerateConfig.fsx`

**New Capabilities:**
- `generate` — Generate mkdocs.yml (existing)
- `sync` — Sync YML → .fs (existing)
- `deploy-scripts` — Generate shell scripts from `.github-repo-config.fs`
- `all` — Generate ALL configs + deploy scripts

**Interface additions:**
```fsharp
/// Generate deployment scripts for all configured sites
let generateDeployScripts() = ...

/// Entry point additions:
/// | Some "deploy-scripts" -> generateDeployScripts()
```

### 3. `src/generator/Program.fs`

**New CLI Commands:**

| Command | Description |
|---------|-------------|
| `deploy <folder>` | Build and push a single folder to its target repo |
| `deploy-all` | Build and push ALL configured folders |
| `config` | Show current repo mapping |

**Implementation:** Uses `LibGit2Sharp` or shell `git` commands with token-based remote URLs:
```bash
git remote add target https://x-access-token:${TOKEN}@github.com/OWNER/REPO.git
git push target HEAD:main --force
```

### 4. `.github/config/deploy-website.fs` 

Pushes built `site/` folder to TARGET repo using git with token auth.

**Workflow Steps:**
1. Checkout source repo
2. Setup .NET + Python
3. Generate configs (`dotnet fsi GenerateConfig.fsx all`)
4. Build site (`mkdocs build` or `dotnet fsi render`)
5. Push built output to target repo:
   ```bash
   cd site/
   git init
   git remote add target https://x-access-token:${TOKEN}@github.com/${TARGET_OWNER}/${TARGET_REPO}.git
   git add .
   git commit -m "Deploy ${SITE_NAME} from ${GITHUB_SHA}"
   git push target HEAD:${TARGET_BRANCH} --force
   ```
6. Optionally configure CNAME if specified

### 5. Sharpendabot (`sharpendabot.fs` / `sharpendabot.yml`) 

Two-phase state machine, inline generation, pattern-based generation, essential vs transient workflow classification.

After State 2 cleanup, if `deploy-state` marker exists, trigger deployment workflow generation.

**Trigger Paths:**
```yaml
paths:
  - '.github/config/*.fs'
  - '.github/sources/*.fs'
  - '.github-repo-config.fs'
  - 'bool'
  - 'deploy-state'
```

---

## Deployment Flow Specification

### Manual Deploy (Local)

```bash
# 1. Configure your repos
cp .github-repo-config.fs.example .github-repo-config.fs
# Edit: replace {{OWNER}} with your GitHub username/org

# 2. Set token
export GH_PAGES_TOKEN=ghp_xxxxxxxx

# 3. Deploy specific site
dotnet run --project src/generator -- deploy main

# 4. Deploy all sites
dotnet run --project src/generator -- deploy-all
```

### Automated Deploy (GitHub Actions)

```
Push to main/  ──►  Build & Deploy workflow  ──►  Build site/  ──►  Push to target repo
                                    ▲
                                    │
                         Sharpendabot generates this workflow
                         from .github/config/build-and-deploy.fs
```

### Full Orchestration Sequence

```
Developer push
      │
      ▼
┌─────────────────┐
│ Sharpendabot    │  (State 1: generates deploy-*.yml + deploy-state marker)
│ (if .fs changed)│
└─────────────────┘
      │
      ▼
┌─────────────────┐
│ Build & Deploy  │  (triggers on content change)
│ Workflow        │  1. Generate configs
│                 │  2. Build all sites (mkdocs / F# render)
│                 │  3. Push each built site/ to target repo via git+token
└─────────────────┘
      │
      ▼
┌─────────────────┐
│ Sharpendabot    │  (State 2: cleans up generated YMLs + deploy-state)
│ (on deploy-state│
│  completion)    │
└─────────────────┘
```
