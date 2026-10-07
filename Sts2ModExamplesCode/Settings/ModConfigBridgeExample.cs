using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Sts2ModExamples.Settings;

/// <summary>
/// 【skill 编译验证】ModConfig 第三方设置框架接入模板（反射零依赖桥）。
/// 对照 skill：settings/modconfig-integration.md（接入）+ settings/modconfig.md（选型）。
/// 真实 API：不引用 ModConfig.dll——AppDomain 扫类型检测 + 反射注册/读写；
/// 玩家没装 ModConfig 时所有 GetValue 返回 fallback，模组照常运行。
/// 源模板：xhyrzldf/ModConfig-STS2 examples/ModConfigBridge.cs（v0.2.2，MIT）。
/// </summary>
internal static class ModConfigBridgeExample
{
    private static bool _available;
    private static bool _registered;
    private static Type? _apiType;
    private static Type? _entryType;
    private static Type? _configTypeEnum;

    internal static bool IsAvailable => _available;

    // ─── Step 1: 延迟注册（mod 可能比 ModConfig 先加载，字母序）───
    internal static void DeferredRegister()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        tree.ProcessFrame += OnNextFrame;
    }

    private static void OnNextFrame()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        tree.ProcessFrame -= OnNextFrame;
        Detect();
        if (_available) Register();
    }

    // ─── Step 2: 反射检测（3 个类型全命中才算可用）───
    private static void Detect()
    {
        try
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .ToArray();

            _apiType = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ModConfigApi");
            _entryType = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ConfigEntry");
            _configTypeEnum = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ConfigType");
            _available = _apiType != null && _entryType != null && _configTypeEnum != null;
        }
        catch
        {
            _available = false;
        }
    }

    // ─── Step 3: 注册（4 参重载优先：带双语显示名）───
    private static void Register()
    {
        if (_registered) return;
        _registered = true;

        try
        {
            var entries = BuildEntries();
            var displayNames = new Dictionary<string, string>
            {
                ["en"] = "Example Mod",
                ["zhs"] = "示例模组",
            };

            var registerMethod = _apiType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Register")
                .OrderByDescending(m => m.GetParameters().Length)
                .First();

            if (registerMethod.GetParameters().Length == 4)
            {
                registerMethod.Invoke(null, new object[]
                {
                    "sts2.mod.examples", displayNames["en"], displayNames, entries
                });
            }
            else
            {
                registerMethod.Invoke(null, new object[]
                {
                    "sts2.mod.examples", displayNames["en"], entries
                });
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[Sts2ModExamples] ModConfig registration failed: {e}");
        }
    }

    // ─── 运行时读写（fallback：ModConfig 未装）───
    internal static T GetValue<T>(string key, T fallback)
    {
        if (!_available) return fallback;
        try
        {
            var result = _apiType!.GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static)
                ?.MakeGenericMethod(typeof(T))
                ?.Invoke(null, new object[] { "sts2.mod.examples", key });
            return result != null ? (T)result : fallback;
        }
        catch { return fallback; }
    }

    internal static void SetValue(string key, object value)
    {
        if (!_available) return;
        try
        {
            _apiType!.GetMethod("SetValue", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { "sts2.mod.examples", key, value });
        }
        catch { }
    }

    // ─── 配置项声明（9 种控件全示例）───
    private static Array BuildEntries()
    {
        var list = new List<object>();

        // Header（视觉分组）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Label", "General");
            Set(cfg, "Labels", L("General", "常规设置"));
            Set(cfg, "Type", EnumVal("Header"));
        }));

        // Toggle（bool）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "featureEnabled");
            Set(cfg, "Label", "Enable Feature");
            Set(cfg, "Labels", L("Enable Feature", "启用功能"));
            Set(cfg, "Type", EnumVal("Toggle"));
            Set(cfg, "DefaultValue", (object)true);
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        // Slider（float，Min/Max/Step/Format）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "speedMultiplier");
            Set(cfg, "Label", "Speed (x)");
            Set(cfg, "Labels", L("Speed (x)", "速度倍率"));
            Set(cfg, "Type", EnumVal("Slider"));
            Set(cfg, "DefaultValue", (object)1.0f);
            Set(cfg, "Min", 0.5f);
            Set(cfg, "Max", 5.0f);
            Set(cfg, "Step", 0.5f);
            Set(cfg, "Format", "F1");
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        // Separator（视觉分隔）
        list.Add(Entry(cfg => Set(cfg, "Type", EnumVal("Separator"))));

        // Dropdown（string 选项）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "difficulty");
            Set(cfg, "Label", "Difficulty");
            Set(cfg, "Labels", L("Difficulty", "难度"));
            Set(cfg, "Type", EnumVal("Dropdown"));
            Set(cfg, "DefaultValue", (object)"Normal");
            Set(cfg, "Options", new[] { "Easy", "Normal", "Hard" });
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        // KeyBind（long：Godot keycode + 修饰键位）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "toggleKey");
            Set(cfg, "Label", "Toggle Hotkey");
            Set(cfg, "Labels", L("Toggle Hotkey", "切换快捷键"));
            Set(cfg, "Type", EnumVal("KeyBind"));
            Set(cfg, "DefaultValue", (object)(long)Key.F9);
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        // TextInput（string + Validator 校验）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "playerName");
            Set(cfg, "Label", "Player Name");
            Set(cfg, "Type", EnumVal("TextInput"));
            Set(cfg, "DefaultValue", (object)"");
            Set(cfg, "MaxLength", 32);
            Set(cfg, "Placeholder", "Enter name...");
            Set(cfg, "Validator", new Func<object, bool>(v =>
                !string.IsNullOrWhiteSpace(Convert.ToString(v))));
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        // Button（动作，不持久化）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "resetAll");
            Set(cfg, "Label", "Reset Data");
            Set(cfg, "Labels", L("Reset Data", "重置数据"));
            Set(cfg, "Type", EnumVal("Button"));
            Set(cfg, "ButtonText", "Reset");
            Set(cfg, "ButtonTexts", L("Reset", "重置"));
            Set(cfg, "OnChanged", new Action<object>(_ => { }));
        }));

        // ColorPicker（hex 字符串 "#RRGGBB"）
        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "highlightColor");
            Set(cfg, "Label", "Highlight Color");
            Set(cfg, "Type", EnumVal("ColorPicker"));
            Set(cfg, "DefaultValue", (object)"#FF6600");
            Set(cfg, "OnChanged", new Action<object>(v => { }));
        }));

        var result = Array.CreateInstance(_entryType!, list.Count);
        for (int i = 0; i < list.Count; i++)
            result.SetValue(list[i], i);
        return result;
    }

    // ─── 反射辅助 ───
    private static object Entry(Action<object> configure)
    {
        var inst = Activator.CreateInstance(_entryType!)!;
        configure(inst);
        return inst;
    }

    private static void Set(object obj, string name, object value)
        => obj.GetType().GetProperty(name)?.SetValue(obj, value);

    private static Dictionary<string, string> L(string en, string zhs)
        => new() { ["en"] = en, ["zhs"] = zhs };

    private static object EnumVal(string name)
        => Enum.Parse(_configTypeEnum!, name);
}
