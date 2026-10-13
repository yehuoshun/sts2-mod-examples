using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 示例增益扩散器：持有者给其他人施加 ExampleModifyPower 时，自动扩散给全队玩家队友，
/// 扩散量 = 原始量 + 本层数 - 1（层数越高队友吃得越多）。
/// 对照 skill：power/power-advanced.md（增益扩散器，MarisaMod PrismaticNebularPower）。
/// 真实 API：ModifyPowerAmountGivenAdditive(PowerModel, Creature, decimal, Creature?, CardModel?)；
/// 同步回调里发起异步命令用 new ThrowingPlayerChoiceContext()。
/// </summary>
public class ExampleSpreadPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyPowerAmountGivenAdditive(
        PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        // 拦截点：自己（giver == Owner）给他人上目标 Buff 且量为正
        if (power is ExampleModifyPower && giver == Owner && amount > 0)
        {
            var teammates = CombatState.GetTeammatesOf(Owner)
                .Where(c => c is { IsAlive: true, IsPlayer: true } && c != Owner);
            foreach (var creature in teammates)
            {
                _ = PowerCmd.Apply<ExampleModifyPower>(
                    new ThrowingPlayerChoiceContext(), creature, amount + Amount - 1, Owner, null);
            }
        }

        return base.ModifyPowerAmountGivenAdditive(power, giver, amount, target, cardSource);
    }
}
