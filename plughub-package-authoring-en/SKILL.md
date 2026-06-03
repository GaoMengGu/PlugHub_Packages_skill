---
name: plughub-package-authoring-en
description: Use when creating, reviewing, or repairing Revit 2020 PlugHub external plugin packages that PlugHub must discover, install, and load. Applies to package.json or *.package.json manifests, dist/*.dll payloads, PlugHub.Contracts modules, Autodesk.Revit.UI.IExternalCommand commands, icons, build.ps1, local/GitHub/Gitee package sources, and plugin package authoring across Codex, Hermes, OpenClaw, Trae, and CodeBuddy.
---

# PlugHub Package Authoring

Use this skill to produce a complete PlugHub-loadable package, not just a Revit command class. A valid package has a discoverable manifest, package-relative payload paths, a `net48` command assembly, and verification evidence.

## Workflow

1. Locate the target package root and nearby examples.
   - Prefer existing `PlugHub_Packages` patterns when present.
   - If the package format is uncertain, read `references/plughub-package-contract.md`.
   - If implementing a new package, read `references/authoring-playbook.md`.
   - If operating in Hermes, OpenClaw, Trae, CodeBuddy, or another non-Codex agent, read `references/agent-compatibility.md`.

2. Make the package discoverable.
   - Put a valid `package.json` in the package root, or use a colocated `<DllName>.package.json` for a flat single-DLL drop-in.
   - Keep payload paths relative to the manifest directory.
   - Use `dist/<AssemblyName>.dll` for command assemblies and `icons/<feature>.png` for feature icons.

3. Implement the Revit command assembly.
   - Target `.NET Framework 4.8` / `net48`.
   - Reference `RevitAPI.dll`, `RevitAPIUI.dll`, and `PlugHub.Contracts`.
   - Add one `IExternalCommand` class per user-triggered feature.
   - Add an `IPlugHubModule` class whose `Describe()` mirrors the manifest metadata, but do not rely on this class alone; PlugHub package discovery reads JSON manifests.

4. Update repository integration files.
   - Add the project to the solution and `build.ps1` if the repo uses them.
   - Add or update the root manifest module and feature records.
   - Ensure `dist/*.dll` is produced by the build and retained for package distribution.
   - Ensure icons are real package files, not `builtin:` references or absolute paths.

5. Validate before completion.
   - Run `python <skill>/scripts/validate_plughub_package.py <package-root>` for manifest and payload checks.
   - Run the repository validator when present, usually `.\tests\Validate-Package.ps1`.
   - Run `.\build.ps1 -UseRevitApiNuGet` for CI-style compile checks, or `.\build.ps1 -RevitApiDir "<Revit 2020 install dir>"` for installed Revit API references.
   - For behavior, smoke-test in Windows + Revit 2020 with a safe test model or family file.

## Hard Rules

- Do not create only a `.addin` file. PlugHub loads package features from `package.json` / `*.package.json`.
- Do not leave `commandAssembly`, `assembly`, or `iconPath` absolute. PlugHub resolves relative paths from the manifest directory and rejects install payloads outside that directory.
- Do not omit `commandType`; it must be the full type name of an `Autodesk.Revit.UI.IExternalCommand`.
- Do not put business logic in the module descriptor class. Keep behavior in command classes.
- Do not claim the package is loadable until manifest validation, build validation, and relevant Revit smoke testing are reported.

## Reference Map

- `references/plughub-package-contract.md`: PlugHub discovery, manifest, install, and runtime loading rules.
- `references/authoring-playbook.md`: implementation templates, package layout, and validation commands.
- `references/agent-compatibility.md`: portable operating rules for Codex, Hermes, OpenClaw, Trae, and CodeBuddy.
