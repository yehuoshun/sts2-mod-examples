using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Resources;

/// <summary>
/// 示例：把自定义图标路径并入运行期预加载列表。
/// 对照 skill：resource/resource-lifecycle.md「自定义图标预加载」。
/// 真实 API：PreloadManager.GetRunAssetPaths（private static → 用字符串名 patch）+ HarmonyPostfix ref __result。
/// </summary>
[HarmonyPatch(typeof(PreloadManager), "GetRunAssetPaths")]
public static class ExampleIconPreloadPatch
{
    [HarmonyPostfix]
    private static void AddCustomIcons(ref IEnumerable<string> __result)
        => __result = __result.Concat(GetCustomIconPaths()).Distinct(StringComparer.Ordinal);

    private static IEnumerable<string> GetCustomIconPaths()
    {
        foreach (PowerModel power in ModelDb.AllPowers)
        {
            if (power is IExampleCustomIcon icon)
            {
                yield return icon.PackedIconPath;
            }
        }

        foreach (CardPoolModel pool in ModelDb.AllCardPools)
        {
            yield return pool.EnergyIconPath;
        }
    }
}

/// <summary>自研接口：给能力模型标记自定义图标路径（原生 PowerModel 无此属性）。</summary>
public interface IExampleCustomIcon
{
    string PackedIconPath { get; }
}
