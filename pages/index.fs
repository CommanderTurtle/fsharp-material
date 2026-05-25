
module Pages.Home

open Giraffe.ViewEngine

let styles = """
:root {
    --primary: #6366f1;
    --primary-dark: #4f46e5;
    --bg: #f8fafc;
    --text: #1e293b;
    --text-muted: #64748b;
}

* { margin: 0; padding: 0; box-sizing: border-box; }

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    color: var(--text);
    line-height: 1.6;
    background: var(--bg);
}

.hero {
    text-align: center;
    padding: 5rem 2rem;
    background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
    color: white;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

.hero h1 {
    font-size: 3.5rem;
    font-weight: 800;
    margin-bottom: 1rem;
}

.hero p {
    font-size: 1.25rem;
    opacity: 0.9;
    max-width: 600px;
    margin-bottom: 2rem;
}

.btn {
    display: inline-block;
    padding: 1rem 2rem;
    background: white;
    color: var(--primary);
    text-decoration: none;
    border-radius: 8px;
    font-weight: 600;
    transition: transform 0.2s, box-shadow 0.2s;
}

.btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

.features {
    padding: 4rem 2rem;
    max-width: 1200px;
    margin: 0 auto;
}

.features h2 {
    text-align: center;
    font-size: 2.5rem;
    margin-bottom: 3rem;
}

.feature-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 2rem;
}

.feature-card {
    background: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0,0,0,0.05);
}

.feature-icon {
    font-size: 2.5rem;
    margin-bottom: 1rem;
}

footer {
    background: var(--text);
    color: white;
    text-align: center;
    padding: 2rem;
}
"""

let render() =
    html [] [
        head [] [
            meta [_charset "UTF-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1.0"]
            title [] [ str "Html2Giraffe - F# to GitHub Pages" ]
            style [] [ rawText styles ]
        ]
        body [] [
            section [_class "hero"] [
                h1 [] [ str "Html2Giraffe" ]
                p [] [ str "Write type-safe F# pages. Deploy to GitHub Pages automatically." ]
                a [_class "btn"; _href "/docs/"] [ str "Get Started" ]
            ]
            section [_class "features"] [
                h2 [] [ str "Why F# for Web?" ]
                div [_class "feature-grid"] [
                    div [_class "feature-card"] [
                        div [_class "feature-icon"] [ str "🔒" ]
                        h3 [] [ str "Type Safe" ]
                        p [] [ str "Catch errors at compile time, not runtime." ]
                    ]
                    div [_class "feature-card"] [
                        div [_class "feature-icon"] [ str "⚡" ]
                        h3 [] [ str "Fast" ]
                        p [] [ str "Optimized output with inline everything." ]
                    ]
                    div [_class "feature-card"] [
                        div [_class "feature-icon"] [ str "🎨" ]
                        h3 [] [ str "Beautiful" ]
                        p [] [ str "Material Design with MkDocs integration." ]
                    ]
                ]
            ]
            footer [] [
                p [] [ str "Built with F# and Giraffe.ViewEngine" ]
            ]
        ]
    ]
    |> RenderView.AsString.htmlDocument


