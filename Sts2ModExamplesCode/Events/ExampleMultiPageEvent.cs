using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Commands;

namespace Sts2ModExamples.Events;

/// <summary>
/// 示例多页事件：第一页给金币，第二页给遗物。
/// 对照 skill：event/event-advanced.md。
/// 真实 API：SetEventState(LocString, IEnumerable&lt;EventOption&gt;) protected（563 行）。
/// </summary>
public class ExampleMultiPageEvent : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, OnTakeGold,
                "EXAMPLE_MP_EVENT.pages.INITIAL.options.TAKE_GOLD"),
            new EventOption(this, OnLeave,
                "EXAMPLE_MP_EVENT.pages.INITIAL.options.LEAVE"),
        };
    }

    private async Task OnTakeGold()
    {
        await PlayerCmd.GainGold(50, Owner!);
        // 切换到第二页（新描述 + 新选项）
        SetEventState(
            L10NLookup("EXAMPLE_MP_EVENT.pages.SECOND.description"),
            new List<EventOption>
            {
                new EventOption(this, OnTakeRelic,
                    "EXAMPLE_MP_EVENT.pages.SECOND.options.TAKE_RELIC"),
                new EventOption(this, OnLeave,
                    "EXAMPLE_MP_EVENT.pages.SECOND.options.LEAVE"),
            });
    }

    private async Task OnTakeRelic()
    {
        await RelicCmd.Obtain<Relics.ExampleRelic>(Owner!);
        SetEventFinished(L10NLookup("EXAMPLE_MP_EVENT.pages.SECOND.options.TAKE_RELIC.description"));
    }

    private async Task OnLeave()
    {
        SetEventFinished(L10NLookup("EXAMPLE_MP_EVENT.pages.INITIAL.options.LEAVE.description"));
    }
}
