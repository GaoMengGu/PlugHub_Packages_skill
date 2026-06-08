# PlugHub Package Contract

This reference captures the package shape that PlugHub currently discovers, installs, and loads.

## Discovery

- PlugHub package manifests are named `package.json` or `*.package.json`.
- A source root is scanned by checking the root `package.json` first, then recursively finding other `package.json` and `*.package.json` files.
- Paths under `.git` are ignored.
- A manifest is readable only when the JSON object contains both `schemaVersion` and `modules`.
- A repository package uses `module.id` as both the package id and module id in repository browsing.

## Runtime Compatibility

- PlugHub Revit runtime currently targets Revit `2020`.
- If a module declares `revitVersions`, the list must include `"2020"` or PlugHub skips that module with a compatibility diagnostic.
- `frameworkVersionRange` is preserved as metadata and is not strictly evaluated by the current runtime, but package repositories should keep `">=1.3.0"` unless the PlugHub framework contract changes.
- Package command assemblies should target `net48`.

## Manifest Shape

Use this as the default manifest skeleton:

```json
{
  "schemaVersion": "1.0",
  "version": "V1.0.0",
  "revitVersions": ["2020"],
  "frameworkVersionRange": ">=1.3.0",
  "modules": [
    {
      "id": "plughub.modules.example-tool",
      "assembly": "dist/PlugHub.ExampleTool.dll",
      "displayName": "Example Tools",
      "description": "Example PlugHub tool package.",
      "enabled": true,
      "visible": true,
      "order": 500,
      "tags": ["example", "revit-api"],
      "features": [
        {
          "id": "plughub.modules.example-tool.run",
          "displayName": "Run Example",
          "description": "Run the example command.",
          "category": "example",
          "group": "Example Tools",
          "tags": ["example"],
          "order": 510,
          "defaultState": "Visible",
          "buttonSize": "large",
          "iconPath": "icons/example-tool.png",
          "commandAssembly": "dist/PlugHub.ExampleTool.dll",
          "commandType": "PlugHub.ExampleTool.RunExampleCommand"
        }
      ]
    }
  ]
}
```

## Field Rules

- `module.id` and `feature.id`: make globally unique; use stable lowercase dotted/kebab ids such as `plughub.modules.level-visibility.toggle`.
- `module.assembly`: package-relative DLL path. It is also the fallback when a feature omits `commandAssembly`.
- `module.displayName`: module grouping label. Existing packages often share display names across related modules, such as view tools.
- `module.enabled` and `module.visible`: required booleans in PlugHub's package schema.
- `feature.group`: fallback Ribbon panel name when the workspace has no explicit group layout.
- `feature.commandAssembly`: package-relative DLL path, usually the same as `module.assembly`.
- `feature.commandType`: full CLR type name of an `Autodesk.Revit.UI.IExternalCommand`.
- `feature.buttonSize`: use `large` or `small`.
- `feature.iconPath`: package-relative icon path. Prefer a generated or supplied PNG file named `icons/<feature>.png`; the file must exist and ship as package payload.

For external package repositories, avoid root `packageDirectories`, `moduleSources`, `repositories`, and `conflictPolicy` unless you are editing PlugHub framework configuration. The package repository validator rejects these root config keys for package manifests.

## Installation Payload

When installing a repository package, PlugHub writes a single-module `package.json` and copies only these package-relative payloads:

- `module.assembly`
- each `feature.commandAssembly`
- each `feature.iconPath`

Absolute payload paths are ignored for copying and are a bad package practice. Missing relative payload files fail installation.

## Runtime Loading

- PlugHub resolves `feature.commandAssembly` relative to the module's resolved base directory, normally the manifest directory.
- The runtime shadow-copies package files into `runtime-cache` before loading command assemblies.
- `commandType` is loaded from the cached assembly and must be assignable to `Autodesk.Revit.UI.IExternalCommand`.
- The command instance is created with a parameterless constructor.

## Source Configuration Examples

Local development source:

```json
{
  "id": "local-plughub-packages",
  "type": "localFolder",
  "path": "D:/path/to/PlugHub_Packages",
  "manifestPath": "package.json",
  "enabled": true,
  "autoUpdate": false
}
```

GitHub source:

```json
{
  "id": "plughub-packages",
  "type": "github",
  "repository": "GaoMengGu/PlugHub_Packages",
  "ref": "main",
  "path": "packages/github/GaoMengGu_PlugHub_Packages",
  "manifestPath": "package.json",
  "enabled": true,
  "autoUpdate": true
}
```

User drop-in layout:

```text
packages/dropins/PlugHub_Packages/
  package.json
  dist/
    PlugHub.ExampleTool.dll
  icons/
    example-tool.png
```
