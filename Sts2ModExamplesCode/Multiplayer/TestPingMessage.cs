using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 【skill 全面测试】测试自定义网络消息：Ping 消息（同步格挡值）。
/// 对照 skill：multiplayer/multiplayer-core.md。
/// 真实 API：INetMessage/IPacketSerializable/IRunLocationTargetedMessage 四接口 + PacketWriter/PacketReader。
/// </summary>
public struct TestPingMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
    public required double BlockValue { get; set; }
    public required ulong TargetNetId { get; set; }
    public required RunLocation Location { get; set; }

    public bool ShouldBroadcast => true;                  // 广播
    public NetTransferMode Mode => NetTransferMode.Unreliable;
    public LogLevel LogLevel => LogLevel.Debug;
    public bool ShouldBuffer => false;

    RunLocation IRunLocationTargetedMessage.Location => Location;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteDouble(BlockValue);
        writer.WriteULong(TargetNetId);
        writer.Write(Location);
    }

    public void Deserialize(PacketReader reader)
    {
        BlockValue = reader.ReadDouble();
        TargetNetId = reader.ReadULong();
        Location = reader.Read<RunLocation>();
    }
}
