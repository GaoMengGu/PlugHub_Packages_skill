# Agent Instructions

This repository is a cross-agent skill package for authoring PlugHub external plugin packages. It is intended to work in Codex, Hermes, OpenClaw, Trae, CodeBuddy, and similar coding agents.

## Load Order

1. Read `IDENTITY.md` to establish the outward role.
2. Read `SOUL.md` to establish durable behavior and non-negotiable boundaries.
3. Read `skills.json` to discover installable skill paths.
4. Load one skill directory:
   - Chinese: `plughub-package-authoring`
   - English: `plughub-package-authoring-en`
5. Read the selected `SKILL.md`, then load reference files only when needed:
   - `references/plughub-package-contract.md` before writing or changing manifests.
   - `references/authoring-playbook.md` before creating a new package.
   - `references/agent-compatibility.md` when adapting to a non-Codex agent.

## Hermes And OpenClaw

- Hermes: if native skill discovery is available, import the selected skill path from `skills.json`; otherwise treat the selected directory as repository-local instructions.
- OpenClaw: use `SOUL.md`, `IDENTITY.md`, and this `AGENTS.md` as the root instruction layer, then load the selected skill directory for task-specific details.
- In both environments, do not assume Codex-specific tool names, local install paths, or hidden machine state.

## PlugHub Package Contract

The target output is a complete PlugHub package:

- Manifest: `package.json` or `*.package.json` with `schemaVersion`, `version`, `revitVersions`, `frameworkVersionRange`, and non-empty `modules`.
- Runtime: Revit `2020`, `.NET Framework 4.8` / `net48`, and `PlugHub.Contracts`.
- Payloads: package-relative `dist/*.dll` command assemblies and `icons/*.png` feature icons.
- Commands: every user-triggered feature declares a full `commandType` implementing `Autodesk.Revit.UI.IExternalCommand`.
- Metadata: module and feature ids remain unique, stable, and aligned with Ribbon grouping.

## Completion Rule

Do not stop after writing C# code. Report manifest changes, payload paths, build integration, C# validation evidence, compile evidence, and Revit 2020 runtime smoke-test status. If Revit 2020 is unavailable, say that runtime verification is pending.
