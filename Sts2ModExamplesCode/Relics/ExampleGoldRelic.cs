using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Relics;

/// <summary>
/// 示例金币修改遗物：获得金币减半 + 防递归守卫。
/// 对照 skill：relic/relic-callbacks.md（数值修改钩子 + 防递归）。
/// 真实 API：ModifyGoldGained(Player, decimal) + AfterModifyingGoldGained(Player, decimal)（AbstractModel 虚方法）。
/// </summary>
[RelicPool(typeof(SharedRelicPool))]
public class ExampleGoldRelic : RelicModel
{
    private bool _modifyingGold;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override decimal ModifyGoldGained(Player player, decimal amount)
    {
        if (_modifyingGold) return amount;   // 防递归：副作用扣减时放行
        return amount * 0.5m;
    }

    public override async Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        try
        {
            _modifyingGold = true;
            // 副作用：把减半部分给……示例只演示守卫，不做事
        }
        finally
        {
            _modifyingGold = false;
        }
        await Task.CompletedTask;
    }
}
