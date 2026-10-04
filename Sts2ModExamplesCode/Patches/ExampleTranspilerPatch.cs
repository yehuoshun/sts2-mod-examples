using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例 Transpiler：用「结构判定」匹配泛型方法调用（跨闭合泛型实例稳定）。
/// 对照 skill：harmony/harmony-transpiler.md。
/// 真实 API：CodeInstruction/OpCodes（0Harmony.Cecil）+ MethodInfo.DeclaringType.GetGenericTypeDefinition()。
/// ⚠️ 不要用 AccessTools.Method 拿到的 MemberInfo 和 operand 比引用（开放泛型 ≠ 闭合实例）。
/// </summary>
[HarmonyPatch(typeof(ModelIdSerializationCache), nameof(ModelIdSerializationCache.Init))]
public static class ExampleTranspilerPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            Type? declaring = (instruction.operand as MethodInfo)?.DeclaringType;
            bool matchesGenericSort = instruction.opcode == OpCodes.Callvirt
                && instruction.operand is MethodInfo method
                && method.Name == nameof(List<(Type, Mod)>.Sort)
                && declaring?.IsGenericType == true
                && declaring.GetGenericTypeDefinition() == typeof(List<>);

            if (matchesGenericSort)
            {
                // 命中：此处可改 opcode / 替换 operand（示例不改动，仅演示结构判定）
                MainFile.Logger.Info("matched generic List<>.Sort call");
            }

            yield return instruction;
        }
    }
}
