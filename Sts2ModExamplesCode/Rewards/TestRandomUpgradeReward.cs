using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Rewards;

/// <summary>
/// 【skill 全面测试】测试奖励：升级一张随机卡牌。
/// 对照 skill：reward/reward-examples-more.md。
/// ⚠️ 真实 API：IconPath 是 protected virtual string?（文档写 public override string，待修）；
/// 升级用 IsUpgradable + UpgradeInternal()（文档写 CanUpgrade()，编造，待修）；
/// 原生 Reward.FromSerializable 是硬编码 switch，自定义 RewardType 会抛 NotImplementedException——
/// 无 CustomRewardRegistry（编造，待修），存档需自己 Patch FromSerializable。
/// </summary>
public class TestRandomUpgradeReward : Reward
{
    [EnumInjector.RewardType] public static RewardType RandomUpgrade;

    public TestRandomUpgradeReward(Player player) : base(player) { }

    protected override RewardType RewardType => RandomUpgrade;
    public override bool IsPopulated => true;
    public override int RewardsSetIndex => 9;

    public override LocString Description =>
        new LocString("gameplay_ui", "RANDOM_UPGRADE_TITLE");

    protected override string? IconPath =>
        "res://Sts2ModExamples/images/rewards/random_upgrade.png";

    public override void Populate()
    {
        var upgradable = Player.Deck.Cards
            .Where(c => c.IsUpgradable && !c.IsUpgraded)
            .ToList();
        if (upgradable.Count > 0)
        {
            var card = upgradable[0];
            card.UpgradeInternal();
        }
    }

    public override void MarkContentAsSeen() { }

    protected override async Task<bool> OnSelect()
    {
        return true;
    }
}
