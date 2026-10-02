using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2ModExamples.RestSite;

/// <summary>
/// 示例休息站选项：回复 10 点生命。
/// 对照 skill：rest-site/rest-site-options.md。
/// 真实 API：OptionId abstract、OnSelect() abstract Task&lt;bool&gt;、构造 RestSiteOption(Player owner)。
/// </summary>
public class ExampleRestOption : RestSiteOption
{
    public ExampleRestOption(Player owner) : base(owner) { }

    public override string OptionId => "EXAMPLE_REST";

    public override async Task<bool> OnSelect()
    {
        await CreatureCmd.Heal(Owner.Creature, 10);
        return true;
    }
}
