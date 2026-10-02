using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Sts2ModExamples.Badges;

/// <summary>
/// 【skill 全面测试】测试徽章：无伤击败 Boss 获得（Silver）。
/// 对照 skill：badge/badge-core.md。
/// 真实 API：Id/RequiresWin/MultiplayerOnly 构造传入（非 virtual）；override Rarity + IsObtained()。
/// </summary>
public class TestNoHitBadge : Badge
{
    public TestNoHitBadge(SerializableRun run, bool won, ulong playerId)
        : base(run, won, playerId, "STS2MODEXAMPLES-NO_HIT_BOSS",
            requiresWin: true, multiplayerOnly: false)
    {
    }

    public override BadgeRarity Rarity => BadgeRarity.Silver;

    public override bool IsObtained()
    {
        return _won;   // Badge 自带字段（_run.Completed 不存在）
    }
}
