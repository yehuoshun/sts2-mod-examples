using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 示例自定义网络行动（走行动队列）。
/// 对照 skill：multiplayer/multiplayer-netactions.md。
/// 真实 API：GameAction（OwnerId/ActionType/ExecuteAction/ToNetAction）+ INetAction
///          + ActionQueueSynchronizer.RequestEnqueue / CombatState / ActionSynchronizerCombatState。
/// ⚠️ 自定义 INetAction 子类型不被 [GenerateSubtypes] 覆盖（只在游戏程序集生成）→ 联机序列化需额外 patch；
///    本示例仅演示编译骨架，能用 INetMessage 就别自造行动。
/// </summary>
public sealed class ExampleGameAction(Player player) : GameAction
{
    public Player Player { get; } = player;

    public override ulong OwnerId => Player.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    protected override async Task ExecuteAction()
    {
        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"ExampleGameAction failed: {ex}");
            throw;   // 网络行动失败必须抛出，不能吞（否则各端状态静默发散）
        }
    }

    public override INetAction ToNetAction() => new ExampleNetAction(Player.NetId);

    /// <summary>阶段门控：仅玩家操作阶段（PlayPhase）才发起。</summary>
    public static bool TryRequest(Player player)
    {
        if (RunManager.Instance?.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
        {
            return false;
        }

        RunManager.Instance?.ActionQueueSynchronizer.RequestEnqueue(new ExampleGameAction(player));
        return true;
    }
}

/// <summary>示例网络封包（INetAction : IPacketSerializable）。</summary>
public sealed class ExampleNetAction(ulong ownerNetId) : INetAction
{
    public ulong OwnerNetId { get; set; } = ownerNetId;

    public void Serialize(PacketWriter writer) => writer.WriteULong(OwnerNetId);

    public void Deserialize(PacketReader reader) => OwnerNetId = reader.ReadULong();

    public GameAction ToGameAction(Player player) => new ExampleGameAction(player);
}
