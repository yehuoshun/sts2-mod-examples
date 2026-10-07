using System.Reflection;
using DetourHook = MonoMod.RuntimeDetour.Hook;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：MonoMod RuntimeDetour（DetourHook）——Harmony 之外的第二种运行时改写方案。
/// 对照 skill：harmony/harmony-detour-monomod.md。
/// 真实 API：MonoMod.RuntimeDetour.Hook(目标方法, detour 方法组)；detour 签名 = (Orig 委托, 原参数...)；
/// detour 内手动调用 orig。⚠️ 本示例仅编译验证，运行时需游戏自带/随包 MonoMod.RuntimeDetour.dll。
/// </summary>
public static class ExampleMonoModDetour
{
    // ① 原方法签名委托
    private delegate void OrigSetMaxAscension(NAscensionPanel self, int maxAscension);

    private static DetourHook? _setMaxAscensionHook;

    /// <summary>安装：new DetourHook(目标方法, detour)。字段持引用防 GC。</summary>
    public static void Install()
    {
        _setMaxAscensionHook = new DetourHook(
            typeof(NAscensionPanel).GetMethod(nameof(NAscensionPanel.SetMaxAscension),
                BindingFlags.Instance | BindingFlags.Public, binder: null,
                new[] { typeof(int) }, modifiers: null)
                ?? throw new InvalidOperationException("Could not find NAscensionPanel.SetMaxAscension."),
            SetMaxAscensionDetour);
    }

    // ② detour：第一个参数是 orig 委托，后续参数与原方法一致；手动调用 orig
    private static void SetMaxAscensionDetour(OrigSetMaxAscension orig, NAscensionPanel self, int maxAscension)
    {
        maxAscension = Math.Max(maxAscension, 15);   // 改参数：进阶上限提到 15
        orig(self, maxAscension);                     // 调原方法
    }
}

/// <summary>示例：静态方法 detour（返回 decimal）。</summary>
public static class ExampleMonoModStaticDetour
{
    private delegate decimal OrigModifyCardRewardUpgradeOdds(
        MegaCrit.Sts2.Core.Runs.IRunState runState, MegaCrit.Sts2.Core.Entities.Players.Player player,
        CardModel card, decimal originalOdds);

    private static DetourHook? _hook;

    public static void Install()
    {
        var target = typeof(MegaCrit.Sts2.Core.Hooks.Hook).GetMethod(
            nameof(MegaCrit.Sts2.Core.Hooks.Hook.ModifyCardRewardUpgradeOdds),
            BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Hook not found.");

        _hook = new DetourHook(target, ModifyCardRewardUpgradeOddsDetour);
    }

    private static decimal ModifyCardRewardUpgradeOddsDetour(OrigModifyCardRewardUpgradeOdds orig,
        MegaCrit.Sts2.Core.Runs.IRunState runState, MegaCrit.Sts2.Core.Entities.Players.Player player,
        CardModel card, decimal originalOdds)
    {
        if (runState.AscensionLevel >= 15)
        {
            return decimal.MinValue;   // 15 进阶：已升级卡零概率
        }

        return orig(runState, player, card, originalOdds);
    }
}