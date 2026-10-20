# Lorea documentation redesign

Tool: DeepSeek Harness (`deepseek-flash`). Worked directly on `main`; `lorea.tokens.css`, `Content/*/*.api.json`, `Lorea.Web.csproj`, `packages/`, `nuget.config`, `Directory.*`, `global.json` and `.github/` are untouched, and no tag was moved.

## What changed, by area

### 1. Site shell and the shared page frame

- A new masthead: brand + wordmark, a small "Documentation" label, the primary `Packages` / `Map` links, theme toggle and language switch. It is sticky and hairline-bordered. A matching footer ends every page with the same links and the site note.
- One container for every page (`min(100% - 2 × gutter, 80rem)`), so the home, package, topic and not-found pages share the same left and right edges.
- A documentation frame with three columns: a side index (sticky, with its own `max-height` scroll), the reading column, and an "On this page" column on wide screens. From 64 rem to 78 rem the third column is dropped; below 64 rem all side columns are replaced by phone navigation under the page head.
- Shared page parts: breadcrumbs, numbered section headings with one anchor scheme, previous/next steps, an "On this page" list whose current item follows the reading position, and a phone navigation block.
- New components: `PageCrumbs`, `PageSteps`, `DocSection`, `DocAside`, `PackageIndex`, `TopicIndex`, `MobileDocNav`, `SvgText`; shared list/search logic in `Data/LibraryBrowse.cs` and group/count text in `Data/TextLookup.cs`.
- `wwwroot/js/lorea-pages.js`: the table-of-contents scroll spy, horizontal code blocks made keyboard-reachable, and the phone guide strip scrolled to the current guide. `wwwroot/js/lorea-culture.js` gained a `wideScreen()` helper used for the map default.
- Persian type rules are applied by language in CSS (no letter-spacing or tight leading on Persian headings/labels); the same structure is used in both languages.

Changed files: `Layout/MainLayout.razor`, `Layout/MainLayout.razor.cs`, `_Imports.cs`, `wwwroot/css/layers/lorea.base.css`, `wwwroot/css/layers/parts/lorea.layout.css`, `wwwroot/css/layers/parts/lorea.code.css`, `wwwroot/css/layers/parts/lorea.motion.css`, `wwwroot/js/lorea-culture.js`, `wwwroot/js/lorea-pages.js`, and the new components and helpers listed above.

### 2. Home page

- An editorial opening: an eyebrow with an accent rule, a large balanced title, a two-sentence deck, the primary "See the packages" action and a text link to the map.
- A searchable catalog of every package (24 today), grouped exactly as the map groups them: monospace package name, title, one-line summary and guide count. Filtering is live and the count updates; there is an empty state.
- `StackMap` simplified to one view: search, "show/hide add-ons", fit-to-width / actual-size, clear, a technical-drawing background of row hairlines and level labels, direct/indirect wires, and a selection panel with depends-on / built-on chips and an "open the package" action. Nothing is selected at first.
- The map defaults to fitted on wide screens and to its actual size on narrow screens, where it scrolls inside its own frame and a hint says so; it never scrolls the page sideways.
- The other half of the opening band is a group index (name and package count per group) that jumps to the matching catalog group, so the wide layout carries navigation instead of blank space. It is hidden on phones.

Changed files: `Pages/Home.razor`, `Pages/Home.razor.cs`, `Components/StackMap.razor`, `Components/StackMap.razor.cs`, `Components/Map/SvgText.cs`, `wwwroot/css/layers/parts/lorea.home.css`, `wwwroot/css/layers/parts/lorea.map.css`, `wwwroot/css/layers/parts/lorea.layout.css`, `Locales/en-US.json`, `Locales/fa-IR.json`.

### 3. Package page

- One fixed structure in both languages, always present, same order and names: Overview → Install and setup → Quick start → Guides → API reference. Each section is numbered and has an empty state when the data has nothing.
- A spec-sheet head: package title, deck, and Package / Group / Guides values on a hairline row.
- Overview as a short list; the install command with copy; the dependency path; samples; a guides index with numbers; API types grouped by guide and linked to it.
- The side index lists every package by group with a filter (names and titles), marks the current package, and scrolls inside itself without a sideways scrollbar; long package names fit on one line. The third column is deliberately short: "On this page" and the way back to all packages. Related packages (Relies on / Built on it / Similar packages) close the page. "All packages" is also one click from the page end.

