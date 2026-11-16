using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例「历史缩放打击」：伤害乘数取自本场战斗历史——本场生成过几张灼伤（Burn）就乘几。
/// 对照 skill：card/card-variables.md「乘数取自战斗历史（动态倍率）」。
/// 原生机制：CombatManager.Instance.History.Entries + CardGeneratedEntry + CalculatedDamageVar.WithMultiplier。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleHistoryScaledCard : CardModel
{
    public ExampleHistoryScaledCard() : base(
        1,
        type: CardType.Attack,
        rarity: CardRarity.Uncommon,
        targetType: TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new CalculationBaseVar(4m);        // 基础伤害
            yield return new ExtraDamageVar(2m);            // 每层历史倍率的额外伤害
            yield return new CalculatedDamageVar(ValueProp.Move)
                // 乘数 = 本场战斗里「生成灼伤」的历史条数（动态求值，打出瞬间才读）
                .WithMultiplier((_, _) => CombatManager.Instance.History.Entries
                    .Count(x => x is CardGeneratedEntry { Card: Burn }));
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }
}
