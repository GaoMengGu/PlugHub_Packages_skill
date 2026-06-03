# Agent Compatibility

The skill is intentionally Markdown-first so Codex, Hermes, OpenClaw, Trae, CodeBuddy, and similar coding agents can use the same folder.

## Portable Rules

- Load `SKILL.md` first, then load only the reference file needed for the task.
- Use the agent's native file read, patch, shell, and validation tools; do not assume Codex-specific tool names.
- Keep commands explicit and copyable. Prefer PowerShell examples on Windows because PlugHub/Revit package work is Windows-oriented.
- Preserve the root `SKILL.md` filename and YAML frontmatter with only `name` and `description`.
- Keep references one level below the skill root in `references/`.
- Keep helper scripts in `scripts/` and call them by path instead of retyping their logic.

## Platform Notes

- Codex: use the current workspace, make scoped patches, and report validation commands and outcomes.
- Hermes: use `skills_list` / `skill_view` if available; otherwise read the skill folder directly. Load `references/plughub-package-contract.md` before writing manifests.
- OpenClaw: treat the skill as repository-local instructions. Run shell validation from the package root and avoid hidden global assumptions.
- Trae and CodeBuddy: read `SKILL.md` plus the required reference file, then apply normal IDE agent edit and terminal workflows.

## Cross-Agent Output Contract

When finishing a package task, report:

- package root and manifest path
- created or changed project files
- changed manifest module and feature ids
- build command and result
- manifest validator command and result
- whether Revit 2020 runtime smoke testing was performed or remains pending

Do not claim PlugHub runtime load success from static validation alone.
