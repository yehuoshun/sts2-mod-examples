using System.Collections;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace Sts2ModExamples.Settings;

/// <summary>
/// 【skill 编译验证】设置页「Mods」标签页注入 —— ModConfig 框架核心机制的纯原生转译。
/// 对照 skill：settings/modconfig-internals.md §1（源框架：xhyrzldf/ModConfig-STS2
/// SettingsTabInjector.cs，v0.2.2，MIT）。
/// 真实 API：NSettingsTabManager._tabs 是 private Dictionary&lt;NSettingsTab,NSettingsPanel&gt;
/// （sts2-res 验证）；SwitchTabTo 是 private（Godot Call 反射调用）；
/// NSettingsTab 继承 NButton，有 public Select()/Deselect()。
/// 零 Harmony、零第三方：tree.NodeAdded 监听 + Duplicate 原生 tab/panel + 反射注册。
/// 运行时验证：进游戏设置页应有「Mods」标签页（含开关/滑条示例）。
/// </summary>
public static class ModsTabInjector
{
    private const string TabName = "Mods";

    /// <summary>在 ModEntry.Initialize 三阶段（Harmony → 注册 → 设置）的「设置」阶段调用。</summary>
    public static void Initialize()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        tree.NodeAdded += OnNodeAdded;
    }

    /// <summary>任何 NSettingsTabManager 进树 → 等 ready 后注入（主菜单/暂停菜单各一个实例）。</summary>
    private static void OnNodeAdded(Node node)
    {
        if (node is not NSettingsTabManager) return;
        if (node.GetNodeOrNull(TabName) != null) return;

        node.Connect("ready",
            Callable.From(() => InjectModsTab((NSettingsTabManager)node)),
            (uint)GodotObject.ConnectFlags.OneShot);
    }

    private static void InjectModsTab(NSettingsTabManager tabManager)
    {
        try
        {
            // 1. 反射读私有字段 _tabs
            var tabsField = typeof(NSettingsTabManager).GetField("_tabs",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (tabsField == null) return;
            var tabs = tabsField.GetValue(tabManager) as IDictionary;
            if (tabs == null || tabs.Count == 0) return;

            NSettingsTab? firstTab = null;
            NSettingsPanel? firstPanel = null;
            foreach (DictionaryEntry entry in tabs)
            {
                firstTab = entry.Key as NSettingsTab;
                firstPanel = entry.Value as NSettingsPanel;
                break;
            }
            if (firstTab == null || firstPanel == null) return;

            // 2. 克隆原生 tab → 改名/标签/取消选中
            var modsTab = (NSettingsTab)firstTab.Duplicate();
            modsTab.Name = TabName;
            tabManager.AddChild(modsTab);
            modsTab.SetLabel(TabName);
            modsTab.Deselect();

            // 3. 克隆原生 panel → 只留 Content 容器
            //    ⚠️ 必须在 AddChild 前清理：游戏内部节点（NDropdownPositioner 等）
            //       _Ready 时引用原 panel 控件 → ObjectDisposedException
            var modsPanel = (NSettingsPanel)firstPanel.Duplicate();
            modsPanel.Name = "ModsSettings";
            modsPanel.Visible = false;

            VBoxContainer? content = null;
            foreach (var child in modsPanel.GetChildren().ToArray())
            {
                if (child is VBoxContainer vbox)
                {
                    content = vbox;
                    foreach (var inner in vbox.GetChildren().ToArray())
                    {
                        vbox.RemoveChild(inner);
                        inner.Free();
                    }
                }
                else
                {
                    modsPanel.RemoveChild(child);
                    child.Free();
                }
            }

            firstPanel.GetParent().AddChild(modsPanel);
            content ??= modsPanel.Content;

            // 4. 注册进 _tabs + 点击切换（SwitchTabTo 是 private，Godot Call）
            tabs.Add(modsTab, modsPanel);
            modsTab.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(delegate
                {
                    try { tabManager.Call("SwitchTabTo", modsTab); }
                    catch { /* 面板未就绪 */ }
                }));

            // 5. 高度封顶：NSettingsPanel.RefreshSize 会按内容撑高（ScrollContainer 失效）
            float maxHeight = firstPanel.Size.Y;
            if (maxHeight < 100)
                maxHeight = modsPanel.GetParent<Control>().Size.Y * 0.85f;
            modsPanel.Size = new Vector2(modsPanel.Size.X, maxHeight);

            // 6. 填充示例控件（纯原生方案完整实现见 Settings/NModConfigSubmenu.cs）
            Populate(content);
        }
        catch { /* 注入失败不崩游戏 */ }
    }

    private static void Populate(VBoxContainer content)
    {
        var header = new Label { Text = "Example Mod Settings" };
        content.AddChild(header);

        var toggle = new CheckButton
        {
            Text = "Enable Feature",
            ButtonPressed = true,
        };
        content.AddChild(toggle);

        var slider = new HSlider
        {
            MinValue = 0.5f,
            MaxValue = 5.0f,
            Step = 0.5f,
            Value = 1.0f,
        };
        content.AddChild(slider);
    }
}
