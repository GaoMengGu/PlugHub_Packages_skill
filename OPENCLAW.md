# OpenClaw Entry Point

Read root instruction files first:

1. `IDENTITY.md`
2. `SOUL.md`
3. `AGENTS.md`

Use `skills.json` to discover installable paths, then load one skill directory:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

Treat the selected directory as repository-local task instructions. Read `SKILL.md`, then load the referenced files only when the task needs them.

Run package validation with the bundled C# project:

```powershell
dotnet run --project <skill-dir>\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```
