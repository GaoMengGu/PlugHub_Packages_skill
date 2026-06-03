# Agent Instructions

If a user gives this repository URL and asks to install the PlugHub package authoring skill, install from these paths:

- Chinese: `plughub-package-authoring`
- English: `plughub-package-authoring-en`

For Codex, use:

```powershell
python C:\Users\Yilan\.codex\skills\.system\skill-installer\scripts\install-skill-from-github.py --repo GaoMengGu/PlugHub_Packages_skill --path plughub-package-authoring plughub-package-authoring-en
```

After installation, tell the user to restart Codex.

When using the skill to implement a PlugHub plugin package, do not stop after writing C# code. The output must include a discoverable manifest, package-relative payloads, build registration, validation evidence, and a Revit 2020 runtime smoke-test status.
