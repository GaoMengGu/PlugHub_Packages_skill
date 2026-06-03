# OpenClaw Entry Point

Default Chinese entry files:

1. `agent-entries/core/IDENTITY.md`
2. `agent-entries/core/SOUL.md`
3. `agent-entries/core/AGENTS.md`

English entry files:

1. `agent-entries/core/IDENTITY.en-US.md`
2. `agent-entries/core/SOUL.en-US.md`
3. `agent-entries/core/AGENTS.en-US.md`

Use root `skills.json` to discover installable paths, then load one skill directory:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

Treat the selected directory as repository-local task instructions. Read `SKILL.md`, then load the referenced files only when the task needs them.

Run package validation with the bundled C# project:

```powershell
dotnet run --project <skill-dir>\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```
