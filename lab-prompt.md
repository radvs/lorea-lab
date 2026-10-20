# Task: redesign the Lorea documentation site (UI/UX)

You work in the public GitHub repository `radvs/lorea-lab`. Read its `README.md` first and obey its ground rules. The site is rebuilt and published on every push to `main` at https://radvs.github.io/lorea-lab/ . The owner reviews your result there.

Lorea is a bilingual documentation site (Persian RTL and English LTR), Blazor WebAssembly, dark and light themes. It is data-driven: every library's text and API come from `apps/Lorea/Lorea.Web/Content/{slug}/` JSON. The site code must stay ignorant of any specific library.

## Goal

Make the whole site look and feel like a professional, tidy documentation product: clear hierarchy, consistent structure on every page, easy to move between 30+ libraries, no dead ends. The design must be creative, current and modern: apply real UI/UX craft (visual hierarchy, rhythm and spacing scale, typography, motion with purpose, states, accessibility). Your own design judgement is wanted; do not copy another documentation site.

**Hard rule: do not change the color system.** Keep the palette, the token names and values in `wwwroot/css/layers/lorea.tokens.css`, and the color structure (neutral greys plus one blue accent used only for emphasis, light and dark themes) exactly as they are. Only use the existing tokens; add no new colors, gradients or tints.

**It must not look like generic AI-generated design.** Avoid: purple/blue gradient heroes, glassmorphism and glow effects, emoji or generic icon-in-circle feature cards, identical rounded cards in endless grids, centered marketing-style hero text with badges, decorative blobs, meaningless animation. Prefer a distinct, editorial, content-first identity that feels hand-designed for developer documentation.

## What is wrong today (fix all of it)

1. Untidy layout: inconsistent spacing, widths and alignment between the home page, package page and topic page.
2. Dead ends: some pages offer no way back, no related or dependent packages, no previous/next. Every page must have a clear path out (breadcrumb, related packages, previous/next topic, back to package list).
3. No fixed structure: every package page must show the same sections, with the same names and order, in both languages.
4. Home page: a searchable list of all packages with a short description, plus the interactive dependency map (StackMap). Nothing is selected at first; selecting a package draws its direct dependencies as solid lines and indirect ones as dashed lines. It must still work with 20+ more packages (search, fit to width).
5. Topic page: short "at a glance" cards, then samples, then expandable details, then the API list, then previous/next and related packages. A third column "On this page" on wide screens. Keep text blocks short; no walls of text.
6. Contrast: breadcrumbs, inline code, small table of contents items and muted texts must pass WCAG AA in both themes.
7. Sticky side panels must scroll inside themselves (max-height) and never cut off their last item.
8. Mobile (390 px wide): no horizontal page scroll, related packages and navigation near the top, tap targets at least 40 px.

## Persian text rules (important)

- Plain, natural, conversational technical Persian. Never a literal translation of English.
- Use half-spaces correctly (نیم‌فاصله). No em-dashes.
- Latin identifiers and code inside Persian text are isolated (`dir="ltr"` or `<bdi>`), and a sentence must not end on a code chip (punctuation detaches in RTL). Never glue Persian suffixes to Latin words ("attributeهای" is wrong).
- The English site shows no Persian and the Persian site shows no English prose.
- Both languages must have exactly the same structure.

## Code rules

- Change only files under `apps/Lorea/Lorea.Web`, plus your report file in `reports/`. Do not touch `Content/*/*.api.json`, `Lorea.Web.csproj`, `packages/`, `nuget.config`, `Directory.*`, `global.json` or `.github/`.
- Do not hard-code any library name, type or documentation text in C#, Razor, CSS or JS. UI strings live in `Locales/fa-IR.json` and `Locales/en-US.json` (add every new key to both).
- Prefer components from the OS.Blazor package where one exists (page head, inline text, code block, accordion, tabs and so on). Inspect the package before writing your own.
- CSS is layered and split by concern in `wwwroot/css/layers/` (tokens, base, parts). JS goes in `wwwroot/js/`. `index.html` stays clean: no inline CSS or JS.
- Modern C# 14, shared `using` lines only in `_Imports.cs`, no XML doc comments, no duplicated code.
- Keep the existing file and folder structure; improve incrementally instead of rewriting everything.
- Do not add dependencies or external CDN assets. Fonts are already self-hosted.

## How to work

1. Read the existing pages, components and CSS before changing anything.
2. Work on `main` in small, descriptive commits. Push after each meaningful step. Do not force-push and do not rewrite history.
3. After every push, wait for the "Preview" workflow in GitHub Actions. If it fails, read the log and fix it before doing anything else.
4. Check the published site in both languages (the language is stored in `localStorage` under `lorea.culture`, values `fa-IR` and `en-US`), in dark and light theme, at 1280 px and 390 px wide, and fix what you see. Test the home page, one package page and one topic page (for example `/packages/os-core` and `/packages/os-core/config`).
5. If you cannot build or run something, say so plainly. Never claim a check passed that you did not run.

## Restore point

The tag `stable` marks the last accepted state. Never move, delete or re-create any tag, never force-push, never rewrite history.

## Final report (required)

Write it as a Markdown file `reports/<your-tool-name>.md` (create the folder), commit and push it. It must contain: every change you made grouped by page or area with the changed file paths; which pages you checked, in which language, theme and width, and how; the commit hashes; anything undone, uncertain or that you could not verify. Be factual: the owner will have the work reviewed from this file, so claim only what you actually did and ran.