Changed files: `Pages/Package.razor`, `Pages/Package.razor.cs`, `Components/PackageIndex.razor(.cs)`, `Components/DocSection.razor(.cs)`, `Components/DocAside.razor(.cs)`, `Components/MobileDocNav.razor(.cs)`, `Components/RelatedPackages.razor`, `Components/PackageLinks.razor`, `wwwroot/css/layers/parts/lorea.package.css`, `wwwroot/css/layers/parts/lorea.path.css`.

### 4. Topic page

- One fixed structure: At a glance → Examples → Full explanation → API at a glance, then previous/next and related packages.
- At a glance is a hairline grid of short cards; examples are title + optional note + code/output; details are an accordion; the API list is an accordion of type signatures with member tables, obsolete notes and per-member text.
- The side index lists the package's guides with the current one marked; the third column carries "On this page" and the back link, and the page closes with previous/next and related packages. On phones the guides become a horizontal strip under the head and "On this page" / "Related packages" become folds there, so navigation and cross-links are above the long content. Targets are at least 40 px.

Changed files: `Pages/Topic.razor`, `Pages/Topic.razor.cs`, `Components/TopicIndex.razor(.cs)`, `wwwroot/css/layers/parts/lorea.topic.css`.

### 5. Not found and missing pages

- One 404 page: breadcrumbs, the heading, a back action and the full package list, centred in the page column so wide screens do not leave the content stranded on one side. A missing package or topic uses the same page head, a short message, back links and the package list, so those routes are not dead ends.

Changed files: `Pages/NotFound.razor`, `Pages/NotFound.razor.cs`, plus the missing branches in `Pages/Package.razor` and `Pages/Topic.razor`.

### 6. Language

- The layout reads the saved language synchronously before its first render (instead of after an async JS call), and a language change rebuilds the page subtree. No component can keep the previous language's text; this also fixed the 404 page rendering in English on a Persian load.
- All new UI strings exist in both locale files: navigation labels, catalog/search/count/empty strings, map fit/swipe strings, related-package strings, empty states for every section.

Changed files: `Layout/MainLayout.razor`, `Layout/MainLayout.razor.cs`, `Locales/en-US.json`, `Locales/fa-IR.json`.

### 7. Contrast, focus and motion

- Page-head labels no longer inherit the library's 0.7 opacity; the current package title in the side index, code-language labels, muted texts, breadcrumbs, inline code and TOC items all use the existing grey/link tokens and pass AA in both themes.
- Inline code is now a neutral chip (chrome background, hairline, ink text) instead of accent-coloured text; the accent stays for links, marks, focus and the map.
- Code blocks that scroll sideways get `tabindex` from JS, so keyboards can reach them.
- The only retained animations are the dashed flow on indirect wires and the loading mark; both stop under `prefers-reduced-motion`.

Changed files: `wwwroot/css/layers/parts/lorea.layout.css`, `lorea.code.css`, `lorea.topic.css`, `lorea.map.css`, `lorea.base.css`, `wwwroot/js/lorea-pages.js`.

## How and what I checked

Local: `dotnet build apps/Lorea/Lorea.Web` (succeeded) and `dotnet run --project apps/Lorea/Lorea.Web`; then headless Google Chrome through Playwright against `http://localhost:5000`. Published: https://radvs.github.io/lorea-lab/ after each push.

| Page | Languages | Themes | Widths |
| --- | --- | --- | --- |
| `/` (home) | fa-IR, en-US | light, dark | 1280×900, 390×844 |
| `/packages/os-core` | fa-IR, en-US | light, dark | 1280×900, 390×844, 1024×900 |
| `/packages/os-core/config` | fa-IR, en-US | light, dark | 1280×900, 390×844, 1024×900 |
| `/not-found` | fa-IR, en-US | light, dark | 1280×900, 390×844 |
| `/packages/nope` (missing package) | fa-IR, en-US | light, dark | 1280×900, 390×844 |
| `/packages/os-core/nope` (missing topic) | fa-IR, en-US | light, dark | 1280×900, 390×844 |

Also spot-checked at 1920×1025 in fa-IR dark after the wide-screen feedback: home (group index and catalog alignment), `/packages/os-web-identity` (rail fit, no sideways scrollbar, long names on one line) and a missing topic (centred block).

