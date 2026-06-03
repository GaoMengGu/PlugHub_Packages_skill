#!/usr/bin/env python3
"""验证 PlugHub 插件包清单和载荷文件的静态形态。"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from pathlib import Path
from typing import Any


VERSION_RE = re.compile(r"^V\d+\.\d+\.\d+$")
DISALLOWED_ROOT_KEYS = {"packageDirectories", "moduleSources", "repositories", "conflictPolicy"}
VALID_DEFAULT_STATES = {"Visible", "Disabled", "Hidden"}
VALID_BUTTON_SIZES = {"large", "small"}


def is_relative_payload(value: str) -> bool:
    if not value or os.path.isabs(value):
        return False
    parts = Path(value).parts
    return ".." not in parts


def as_list(value: Any) -> list[Any]:
    return value if isinstance(value, list) else []


def main() -> int:
    parser = argparse.ArgumentParser(description="验证 PlugHub 插件包清单。")
    parser.add_argument("package_root", nargs="?", default=".", help="包含 package.json 的插件包根目录")
    parser.add_argument("--manifest", default="package.json", help="相对插件包根目录的清单路径")
    args = parser.parse_args()

    package_root = Path(args.package_root).resolve()
    manifest_path = (package_root / args.manifest).resolve()
    errors: list[str] = []
    warnings: list[str] = []

    if not package_root.exists():
        errors.append(f"插件包根目录不存在：{package_root}")
    if not manifest_path.exists():
        errors.append(f"清单文件不存在：{manifest_path}")
    if errors:
        return finish(errors, warnings)

    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    except Exception as exc:  # noqa: BLE001
        errors.append(f"清单 JSON 无法解析：{exc}")
        return finish(errors, warnings)

    if not isinstance(manifest, dict):
        errors.append("清单根节点必须是 JSON 对象。")
        return finish(errors, warnings)

    if not manifest.get("schemaVersion"):
        errors.append("缺少必需根字段：schemaVersion")
    modules = manifest.get("modules")
    if not isinstance(modules, list) or not modules:
        errors.append("缺少必需根字段 modules，或 modules 为空。")

    version = str(manifest.get("version", ""))
    if not VERSION_RE.match(version):
        errors.append("根字段 version 必须匹配 V<major>.<minor>.<patch>。")

    root_revit_versions = [str(item) for item in as_list(manifest.get("revitVersions"))]
    if "2020" not in root_revit_versions:
        errors.append('根字段 revitVersions 必须包含 "2020"。')

    if manifest.get("frameworkVersionRange") != ">=1.3.0":
        errors.append('根字段 frameworkVersionRange 应为 ">=1.3.0"。')

    for key in sorted(DISALLOWED_ROOT_KEYS.intersection(manifest.keys())):
        errors.append(f"外部插件包清单不应定义根配置键：{key}")

    module_ids: set[str] = set()
    feature_ids: set[str] = set()
    manifest_dir = manifest_path.parent

    for index, module in enumerate(as_list(modules)):
        location = f"modules[{index}]"
        if not isinstance(module, dict):
            errors.append(f"{location} 必须是对象。")
            continue

        module_id = str(module.get("id", "")).strip()
        if not module_id:
            errors.append(f"{location}.id 为必填。")
        elif module_id.lower() in module_ids:
            errors.append(f"重复的 module id：{module_id}")
        else:
            module_ids.add(module_id.lower())

        for bool_key in ("enabled", "visible"):
            if not isinstance(module.get(bool_key), bool):
                errors.append(f"{location}.{bool_key} 必须是布尔值。")

        module_revit_versions = [str(item) for item in as_list(module.get("revitVersions"))]
        if module_revit_versions and "2020" not in module_revit_versions:
            errors.append(f"{location}.revitVersions 声明了版本，但未包含 2020。")

        assembly = str(module.get("assembly", "")).strip()
        if assembly:
            validate_payload_path(errors, manifest_dir, assembly, f"{location}.assembly")
        else:
            warnings.append(f"{location}.assembly 为空；每个 feature 必须定义 commandAssembly。")

        features = module.get("features")
        if not isinstance(features, list) or not features:
            errors.append(f"{location}.features 必须是非空数组。")
            continue

        for feature_index, feature in enumerate(features):
            feature_location = f"{location}.features[{feature_index}]"
            if not isinstance(feature, dict):
                errors.append(f"{feature_location} 必须是对象。")
                continue

            feature_id = str(feature.get("id", "")).strip()
            if not feature_id:
                errors.append(f"{feature_location}.id 为必填。")
            elif feature_id.lower() in feature_ids:
                errors.append(f"重复的 feature id：{feature_id}")
            else:
                feature_ids.add(feature_id.lower())

            default_state = feature.get("defaultState")
            if default_state is not None and str(default_state) not in VALID_DEFAULT_STATES:
                errors.append(f"{feature_location}.defaultState 必须是 {sorted(VALID_DEFAULT_STATES)} 之一。")

            button_size = feature.get("buttonSize")
            if button_size is not None and str(button_size) not in VALID_BUTTON_SIZES:
                errors.append(f"{feature_location}.buttonSize 必须是 {sorted(VALID_BUTTON_SIZES)} 之一。")

            command_assembly = str(feature.get("commandAssembly", "")).strip() or assembly
            if not command_assembly:
                errors.append(f"{feature_location}.commandAssembly 在 module.assembly 为空时为必填。")
            else:
                validate_payload_path(errors, manifest_dir, command_assembly, f"{feature_location}.commandAssembly")

            command_type = str(feature.get("commandType", "")).strip()
            if not command_type:
                errors.append(f"{feature_location}.commandType 为必填。")
            elif "." not in command_type or any(ch.isspace() for ch in command_type):
                errors.append(f"{feature_location}.commandType 必须是完整 CLR 类型名。")

            icon_path = str(feature.get("iconPath", "")).strip()
            if icon_path:
                validate_payload_path(errors, manifest_dir, icon_path, f"{feature_location}.iconPath")
                if Path(icon_path).suffix.lower() != ".png":
                    errors.append(f"{feature_location}.iconPath 应指向 PNG 文件。")
            else:
                warnings.append(f"{feature_location}.iconPath 为空；PlugHub 将使用默认图标。")

    return finish(errors, warnings)


def validate_payload_path(errors: list[str], manifest_dir: Path, relative_path: str, label: str) -> None:
    if not is_relative_payload(relative_path):
        errors.append(f"{label} 必须是清单目录内的包相对路径：{relative_path}")
        return
    resolved = manifest_dir / relative_path
    if not resolved.exists():
        errors.append(f"{label} 指向缺失的包文件：{relative_path}")


def finish(errors: list[str], warnings: list[str]) -> int:
    for warning in warnings:
        print(f"警告：{warning}")
    if errors:
        for error in errors:
            print(f"错误：{error}")
        print(f"PlugHub 插件包验证失败，共 {len(errors)} 个错误。")
        return 1
    print("PlugHub 插件包验证通过。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
