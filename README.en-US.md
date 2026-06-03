# PlugHub Packages Skill

This repository ships skill resources for authoring PlugHub external plugin packages. It includes Chinese and English variants that guide agents to create Revit 2020 packages PlugHub can discover, install, and load.

## Installable Paths

| Language | Skill name | Path |
| --- | --- | --- |
| Chinese | `plughub-package-authoring` | `plughub-package-authoring` |
| English | `plughub-package-authoring-en` | `plughub-package-authoring-en` |

When an agent receives this repository URL, it should read [skills.json](skills.json) first and then use its native skill installer, repository import, or subpath loading mechanism for the listed paths. Do not assume a machine-specific local install directory.

## Scope

The skill is for creating, reviewing, or repairing PlugHub packages. It covers:

- `package.json` / `*.package.json` manifests.
- Package-relative `dist/*.dll` payloads.
- `Autodesk.Revit.UI.IExternalCommand` command types.
- `PlugHub.Contracts` module descriptors.
- Icons, build scripts, package sources, and validation evidence.
- Portable use across Codex, Hermes, OpenClaw, Trae, CodeBuddy, and similar agents.

## Validation

Repository static validation is implemented in C#:

```powershell
dotnet build src\PlugHub.PackagesSkill.StaticValidation\PlugHub.PackagesSkill.StaticValidation.csproj -c Release
dotnet run --project src\PlugHub.PackagesSkill.StaticValidation\PlugHub.PackagesSkill.StaticValidation.csproj -- .
```

The PlugHub package validator bundled with each skill is also C#:

```powershell
dotnet run --project plughub-package-authoring-en\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```

Runtime behavior still requires a smoke test in Windows + Revit 2020 with a safe test model. Static validation and compilation do not replace Revit runtime verification.
