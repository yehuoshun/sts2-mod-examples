using System.Text;
using AutoAnthony;
using ChaosCardGenerator;

namespace Sts2ModExamples.Cardgen;

/// <summary>
/// 示例：AutoAnthony 卡牌编辑（CardTinkeringApi v6）完整闭环 —— 枚举原型 → 改数值槽 → 校验 → 重建 → 序列化往返 → 预算分析。
/// 对照 skill：cardgen/cardgen-editor.md「二、核心 API 签名」「三、预算评估」。
/// 全程只碰结构化 GeneratedCard，不解析渲染文本，也不需要战斗/存档上下文。
/// </summary>
/// <remarks>
/// 本文件只在装了 AutoAnthony 的机器上参与编译（见 csproj 的 AUTOANTHONY 条件）。
/// 它在编译期锁死卡片编辑 API 的全部签名：文档改错字段/参数，这里就编译不过。
/// </remarks>
public static class CardgenTinkeringFlow
{
    /// <summary>文档声明的卡片编辑 API 版本。守卫别只写顶层一条，用到哪一面就核哪一面。</summary>
    public const int RequiredApiVersion = 6;

    /// <summary>运行期探测：没装 AutoAnthony 时本类的方法一律不可调（类型解析会失败）。</summary>
    public static bool IsAvailable =>
        AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "AutoAnthony");

    /// <summary>
    /// 跑一遍完整闭环并返回可读报告。失败不抛，把原因写进报告（示例 mod 不该因为演示失败中断游戏）。
    /// </summary>
    public static string Run(int seed = 20261120)
    {
        if (!IsAvailable) return "[cardgen] AutoAnthony 未加载，跳过卡牌编辑示例。";
        // ApiVersion 在 DLL 里是 const，版本与文档一致时下面的分支会被编译器折成不可达（CS0162）；
        // 版本不一致时它反而是活的。保留这条守卫——文档写错版本的那天，它才会真的拦住你。
#pragma warning disable CS0162
        if (CardTinkeringApi.ApiVersion != RequiredApiVersion)
            return $"[cardgen] CardTinkeringApi v{CardTinkeringApi.ApiVersion} != v{RequiredApiVersion}，跳过。";
#pragma warning restore CS0162

        var report = new StringBuilder();
        report.AppendLine($"[cardgen] CardTinkeringApi v{CardTinkeringApi.ApiVersion}");

        // 1) 枚举组件原型：拿到的是「可直接用的结构化操作」，不是文本片段。
        var prototypes = CardTinkeringApi.GetComponentPrototypes();
        report.AppendLine($"  prototypes = {prototypes.Count}");
        var editable = prototypes
            .Select(p => (Prototype: p, Values: CardTinkeringApi.GetEditableValues(p.Operation)))
            .FirstOrDefault(x => x.Values.Count > 0);
        if (editable.Prototype is null) return report.AppendLine("  没有带可编辑数值的组件原型，结束。").ToString();

        report.AppendLine($"  picked component = {editable.Prototype.Id} (template {editable.Prototype.Operation.Template})");
        foreach (var value in editable.Values)
            report.AppendLine($"    slot {value.Id} value={value.Value} range=[{value.Minimum},{value.Maximum}] x-offset={value.IsXOffset}");

        // 2) 改数值槽：改完 Api 会自己重渲染中英双语描述，这里只关心结构化结果。
        var slot = editable.Values[0];
        var target = Math.Clamp(slot.Value + 2, slot.Minimum, slot.Maximum);
        if (!CardTinkeringApi.TrySetEditableValue(editable.Prototype.Operation, slot.Id, target, out var updated))
            return report.AppendLine($"  改槽 {slot.Id} 被拒（超出范围或槽不可写）。").ToString();
        report.AppendLine($"  set slot {slot.Id}: {slot.Value} -> {target}");

        // 3) 组一张壳卡：费用/类型/目标/稀有度由编辑器决定，操作列表来自原型。
        var operations = new[] { updated };
        // record 的位置参数在元数据里就是 PascalCase（Cost/Type/...），具名实参必须照这个大小写写。
        var shell = new GeneratedCard(
            Cost: 1,
            Type: GeneratedCardType.Attack,
            Target: TargetMode.SingleEnemy,
            Rarity: GeneratedRarity.Common,
            ChineseDescription: string.Empty,
            Tags: Array.Empty<CardTag>(),
            Operations: operations,
            Name: null,
            Upgrade: null,
            EnglishDescription: string.Empty,
            Character: GeneratedCharacter.Ironclad);

        // 4) 先校验后重建（顺序不可换；Rebuild 内部还会再校验一次，失败抛）。
        var validation = CardTinkeringApi.Validate(shell, operations);
        report.AppendLine($"  validate = {validation.IsValid} (errors {validation.Errors.Count})");
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors) report.AppendLine($"    ! {error}");
            return report.ToString();
        }
        // 注意：源码 HEAD 的 CardTinkeringApi.Valid 常量并未进 0.3.165 包（DLL 里查不到），
        // 所以这里只用 Validate 的返回值当基准，不引用那个成员。

        var rebuilt = CardTinkeringApi.Rebuild(shell, operations);
        report.AppendLine($"  rebuilt: cost={rebuilt.Cost} desc={Trim(rebuilt.ChineseDescription)}");

        // 5) 序列化往返：编辑器持久化 / 多人 payload 都走这一条，不依赖任何渲染文本。
        var payload = CardTinkeringApi.SerializeCard(rebuilt);
        var restored = CardTinkeringApi.DeserializeCard(payload);
        report.AppendLine($"  round-trip payload={payload.Length}B operations={restored.Operations.Count} cost={restored.Cost}");

        // 6) 预算分析：整卡 / 指定 X / 单组件裸值三条口径互相独立。
        var analysis = CardTinkeringApi.Analyze(rebuilt);
        report.AppendLine($"  analyze: components={analysis.Components.Count} exceeds={analysis.ExceedsOrdinaryUpperBound} budget={analysis.Budget}");
        var atX = CardTinkeringApi.AnalyzeAtX(rebuilt, resolvedX: 3);
        report.AppendLine($"  analyzeAtX(3): components={atX.Components.Count}");
        var standalone = CardTinkeringApi.AnalyzeStandaloneComponent(updated, GeneratedRarity.Common);
        report.AppendLine($"  standalone: atomic={standalone.AtomicValue} contextual={standalone.ContextualValue} negative={standalone.IsNegative}");

        // 7) 其余只读查询。
        report.AppendLine($"  variableX={CardTinkeringApi.IsVariableX(rebuilt)} effectiveCost={CardTinkeringApi.GetEffectiveCost(rebuilt, 2)}");
        report.AppendLine($"  keywordValue(Strike)={CardTinkeringApi.GetKeywordValue(CardTag.Strike)}");

        // 8) 关键词原型：基础/升级增/升级删三种权限分开给。
        var keywords = CardTinkeringApi.GetKeywordPrototypes(new ComponentProfileRequest(GeneratedCharacter.Ironclad, false));
        report.AppendLine($"  keyword prototypes = {keywords.Count}");
        foreach (var keyword in keywords.Take(3))
            report.AppendLine($"    {keyword.Id}: base={keyword.AllowedOnBaseCard} add={keyword.AllowedAsUpgradeAddition} remove={keyword.AllowedAsUpgradeRemoval}");

        // 9) 单卡身份编辑：改名/换卡图唯一受支持路径是 RerollEditorIdentity + ApplyEditorDefinition。
        var identity = AutoAnthonyEditorApi.RerollEditorIdentity(restored, seed);
        report.AppendLine($"  identity: name={identity.Name?.Chinese}/{identity.Name?.English} portrait={(identity.PortraitPath ?? "<none>")}");

        return report.ToString();
    }

    private static string Trim(string text) =>
        string.IsNullOrEmpty(text) ? "<empty>" : (text.Length <= 24 ? text : text[..24] + "…");
}