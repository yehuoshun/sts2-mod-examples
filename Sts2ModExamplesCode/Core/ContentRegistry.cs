using System.Reflection;
using MegaCrit.Sts2.Core.Modding;
using Sts2ModExamples;

namespace Sts2ModExamples.Core;

/// <summary>
/// 纯原生自动注册框架：扫描程序集里的 [CardPool]/[RelicPool]/[PotionPool] 标记，
/// 统一调用 ModHelper.AddModelToPool 注册。安全扫描（Android/Mono 兼容）+ 冻结机制。
/// 对照 skill baselib/design-patterns-core.md 模式1 + 工程细节。
/// </summary>
public static class ContentRegistry
{
    private static readonly object _lock = new();
    private static bool _frozen;

    public static int RegisteredCount { get; private set; }

    /// <summary>扫描并注册一个程序集。冻结后调用将跳过。</summary>
    public static void RegisterAll(Assembly assembly)
    {
        if (_frozen) return;

        foreach (var type in GetLoadableTypes(assembly))
        {
            if (type.IsAbstract) continue;

            var cardPool = type.GetCustomAttribute<CardPoolAttribute>();
            if (cardPool != null)
            {
                ModHelper.AddModelToPool(cardPool.PoolType, type);
                RegisteredCount++;
                continue;
            }

            var relicPool = type.GetCustomAttribute<RelicPoolAttribute>();
            if (relicPool != null)
            {
                ModHelper.AddModelToPool(relicPool.PoolType, type);
                RegisteredCount++;
                continue;
            }

            var potionPool = type.GetCustomAttribute<PotionPoolAttribute>();
            if (potionPool != null)
            {
                ModHelper.AddModelToPool(potionPool.PoolType, type);
                RegisteredCount++;
            }
        }
    }

    /// <summary>冻结注册：ModelDb.Init 之后调用，阻止晚期注册污染。</summary>
    public static void Freeze()
    {
        lock (_lock) _frozen = true;
    }

    /// <summary>安全加载类型：反射异常时跳过（Android/Mono 常见）。</summary>
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            MainFile.Logger.Warn($"类型加载不完整（{assembly.GetName().Name}）: {ex.LoaderExceptions.Length} 个类型跳过");
            return ex.Types.Where(t => t != null)!;
        }
    }
}
