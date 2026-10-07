using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：async 方法 transpiler（打 MoveNext 替换常量调用）。
/// 对照 skill：harmony/harmony-transpiler-async.md。
/// 真实 API：async 方法的 IL 在编译器生成的 &lt;X&gt;d__N.MoveNext 里；
/// CardPile.MaxCardsInHand 是静态 getter（原版 10）；CardPileCmd.Draw 是 async。
/// </summary>
[HarmonyPatch]
public static class ExampleAsyncTranspilerPatch
{
    private static readonly MethodInfo MaxCardsInHandGetter = AccessTools.PropertyGetter(typeof(CardPile), nameof(CardPile.MaxCardsInHand))
        ?? throw new InvalidOperationException("Could not find CardPile.MaxCardsInHand.");

    // 打 CardPileCmd.Draw 的 async 状态机 MoveNext
    private static MethodBase? TargetMethod()
    {
        MethodInfo draw = AccessTools.Method(typeof(CardPileCmd), nameof(CardPileCmd.Draw),
            new[] { typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool) });
        return GetAsyncStateMachineTarget(draw);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> patched = instructions.ToList();
        int count = 0;
        foreach (CodeInstruction instruction in patched)
        {
            if (!instruction.Calls(MaxCardsInHandGetter)) continue;
            instruction.opcode = OpCodes.Ldc_I4;   // call → 推入常量
            instruction.operand = 20;
            count++;
        }

        // 数量断言：版本更新导致调用点变化 → 启动显形而不是静默失效
        if (count != 1)
        {
            throw new InvalidOperationException($"Expected 1 MaxCardsInHand call in Draw, patched {count}.");
        }

        return patched;
    }

    private static MethodBase GetAsyncStateMachineTarget(MethodInfo method)
    {
        AsyncStateMachineAttribute? attribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
        if (attribute?.StateMachineType == null)
        {
            return method;
        }

        return attribute.StateMachineType.GetMethod("MoveNext",
                   BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
               ?? throw new InvalidOperationException("Could not find MoveNext on async state machine.");
    }
}

/// <summary>
/// 示例：静态 getter prefix 改手牌上限（配合上面 transpiler 双管齐下）。
/// 真实 API：CardPile.MaxCardsInHand 静态 getter；prefix 返回 false = 短路原版。
/// </summary>
[HarmonyPatch(typeof(CardPile), nameof(CardPile.MaxCardsInHand), MethodType.Getter)]
public static class ExampleMaxCardsInHandPatch
{
    private static bool Prefix(ref int __result)
    {
        __result = 20;
        return false;
    }
}