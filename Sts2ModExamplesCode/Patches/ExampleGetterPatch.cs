using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：资源 getter 覆写（AssetHooks 模式的核心样板，attribute 风格）。
/// 对照 skill：character/character-asset-hooks.md。
/// 真实 API：CardModel.Portrait 是 getter（MethodType.Getter 定位）；postfix ref __result 换资源；
/// RelicModel.Icon 同理可 prefix 短路。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class ExampleCardPortraitPatch
{
    // 自定义卡的 Portrait 返回 mod 资源（未命中则保持原值）
    private static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        if (__instance is ExampleTemporaryStrengthCard)
        {
            __result = GD.Load<Texture2D>("res://Sts2ModExamples/images/cards/temporary_strength.png");
        }
    }
}

/// <summary>
/// 示例：getter prefix 强制接管（想完全替换原版资源时用短路）。
/// 真实 API：RelicModel.Icon getter，prefix 返回 false = 跳过原版体。
/// </summary>
[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Icon), MethodType.Getter)]
public static class ExampleRelicIconPatch
{
    private static bool Prefix(RelicModel __instance, ref Texture2D __result)
    {
        if (__instance is not ExampleStarterRelic)
        {
            return true; // 非目标类型放行
        }

        __result = GD.Load<Texture2D>("res://Sts2ModExamples/images/relics/example_relic.png");
        return false;  // 短路原版
    }
}

/// <summary>占位遗物（仅用于演示 getter 短路，不参与遗物池）。</summary>
public sealed class ExampleStarterRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => "res://Sts2ModExamples/images/relics/example_relic.png";

    protected override string PackedIconOutlinePath => PackedIconPath;

    protected override string BigIconPath => PackedIconPath;
}
