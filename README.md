# PlugHub Packages Skill

本仓库提供用于编写 PlugHub 外部插件包的 Codex skill。仓库同时保留中文和英文两个版本，agent 拿到仓库地址后即可按下方路径自动安装。

This repository provides Codex skills for authoring PlugHub external plugin packages. It keeps both Chinese and English skill versions so an agent can install them from this repository URL.

## Skills

| Language | Skill name | Path |
| --- | --- | --- |
| 中文 | `plughub-package-authoring` | `plughub-package-authoring` |
| English | `plughub-package-authoring-en` | `plughub-package-authoring-en` |

## Codex Install

安装中文版本：

```powershell
python C:\Users\Yilan\.codex\skills\.system\skill-installer\scripts\install-skill-from-github.py --repo GaoMengGu/PlugHub_Packages_skill --path plughub-package-authoring
```

Install the English version:

```powershell
python C:\Users\Yilan\.codex\skills\.system\skill-installer\scripts\install-skill-from-github.py --repo GaoMengGu/PlugHub_Packages_skill --path plughub-package-authoring-en
```

一次安装两个版本：

```powershell
python C:\Users\Yilan\.codex\skills\.system\skill-installer\scripts\install-skill-from-github.py --repo GaoMengGu/PlugHub_Packages_skill --path plughub-package-authoring plughub-package-authoring-en
```

安装后重启 Codex 以加载新技能。

## Agent Auto-Install Contract

When a user gives an agent only this repository URL and asks to install the PlugHub package authoring skill, the agent should install:

- `plughub-package-authoring` for Chinese workflows.
- `plughub-package-authoring-en` for English workflows.

The repository also includes `skills.json` so agents can discover the installable skill paths programmatically.

## Purpose

The skill guides an agent to create Revit 2020 PlugHub external plugin packages that can be discovered, installed, loaded, and run by PlugHub. It covers:

- `package.json` / `*.package.json` manifest rules.
- `dist/*.dll` payload paths.
- `Autodesk.Revit.UI.IExternalCommand` command classes.
- `PlugHub.Contracts` module descriptors.
- Icons, build scripts, package sources, and validation.
- Runtime verification boundaries for Windows + Revit 2020.
