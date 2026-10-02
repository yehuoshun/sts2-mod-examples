using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.RestSite;

/// <summary>
/// 【skill 全面测试】测试休息点选项：花费 10 生命换取 60 金币。
/// 对照 skill：rest-site/rest-site-options.md。
/// ⚠️ 真实 API（文档模板有误，待修）：OptionId abstract + OnSelect() abstract Task&lt;bool&gt;；
/// Title 非 virtual 自动从 rest_site_ui 表 OPTION_&lt;OptionId&gt;.name 生成；IconPath 是 private 不可 override；
/// 不存在 OnOptionSelected(RestSiteRoom, RestSiteContext)。
/// </summary>
public class TestTradeOption : RestSiteOption
{
    public TestTradeOption(Player owner) : base(owner) { }

    public override string OptionId => "TEST_TRADE";

    public override async Task<bool> OnSelect()
    {
        // 真实签名：CreatureCmd.Damage(PlayerChoiceContext, Creature, decimal, ValueProp, Creature?)
        // （无 2 参重载；非战斗场景用 ThrowingPlayerChoiceContext）
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(), Owner.Creature, 10m, ValueProp.Move, Owner.Creature);
        await PlayerCmd.GainGold(60, Owner);
        return true;
    }
}
