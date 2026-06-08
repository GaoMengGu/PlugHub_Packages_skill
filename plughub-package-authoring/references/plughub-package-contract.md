# PlugHub 插件包契约

此参考描述 PlugHub 当前会发现、安装和加载的插件包形态。

## 发现规则

- PlugHub 插件包清单文件名为 `package.json` 或 `*.package.json`。
- 扫描来源根目录时，先检查根目录 `package.json`，再递归查找其他 `package.json` 和 `*.package.json`。
- `.git` 下的路径会被忽略。
- 只有 JSON 根对象同时包含 `schemaVersion` 和 `modules` 时，清单才会被读取。
- 仓库浏览中，`module.id` 同时作为 package id 和 module id。

## 运行时兼容性

- PlugHub Revit 运行时当前面向 Revit `2020`。
- 如果模块声明了 `revitVersions`，列表必须包含 `"2020"`；否则 PlugHub 会用兼容性诊断跳过该模块。
- `frameworkVersionRange` 当前作为元数据保留，运行时尚未严格计算范围；除非 PlugHub 框架契约变化，插件包仓库应保持 `">=1.3.0"`。
- 插件命令程序集应面向 `net48`。

## 清单形态

默认使用以下清单骨架：

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
      "displayName": "示例工具",
      "description": "示例 PlugHub 插件包。",
      "enabled": true,
      "visible": true,
      "order": 500,
      "tags": ["example", "revit-api"],
      "features": [
        {
          "id": "plughub.modules.example-tool.run",
          "displayName": "运行示例",
          "description": "运行示例命令。",
          "category": "example",
          "group": "示例工具",
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

## 字段规则

- `module.id` 和 `feature.id`：必须全局唯一；使用稳定的小写 dotted/kebab id，例如 `plughub.modules.level-visibility.toggle`。
- `module.assembly`：包内相对 DLL 路径。某个功能省略 `commandAssembly` 时，它也是 fallback。
- `module.displayName`：模块分组名称。已有包会让相关模块共享名称，例如视图工具。
- `module.enabled` 和 `module.visible`：PlugHub package schema 要求的布尔字段。
- `feature.group`：workspace 没有显式 group 布局时的 fallback Ribbon panel 名称。
- `feature.commandAssembly`：包内相对 DLL 路径，通常与 `module.assembly` 相同。
- `feature.commandType`：实现 `Autodesk.Revit.UI.IExternalCommand` 的完整 CLR 类型名。
- `feature.buttonSize`：使用 `large` 或 `small`。
- `feature.iconPath`：包内相对图标路径。优先指向生成或提供的 PNG 文件，命名为 `icons/<feature>.png`；该文件必须真实存在并随包载荷分发。PlugHub/Revit 图标使用 32×32 透明底 PNG，主体建议在 24×24 安全区内并保留 4px 留白；高分屏由 Revit 自动缩放，不需要额外多倍图。

对外部插件包仓库，除非正在编辑 PlugHub 框架配置，否则不要在包清单根节点放 `packageDirectories`、`moduleSources`、`repositories`、`conflictPolicy`。包仓库验证器会拒绝这些根配置键。

## 安装载荷

安装仓库插件包时，PlugHub 会写入单模块 `package.json`，并且只复制这些包内相对载荷：

- `module.assembly`
- 每个 `feature.commandAssembly`
- 每个 `feature.iconPath`

绝对载荷路径不会被复制，也不适合作为插件包实践。相对载荷文件缺失会导致安装失败。

## 运行时加载

- PlugHub 会相对模块 resolved base directory 解析 `feature.commandAssembly`，通常也就是清单目录。
- 运行时会先把包文件 shadow copy 到 `runtime-cache`，再加载命令程序集。
- `commandType` 从缓存程序集加载，且必须能赋值给 `Autodesk.Revit.UI.IExternalCommand`。
- 命令实例通过无参构造函数创建。

## 来源配置示例

本地开发来源：

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

GitHub 来源：

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

用户投放目录：

```text
packages/dropins/PlugHub_Packages/
  package.json
  dist/
    PlugHub.ExampleTool.dll
  icons/
    example-tool.png
```
