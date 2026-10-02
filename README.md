# sts2-mod-examples

Slay the Spire 2 **纯原生** Mod 示例仓库 — 把 [slay-the-spire-2-mod-skill](https://github.com/yehuoshun/slay-the-spire-2-mod-skill) 文档里的模式落地成完整可编译代码。

> 定位：教学示例 + 本地编译验证 + 可安装到游戏实测。零第三方运行时依赖（只靠 `0Harmony.dll` + `sts2.dll`）。

---

## 覆盖内容

| 模块 | 示例文件 | 对照文档 |
|------|---------|---------|
| 卡牌 | `Cards/ExampleStrike.cs` `ExampleDefend.cs` `ExampleCalculatedCard.cs` | card/ |
| 能力 | `Powers/ExampleBuffPower.cs` `ExampleTempPower.cs` | power/ |
| 遗物 | `Relics/ExampleRelic.cs` | relic/ |
| 药水 | `Potions/ExamplePotion.cs` | potion/ |
| 附魔 | `Enchantments/ExampleEnchantment.cs` | enchantment/ |
| 事件 | `Events/ExampleEvent.cs` | event/ |
| 先古之民 | `Ancients/ExampleAncient.cs` | event/ancient |
| 怪物 | `Monsters/ExampleMonster.cs` | monster/ |
| 遭遇 | `Encounters/ExampleEncounter.cs` | monster/encounter |
| 充能球 | `Orbs/ExampleOrb.cs` | orb/ |
| 角色 | `Characters/ExampleCharacter.cs` | character/ |
| 修改器 | `Modifiers/ExampleModifier.cs` | modifier/ |
| 休息站 | `RestSite/ExampleRestOption.cs` | rest-site/ |
| 多人消息 | `Multiplayer/ExampleMessage.cs` | multiplayer/ |
| Harmony 补丁 | `Patches/ExamplePatch.cs` | harmony/ |
| 注册框架 | `Core/ContentRegistry.cs` `PoolAttributes.cs` `ModPatcher.cs` | baselib/ + harmony/ |

## 编译（本地 Rider / VS）

1. 安装 .NET 9 SDK + Megadot（STS2 定制 Godot）
2. 打开 `Sts2ModExamples.csproj`，`dotnet build`（自动探测游戏路径，见 `Sts2PathDiscovery.props`）
3. 构建后自动部署到游戏 `mods/Sts2ModExamples/`；启动游戏验证
4. 资源包（`.pck`）：有 Godot 资源后 `dotnet publish` 导出，或用 PckPacker

> ⚠️ GitHub Actions 无法编译本仓库：`sts2.dll` 只能从游戏安装目录引用（版权原因不能上传）。CI 只做静态检查（见 `scripts/check-api.py`）。

## 静态自检

```bash
python3 scripts/check-api.py   # 扫描代码中的 API 调用，对照 sts2-res 白名单
```

## 已知注意点（写代码时踩过的坑）

- 本地化键 = `{SLUGIFY(类名)}.title/.description`（`ExampleStrike` → `EXAMPLE_STRIKE`），扁平键格式
- 无 `WithUpgrade()`（自研扩展），升级一律 `OnUpgrade()` 里 `UpgradeValueBy`
- 无 `Acts`（事件）/`Slots`（遭遇）/`CharacterId`/`GetUpgradeReplacement`（遗物升级）——均为自研/BaseLib 成员，原生不存在
- `PlayerChoiceContext` 无 `Player` 属性，玩家用 `Owner` / `Owner.Creature`
- 能力 `description` 不支持动态变量，动态值写 `smartDescription`
- 角色示例仅演示注册流程：完整角色需要 tscn 场景 + 动画资源（见 skill character 文档）

## 配套

- 文档仓库：[yehuoshun/slay-the-spire-2-mod-skill](https://github.com/yehuoshun/slay-the-spire-2-mod-skill)
- 参考实现：[YuWan886/Sts2-YuWanCard](https://github.com/YuWan886/Sts2-YuWanCard)
