using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using Sts2ModExamples.Core;
using Sts2ModExamples.Enchantments;
using Sts2ModExamples.Powers;
using Sts2ModExamples.Relics;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例：悬停提示 + 原版容器注入。对照 skill：card/card-hover-inject.md。
/// HoverTipFactory（FromCard/FromRelic/FromEnchantment/FromKeyword/FromPower…）追加提示；
/// TrashHeap（垃圾堆商店）Relics/Cards Getter Postfix 注入自定义内容。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleHoverTipCard : CardModel
{
    public ExampleHoverTipCard() : base(
        1,
        type: CardType.Attack,
        rarity: CardRarity.Common,
        targetType: TargetType.AnyEnemy,
        shouldShowInCardLibrary: true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        base.ExtraHoverTips.Concat([
            HoverTipFactory.FromEnchantment<ExampleEnchantment>(),   // 附魔说明
            HoverTipFactory.FromRelic<ExampleRelic>(),               // 遗物说明
            HoverTipFactory.FromCard<ExampleStrike>(),               // 关联卡牌
            HoverTipFactory.FromKeyword(CardKeyword.Innate),       // 关键词（枚举：Exhaust/Ethereal/Innate/Unplayable/Retain/Sly/Eternal）
            HoverTipFactory.FromPower<ExampleBuffPower>()            // 能力
        ]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(5).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

/// <summary>TrashHeap 容器注入：垃圾堆商店可刷出自定义遗物/卡牌。</summary>
[HarmonyPatch(typeof(TrashHeap), "Relics", MethodType.Getter)]
public static class TrashHeapRelicPatch
{
    private static void Postfix(ref RelicModel[] __result)
    {
        __result = __result.Concat([ModelDb.Relic<ExampleRelic>()]).ToArray();
    }
}

[HarmonyPatch(typeof(TrashHeap), "Cards", MethodType.Getter)]
public static class TrashHeapCardPatch
{
    private static void Postfix(ref CardModel[] __result)
    {
        __result = __result.Concat([ModelDb.Card<ExampleHoverTipCard>()]).ToArray();
    }
}
