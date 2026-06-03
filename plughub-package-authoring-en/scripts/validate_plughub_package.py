#!/usr/bin/env python3
"""Validate the static shape of a PlugHub package manifest and payload files."""

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
    parser = argparse.ArgumentParser(description="Validate a PlugHub package manifest.")
    parser.add_argument("package_root", nargs="?", default=".", help="Package root containing package.json")
    parser.add_argument("--manifest", default="package.json", help="Manifest path relative to package root")
    args = parser.parse_args()

    package_root = Path(args.package_root).resolve()
    manifest_path = (package_root / args.manifest).resolve()
    errors: list[str] = []
    warnings: list[str] = []

    if not package_root.exists():
        errors.append(f"Package root does not exist: {package_root}")
    if not manifest_path.exists():
        errors.append(f"Manifest does not exist: {manifest_path}")
    if errors:
        return finish(errors, warnings)

    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    except Exception as exc:  # noqa: BLE001
        errors.append(f"Manifest JSON could not be parsed: {exc}")
        return finish(errors, warnings)

    if not isinstance(manifest, dict):
        errors.append("Manifest root must be a JSON object.")
        return finish(errors, warnings)

    if not manifest.get("schemaVersion"):
        errors.append("Missing required root field: schemaVersion")
    modules = manifest.get("modules")
    if not isinstance(modules, list) or not modules:
        errors.append("Missing or empty required root field: modules")

    version = str(manifest.get("version", ""))
    if not VERSION_RE.match(version):
        errors.append("Root version must match V<major>.<minor>.<patch>.")

    root_revit_versions = [str(item) for item in as_list(manifest.get("revitVersions"))]
    if "2020" not in root_revit_versions:
        errors.append('Root revitVersions must include "2020".')

    if manifest.get("frameworkVersionRange") != ">=1.3.0":
        errors.append('Root frameworkVersionRange should be ">=1.3.0".')

    for key in sorted(DISALLOWED_ROOT_KEYS.intersection(manifest.keys())):
        errors.append(f"External package manifest should not define root config key: {key}")

    module_ids: set[str] = set()
    feature_ids: set[str] = set()
    manifest_dir = manifest_path.parent

    for index, module in enumerate(as_list(modules)):
        location = f"modules[{index}]"
        if not isinstance(module, dict):
            errors.append(f"{location} must be an object.")
            continue

        module_id = str(module.get("id", "")).strip()
        if not module_id:
            errors.append(f"{location}.id is required.")
        elif module_id.lower() in module_ids:
            errors.append(f"Duplicate module id: {module_id}")
        else:
            module_ids.add(module_id.lower())

        for bool_key in ("enabled", "visible"):
            if not isinstance(module.get(bool_key), bool):
                errors.append(f"{location}.{bool_key} must be a boolean.")

        module_revit_versions = [str(item) for item in as_list(module.get("revitVersions"))]
        if module_revit_versions and "2020" not in module_revit_versions:
            errors.append(f"{location}.revitVersions declares versions but does not include 2020.")

        assembly = str(module.get("assembly", "")).strip()
        if assembly:
            validate_payload_path(errors, manifest_dir, assembly, f"{location}.assembly")
        else:
            warnings.append(f"{location}.assembly is empty; each feature must define commandAssembly.")

        features = module.get("features")
        if not isinstance(features, list) or not features:
            errors.append(f"{location}.features must be a non-empty array.")
            continue

        for feature_index, feature in enumerate(features):
            feature_location = f"{location}.features[{feature_index}]"
            if not isinstance(feature, dict):
                errors.append(f"{feature_location} must be an object.")
                continue

            feature_id = str(feature.get("id", "")).strip()
            if not feature_id:
                errors.append(f"{feature_location}.id is required.")
            elif feature_id.lower() in feature_ids:
                errors.append(f"Duplicate feature id: {feature_id}")
            else:
                feature_ids.add(feature_id.lower())

            default_state = feature.get("defaultState")
            if default_state is not None and str(default_state) not in VALID_DEFAULT_STATES:
                errors.append(f"{feature_location}.defaultState must be one of {sorted(VALID_DEFAULT_STATES)}.")

            button_size = feature.get("buttonSize")
            if button_size is not None and str(button_size) not in VALID_BUTTON_SIZES:
                errors.append(f"{feature_location}.buttonSize must be one of {sorted(VALID_BUTTON_SIZES)}.")

            command_assembly = str(feature.get("commandAssembly", "")).strip() or assembly
            if not command_assembly:
                errors.append(f"{feature_location}.commandAssembly is required when module.assembly is empty.")
            else:
                validate_payload_path(errors, manifest_dir, command_assembly, f"{feature_location}.commandAssembly")

            command_type = str(feature.get("commandType", "")).strip()
            if not command_type:
                errors.append(f"{feature_location}.commandType is required.")
            elif "." not in command_type or any(ch.isspace() for ch in command_type):
                errors.append(f"{feature_location}.commandType must be a full CLR type name.")

            icon_path = str(feature.get("iconPath", "")).strip()
            if icon_path:
                validate_payload_path(errors, manifest_dir, icon_path, f"{feature_location}.iconPath")
                if Path(icon_path).suffix.lower() != ".png":
                    errors.append(f"{feature_location}.iconPath should point to a PNG file.")
            else:
                warnings.append(f"{feature_location}.iconPath is empty; PlugHub will use its default icon.")

    return finish(errors, warnings)


def validate_payload_path(errors: list[str], manifest_dir: Path, relative_path: str, label: str) -> None:
    if not is_relative_payload(relative_path):
        errors.append(f"{label} must be a package-relative path inside the manifest directory: {relative_path}")
        return
    resolved = manifest_dir / relative_path
    if not resolved.exists():
        errors.append(f"{label} points to a missing package file: {relative_path}")


def finish(errors: list[str], warnings: list[str]) -> int:
    for warning in warnings:
        print(f"WARNING: {warning}")
    if errors:
        for error in errors:
            print(f"ERROR: {error}")
        print(f"PlugHub package validation failed with {len(errors)} error(s).")
        return 1
    print("PlugHub package validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