Method: full-page and viewport screenshots for the main states; DOM measurements (`getBoundingClientRect`, `scrollWidth`/`clientWidth`, grid `computedStyle`) for widths, sticky panels and scroll; scripted interaction checks; `axe-core` with the `wcag2a`, `wcag2aa` and `wcag21aa` rule sets on 48 states (home, package, topic, missing package, 404 and missing topic × 2 languages × 2 themes × 2 widths).

Results:

- `axe-core`: **0 violations in 48 of 48 states** (after the contrast fixes).
- Home: 24 rows; filter "blazor" → 2 rows and "2 packages"; clearing restores 24. The group index lists all 6 groups; clicking one lands its catalog group 72 px from the top.
- Map: selecting `OS.Core` fills the selection panel, and the diagram has 29 direct and 22 indirect wire paths; the fit toggle switches `data-fit` both ways; fit is on by default at 1280 and off at 390 with the swipe hint visible.
- Package rail: filtering "efcore" leaves the 5 EFCore packages; an unmatched query shows the empty message; clearing restores 24. The rail is 800 px tall at 900 px viewport, scrolls inside itself, its last item is fully visible after scrolling it, and `scrollWidth === clientWidth` at 1920 (no sideways scrollbar) with the longest name on one line.
- Side column: at 1920 and 1280 it holds the TOC (about 250 px of content) and the back/all-packages link, and the related block sits at the page foot; the column is intentionally short rather than filled with duplicated lists. At 1024 the column is hidden and the page is two columns; at 390 the related fold is under the page head.
- Previous/next and the table of contents: a TOC link lands the section 72 px from the top (clear of the sticky header) and the TOC marks it `aria-current="location"`; the TOC follows scrolling.
- Language: switching on a topic page changes `html lang`/`dir`, the h1, the guide rail, the TOC labels and the related labels; switching back restores Persian.
- Theme: the toggle changes `data-theme` without a reload.
- Phones: `scrollWidth === clientWidth` on home, package, topic, 404 and missing pages; header brand/nav/language/theme buttons, map buttons, phone nav links, guide chips and fold summaries are 40 px or taller; the phone nav sits directly under the page head and the related folds are there.
- Language separation: no Arabic-script characters outside code/pre and LTR-isolated text on any English page; sampled English UI strings do not appear on Persian pages.
- GitHub Actions: every push in this work ran the Preview workflow; the runs for `975d39a`, `f7b0671`, `20e7c87`, `9109246`, `90872c7`, `182a542`, `9dcaf02`, `103bbee` and `0b71f4f` completed successfully. The deployed site was loaded afterwards with no failed asset requests: 24 catalog rows, correct RTL/LTR direction, no horizontal scroll, and the 404 in Persian. `/not-found` itself returns HTTP 404 from Pages (the `404.html` fallback), which is expected.

## Commits

- `975d39a` — Rebuild the documentation UI around one page structure
- `f7b0671` — Fix language consistency, contrast and small screen details
- `4ab4796` — Line the 404 marker up with the page head
- `b06aea2` — Tighten two Persian sentences
- `182a542` — Use the wide screen instead of leaving it empty (group index in the opening band, centred error pages, rail fit)
- `9dcaf02` — Fill the side column instead of leaving it blank (reverted by `103bbee`: it duplicated the guide list, the head facts and the related block and read as clutter)
- `103bbee` — Revert the side-column fill
- `0b71f4f` — Keep the side column short and a little narrower
- The report file was added after those; its hash is the commit that carries this file (`git log`).

## Not done, uncertain, or not verified

- At the very end of a long page the tall side index slides up with its grid column (about 61 px at a 900 px viewport, because the footer sits below the grid). It keeps its own scrollbar and every item stays reachable; I left the standard sticky behaviour instead of shrinking the panel for that last screen.
- The third column is intentionally short ("On this page" plus the way out). Filling it with the guide list, the head facts and the related packages made the page feel crowded (the owner's review of `9dcaf02`), so that pass was reverted in `103bbee`; the column is now 12.5 rem wide instead of 14 rem.
- Changing the language rebuilds the page subtree, so the reading position returns to the top. That was deliberate to guarantee the whole page switches language.
- The dependency map has no separate phone layout: it stays at readable size and scrolls inside its frame, with a hint and a fit-to-width toggle. When fitted on a phone its labels become small.
- Two syntax colours (types, numbers) were moved to the site's link/ink tokens to pass AA; the other syntax colours are OS.Blazor's and unchanged.
- Only headless Chrome was used. Firefox, Safari and real touch devices were not tested.
- `lorea.tokens.css` was not edited at all: no colour token, value or mapping changed.
