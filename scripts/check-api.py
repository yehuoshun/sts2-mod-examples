#!/usr/bin/env python3
"""API 自检脚本：扫描示例代码中的 API 调用，对照 sts2-res 提取的白名单（scripts/api-whitelist.txt）。

规则：
  1. 类在游戏类集合 + "类.方法" 在精确对集合 → 通过
  2. 类在游戏类集合 + 方法不在精确对 → 报错（类存在但方法名/签名可疑）
  3. 类不在游戏类集合 + 方法名在方法集合 → 通过（属性访问等场景）
  4. 类不在 + 方法名不在 → 报错（虚构类/虚构方法，如 GoldCmd.Gain）

用法：python3 scripts/check-api.py [--code Sts2ModExamplesCode]
"""
import argparse
import re
import sys
from pathlib import Path

SKIP_PREFIXES = ("Sts2ModExamples", "Example")   # 本仓库命名空间/类
# .NET / Godot / 子命名空间引用（非游戏类型）
SKIP_CLASSES = {
    "ArgumentNullException", "Types", "Monsters", "Cards", "Relics", "Powers",
    "Potions", "Enchantments", "Events", "Ancients", "Encounters", "Orbs",
    "Characters", "Modifiers", "RestSite", "Multiplayer", "Patches", "Core",
    "System", "String", "Math", "Mathf", "Random", "Path", "Console", "Enum",
    "Convert", "Activator", "AppDomain", "Assembly", "Type", "Task", "Godot",
    "ResourceLoader", "Color", "Vector2", "Texture2D", "Node", "Node2D",
    "Label", "PackedScene", "Control", "HorizontalAlignment", "Harmony",
    "AccessTools", "CodeInstruction", "MethodBase", "CurrentDomain", "ModEnergyIconCodec",
    "ModConfigStorage", "NModConfigSubmenu", "ExampleModConfig", "SubmenuStack",
    "SceneTree", "Engine", "OS", "GodotObject", "Key", "InputEvent", "InputEventKey",
    "CheckButton", "HSlider", "OptionButton", "LineEdit", "ColorPickerButton",
    "ScrollContainer", "VBoxContainer", "HBoxContainer", "Button", "Label", "TextureRect",
    "GD", "Array",
    "AllPossibleOptions", "DynamicVars", "Owner", "Card", "Creature", "CombatState",
    "Amount", "IntValue", "BaseValue", "Tags", "Type",
    "CanonicalVars", "ExtraHoverTips",
    # Overlay 模块：Godot 内置类（引擎 API，非游戏类型，规则 3 直通）+ 示例内部辅助类/变量
    "Input", "CanvasLayer", "Sprite2D", "SceneTree", "Image", "ImageTexture",
    "DisplayServer", "Engine", "GD", "GodotObject", "FileAccess", "Time", "OS",
    "Root", "CursorOverlayController",
    # Keystone 示例：ConditionalWeakTable 变量名（规则 4 误报）
    "States",
}

def load_whitelist(path: Path):
    text = path.read_text(encoding="utf-8")
    classes = set(text.split("## METHODS")[0].split("## CLASSES")[1].strip().splitlines())
    methods = set(text.split("## PAIRS")[0].split("## METHODS")[1].strip().splitlines())
    pairs = set(text.split("## PAIRS")[1].strip().splitlines())
    return classes, methods, pairs

def extract_calls(code_root: Path):
    calls = []
    for f in sorted(code_root.rglob("*.cs")):
        lines = f.read_text(encoding="utf-8").splitlines()
        text = "\n".join(l.split("//", 1)[0] for l in lines)   # 剥离行注释
        for m in re.finditer(r"\b([A-Z]\w+\.\w+)(?:<[^>]*>)?\s*\(", text):
            head = m.group(1)
            cls = head.split(".")[0]
            if cls.startswith(SKIP_PREFIXES) or cls in SKIP_CLASSES:
                continue
            line = text[:m.start()].count("\n") + 1
            calls.append((head, f"{f.name}:{line}"))
    return calls

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--code", default="Sts2ModExamplesCode")
    ap.add_argument("--whitelist", default="scripts/api-whitelist.txt")
    args = ap.parse_args()

    wl_path = Path(args.whitelist)
    if not wl_path.exists():
        print(f"❌ 白名单不存在: {wl_path}（需先从 sts2-res 生成）")
        return 2
    classes, methods, pairs = load_whitelist(wl_path)

    calls = extract_calls(Path(args.code))
    print(f"提取 {len(calls)} 个 API 调用")
    if not calls:
        print("⚠️ 未提取到调用")
        return 0

    errors = []
    for head, loc in calls:
        cls, meth = head.split(".")
        if cls in classes:
            if head in pairs:
                print(f"  ✅ {head} ({loc})")
            else:
                errors.append((head, loc, "类存在但方法不在精确对（签名/成员可疑）"))
        else:
            if meth in methods:
                print(f"  ✅ {head} ({loc}) [方法名命中，类为属性访问]")
            else:
                errors.append((head, loc, "类与方法均不在白名单（疑似虚构）"))

    if errors:
        print("\n❌ 需人工核对：")
        for head, loc, why in errors:
            print(f"  {head} ← {loc} | {why}")
        return 1
    print("\n✅ 全部通过")
    return 0

if __name__ == "__main__":
    sys.exit(main())
