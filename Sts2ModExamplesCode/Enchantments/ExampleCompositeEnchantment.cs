using System.Text.Json;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Enchantments;

/// <summary>
/// 示例：复合附魔容器（一个 EnchantmentModel 装多个附魔）。
/// 对照 skill：enchantment/enchantment-composite.md + enchantment-multi.md。
/// 真实 API：EnchantmentModel.ToSerializable()/FromSerializable()/ApplyInternal()/ClearInternal()/ModifyCard()/
/// InvokeExecutionFinished()（AbstractModel）/StatusChanged 事件/Enchant*Additive/Multiplicative/DeepCloneFields。
/// </summary>
public sealed class ExampleCompositeEnchantment : EnchantmentModel
{
    private List<EnchantmentModel> _innerEnchantments = new();

    private List<EnchantmentModel> _subscribedInnerEnchantments = new();

    // [SavedProperty] 只能存基础类型 → 整个内部列表序列化为 JSON 字符串
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    private string? SavedEnchantmentsJson
    {
        get => _innerEnchantments.Count == 0 ? null
            : JsonSerializer.Serialize(_innerEnchantments.Select(static e => e.ToSerializable()).ToArray());
        set
        {
            UnsubscribeFromInnerEnchantments();
            _innerEnchantments = new List<EnchantmentModel>();
            if (string.IsNullOrWhiteSpace(value)) return;
            SerializableEnchantment[]? serialized = JsonSerializer.Deserialize<SerializableEnchantment[]>(value);
            if (serialized == null) return;
            foreach (SerializableEnchantment item in serialized)
            {
                _innerEnchantments.Add(EnchantmentModel.FromSerializable(item));
            }

            Amount = _innerEnchantments.Count;
            RefreshCompositeStatus();
        }
    }

    public override bool CanEnchant(CardModel card) => false;   // 容器本身不可直接附魔

    public IReadOnlyList<EnchantmentModel> InnerEnchantments => _innerEnchantments;

    /// <summary>附魔入口调用：同类叠层，异类新增。</summary>
    public EnchantmentModel AddOrStackEnchantment(EnchantmentModel enchantment, decimal amount)
    {
        EnchantmentModel? existing = _innerEnchantments.FirstOrDefault(e => e.GetType() == enchantment.GetType());
        if (existing != null)
        {
            existing.Amount += (int)amount;
            existing.RecalculateValues();
            Card.DynamicVars.RecalculateForUpgradeOrEnchant();
            RefreshCompositeStatus();
            return existing;
        }

        enchantment.ApplyInternal(Card, amount);
        _innerEnchantments.Add(enchantment);
        SubscribeToInnerEnchantment(enchantment);
        Amount = _innerEnchantments.Count;
        enchantment.ModifyCard();
        RefreshCompositeStatus();
        return enchantment;
    }

    // ---- 回调转发 ----

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        foreach (EnchantmentModel e in _innerEnchantments)
        {
            await e.OnPlay(choiceContext, cardPlay);
            e.InvokeExecutionFinished();   // AbstractModel：多人流程结算钩子
        }

        RefreshCompositeStatus();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        foreach (EnchantmentModel e in _innerEnchantments)
        {
            await e.AfterCardPlayed(context, cardPlay);
        }
    }

    // ---- 数值链式计算（加→乘，对外返回差值/1m） ----

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        decimal current = originalDamage;
        foreach (EnchantmentModel e in _innerEnchantments)
        {
            current += e.EnchantDamageAdditive(current, props);
            current *= e.EnchantDamageMultiplicative(current, props);
        }

        return current - originalDamage;
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props) => 1m;

    public override int EnchantPlayCount(int originalPlayCount)
    {
        int current = originalPlayCount;
        foreach (EnchantmentModel e in _innerEnchantments)
        {
            current = e.EnchantPlayCount(current);
        }

        return current;
    }

    // ---- 内部绑定 / 状态聚合 / 克隆 ----

    private void EnsureInnerBindings()
    {
        if (!HasCard) return;
        foreach (EnchantmentModel e in _innerEnchantments)
        {
            if (!e.HasCard || !ReferenceEquals(e.Card, Card))
            {
                if (e.HasCard) e.ClearInternal();
                e.ApplyInternal(Card, e.Amount);
            }

            SubscribeToInnerEnchantment(e);
        }
    }

    private void SubscribeToInnerEnchantment(EnchantmentModel enchantment)
    {
        if (_subscribedInnerEnchantments.Contains(enchantment)) return;
        enchantment.StatusChanged += OnInnerEnchantmentStatusChanged;
        _subscribedInnerEnchantments.Add(enchantment);
    }

    private void UnsubscribeFromInnerEnchantments()
    {
        foreach (EnchantmentModel e in _subscribedInnerEnchantments)
        {
            e.StatusChanged -= OnInnerEnchantmentStatusChanged;
        }

        _subscribedInnerEnchantments.Clear();
    }

    private void OnInnerEnchantmentStatusChanged() => RefreshCompositeStatus();

    private void RefreshCompositeStatus()
    {
        Status = _innerEnchantments.Any(static e => e.Status == EnchantmentStatus.Normal)
            ? EnchantmentStatus.Normal
            : EnchantmentStatus.Disabled;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _innerEnchantments = _innerEnchantments
            .Select(static e => (EnchantmentModel)e.ClonePreservingMutability())
            .ToList();
        _subscribedInnerEnchantments = new List<EnchantmentModel>();
    }
}

/// <summary>内部附魔示例：攻击伤害 +2（可重复叠层）。</summary>
public sealed class ExampleLayeredEnchantment : EnchantmentModel
{
    public override bool CanEnchant(CardModel card) => card.Type == CardType.Attack;

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props) => 2m * Amount;
}
