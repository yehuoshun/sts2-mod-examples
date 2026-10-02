using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;

namespace Sts2ModExamples.Ancients;

/// <summary>
/// 示例先古之民：对话组 + 一个「获得遗物」选项。
/// 对照 skill：event/event-ancient-core.md。
/// 真实 API：DefineDialogues()/AllPossibleOptions（AncientEventModel 原生成员）。
/// </summary>
public class ExampleAncient : AncientEventModel
{
    protected override AncientDialogueSet DefineDialogues()
    {
        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = new AncientDialogue(""),   // 无音效单行
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>(),   // required 必填
            AgnosticDialogues = new List<AncientDialogue>
            {
                new AncientDialogue(""),
            },
        };
    }

    public override IEnumerable<EventOption> AllPossibleOptions =>
        new List<EventOption>
        {
            new EventOption(this, OnBless, "EXAMPLE_ANCIENT.options.BLESS"),
        };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        AllPossibleOptions.ToList();

    private async Task OnBless()
    {
        await RelicCmd.Obtain<Relics.ExampleRelic>(Owner!);
        SetEventFinished(L10NLookup("EXAMPLE_ANCIENT.options.BLESS.description"));
    }
}
