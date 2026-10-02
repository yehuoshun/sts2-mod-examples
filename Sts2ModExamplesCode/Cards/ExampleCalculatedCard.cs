using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例计算伤害卡：伤害 = (基础 6 + 额外 2 × 场上敌人数量) × 全局系数。
/// 对照 skill：card/card-variables.md（真实 API 版）。
/// 原生机制（PerfectedStrike 同款）：CalculationBaseVar + ExtraDamageVar + CalculatedDamageVar(ValueProp).WithMultiplier()。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleCalculatedCard : CardModel
{
    public ExampleCalculatedCard() : base(
        baseCost: 1,
        type: CardType.Attack,
        rarity: CardRarity.Uncommon,
        target: TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new CalculationBaseVar(6m);                    // 基础伤害
            yield return new ExtraDamageVar(2m);                        // 每层倍率的额外伤害
            yield return new CalculatedDamageVar(ValueProp.Move)        // 显示 {CalculatedDamage:diff()}
                .WithMultiplier((card, target) => card.CombatState?.Enemies?.Count ?? 0);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }
}
