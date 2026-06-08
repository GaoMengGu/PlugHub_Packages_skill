---
name: plughub-package-authoring
description: 用于创建、审查或修复 Revit 2020 PlugHub 外部插件包，使其能被 PlugHub 发现、安装和加载。适用于 package.json 或 *.package.json 清单、dist/*.dll 载荷、PlugHub.Contracts 模块、Autodesk.Revit.UI.IExternalCommand 命令、icons、build.ps1、本地/GitHub/Gitee 插件源，以及需要兼容 Codex、Hermes、OpenClaw、Trae、CodeBuddy 的插件包编写任务。
---

# PlugHub 插件包编写

使用此技能时，目标是产出一个完整的、可被 PlugHub 加载的插件包，而不是只写一个 Revit 命令类。有效插件包必须同时具备可发现清单、包内相对载荷路径、`net48` 命令程序集和验证证据。

## 需求处理

- 将用户需求当作插件功能规格，而不是文档咨询。除非用户明确只要方案，否则要实际创建或修改插件包文件。
- 先把需求落到一个用户可点击的 Revit 功能：模块 id、功能 id、Ribbon 分组、命令类型、输入方式、事务边界、失败消息和验证方式。
- 如果需求涉及模型修改，命令必须使用明确的 Revit API 事务；如果只是读取或选择，也要处理无活动文档、无活动视图、取消选择等失败路径。
- 默认产出一个可被 PlugHub 发现的完整包：`package.json` / `*.package.json`、`dist/*.dll` 构建路径、图标路径、项目文件、构建脚本登记和验证记录。
- 如果当前环境无法运行 Revit 2020，只能声明静态验证和构建结果，并明确标记 Revit runtime smoke test 待执行；不要把静态验证等同于运行成功。

## 图标生成

- 编写或修复插件功能时，要为每个用户可点击功能生成图标 PNG，保存到 `icons/<feature>.png`，并更新 `feature.iconPath` 指向这个包内相对路径。
- 生成图标时根据功能 `displayName`、`description` 和核心动作提炼概念，再套用 PlugHub 图标设计语言：极简几何抽象、100% flat、纯黑/深碳灰 `#1A1A1A`、纯白背景、实心块状 glyph、微圆角、负空间、16x16 小尺寸可辨。
- 图标生成提示词必须包含核心句子 `Create a flat, solid glyph icon`，并明确要求输出严格 32×32 画布、主体控制在 24×24 安全区、四周 4px 留白、透明底 PNG、无文字、无多余底色、无边框、无渐变、无阴影、无描边、无 3D 透视；可直接使用 `references/authoring-playbook.md` 中的模板。
- Revit 高分屏渲染会自动缩放，使用 32×32 原图即可，不用额外做多倍图或 `@2x`/`@3x` 变体。
- 只有用户明确提供图标、明确要求不生成图标，或当前 agent 环境没有可用图像生成能力时，才跳过生成；跳过时必须在交付结果中标记图标资产待补，不要伪造 `iconPath` 指向不存在的文件。

## 工作流

1. 定位目标插件包根目录和附近示例。
   - 优先沿用已有 `PlugHub_Packages` 结构。
   - 如果不确定包格式，读取 `references/plughub-package-contract.md`。
   - 如果要新增插件包，读取 `references/authoring-playbook.md`。
   - 如果运行在 Hermes、OpenClaw、Trae、CodeBuddy 或其他非 Codex agent 中，读取 `references/agent-compatibility.md`。

2. 让插件包可被发现。
   - 在包根目录放置有效 `package.json`；平铺投放单个 DLL 时，可使用同目录 `<DllName>.package.json`。
   - 所有载荷路径都保持为相对清单目录的路径。
   - 命令程序集使用 `dist/<AssemblyName>.dll`，功能图标使用 `icons/<feature>.png`。

3. 实现 Revit 命令程序集。
   - 目标框架使用 `.NET Framework 4.8` / `net48`。
   - 引用 `RevitAPI.dll`、`RevitAPIUI.dll` 和 `PlugHub.Contracts`。
   - 每个用户可触发功能对应一个 `IExternalCommand` 类。
   - 添加一个 `IPlugHubModule` 类，让 `Describe()` 与清单元数据保持一致；但不要只依赖这个类，PlugHub 插件包发现读取的是 JSON 清单。

4. 更新仓库集成文件。
   - 如果仓库使用解决方案和 `build.ps1`，把新项目加入其中。
   - 在根清单中新增或更新模块和功能记录。
   - 确保构建会产出 `dist/*.dll`，并且该 DLL 作为插件包分发载荷保留。
   - 为每个功能生成图标 PNG，保存到 `icons/<feature>.png`，更新 `feature.iconPath`，并确保图标是真实包文件，不使用 `builtin:` 引用或绝对路径。

5. 完成前验证。
   - 运行 `dotnet run --project <skill-dir>/tools/PlugHub.PackageValidator/PlugHub.PackageValidator.csproj -- <package-root>` 检查清单和载荷。
   - 如果仓库有验证器，通常运行 `.\tests\Validate-Package.ps1`。
   - 没有安装 Revit 时，用 `.\build.ps1 -UseRevitApiNuGet` 做 CI 风格编译检查；有本机 Revit API DLL 时，用 `.\build.ps1 -RevitApiDir "<Revit 2020 install dir>"`。
   - 行为验证必须在 Windows + Revit 2020 中使用安全测试模型或族文件做 smoke test。

## 硬性规则

- 不要只创建 `.addin` 文件。PlugHub 从 `package.json` / `*.package.json` 加载插件包功能。
- 不要让 `commandAssembly`、`assembly` 或 `iconPath` 保持绝对路径。PlugHub 按清单目录解析相对路径，并会拒绝复制清单目录外的安装载荷。
- 不要省略 `commandType`；它必须是实现 `Autodesk.Revit.UI.IExternalCommand` 的完整类型名。
- 不要把业务逻辑写在模块描述类里。业务行为放在命令类中。
- 在报告清单验证、构建验证和相关 Revit smoke test 结果前，不要声称插件包可被 PlugHub 加载。

## 参考文件

- `references/plughub-package-contract.md`：PlugHub 的发现、清单、安装和运行时加载规则。
- `references/authoring-playbook.md`：实现模板、包结构和验证命令。
- `references/agent-compatibility.md`：Codex、Hermes、OpenClaw、Trae、CodeBuddy 的通用操作约束。
