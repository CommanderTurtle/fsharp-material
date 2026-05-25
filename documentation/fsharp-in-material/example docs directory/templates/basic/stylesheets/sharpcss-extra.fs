
module Stylesheets.Extra

/// <summary>
/// Extra CSS styles for the documentation site
/// Generated from sharpcss.fs → extra.css
/// </summary>
let content = """/* Extra styles for Material for MkDocs */

/* Custom card styling */
.md-card {
    border: 1px solid var(--md-default-fg-color--lightest);
    border-radius: 0.5rem;
    padding: 1rem;
    margin: 1rem 0;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    transition: box-shadow 0.2s ease, transform 0.2s ease;
}

.md-card:hover {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
}

/* Feature grid styling */
.feature-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 1.5rem;
    margin: 2rem 0;
}

.feature-item {
    display: flex;
    gap: 1rem;
    padding: 1rem;
    border-radius: 0.5rem;
    background: var(--md-code-bg-color);
}

.feature-icon {
    font-size: 2rem;
    flex-shrink: 0;
}

/* Hero section styling */
.hero {
    text-align: center;
    padding: 4rem 2rem;
    background: linear-gradient(135deg, var(--md-primary-fg-color) 0%, var(--md-accent-fg-color) 100%);
    color: white;
    border-radius: 1rem;
    margin: 2rem 0;
}

.hero h1 {
    font-size: 3rem;
    margin: 0 0 1rem 0;
    font-weight: 800;
}

.hero p {
    font-size: 1.25rem;
    margin: 0 0 2rem 0;
    opacity: 0.9;
}

/* Button styling */
.md-button {
    display: inline-block;
    padding: 0.75rem 1.5rem;
    border-radius: 0.5rem;
    font-weight: 500;
    text-decoration: none;
    transition: all 0.2s ease;
}

.md-button--primary {
    background: var(--md-primary-fg-color);
    color: white;
}

.md-button--primary:hover {
    background: var(--md-primary-fg-color--dark);
}

.md-button--secondary {
    background: var(--md-default-fg-color--lightest);
    color: var(--md-default-fg-color);
}

.md-button--secondary:hover {
    background: var(--md-default-fg-color--lighter);
}

/* Badge styling */
.badge {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.badge--info {
    background: var(--md-code-hl-color);
    color: var(--md-code-fg-color);
}

.badge--success {
    background: #22c55e;
    color: white;
}

.badge--warning {
    background: #f59e0b;
    color: white;
}

.badge--danger {
    background: #ef4444;
    color: white;
}

/* Admonition enhancements */
.admonition {
    border-left: 4px solid;
    border-radius: 0 0.5rem 0.5rem 0;
    padding: 1rem;
    margin: 1rem 0;
}

/* Code block enhancements */
.md-typeset pre {
    border-radius: 0.5rem;
}

/* Table enhancements */
.md-typeset table {
    border-radius: 0.5rem;
    overflow: hidden;
}

/* Tab enhancements */
.md-tabs {
    border-radius: 0.5rem 0.5rem 0 0;
}

/* Responsive adjustments */
@media (max-width: 768px) {
    .hero {
        padding: 2rem 1rem;
    }
    
    .hero h1 {
        font-size: 2rem;
    }
    
    .feature-grid {
        grid-template-columns: 1fr;
    }
}
"""

let render() = content


