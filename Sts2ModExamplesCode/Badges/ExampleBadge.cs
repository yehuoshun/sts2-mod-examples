using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Sts2ModExamples.Badges;

/// <summary>
/// 示例徽章：通关即获得。
/// 对照 skill：badge/badge-core.md。
/// ⚠️ 真实 API：Id/RequiresWin/MultiplayerOnly 是普通属性（构造传入），**不是 virtual**——
/// 只能 override Rarity 和 IsObtained()（旧版模板 override RequiresWin 编译不过）。
/// ⚠️ SerializableRun 无 Completed 属性（旧文档编造）——通关判断用构造传入的 _won。
/// </summary>
public class ExampleBadge : Badge
{
    public ExampleBadge(SerializableRun run, bool won, ulong playerId)
        : base(run, won, playerId, "EXAMPLE_FIRST_WIN", requiresWin: true, multiplayerOnly: false)
    {
    }

    public override BadgeRarity Rarity => BadgeRarity.Gold;

    public override bool IsObtained()
    {
        return _won;
    }
}
