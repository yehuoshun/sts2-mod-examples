using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Resources;

/// <summary>
/// 示例自定义资源（法力）：继承 AbstractModel 的战斗状态，非 PowerModel。
/// 对照 skill：resource/resource-core.md + resource-lifecycle.md。
/// ⚠️ 全部为自研基类设计（原生 AbstractModel 无这些虚方法），注册见 lifecycle：ModelDb.Inject + Patch 钩入。
/// </summary>
public class ExampleResource : AbstractModel
{
    protected int _amount;
    public int Amount => _amount;

    public virtual int MaxAmount => 999;
    public virtual bool ResetEachTurn => true;
    public abstract int StartAmount { get; }

    public virtual void PrepForCombat(PlayerCombatState pcs)
    {
        _amount = StartAmount;
    }

    public virtual void StartOfTurnReset(PlayerCombatState pcs)
    {
        if (ResetEachTurn) _amount = StartAmount;
    }

    public virtual bool Spend(int amount)
    {
        if (_amount < amount) return false;
        _amount -= amount;
        return true;
    }

    public virtual void Gain(int amount)
    {
        _amount = Math.Min(_amount + amount, MaxAmount);
    }
}
