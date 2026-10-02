using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Orbs;

/// <summary>
/// 示例充能球：回合结束触发锻造，激发时锻造更多。
/// 对照 skill：orb/orb-core.md。
/// 真实 API：PassiveVal/EvokeVal/DarkenedColor 抽象、Passive(PlayerChoiceContext, Creature?)/Evoke(PlayerChoiceContext) virtual、
/// BeforeTurnEndOrbTrigger(PlayerChoiceContext) virtual。
/// </summary>
public class ExampleOrb : OrbModel
{
    public override Color DarkenedColor => new Color("FFD700");
    public override decimal PassiveVal => 3m;
    public override decimal EvokeVal => 6m;

    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        await ForgeCmd.Forge(PassiveVal, Owner, this);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        await ForgeCmd.Forge(EvokeVal, Owner, this);
        return new[] { Owner.Creature };
    }
}
