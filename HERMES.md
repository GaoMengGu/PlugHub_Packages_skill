# Hermes Entry Point

Default Chinese root instruction files:

1. `IDENTITY.md`
2. `SOUL.md`
3. `AGENTS.md`

English root instruction files:

1. `IDENTITY.en-US.md`
2. `SOUL.en-US.md`
3. `AGENTS.en-US.md`

Use `skills.json` to discover installable paths, then load one skill directory:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

Read `SKILL.md` first. Load `references/plughub-package-contract.md` before writing or changing manifests, `references/authoring-playbook.md` before creating a package, and `references/agent-compatibility.md` when adapting commands to Hermes tools.

Run package validation with the bundled C# project:

```powershell
dotnet run --project <skill-dir>\tools\PlugHub.PackageValidator\PlugHub.PackageValidator.csproj -- <package-root>
```
