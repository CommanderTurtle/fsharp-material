
module App.Docs.Index

/// <summary>
/// App site homepage (app.shel.sh)
/// Generates: index.md
/// </summary>
let content = """---
title: Dashboard
---

# shel.sh App

Welcome to the application dashboard for **shel.sh**.

## Overview

This site is deployed from the `app/` folder to **app.shel.sh**.

## Features

- Real-time monitoring
- Configuration management
- User settings
- Analytics dashboard

## Quick Links

| Section | Description |
|---------|-------------|
| Features | [View features](features.md) |
| Settings | [Configure app](settings.md) |

---

*App dashboard built with MkDocs Material*
"""

let render() = content


