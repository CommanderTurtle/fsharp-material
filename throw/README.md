# /throw/ Directory

The `/throw/` directory is for **HTML to Giraffe DSL conversion**.

## How It Works

1. Drop any `.html` file into `/throw/`
2. Push to GitHub (or trigger the `Build and Deploy` workflow)
3. The Giraffe DSL Engine converts each HTML file:
   ```
   /throw/my-page.html -> /throw/my-page/index.fs
   ```

## Manual Movement

The converted files stay in `/throw/` for you to **manually move** to the target site directory:

```bash
# Converted file exists at:
/throw/my-page/index.fs

# Move to target site (e.g., app/docs/widgets/):
cp -r throw/my-page/ app/docs/widgets/
# Results in: app/docs/widgets/my-page/index.fs
```

## Rendering to HTML

When any deploy workflow runs, it automatically renders all `index.fs` files to `index.html`:

```
app/docs/widgets/my-page/index.fs -> app/docs/widgets/my-page/index.html
```

**Priority Note:** GitHub Pages serves `index.html` before `index.md`. If both exist in the same directory, `index.html` takes precedence. This means you can have both:
- `index.fs` (Giraffe DSL) -> rendered to `index.html`
- `indexmd.fs` (Markdown generator) -> rendered to `index.md`

And the HTML page will be served.

## What Gets Converted

- **Element nodes**: Proper Giraffe DSL functions (`div`, `span`, `p`, etc.)
- **Attributes**: Mapped to Giraffe attribute functions (`_class`, `_id`, `attr "data-foo"`, etc.)
- **Text nodes**: `str` or `rawText` nodes
- **`<script>` content**: Triple-quoted strings (no escaping needed for `"""`)
- **`<style>` content**: Triple-quoted strings (no escaping needed for `"""`)

## Files in this Directory

- `README.md` — This file
- `*.html` — Any HTML files you drop here for conversion
- After conversion: `*/index.fs` — Converted Giraffe DSL modules
