using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Events;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Saves;
using Sts2ModExamples.Cards;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例：原版私有成员集中反射（VanillaMembers 模式）——句柄集中定义，找不到进 Missing，
/// 依赖它的补丁用 [HarmonyPrepare] 整体降级。
/// 对照 skill：harmony/harmony-attribute-patcher.md。
/// </summary>
internal static class ExampleVanillaMembers
{
    private static readonly List<string> Missing = [];

    internal static IReadOnlyList<string> MissingMembers => Missing;

    /// <summary>原版 AncientEventModel.RelicOption(RelicModel, string, string)（protected，0.107.1–0.111.0）。</summary>
    internal static readonly MethodInfo? AncientRelicOption = Method(
        typeof(AncientEventModel), "RelicOption", [typeof(RelicModel), typeof(string), typeof(string)]);

    private static MethodInfo? Method(Type type, string name, Type[] parameters)
    {
        MethodInfo? method = AccessTools.Method(type, name, parameters);
        if (method == null)
        {
            Missing.Add($"{type.FullName}.{name}({string.Join(", ", parameters.Select(static p => p.Name))})");
        }

        return method;
    }
}

/// <summary>
/// 示例：[HarmonyPrepare] 门控——原版受保护成员反射不到时整个补丁跳过（版本兼容降级）。
/// 真实 API：Neow.GenerateInitialOptions() 是 protected override；AncientEventModel.RelicOption 是 protected 工厂，
/// 反射复用能拿到与原版选项同构的选项。
/// </summary>
[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
public static class ExampleNeowExtraOptionPatch
{
    [HarmonyPrepare]
    private static bool Prepare() => ExampleVanillaMembers.AncientRelicOption != null;

    [HarmonyPostfix]
    private static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner == null || __instance.Owner.RunState.Modifiers.Count > 0)
        {
            return; // 修正器局保持原版选项集合
        }

        if (__result.Any(static o => o.Relic is ExampleAncientSwordRelic))
        {
            return;
        }

        var relic = ModelDb.Relic<ExampleAncientSwordRelic>().ToMutable();
        if (ExampleVanillaMembers.AncientRelicOption!.Invoke(
                __instance, [relic, "INITIAL", "NEOW.pages.DONE.POSITIVE.description"]) is EventOption option)
        {
            __result = [.. __result, option];
        }
    }
}

/// <summary>
/// 示例遗物：获得时给牌组底部加入一张卡（AfterObtained 流程）。
/// 对照 skill：relic/relic-ancient-sword.md。
/// 真实 API：RelicModel.AfterObtained()；RunState.CreateCard(CardModel, Player)；
/// CardPileCmd.Add(CardModel, PileType, CardPilePosition.Bottom)；CardPileAddResult.success/cardAdded；
/// SaveManager.Instance.MarkCardAsSeen(CardModel)；CardCmd.PreviewCardPileAdd(IReadOnlyList&lt;CardPileAddResult&gt;, float)；
/// HoverTipFactory.FromCardWithCardHoverTips&lt;TCard&gt;()。
/// </summary>
public sealed class ExampleAncientSwordRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => "res://Sts2ModExamples/images/relics/example_ancient_sword.png";

    protected override string PackedIconOutlinePath => PackedIconPath;

    protected override string BigIconPath => PackedIconPath;

    // 遗物 hover 直接显示关联卡（含卡牌自身 hover 内容）
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<ExampleStrike>();

    public override async Task AfterObtained()
    {
        if (Owner == null)
        {
            return;
        }

        CardModel card = Owner.RunState.CreateCard(ModelDb.Card<ExampleStrike>(), Owner);
        CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom);
        if (result.success)
        {
            SaveManager.Instance.MarkCardAsSeen(result.cardAdded);   // 图鉴已见
            Flash();
            CardCmd.PreviewCardPileAdd([result], 2f);                // 加牌展示动画
        }
    }
}