using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例攻击牌。对照 skill：card/card-constructor.md + card/card-api.md。
/// 真实 API：DamageCmd.Attack(decimal).FromCard().Targeting().WithHitFx().Execute(PlayerChoiceContext)。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]   // 无色卡池（ContentRegistry 自动注册）
public class ExampleStrike : CardModel
{
    public ExampleStrike() : base(
        1,
        type: CardType.Attack,
        rarity: CardRarity.Common,
        targetType: TargetType.AnyEnemy,
        shouldShowInCardLibrary: true)
    {
    }

    // 打击系标签（被"完美打击"类协同计算）
    public override CardTag[] Tags => new[] { CardTag.Strike };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(6)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);   // 升级 +3 伤害
    }
}
