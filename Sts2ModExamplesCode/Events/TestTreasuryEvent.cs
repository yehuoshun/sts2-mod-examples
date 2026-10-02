using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Events;

/// <summary>
/// 【skill 全面测试】测试事件：双选项单页事件（拿金币 or 支付生命换遗物）。
/// 对照 skill：event/event-core.md。
/// 真实 API：GenerateInitialOptions() protected abstract、EventOption(EventModel, Func&lt;Task&gt;?, string textKey)、
/// SetEventFinished(LocString)、L10NLookup(string)。
/// ⚠️ EventModel 无 Acts 属性（章节有效性由章节配置控制）。
/// </summary>
public class TestTreasuryEvent : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, OnTakeGold, "TEST_TREASURY.pages.INITIAL.options.TAKE_GOLD"),
            new EventOption(this, OnPayForRelic, "TEST_TREASURY.pages.INITIAL.options.PAY_RELIC"),
        };
    }

    private async Task OnTakeGold()
    {
        await PlayerCmd.GainGold(75, Owner!);
        SetEventFinished(L10NLookup("TEST_TREASURY.pages.INITIAL.options.TAKE_GOLD.description"));
    }

    private async Task OnPayForRelic()
    {
        await CreatureCmd.Damage(Owner!.Creature, 15);
        await RelicCmd.Obtain(Owner, ModelDb.Relic<Relics.ExampleRelic>());
        SetEventFinished(L10NLookup("TEST_TREASURY.pages.INITIAL.options.PAY_RELIC.description"));
    }
}
