using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 示例：临时力量装饰层（ITemporaryPower + 负值抵消）。
/// 对照 skill：power/power-signature-and-temp.md。
/// 真实 API：ITemporaryPower 在 MegaCrit.Sts2.Core.Models（OriginModel / InternallyAppliedPower / IgnoreNextInstance）；
/// PowerModel.BeforeApplied(Creature, decimal, Creature?, CardModel?)；AbstractModel.AfterPowerAmountChanged
/// （带 PlayerChoiceContext 第一参）；AbstractModel.AfterSideTurnEnd(choiceContext, side, participants)。
/// </summary>
public abstract class ExampleTemporaryStatPower<TStatPower> : PowerModel, ITemporaryPower
    where TStatPower : PowerModel
{
    private bool _shouldIgnoreNextInstance;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public abstract AbstractModel OriginModel { get; }

    public PowerModel InternallyAppliedPower => ModelDb.Power<TStatPower>();

    public void IgnoreNextInstance() => _shouldIgnoreNextInstance = true;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        // 同步到真实力量（silent 避免双 UI 闪烁；PowerCmd.Apply 第一参必是 PlayerChoiceContext）
        await PowerCmd.Apply<TStatPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount == Amount)
        {
            return; // 层数没变：不重复应用
        }

        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<TStatPower>(new ThrowingPlayerChoiceContext(), Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || Amount <= 0)
        {
            return;
        }

        Creature owner = Owner;
        decimal amount = Amount;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<TStatPower>(owner, -amount, owner, null); // 负值抵消真力量
    }
}

/// <summary>临时力量具体实现：OriginModel 指向来源卡。</summary>
public sealed class ExampleTemporaryStrengthPower : ExampleTemporaryStatPower<StrengthPower>
{
    public override AbstractModel OriginModel => ModelDb.Card<ExampleTemporaryStrengthCard>();
}

/// <summary>隐藏提示能力：不进状态栏，只用于 hover 提示。</summary>
public sealed class ExampleHiddenTipPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false; // PowerModel.IsVisible 内部检查

    protected override System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> ExtraHoverTips =>
    [
        new MegaCrit.Sts2.Core.HoverTips.HoverTip(Title, Description.GetFormattedText())
        {
            Id = Id.ToString(),
            IsDebuff = Type == PowerType.Debuff,
            IsSmart = false,
        }
    ];
}

/// <summary>仅用于 OriginModel 指向的占位卡（不参与卡池）。</summary>
public sealed class ExampleTemporaryStrengthCard : CardModel
{
    public ExampleTemporaryStrengthCard()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("TemporaryStrength", 1m)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return PowerCmd.Apply<ExampleTemporaryStrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
            DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
    }
}
