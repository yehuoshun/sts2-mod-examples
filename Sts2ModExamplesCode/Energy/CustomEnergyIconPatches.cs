using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Energy;

/// <summary>大图标 Patch：拦截 EnergyIconHelper.GetPath，自定义池返回自定义路径。</summary>
[HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPath), typeof(string))]
public static class CustomEnergyBigIconPatch
{
    private static bool Prefix(string prefix, ref string __result)
    {
        var pool = ModEnergyIconCodec.DecodePool<AbstractModel>(prefix);
        if (pool is ICustomEnergyIcon { BigIconPath: string path })
        {
            __result = path;
            return false;   // 跳过原生逻辑
        }
        return true;        // 不是自定义池，走原生
    }
}

/// <summary>文本内联图标 Patch：Transpiler 替换 EnergyIconsFormatter 的拼接结果。</summary>
[HarmonyPatch]
public static class CustomEnergyTextIconPatch
{
    private static MethodBase? TargetMethod()
    {
        var type = AccessTools.TypeByName(
            "MegaCrit.Sts2.Core.Localization.Formatters." +
            "EnergyIconsFormatter");
        return AccessTools.Method(type, "TryEvaluateFormat");
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        // 找到最后一个 String.Concat(string,string,string)，把结果替换为自定义文本图标
        var concatThree = AccessTools.Method(typeof(string),
            nameof(string.Concat),
            [typeof(string), typeof(string), typeof(string)]);

        for (int i = codes.Count - 1; i >= 0; i--)
        {
            if (codes[i].opcode == OpCodes.Call &&
                codes[i].operand is MethodInfo mi &&
                mi == concatThree &&
                i + 1 < codes.Count &&
                codes[i + 1].opcode == OpCodes.Stloc_3)
            {
                var inject = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_0), // prefix
                    new CodeInstruction(OpCodes.Ldloc_3), // 刚拼好的文本
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(
                            typeof(CustomEnergyTextIconPatch),
                            nameof(ResolveTextIcon))),
                    new CodeInstruction(OpCodes.Stloc_3),
                };
                codes.InsertRange(i + 1, inject);
                break;
            }
        }
        return codes.AsEnumerable();
    }

    private static string ResolveTextIcon(string prefix, string oldText)
    {
        var pool = ModEnergyIconCodec.DecodePool<AbstractModel>(prefix);
        if (pool is ICustomEnergyIcon { TextIconPath: string path })
            return $"[img]{path}[/img]";
        return oldText;
    }
}
