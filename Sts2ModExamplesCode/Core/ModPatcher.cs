using System.Reflection;
using HarmonyLib;
using Sts2ModExamples;

namespace Sts2ModExamples.Core;

/// <summary>
/// Harmony 批量补丁安全模式：逐类 Patch + try-catch，坏类型只跳过不中断初始化。
/// 对照 skill harmony/harmony-patches.md（PatchAllSafe）。
/// </summary>
public static class ModPatcher
{
    /// <summary>
    /// 批量应用程序集内所有 [HarmonyPatch] 类。exclude 里的类名跳过（留给调用方单独条件应用）。
    /// </summary>
    public static void PatchAllSafe(Harmony harmony, Assembly assembly, IReadOnlySet<string>? exclude = null)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<HarmonyPatch>() == null) continue;
            if (exclude?.Contains(type.Name) == true) continue;

            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception e)
            {
                MainFile.Logger.Error($"Patch failed [{type.Name}]: {e.Message}");
            }
        }
    }
}
