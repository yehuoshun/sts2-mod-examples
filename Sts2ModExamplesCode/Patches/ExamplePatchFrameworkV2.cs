using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：属性式 Patch 框架 v2 —— VanillaGuard IL 指纹守卫 + 冲突拓扑分析。
/// 对照 skill：harmony/harmony-patch-framework-v2.md。
/// 真实 API：MethodBase.GetMethodBody()?.GetILAsByteArray()；AsyncStateMachineAttribute.StateMachineType；
/// Harmony.GetAllPatchedMethods()/GetPatchInfo；PatchProcessor.GetSortedPatchMethods；
/// ModManager.OnModDetected 事件。
/// </summary>
internal static class ExampleVanillaGuard
{
    internal static string Key(MethodBase method)
        => $"{method.DeclaringType!.FullName}::{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName))})";

    /// <summary>async 入口只创建状态机；结算逻辑在 MoveNext，必须一同冻结。</summary>
    internal static string Fingerprint(MethodInfo method)
    {
        MethodInfo? moveNext = method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType
            .GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        string bodies = string.Join("|", new[] { method, moveNext }.Where(static i => i != null)
            .Select(i => Convert.ToHexString(i!.GetMethodBody()?.GetILAsByteArray() ?? [])));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(bodies)));
    }

    /// <summary>校验：快照缺失 = 版本结构变了（fail-fast）；指纹不匹配 = 方法体悄悄改了（降级 Warn）。</summary>
    internal static void Verify(MethodInfo target, IReadOnlyDictionary<string, string> frozen)
    {
        if (!frozen.TryGetValue(Key(target), out string? expected))
        {
            throw new InvalidOperationException($"Missing frozen target: {Key(target)}");
        }

        if (expected != Fingerprint(target))
        {
            MainFile.Logger.Warn($"[Guard] DRIFT {Key(target)}");
        }
        else
        {
            MainFile.Logger.Info($"[Guard] OK {Key(target)}");
        }
    }
}

/// <summary>示例：共享补丁目标 + skipping prefix 拓扑分析（启动日志提前告知潜在冲突）。</summary>
internal static class ExampleConflictReporter
{
    private const string HarmonyId = "Sts2ModExamples.PatchFrameworkV2";

    internal static void ReportConflicts()
    {
        foreach (MethodBase target in Harmony.GetAllPatchedMethods())
        {
            HarmonyLib.Patches? info = Harmony.GetPatchInfo(target);
            if (info == null || !info.Owners.Contains(HarmonyId)) continue;

            // 共享目标列表
            foreach (string owner in info.Owners.Where(static o => o != HarmonyId))
            {
                MainFile.Logger.Info($"[Conflict] {target.Name} shared with {owner}");
            }

            // 拓扑序里自己的 bool prefix 之后的他人 prefix 会被跳过
            var order = PatchProcessor.GetSortedPatchMethods(target, info.Prefixes.ToArray());
            int skipping = order.FindIndex(m => info.Prefixes.Any(p => p.PatchMethod == m
                && p.owner == HarmonyId && m.ReturnType == typeof(bool)));
            if (skipping < 0) continue;
            foreach (var method in order.Skip(skipping + 1))
            {
                foreach (var patch in info.Prefixes.Where(p => p.PatchMethod == method && p.owner != HarmonyId))
                {
                    MainFile.Logger.Warn($"[Conflict] {patch.owner}.{method.Name} follows our skipping prefix; may be skipped.");
                }
            }
        }
    }
}

/// <summary>示例：ModManager.OnModDetected 订阅（后加载的 mod 补测冲突）。</summary>
internal static class ExampleDynamicConflictRescan
{
    private static bool _subscribed;

    internal static void EnsureSubscribed()
    {
        if (_subscribed) return;
        ModManager.OnModDetected += _ => ExampleConflictReporter.ReportConflicts();
        _subscribed = true;
    }
}