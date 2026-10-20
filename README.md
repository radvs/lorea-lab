# lorea-lab

Test bench for the Lorea docs site. It holds the Lorea source and the prebuilt OS.Blazor packages (`packages/`), nothing else.

- Run: `dotnet watch --project apps/Lorea/Lorea.Web`
- Edit anything under `apps/Lorea/Lorea.Web`. `Content/*/*.api.json` is generated in OriginStack and is read-only here.
- The only difference from OriginStack is `Lorea.Web.csproj`: `OS.Blazor` is a PackageReference instead of a ProjectReference. Do not copy that file back.
- Refresh `packages/`: from the OriginStack root run `..\lorea-lab\pack.cmd`.
