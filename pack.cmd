@echo off
rem Run from the OriginStack repo root: ..\lorea-lab\pack.cmd
set OUT=%~dp0packages
for %%P in (OS.Core OS.Criteria OS.Locale) do dotnet pack src\Core\%%P -c Release -o "%OUT%" || exit /b 1
dotnet pack src\Razor\OS.Blazor -c Release -o "%OUT%" || exit /b 1
