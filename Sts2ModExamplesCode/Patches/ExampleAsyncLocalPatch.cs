using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：AsyncLocal 异步 Patch。对照 skill：harmony/harmony-async-local.md。
/// Prefix 不能 await；原方法返回 Task 时，在 Prefix 里构造异步委托赋给 ref Task __result，
/// 用 AsyncLocal 跨 Prefix/Postfix 传递。适合「替换原异步方法」。
/// </summary>
[HarmonyPatch(typeof(Entomancer), "SpitMove")]
public static class ExampleAsyncLocalPatch
{
    private static AsyncLocal<Func<Task>> _asyncWork = new();

    private static bool Prefix(Entomancer __instance, ref Task __result)
    {
        if (__instance.Creature.HasPower<ExampleFreezePower>())
        {
            return true; // 有特定能力 → 走原逻辑
        }

        _asyncWork.Value = async () =>
        {
            await CreatureCmd.TriggerAnim(__instance.Creature, "Cast", 0.5f);
            await PowerCmd.Apply<ExampleFreezePower>(
                new ThrowingPlayerChoiceContext(), __instance.Creature, 1, __instance.Creature, null);
        };

        __result = _asyncWork.Value();
        return false; // 跳过原方法体
    }

    private static void Postfix()
    {
        _asyncWork.Value = null!; // 清理，防跨帧残留
    }
}

/// <summary>配合示例用的简易能力（真实 API：PowerModel 子类）。</summary>
public class ExampleFreezePower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
