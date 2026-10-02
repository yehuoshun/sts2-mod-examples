using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using Sts2ModExamples.Core;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Relics;

/// <summary>
/// 【skill 全面测试】测试遗物：进入战斗时获得 2 层 TestVigorPower。
/// 对照 skill：relic/relic-core.md（模板/RelicRarity/PowerVar）+ relic-callbacks.md（AfterRoomEntered/Flash/ExtraHoverTips）。
/// 真实 API：RelicModel.Flash()（public）、AfterRoomEntered(AbstractRoom) @ AbstractModel.cs、HoverTipFactory.FromPower&lt;T&gt;。
/// </summary>
[RelicPool(typeof(SharedRelicPool))]
public class TestVigorRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    // 动态变量（用于描述 {TestVigorPower}）
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new PowerVar<TestVigorPower>(2m); }
    }

    // 悬停提示：能力图标
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield return HoverTipFactory.FromPower<TestVigorPower>(); }
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();   // RelicModel.Flash()（public，遗物专用）
            await PowerCmd.Apply<TestVigorPower>(
                new ThrowingPlayerChoiceContext(), Owner.Creature,
                DynamicVars["TestVigorPower"].BaseValue, Owner.Creature, null);
        }
    }
}
