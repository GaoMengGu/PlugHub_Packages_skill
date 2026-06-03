# Trae Entry Point

Use `skills.json` to discover installable paths, then load one skill directory:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

In the IDE agent flow, pin `SKILL.md` as the task instruction file and open `references/plughub-package-contract.md` before editing package manifests.

Run package validation with the bundled C# project:

```powershell
dotnet run --project <skill-dir>\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```
