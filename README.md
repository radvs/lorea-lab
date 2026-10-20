# lorea-lab

Lorea is a bilingual (Persian, RTL / English, LTR) documentation site built with Blazor WebAssembly. This repository is a self-contained test bench for its design and UI.

Live preview: https://radvs.github.io/lorea-lab/ (rebuilt on every push to `main`).

## Run locally

```
dotnet watch --project apps/Lorea/Lorea.Web
```

Needs the .NET SDK from `global.json`. The UI component library comes from the prebuilt packages in `packages/` (see `nuget.config`).

## Layout

- `apps/Lorea/Lorea.Web` the site: `Pages`, `Components`, `Layout`, `Data`, `Locales` (UI strings), `Content` (documentation data), `wwwroot` (CSS, JS, fonts, images).
- `packages/` prebuilt `.nupkg` files. Read-only: do not edit, add or remove.
- `.github/workflows/preview.yml` builds and deploys the preview.

## Ground rules

- Change only files under `apps/Lorea/Lorea.Web`, except `Content/*/*.api.json` (generated, read-only) and `Lorea.Web.csproj` (keep as is).
- The site code knows nothing about any specific library: every library's text and API come from `Content/{slug}/` data. Never hard-code library names, types or texts in C#, Razor, CSS or JS.
- CSS and JS are split by concern in layers (`wwwroot/css/layers`, `wwwroot/js`). `index.html` stays clean: file references only.
- Persian texts must read as plain, natural Persian, not as literal translations. Latin identifiers inside Persian text are isolated (`dir="ltr"` / `<bdi>`).
