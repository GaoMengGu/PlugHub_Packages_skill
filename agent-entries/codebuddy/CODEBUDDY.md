# CodeBuddy Entry Point

Use root `skills.json` to discover installable paths, then load one skill directory:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

Use the selected `SKILL.md` as the primary instruction file. Load `references/authoring-playbook.md` before scaffolding a new PlugHub package and `references/plughub-package-contract.md` before changing manifests.

Run package validation with the bundled C# project:

```powershell
dotnet run --project <skill-dir>\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```
