using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例 PatchCategory 分类补丁（HarmonyLib 原生属性）。
/// 对照 skill：harmony/harmony-patches.md（PatchCategory）。
/// 真实 API：[HarmonyPatchCategory] + harmony.PatchCategory(assembly, "Core")（ModEntry 里调用）。
/// </summary>
[HarmonyPatchCategory("Core")]
[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanBeGeneratedInCombat), MethodType.Getter)]
public static class ExampleCategoryPatch
{
    private static void Postfix(ref bool __result)
    {
        // 分类加载示例：此 Patch 属于 "Core" 类别，由 PatchCategory 批量加载
    }
}
