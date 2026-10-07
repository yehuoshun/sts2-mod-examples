using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Events;

/// <summary>
/// 示例：共享事件注入（ModelDb.AllSharedEvents getter postfix）。
/// 对照 skill：event/event-shared-inject.md。
/// 真实 API：ModelDb.AllSharedEvents 是 static getter（内部缓存字段）；ModelDb.Event&lt;T&gt;()。
/// </summary>
public static class ExampleSharedEventRegistration
{
    private static bool _installed;

    public static void Install()
    {
        if (_installed) return;

        MethodInfo getter = typeof(ModelDb)
            .GetProperty(nameof(ModelDb.AllSharedEvents), BindingFlags.Static | BindingFlags.Public)?.GetMethod
            ?? throw new InvalidOperationException("Could not find ModelDb.AllSharedEvents getter.");

        new Harmony("Sts2ModExamples.SharedEvent")
            .Patch(getter, postfix: new HarmonyMethod(typeof(ExampleSharedEventRegistration), nameof(AppendSharedEvent)));
        _installed = true;
    }

    private static void AppendSharedEvent(ref IEnumerable<EventModel> __result)
    {
        __result = __result.Concat([ModelDb.Event<ExampleSharedEvent>()]).Distinct();
    }
}

/// <summary>
/// 示例共享事件：选项链式构建（遗物/扣血/锁定）+ IsAllowed 出现条件。
/// 对照 skill：event/event-shared-inject.md。
/// 真实 API：EventOption(EventModel, Func&lt;Task&gt;?, string textKey, params IHoverTip[])；
/// EventOption.WithRelic(RelicModel) / ThatDoesDamage(decimal)；EventModel.IsAllowed(IRunState)；
/// SetEventFinished(LocString) / InitialOptionKey(string) 均为 protected。
/// </summary>
public sealed class ExampleSharedEvent : EventModel
{
    private const int GreatRewardCost = 250;
    private const int RiskyHpLoss = 28;

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(static p => p.Gold >= GreatRewardCost || p.Creature.CurrentHp >= RiskyHpLoss + 1);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        Player owner = Owner ?? throw new InvalidOperationException("No owner.");
        return
        [
            new EventOption(this, Greet, InitialOptionKey("GREET")),
            owner.Gold >= GreatRewardCost
                ? new EventOption(this, GreatReward, InitialOptionKey("GREAT_REWARD"))
                    .WithRelic(ModelDb.Relic<Patches.ExampleStarterRelic>().ToMutable())
                : new EventOption(this, null, InitialOptionKey("GREAT_REWARD_LOCKED")),   // 锁定选项（置灰）
            owner.Creature.CurrentHp >= RiskyHpLoss + 1
                ? new EventOption(this, RiskyReward, InitialOptionKey("RISKY_REWARD"))
                    .ThatDoesDamage(RiskyHpLoss)
                : new EventOption(this, null, InitialOptionKey("RISKY_REWARD_LOCKED"))
        ];
    }

    private async Task Greet()
    {
        Player owner = Owner ?? throw new InvalidOperationException("No owner.");
        await PlayerCmd.GainGold(20m, owner);
        SetEventFinished(new MegaCrit.Sts2.Core.Localization.LocString("events", "EXAMPLE_SHARED_EVENT.doneGreet"));
    }

    private async Task GreatReward()
    {
        Player owner = Owner ?? throw new InvalidOperationException("No owner.");
        await PlayerCmd.LoseGold(GreatRewardCost, owner, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Spent);
        await RelicCmd.Obtain<Patches.ExampleStarterRelic>(owner);
        await CreatureCmd.GainMaxHp(owner.Creature, 6m);
        SetEventFinished(new MegaCrit.Sts2.Core.Localization.LocString("events", "EXAMPLE_SHARED_EVENT.doneGreatReward"));
    }

    private async Task RiskyReward()
    {
        Player owner = Owner ?? throw new InvalidOperationException("No owner.");
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature,
            RiskyHpLoss, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable | MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, dealer: null);
        await RelicCmd.Obtain<Patches.ExampleStarterRelic>(owner);
        SetEventFinished(new MegaCrit.Sts2.Core.Localization.LocString("events", "EXAMPLE_SHARED_EVENT.doneRiskyReward"));
    }
}