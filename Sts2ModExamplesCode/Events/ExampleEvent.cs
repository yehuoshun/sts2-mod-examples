using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;

namespace Sts2ModExamples.Events;

/// <summary>
/// 示例事件：单页，一个「获得 50 金币」选项。
/// 对照 skill：event/event-core.md。
/// 真实 API：GenerateInitialOptions() protected abstract、EventOption(EventModel, Func&lt;Task&gt;?, string textKey)、
/// SetEventFinished(LocString)、L10NLookup(string)。
/// ⚠️ EventModel 无 Acts 属性（YuWan 自研）——章节有效性由章节配置控制。
/// </summary>
public class ExampleEvent : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, OnTakeGold, "EXAMPLE_EVENT.pages.INITIAL.options.TAKE_GOLD"),
        };
    }

    private async Task OnTakeGold()
    {
        await PlayerCmd.GainGold(50, Owner!);
        SetEventFinished(L10NLookup("EXAMPLE_EVENT.pages.INITIAL.options.TAKE_GOLD.description"));
    }
}
