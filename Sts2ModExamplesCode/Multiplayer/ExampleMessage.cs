using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 示例自定义网络消息。对照 skill：multiplayer/multiplayer-core.md。
/// 真实 API：INetMessage/IPacketSerializable/IRunLocationTargetedMessage 四接口 + PacketWriter/PacketReader。
/// </summary>
public struct ExampleMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
    public required int Value { get; set; }
    public required ulong TargetNetId { get; set; }
    public required RunLocation Location { get; set; }

    public bool ShouldBroadcast => false;                 // 定向发送
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public bool ShouldBuffer => false;

    RunLocation IRunLocationTargetedMessage.Location => Location;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Value);
        writer.WriteULong(TargetNetId);
        writer.Write(Location);                           // 泛型 Write<T> where T : IPacketSerializable
    }

    public void Deserialize(PacketReader reader)
    {
        Value = reader.ReadInt();
        TargetNetId = reader.ReadULong();
        Location = reader.Read<RunLocation>();
    }
}
