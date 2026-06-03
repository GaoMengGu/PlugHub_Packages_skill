# Authoring Playbook

Use this playbook when implementing or repairing a PlugHub package repository.

## Implementation Steps

1. Choose stable names.
   - Assembly: `PlugHub.<FeatureArea>`.
   - Namespace: match the assembly.
   - Module id: `plughub.modules.<feature-area>`.
   - Feature id: `plughub.modules.<feature-area>.<verb-or-action>`.

2. Create or update the project.
   - Target `net48`.
   - Reference `PlugHub.Contracts`.
   - Reference Revit API by installed DLLs for local development or `Autodesk.Revit.SDK` with `PrivateAssets="all"` and `ExcludeAssets="runtime"` for CI compile references.
   - Keep Revit API DLLs out of the package payload.

Minimal project shape:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AssemblyName>PlugHub.ExampleTool</AssemblyName>
    <RootNamespace>PlugHub.ExampleTool</RootNamespace>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup Condition="'$(RevitApiReferenceMode)' == 'Installed'">
    <Reference Include="RevitAPI">
      <HintPath>$(RevitApiDir)\RevitAPI.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="RevitAPIUI">
      <HintPath>$(RevitApiDir)\RevitAPIUI.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <ItemGroup Condition="'$(RevitApiReferenceMode)' == 'NuGet'">
    <PackageReference Include="Autodesk.Revit.SDK" Version="$(RevitApiNuGetVersion)" PrivateAssets="all" ExcludeAssets="runtime" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\revittool\src\PlugHub.Contracts\PlugHub.Contracts.csproj" />
  </ItemGroup>
</Project>
```

Adjust the `ProjectReference` path to the actual PlugHub checkout.

3. Add a module descriptor class.

```csharp
using System.Collections.Generic;
using PlugHub.Contracts.Features;
using PlugHub.Contracts.Modules;

namespace PlugHub.ExampleTool
{
    public sealed class ExampleToolModule : IPlugHubModule
    {
        public ModuleDescriptor Describe()
        {
            return new ModuleDescriptor
            {
                Id = "plughub.modules.example-tool",
                Name = "Example Tools",
                Description = "Example PlugHub tool package.",
                State = ModuleState.Enabled,
                Order = 500,
                Tags = new[] { "example", "revit-api" },
                Features = new List<FeatureDescriptor>
                {
                    new FeatureDescriptor
                    {
                        Id = "plughub.modules.example-tool.run",
                        ModuleId = "plughub.modules.example-tool",
                        Name = "Run Example",
                        Description = "Run the example command.",
                        Category = "example",
                        Group = "Example Tools",
                        Tags = new[] { "example" },
                        Order = 510,
                        DefaultState = FeatureState.Visible,
                        ButtonSize = "large",
                        CommandAssembly = "dist/PlugHub.ExampleTool.dll",
                        CommandType = "PlugHub.ExampleTool.RunExampleCommand"
                    }
                }
            };
        }

        public void Initialize(IModuleContext context) { }
        public void Shutdown() { }
    }
}
```

4. Add an external command.

```csharp
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PlugHub.ExampleTool
{
    [Transaction(TransactionMode.Manual)]
    public sealed class RunExampleCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var document = commandData?.Application?.ActiveUIDocument?.Document;
            if (document == null)
            {
                message = "No active Revit document.";
                return Result.Failed;
            }

            try
            {
                using (var transaction = new Transaction(document, "Run Example"))
                {
                    transaction.Start();
                    // Perform the Revit API change here.
                    transaction.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
```

5. Register the package.
   - Add a module entry and feature entry to `package.json`.
   - Add the project to the solution file if one exists.
   - Add the project path to `build.ps1` if the repo uses a central build script.
   - Add `icons/<feature>.png`.
   - Build to `dist/<AssemblyName>.dll`.

6. Validate.
   - Run `python <skill>/scripts/validate_plughub_package.py <package-root>`.
   - Run `.\tests\Validate-Package.ps1` when the package repo has it.
   - Run `.\build.ps1 -UseRevitApiNuGet` when Revit is not installed.
   - Run `.\build.ps1 -RevitApiDir "D:\Program Files\Autodesk\Revit 2020"` when local Revit API DLLs are available.
   - Revit behavior still requires Windows + Revit 2020 runtime validation.

## Repository Checklist

- `package.json` has `schemaVersion`, `version`, `revitVersions`, `frameworkVersionRange`, and `modules`.
- Every `module.id` and `feature.id` is unique and stable.
- Every command feature has `commandAssembly` and `commandType`.
- Every relative `assembly`, `commandAssembly`, and `iconPath` exists.
- `dist/*.dll` contains the command type and is part of the distributed package.
- `icons/*.png` exists and is package-relative.
- `bin/`, `obj/`, PDBs, and Revit API DLLs are not treated as package release payload.
- The final release or ZIP includes only user-needed payload such as `package.json`, `dist/*.dll`, and `icons/*.png`.
