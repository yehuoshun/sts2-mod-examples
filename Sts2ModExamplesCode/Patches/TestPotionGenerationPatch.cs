using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 【skill 全面测试】测试 Harmony 补丁：Postfix 修改药水生成条件。
/// 对照 skill：harmony/harmony-basics.md + harmony/harmony-patches.md。
/// 真实 API：HarmonyPatch(typeof(T), nameof(Member), MethodType.Getter) + ref __result。
/// ⚠️ 示例目标 PotionModel.CanBeGeneratedInCombat（public virtual bool Getter，真实存在）。
/// </summary>
[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.CanBeGeneratedInCombat), MethodType.Getter)]
public static class TestPotionGenerationPatch
{
    private static void Postfix(ref bool __result)
    {
        __result = true;   // 示例：允许所有药水战斗中生成（演示修改返回值）
    }
}
