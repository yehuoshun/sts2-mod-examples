# sts2-mod-examples

Slay the Spire 2 **纯原生** Mod 示例仓库 — 把 [slay-the-spire-2-mod-skill](https://github.com/yehuoshun/slay-the-spire-2-mod-skill) 文档里的模式落地成完整可编译代码。

> 定位：教学示例 + 本地编译验证 + 可安装到游戏实测。零第三方运行时依赖（只靠 `0Harmony.dll` + `sts2.dll`）。

---

## 覆盖内容

| 模块 | 示例文件 | 对照文档 |
|------|---------|---------|
| 卡牌 | `Cards/ExampleStrike.cs` `ExampleDefend.cs` `ExampleCalculatedCard.cs` `ExampleSelectCard.cs` `ExampleFullSelectCard.cs` `ExampleReplayCard.cs` `ExampleMultiplayerOnlyCard.cs` `ExampleAmplifyCard.cs`（增幅系统）`ExampleHoverTipCard.cs`（悬停提示+容器注入） | card/ |
| 能力 | `Powers/ExampleBuffPower.cs` `ExampleTempPower.cs` `ExampleModifyPower.cs` `ExampleTemporaryStatPower.cs`（ITemporaryPower 装饰层+隐藏提示） | power/ + power-signature-and-temp |
| 遗物 | `Relics/ExampleRelic.cs` `ExampleGoldRelic.cs` `ExampleKeystoneRelic.cs`（计数器/选择流程/TempPower 继承） | relic/ + relic-keystone |
| 药水 | `Potions/ExamplePotion.cs` | potion/ |
| 附魔 | `Enchantments/ExampleEnchantment.cs` `ExampleAdvancedEnchantment.cs` `ExampleCompositeEnchantment.cs`（复合容器：JSON 持久化/回调转发/数值链式/叠层） | enchantment/ + enchantment-composite + enchantment-multi |
| 事件 | `Events/ExampleEvent.cs` `ExampleMultiPageEvent.cs` | event/ |
| 先古之民 | `Ancients/ExampleAncient.cs` `ExampleMultiDialogueAncient.cs` | event/ancient |
| 怪物 | `Monsters/ExampleMonster.cs` | monster/ |
| 遭遇 | `Encounters/ExampleEncounter.cs` | monster/encounter |
| 充能球 | `Orbs/ExampleOrb.cs` | orb/ |
| 角色 | `Characters/ExampleCharacter.cs` `ExampleCharacterOverrides.cs`（覆写点+解锁屏蔽+动画状态机） | character/ + monster/ |
| 修改器 | `Modifiers/ExampleModifier.cs` `ExampleNeowModifier.cs` | modifier/ |
| 章节 | `Acts/ExampleAct.cs` | act/ |
| 宠物 | `Pets/ExamplePet.cs` `ExamplePersistentPet.cs`（位置持久化/视觉复制/战斗内生成） | pet/ + pet-advanced |
| 自定义资源 | `Resources/ExampleResource.cs` `ExampleIconPreloadPatch.cs` | resource/ |
| 徽章 | `Badges/ExampleBadge.cs` | badge/ |
| 休息站 | `RestSite/ExampleRestOption.cs` | rest-site/ |
| 牌堆 | `Piles/ExampleVoidPile.cs` | pile/ |
| 奖励 | `Rewards/ExampleCardTransformReward.cs` | reward/ |
| 能量 | `Energy/ExampleEnergyPool.cs` `ICustomEnergyIcon.cs` `CustomEnergyIconPatches.cs` | energy/ |
| 多人消息 | `Multiplayer/ExampleMessage.cs` `ExampleMessageHandler.cs` `ExampleMessageSender.cs` `ExampleManagedGameAction.cs` `ExampleInteractionGuard.cs` | multiplayer/ |
| Harmony 补丁 | `Patches/ExamplePatch.cs` `ExampleCategoryPatch.cs` `ExamplePrefixPatch.cs` `ExampleTranspilerPatch.cs` `ExampleAsyncLocalPatch.cs`（AsyncLocal 异步替换） `ExampleGetterPatch.cs`（资源 getter 覆写） | harmony/ + character-asset-hooks |
| 注册框架 | `Core/ContentRegistry.cs` `PoolAttributes.cs` `EnumInjector.cs` `ModPatcher.cs` `ExampleManualRegistration.cs`（手动注册+缓存重置+进度跳过） | baselib/ + serialization/ + character-manual-register |
| 设置界面 | `Settings/ModConfig.cs` `ModConfigPatches.cs` `NModConfigSubmenu.cs` `ModConfigBridgeExample.cs`（ModConfig 第三方框架反射桥接入） `ModsTabInjector.cs`（ModConfig 设置页 Tab 注入机制纯原生转译） | settings/ |
| Overlay 渲染 | `Overlays/ExampleCursorOverlay.cs`（动态光标：CanvasLayer+Sprite2D+ProcessFrame 信号，单 DLL 源生成器坑） | overlay/ |

## 编译（本地 Rider / VS）

1. 安装 .NET 9 SDK + Megadot（STS2 定制 Godot）
2. 打开 `Sts2ModExamples.csproj`，`dotnet build`（自动探测游戏路径，见 `Sts2PathDiscovery.props`）
3. 构建后自动部署到游戏 `mods/Sts2ModExamples/`；启动游戏验证
4. 资源包（`.pck`）：有 Godot 资源后 `dotnet publish` 导出，或用 PckPacker

> ✅ CI 已支持真编译：`sts2.dll`/`0Harmony.dll` 从 yehuoshun/STS2-ShunMod 的 deps release（tag=游戏版本）拉取，`dotnet build -p:Sts2DataDir` 完整编译。每次 push 自动跑：编译 + ModAnalyzers + API 白名单 + 本地化 JSON 校验。

## 测试流程

1. **改代码/文档后 push** → GitHub Actions 自动跑 `ci.yml`
2. 观察 Actions 结果：`dotnet build` 报错 → 按错误修示例（或修 skill 文档）→ 重推
3. **本地快速自检**：`python3 scripts/check-api.py`（API 白名单，防幻觉 API）
4. **运行时验证**（CI 替代不了）：把 `mods/Sts2ModExamples/` 装进游戏，主菜单应有「Mod Settings」按钮，战斗内能抽到示例无色卡；报错看 `%AppData%\SlayTheSpire2\logs\`
5. 编译通过的示例是 skill 文档正确性的裁判：**文档改动的 API 必须在示例仓库有对应代码**（见 skill LEARN.md 测试流程）

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
