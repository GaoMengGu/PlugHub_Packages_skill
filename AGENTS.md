# Agent Instructions

This repository exposes two installable skill paths:

- Chinese: `plughub-package-authoring`
- English: `plughub-package-authoring-en`

If a user gives only this repository URL, read `skills.json` and use the current agent's native skill installer, repository import, or subpath loading flow. Do not assume a user-specific local path.

Compatibility entry points:

- Codex: load `SKILL.md`, `agents/openai.yaml`, and the referenced files in the chosen skill directory.
- Hermes: use native skill discovery when available; otherwise load the selected skill directory as repository-local instructions.
- OpenClaw: treat the selected skill directory as task instructions and run validation from the package root.
- Trae and CodeBuddy: load `SKILL.md` plus the needed `references/*.md` files through the IDE agent workflow.

When using the skill to implement a PlugHub plugin package, do not stop after writing C# code. The output must include a discoverable manifest, package-relative payloads, build registration, C# validation evidence, compile evidence, and a Revit 2020 runtime smoke-test status.
