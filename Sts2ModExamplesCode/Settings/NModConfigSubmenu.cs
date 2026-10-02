using Godot;
using System.Reflection;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Sts2ModExamples.Settings;

/// <summary>
/// 纯原生设置子菜单：继承游戏原生 NSubmenu，动态生成控件（分组标题 + CheckButton/HSlider）。
/// 对照 skill：settings/settings-core.md（纯原生方案，替代 BaseLib SimpleModConfig）。
/// </summary>
public partial class NModConfigSubmenu : NSubmenu
{
    private VBoxContainer _content = null!;
    private Label _title = null!;

    protected override Control? InitialFocusedControl => _content;

    public override void _Ready()
    {
        base._Ready();
        BuildUI();
    }

    private void BuildUI()
    {
        _title = new Label
        {
            Text = "Mod Settings",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(600, 60),
        };

        _content = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(600, 400),
        };

        var scroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(600, 400),
        };
        scroll.AddChild(_content);

        AddChild(_title);
        AddChild(scroll);

        // 按 [ConfigSection] 分组动态生成控件
        string? currentSection = null;
        foreach (var prop in typeof(ExampleModConfig).GetProperties()
                     .OrderBy(p => p.GetCustomAttribute<ConfigSectionAttribute>()?.Name))
        {
            var sectionAttr = prop.GetCustomAttribute<ConfigSectionAttribute>();
            if (sectionAttr != null && sectionAttr.Name != currentSection)
            {
                currentSection = sectionAttr.Name;
                _content.AddChild(new Label
                {
                    Text = $"[b]{currentSection}[/b]",
                    CustomMinimumSize = new Vector2(600, 30),
                });
            }

            if (prop.PropertyType == typeof(bool))
            {
                var check = new CheckButton
                {
                    Text = prop.Name,
                    ButtonPressed = (bool)prop.GetValue(null)!,
                };
                check.Toggled += on => prop.SetValue(null, on);
                _content.AddChild(check);
            }
            else if (prop.PropertyType == typeof(double))
            {
                var sliderAttr = prop.GetCustomAttribute<ConfigSliderAttribute>()
                                 ?? new ConfigSliderAttribute();
                var slider = new HSlider
                {
                    MinValue = sliderAttr.Min,
                    MaxValue = sliderAttr.Max,
                    Step = sliderAttr.Step,
                    Value = (double)prop.GetValue(null)!,
                    CustomMinimumSize = new Vector2(400, 30),
                };
                slider.ValueChanged += v => prop.SetValue(null, v);
                var row = new HBoxContainer();
                row.AddChild(new Label { Text = prop.Name, CustomMinimumSize = new Vector2(150, 30) });
                row.AddChild(slider);
                _content.AddChild(row);
            }
        }

        var saveButton = new Button { Text = "Save & Close" };
        saveButton.Pressed += () =>
        {
            ModConfigStorage.Save();
            Visible = false;
        };
        _content.AddChild(saveButton);
    }
}
