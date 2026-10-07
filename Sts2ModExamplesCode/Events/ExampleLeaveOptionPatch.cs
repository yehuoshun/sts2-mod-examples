using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Sts2ModExamples.Events;

/// <summary>
/// 示例：全事件「离开」选项（SetEventState prefix 注入）。
/// 对照 skill：event/event-leave-option.md。
/// 真实 API：EventModel.SetEventState(LocString, IEnumerable&lt;EventOption&gt;) 是 protected virtual；
/// EventModel.SetEventFinished(LocString) protected；EventOption.IsProceed/TextKey；
/// LocalContext.IsMine(EventModel?)；NEventRoom.Proceed() static。
/// </summary>
[HarmonyPatch(typeof(EventModel), "SetEventState")]
public static class ExampleLeaveOptionPatch
{
    private const string LeaveTextKey = "EXAMPLE_LEAVE.leave";
    private const string LeaveTitleKey = "EXAMPLE_LEAVE.leave.title";
    private const string LeaveDescriptionKey = "EXAMPLE_LEAVE.leave.description";

    // SetEventFinished 是 protected：反射句柄启动时缓存
    private static readonly System.Reflection.MethodInfo SetEventFinishedMethod =
        typeof(EventModel).GetMethod("SetEventFinished",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null, [typeof(LocString)], modifiers: null)
        ?? throw new InvalidOperationException("Could not find EventModel.SetEventFinished.");

    // Priority.Last：其他 mod 的注入先跑，自己最后追加
    [HarmonyPriority(Priority.Last)]
    private static void Prefix(EventModel __instance, ref IEnumerable<EventOption> eventOptions)
    {
        eventOptions = BuildOptions(__instance, eventOptions);
    }

    private static IEnumerable<EventOption> BuildOptions(EventModel eventModel, IEnumerable<EventOption> eventOptions)
    {
        List<EventOption> options = eventOptions.ToList();
        if (!ShouldAddLeaveOption(eventModel, options))
        {
            return options;
        }

        options.Add(new EventOption(
            eventModel,
            () => LeaveEvent(eventModel),
            new LocString("events", LeaveTitleKey),
            new LocString("events", LeaveDescriptionKey),
            LeaveTextKey,
            Array.Empty<MegaCrit.Sts2.Core.HoverTips.IHoverTip>()));
        return options;
    }

    private static bool ShouldAddLeaveOption(EventModel eventModel, List<EventOption> options)
    {
        if (eventModel.Id.Entry == "THE_ARCHITECT") return false;   // 终局事件不许跳
        if (eventModel.Description != null) return false;           // 自定义描述 = 特殊流程，不加
        if (options.Count == 0) return false;
        return !options.Any(o => o.IsProceed || o.TextKey == LeaveTextKey);
    }

    private static Task LeaveEvent(EventModel eventModel)
    {
        if (!eventModel.IsFinished)
        {
            SetEventFinishedMethod.Invoke(eventModel, new object[] { new LocString("events", LeaveDescriptionKey) });
        }

        // 多人：只有本地事件推进地图，远程等主机同步
        if (LocalContext.IsMine(eventModel) && eventModel.Node != null)
        {
            return NEventRoom.Proceed();
        }

        return Task.CompletedTask;
    }
}