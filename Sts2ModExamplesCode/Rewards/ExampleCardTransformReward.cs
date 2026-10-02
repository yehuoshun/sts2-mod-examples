using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

using Sts2ModExamples.Core;

namespace Sts2ModExamples.Rewards;

/// <summary>
/// 示例奖励：卡牌变形（选 1 张牌变成随机攻击牌）。
/// 对照 skill：reward/reward-examples.md。
/// 真实 API：Reward 抽象成员 = RewardType/RewardsSetIndex/Description/IsPopulated/Populate/OnSelect/MarkContentAsSeen；
/// ⚠️ [RewardType] 是自研注入 Attribute（见 Core/EnumInjector.cs），不是游戏 API。
/// </summary>
public class ExampleCardTransformReward : Reward
{
    public int MaxCards { get; set; } = 1;

    public ExampleCardTransformReward(Player player) : base(player) { }

    protected override RewardType RewardType => ExampleRewardTypes.CardTransform;
    public override bool IsPopulated => true;
    public override int RewardsSetIndex => 9;

    public override LocString Description =>
        new LocString("gameplay_ui", "CARD_TRANSFORM_TITLE");

    protected override string? IconPath =>
        "res://Sts2ModExamples/images/rewards/card_transform.png";

    public override void Populate() { }

    protected override async Task<bool> OnSelect()
    {
        return true;   // 真实选择逻辑见 skill reward 文档（CardSelectCmd.FromHand）
    }

    public override void MarkContentAsSeen() { }

    public override SerializableReward ToSerializable()
    {
        var save = base.ToSerializable();
        return save;
    }
}
