using Godot;
using MegaCrit.Sts2.addons.mega_text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例增幅卡（Amplify/Kicker）。对照 skill：card/card-amplify.md。
/// 增幅 = 多付额外费用换更强效果：1 费打 6，增幅共 2 费打 12。
/// 三件套：基类费用结算 + CardEnergyCost Patch（扣费）+ NCard Patch（标签）+ NHandCardHolder Patch（高亮）。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleAmplifyCard : AmplifiedCard
{
    public ExampleAmplifyCard() : base(1, 1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new DamageVar("DamageAmplified", 12, ValueProp.Move)]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var damage = !AmplifiedInPlay
            ? DynamicVars.Damage.BaseValue
            : DynamicVars["DamageAmplified"].BaseValue;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["DamageAmplified"].UpgradeValueBy(3);
    }
}

/// <summary>增幅卡基类：Kicker 费用结算 + 增幅状态（纯原生，零 BaseLib）。</summary>
public abstract class AmplifiedCard(
    int baseCost, int kickerCost, CardType type, CardRarity rarity, TargetType target)
    : CardModel(baseCost, type, rarity, target)
{
    public int KickerCost { get; } = kickerCost;
    public bool AmplifiedInPlay { get; protected set; }
    public bool AmplifiedInPreview { get; private set; }

    public static readonly Color AmplifiedGlowColor = new("5244ff");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new EnergyVar(KickerCost)]);

    /// <summary>费用结算：增幅则把基础费 + Kicker（被 CostPatch 调用，返回是否增幅）。</summary>
    public bool CalculateAmplifiedCost(ref int cost)
    {
        if (Owner.PlayerCombatState?.Energy < cost + KickerCost) return false;
        cost += KickerCost;
        return true;
    }

    /// <summary>预览判定：手牌 + 能量够增幅价 → 高亮/标签切增幅态。</summary>
    public void RefreshPreviewState()
    {
        AmplifiedInPreview = Pile?.Type == PileType.Hand &&
                             Owner.PlayerCombatState?.Energy >=
                             EnergyCost.GetWithModifiers(CostModifiers.All) + KickerCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 标记本次是否增幅：AutoPlay 不经过费用 Patch，强制按增幅结算
        AmplifiedInPlay = cardPlay.IsAutoPlay || AmplifiedInPreview;
        await Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card == this && cardPlay.IsLastInSeries) AmplifiedInPlay = false;
        return Task.CompletedTask;
    }
}

/// <summary>费用计算 Patch：打牌扣费走这里，把基础费改成基础费 + Kicker。</summary>
[HarmonyPatch(typeof(CardEnergyCost), "GetAmountToSpend")]
public static class AmplifyCostPatch
{
    private static void Postfix(CardEnergyCost __instance, CardModel ____card, ref int __result)
    {
        if (____card is AmplifiedCard amp)
            amp.CalculateAmplifiedCost(ref __result);
    }
}

/// <summary>手牌费用标签 Patch：悬停时显示增幅后总费用（____energyLabel = 私有字段注入）。</summary>
[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
public static class AmplifyCostVisualPatch
{
    private static void Postfix(NCard __instance, PileType pileType, MegaLabel ____energyLabel)
    {
        if (pileType != PileType.Hand) return;
        if (__instance.Model is not AmplifiedCard amp) return;
        var hovered = RunManager.Instance.HoveredModelTracker
            .GetHoveredModel(amp.Owner.NetId) == amp;
        if (!hovered) return;

        amp.RefreshPreviewState();
        var cost = amp.EnergyCost.GetWithModifiers(CostModifiers.All) + amp.KickerCost;
        ____energyLabel.SetTextAutoSize(cost.ToString());
    }
}

/// <summary>可打高亮 Patch：增幅态用专属辉光色。</summary>
[HarmonyPatch(typeof(NHandCardHolder), "UpdateCard")]
public static class AmplifyHighlightPatch
{
    private static void Postfix(NHandCardHolder __instance)
    {
        if (__instance.CardNode?.Model is not AmplifiedCard amp) return;
        if (amp.Model.CanPlay() != true) return;
        __instance.CardNode.CardHighlight.Modulate =
            amp.AmplifiedInPreview ? AmplifiedGlowColor : NCardHighlight.playableColor;
    }
}

/// <summary>悬停刷新 Patch：切换悬停时重算手牌标签。</summary>
[HarmonyPatch(typeof(RunManager), "InitializeShared")]
public static class AmplifyHoverRefreshPatch
{
    private static void Postfix(RunManager __instance)
    {
        __instance.HoveredModelTracker.HoverChanged += OnHoverChanged;
    }

    private static void OnHoverChanged(ulong player)
    {
        var hovered = RunManager.Instance.HoveredModelTracker.GetHoveredModel(player);
        if (hovered is AmplifiedCard amp)
            NCard.FindOnTable(amp)?.UpdateVisuals(PileType.Hand, CardPreviewMode.None);
    }
}
