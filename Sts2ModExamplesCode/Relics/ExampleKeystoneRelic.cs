using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Relics;

/// <summary>
/// 示例：开局选择遗物（Keystone 模式）+ 计数器遗物。
/// 对照 skill：relic/relic-keystone-flow.md + relic-keystone-mechanics.md。
/// 真实 API：RelicModel.Status/RelicStatus、Flash(IEnumerable&lt;Creature&gt;)、CardPlay.IsLastInSeries、
/// RelicCmd.Obtain(RelicModel, Player)、SaveManager.Instance.MarkRelicAsSeen、Player.RelicGrabBag、
/// PowerModel.GetTypeForAmount(decimal)。
/// </summary>
public abstract class ExampleKeystoneRelicBase : RelicModel
{
    public sealed override RelicRarity Rarity => RelicRarity.Starter;   // 开局选择类遗物固定 Starter

    // 选择状态标记（序列化 marker：写在遗物 SavedProperty 里）
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool KeystoneRunes_SelectionHandled { get; set; }

    // 子类必须提供图标
    protected abstract string GetIconPath();

    public override string PackedIconPath => GetIconPath();

    protected override string PackedIconOutlinePath => PackedIconPath;

    protected override string BigIconPath => PackedIconPath;
}

/// <summary>计数器遗物：每 3 次连续命中同一敌人，额外造成伤害（电刑式）。</summary>
public sealed class ExampleKeystoneRelic : ExampleKeystoneRelicBase
{
    private const int RequiredHits = 3;
    private const int BaseDamage = 5;

    private int _consecutiveHitsThisTurn;
    private int _trackedTargetCombatId = -1;
    private CardModel? _currentTrackedCard;
    private bool _currentTrackedCardHadHit;

    // 战斗内瞬态字段也序列化（中途读档不丢计数）
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int SavedConsecutiveHitsThisTurn
    {
        get => _consecutiveHitsThisTurn;
        set { _consecutiveHitsThisTurn = Math.Max(0, value); RefreshVisualState(); }
    }

    public override bool ShowCounter => CombatManager.Instance?.IsInProgress == true && !IsCanonical;

    public override int DisplayAmount => !IsCanonical ? _consecutiveHitsThisTurn : 0;

    protected override string GetIconPath() => "res://Sts2ModExamples/images/relics/example_keystone.png";

    public override Task BeforeCombatStart() { ResetTracking(); return Task.CompletedTask; }

    public override Task AfterCombatEnd(CombatRoom room) { ResetTracking(); return Task.CompletedTask; }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card?.Owner != Owner) return Task.CompletedTask;
        _currentTrackedCard = cardPlay.Card;
        _currentTrackedCardHadHit = false;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) ResetTracking();
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner?.Creature || target.Side != CombatSide.Enemy
            || result.TotalDamage <= 0 || props.HasFlag(ValueProp.Unpowered))
        {
            return;
        }

        if (cardSource == null || cardSource.Owner != Owner || cardSource.TargetType != TargetType.AnyEnemy)
        {
            return;
        }

        if (ReferenceEquals(cardSource, _currentTrackedCard)) _currentTrackedCardHadHit = true;

        int targetCombatId = target.CombatId.HasValue ? checked((int)target.CombatId.Value) : -1;
        _consecutiveHitsThisTurn = targetCombatId == _trackedTargetCombatId
            ? _consecutiveHitsThisTurn + 1
            : (_trackedTargetCombatId = targetCombatId, 1).Item2;
        RefreshVisualState();

        if (_consecutiveHitsThisTurn < RequiredHits || !target.IsAlive) return;

        ResetTracking();
        Flash([target]);
        await CreatureCmd.Damage(choiceContext, target, BaseDamage,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner!.Creature, cardSource: null);
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出牌没造成伤害 → 清计数；多段攻击结束 → 清追踪
        if (ReferenceEquals(cardPlay.Card, _currentTrackedCard) && !_currentTrackedCardHadHit)
        {
            ResetTracking();
        }

        if (cardPlay.IsLastInSeries)
        {
            _currentTrackedCard = null;
            _currentTrackedCardHadHit = false;
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player) ResetTracking();
        return Task.CompletedTask;
    }

    private void ResetTracking()
    {
        _consecutiveHitsThisTurn = 0;
        _trackedTargetCombatId = -1;
        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        Status = _consecutiveHitsThisTurn >= RequiredHits - 1 ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }
}

/// <summary>选择流程骨架：标记已见 + 剔出随机池 + 获得。</summary>
public static class ExampleKeystoneSelection
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Player, SelectionState> States = new();

    public static async Task EnsureSelected(Player player)
    {
        // 图鉴「已见」标记（选择界面出现过的遗物进图鉴）
        SaveManager.Instance.MarkRelicAsSeen(ModelDb.Relic<ExampleKeystoneRelic>());
        // 从随机池剔除（开局已选的不该自然掉落）
        player.RelicGrabBag.Remove(ModelDb.Relic<ExampleKeystoneRelic>());
        player.RunState.SharedRelicGrabBag.Remove(ModelDb.Relic<ExampleKeystoneRelic>());

        if (States.TryGetValue(player, out SelectionState? state) && state.Handled) return;
        if (player.Relics.Any(static r => r is ExampleKeystoneRelic)) return;

        // 真实项目这里弹自定义选择界面（见 skill relic-keystone-mechanics.md ④）
        await RelicCmd.Obtain(ModelDb.Relic<ExampleKeystoneRelic>().ToMutable(), player);
        States.GetOrCreateValue(player).Handled = true;
    }

    private sealed class SelectionState
    {
        public bool Handled { get; set; }
    }
}

/// <summary>继承原版临时力量改来源（遗物来源显示，隐藏本体）。</summary>
public sealed class ExampleKeystoneTempStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<ExampleKeystoneRelic>();

    protected override bool IsVisibleInternal => false;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
}

/// <summary>敌方 debuff 判定辅助（GetTypeForAmount：按层数符号判 Buff/Debuff）。</summary>
public static class ExampleKeystoneDebuffHelper
{
    public static bool IsOwnedEnemyDebuff(PowerModel power, decimal amount, out Creature? target)
    {
        target = power.Owner;
        return amount != 0m
            && power.GetTypeForAmount(amount) == PowerType.Debuff
            && target?.Side == CombatSide.Enemy
            && power is not ITemporaryPower;
        // 真实项目再加施放者判定：applier == 遗物Owner?.Creature（Creature.Player 是另一方向 API）
    }
}
