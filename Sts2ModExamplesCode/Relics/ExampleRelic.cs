using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using Sts2ModExamples.Core;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Relics;

/// <summary>
/// 示例遗物：进入战斗时获得 6 层镀层（Plating）。
/// 对照 skill：relic/relic-core.md + relic/relic-callbacks.md。
/// 真实 API：RelicModel.Flash()、PowerCmd.Apply、CanonicalVars、ExtraHoverTips。
/// </summary>
[RelicPool(typeof(SharedRelicPool))]
public class ExampleRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    // 动态变量（用于描述 {PlatingPower}）
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new PowerVar<PlatingPower>(6m); }
    }

    // 悬停提示：镀层 + 格挡图标
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<PlatingPower>();
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();   // RelicModel.Flash()（public，遗物专用）
            await PowerCmd.Apply<PlatingPower>(
                new ThrowingPlayerChoiceContext(), Owner.Creature,
                DynamicVars["PlatingPower"].BaseValue, Owner.Creature, null);
        }
    }
}
