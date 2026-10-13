using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例 Token 增幅卡：0 费 1 增幅。未增幅打 6；增幅后改打「计算伤害」——
/// CalculatedDamageVar.WithMultiplier 动态读目标身上的中毒层数作乘数（目标越毒增幅越痛）。
/// 对照 skill：card/card-amplify-examples.md（MarisaMod TabooSpark）。
/// 真实 API：CalculatedDamageVar(ValueProp).WithMultiplier(Func&lt;CardModel, Creature?, decimal&gt;)；
/// 不进阶角色卡池，只由发卡类卡牌生成（见 ExampleTeamCardGrantCard）。
/// </summary>
[CardPool(typeof(TokenCardPool))]
public class ExampleTokenAmplifyCard : AmplifiedCard
{
    public ExampleTokenAmplifyCard() : base(0, 1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new CalculationBaseVar(6),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((_, monster) => monster?.GetPowerAmount<PoisonPower>() ?? 0),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.CalculationBase.UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 增幅分支：未增幅打基础伤害，增幅后改打计算伤害（吃目标中毒层数乘数）
        var damage = !AmplifiedInPlay
            ? DynamicVars.Damage.BaseValue
            : DynamicVars.CalculatedDamage.BaseValue;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }
}
