using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Sts2ModExamples.Resources;

/// <summary>
/// 【skill 全面测试】测试自定义资源：怒气（每回合重置，上限 10）。
/// 对照 skill：resource/resource-core.md + resource-lifecycle.md。
/// ⚠️ 全部为自研基类设计（ExampleResource 抽象基类，原生 AbstractModel 无这些虚方法），
/// 注册见 lifecycle：ModelDb.Inject + Patch 钩入。
/// </summary>
public class TestRageResource : ExampleResource
{
    public override int MaxAmount => 10;
    public override bool ResetEachTurn => true;
    public override int StartAmount => 0;

    // 被打时积累怒气（供卡牌/能力消费）
    public override void PrepForCombat(PlayerCombatState pcs)
    {
        base.PrepForCombat(pcs);
        _amount = 0;
    }

    public void OnTakeDamage(int damage)
    {
        Gain(damage);
    }
}
