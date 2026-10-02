using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Modifiers;

/// <summary>
/// 示例带 Neow 选项的修改器：开局选择「+10 最大生命」，且仅本地玩家生效。
/// 对照 skill：modifier/modifier-core.md + multiplayer（LocalContext.IsMe）。
/// 真实 API：GenerateNeowOption(EventModel) virtual（ModifierModel 无 Owner，玩家取自 eventModel.Owner）、LocalContext.IsMe(Player)。
/// </summary>
public class ExampleNeowModifier : ModifierModel
{
    public override Func<Task>? GenerateNeowOption(EventModel eventModel)
    {
        var player = eventModel.Owner;   // Player?
        return async () =>
        {
            if (player != null && LocalContext.IsMe(player))
            {
                await CreatureCmd.GainMaxHp(player.Creature, 10m);
            }
        };
    }
}
