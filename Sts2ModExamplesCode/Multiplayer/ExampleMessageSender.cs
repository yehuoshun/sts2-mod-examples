using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 示例消息发送：向指定玩家定向发送 + 广播。
/// 对照 skill：multiplayer/multiplayer-core.md。
/// 真实 API：INetGameService.SendMessage&lt;T&gt;(T, ulong playerId) 定向 / SendMessage&lt;T&gt;(T) 广播。
/// </summary>
public static class ExampleMessageSender
{
    public static void SendTo(ulong targetNetId, int value)
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        netService.SendMessage(new ExampleMessage
        {
            Value = value,
            TargetNetId = targetNetId,
            Location = default,   // 真实场景从 IRunState.RunLocation 取
        }, targetNetId);
    }
}
