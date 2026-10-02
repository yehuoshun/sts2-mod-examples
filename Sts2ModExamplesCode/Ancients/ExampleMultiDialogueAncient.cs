using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Ancients;

/// <summary>
/// 示例多角色对话先古之民：每个角色专属对话 + 第 2 次拜访台词。
/// 对照 skill：event/event-ancient-core.md。
/// 真实 API：AncientDialogueSet.CharacterDialogues（required）、AncientEventModel.CharKey&lt;T&gt;()、AncientDialogue.VisitIndex。
/// </summary>
public class ExampleMultiDialogueAncient : AncientEventModel
{
    protected override AncientDialogueSet DefineDialogues()
    {
        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = new AncientDialogue(""),
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
            {
                [AncientEventModel.CharKey<Characters.ExampleCharacter>()] = new List<AncientDialogue>
                {
                    new AncientDialogue("") { VisitIndex = 0 },
                    new AncientDialogue("") { VisitIndex = 1 },   // 第 2 次拜访
                },
            },
            AgnosticDialogues = new List<AncientDialogue>
            {
                new AncientDialogue(""),
            },
        };
    }

    public override IEnumerable<EventOption> AllPossibleOptions =>
        new List<EventOption>
        {
            new EventOption(this, OnBless, "EXAMPLE_MD_ANCIENT.options.BLESS"),
        };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        AllPossibleOptions.ToList();

    private async Task OnBless()
    {
        await RelicCmd.Obtain<Relics.ExampleRelic>(Owner!);
        SetEventFinished(L10NLookup("EXAMPLE_MD_ANCIENT.options.BLESS.description"));
    }
}
