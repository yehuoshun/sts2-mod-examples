using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例 Harmony 补丁：Postfix 修改属性返回值。
/// 对照 skill：harmony/harmony-basics.md + harmony/harmony-patches.md。
/// 真实 API：HarmonyPatch(typeof(T), nameof(Member), MethodType.Getter) + ref __result。
/// ⚠️ 示例目标 CardModel.CanBeGeneratedInCombat（public virtual bool Getter，真实存在）。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanBeGeneratedInCombat), MethodType.Getter)]
public static class ExamplePatch
{
    private static void Postfix(ref bool __result)
    {
        __result = false;   // 示例：禁止所有卡牌在战斗中生成（演示修改返回值）
    }
}
