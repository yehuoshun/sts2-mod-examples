using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 示例网络消息处理器：注册 + 处理 ExampleMessage。
/// 对照 skill：multiplayer/multiplayer-core.md。
/// 真实 API：RunManager.Instance.NetService（INetGameService）→ RegisterMessageHandler&lt;T&gt;(MessageHandlerDelegate&lt;T&gt;)。
/// </summary>
public static class ExampleMessageHandler
{
    public static void Register()
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;   // 单人/未联网时跳过

        netService.RegisterMessageHandler<ExampleMessage>(Handle);
    }

    private static void Handle(ExampleMessage message, ulong senderId)
    {
        // 收到消息后的处理逻辑（如应用金币变更）
        MainFile.Logger.Info($"收到 ExampleMessage: Value={message.Value} sender={senderId}");
    }
}
