using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Commands;

namespace Sts2ModExamples.Rewards;

/// <summary>
/// 示例：奖励卡内容修改（卡奖励附魔）。
/// 对照 skill：reward/reward-enchant.md。
/// 真实 API：Hook.TryModifyCardRewardOptions 静态方法（out List&lt;AbstractModel&gt; 参数，
/// 反射定位用 typeof(List&lt;AbstractModel&gt;).MakeByRefType()）；CardCreationOptions.Source/Flags/RngOverride；
/// CardCreationResult.ModifyCard(CardModel)；ModelDb.DebugEnchantments；PlayerRngSet.Rewards；
/// Rng(uint, string) 派生构造 / NextFloat / NextItem；CardCmd.Enchant。
/// </summary>
[HarmonyPatch]
public static class ExampleRewardEnchantPatch
{
    private static readonly HashSet<Type> Excluded = [typeof(Clone), typeof(DeprecatedEnchantment)];

    private static MethodBase? TargetMethod()
    {
        // out 参数 → MakeByRefType 精确定位
        return AccessTools.Method(typeof(Hook), nameof(Hook.TryModifyCardRewardOptions),
            new[] { typeof(IRunState), typeof(Player), typeof(List<CardCreationResult>),
                    typeof(CardCreationOptions), typeof(List<AbstractModel>).MakeByRefType() });
    }

    private static void Postfix(Player player, List<CardCreationResult> cardRewardOptions,
        CardCreationOptions creationOptions, ref bool __result)
    {
        if (cardRewardOptions.Count == 0) return;
        if (creationOptions.Source != CardCreationSource.Encounter) return;
        if (creationOptions.Flags.HasFlag(CardCreationFlags.NoModifyHooks)) return;

        Rng rng = creationOptions.RngOverride ?? player.PlayerRng.Rewards;
        bool enchantedAny = false;
        foreach (CardCreationResult reward in cardRewardOptions)
        {
            enchantedAny = TryEnchant(reward, player, rng) || enchantedAny;
        }

        __result = __result || enchantedAny;
    }

    private static bool TryEnchant(CardCreationResult result, Player player, Rng rng)
    {
        CardModel currentCard = result.Card;
        List<EnchantmentModel> candidates = GetEligibleEnchantments(currentCard);
        if (candidates.Count == 0) return false;
        if ((decimal)rng.NextFloat() > 0.125m * (player.RunState.CurrentActIndex + 1)) return false;

        EnchantmentModel? selected = rng.NextItem(candidates);
        if (selected == null) return false;

        // 克隆 → 附魔 → 替换（不污染原卡）
        CardModel enchantedCard = player.RunState.CloneCard(currentCard);
        CardCmd.Enchant(selected.ToMutable(), enchantedCard, player.RunState.CurrentActIndex + 1);
        result.ModifyCard(enchantedCard);
        return true;
    }

    private static List<EnchantmentModel> GetEligibleEnchantments(CardModel card)
    {
        return ModelDb.DebugEnchantments
            .Where(e => e.GetType().Namespace == "MegaCrit.Sts2.Core.Models.Enchantments") // 只要原版
            .Where(e => !Excluded.Contains(e.GetType()))
            .Where(e => e.CanEnchant(card))
            .OrderBy(static e => e.Id.Entry, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>商店确定性派生 Rng（不消费公共 Shops 源）。</summary>
    public static Rng CreateShopRng(Player player, CardModel card)
    {
        int counter = player.PlayerRng.Shops.Counter;
        uint shopSeed = unchecked(player.PlayerRng.Seed + (uint)StringHelper.GetDeterministicHashCode("shops"));
        string derivedName = $"ExampleRewardEnchant.shop.{counter}.{card.Id.Entry}.{card.CurrentUpgradeLevel}";
        return new Rng(shopSeed, derivedName);
    }
}
