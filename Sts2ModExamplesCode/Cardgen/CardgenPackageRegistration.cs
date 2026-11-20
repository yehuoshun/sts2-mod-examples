using AutoAnthony;
using ChaosCardGenerator;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Sts2ModExamples.Cardgen;

/// <summary>
/// 示例：AutoAnthony 组件包注册七步法（生成器侧 v3 / 游戏侧 v1-v4）。
/// 对照 skill：cardgen/cardgen-api.md「二、组件包注册」「三、运行时执行路由」「四、自定义关键词」「五、外部角色接入」。
/// </summary>
/// <remarks>
/// 本文件只在装了 AutoAnthony 的机器上参与编译（见 csproj 的 AUTOANTHONY 条件）。
/// 它在编译期锁死注册 API 的全部签名：文档写错参数名/参数类型/重载，这里就编译不过。
/// 所有方法都**不**自动执行 —— 注册必须在首次 profile/卡查询之前完成，且首用即冻结。
/// </remarks>
public static class CardgenPackageRegistration
{
    public const string PackageId = "sts2modeexamples:cardgen_package";
    public const string ProfileId = "sts2modeexamples:cardgen_profile";

    /// <summary>守卫：每一个用到的面都要核，别只核顶层一条。</summary>
    public static void EnsureApiVersions()
    {
        if (ComponentApi.ApiVersion != 3 || ComponentPackageApi.ApiVersion != 3
            || ComponentRuntimeApi.ApiVersion != 1 || ExternalComponentCharacterApi.ApiVersion != 4
            || ComponentKeywordApi.ApiVersion != 1 || ComponentPresentationApi.ApiVersion != 2
            || ComponentKeywordRuntimeApi.ApiVersion != 1 || ComponentValuationApi.ApiVersion != 1
            || CardTinkeringApi.ApiVersion != 6 || AutoAnthonyEditorApi.ApiVersion != 2
            || AutoAnthonySettingsApi.ApiVersion != 5 || AutoAnthonyNativeCardApi.ApiVersion != 3
            || AutoAnthonyFreeformCardApi.ApiVersion != 2 || AutoAnthonyBalanceAdjustmentApi.ApiVersion != 1)
            throw new NotSupportedException("Unsupported AutoAnthony component API version.");
    }

    /// <summary>第 1 步：组件包（recipe/组件/本地化/关键词升级/multiplicity/估值一并原子提交）。</summary>
    public static void RegisterPackage()
    {
        var archetype = GeneratedCharacter.Ironclad;
        var recipes = Array.Empty<IroncladCardRecipe>();
        var shells = new ImmutableComponentCatalog(archetype, recipes);
        var components = new ImmutableComponentCatalog(archetype, recipes);
        var names = new ImmutableComponentCatalog(archetype, recipes);

        var profile = new ComponentGenerationProfile(
            ProfileId,
            archetype,
            unlockComponentRoles: false,
            shellCatalog: shells,
            componentCatalog: components,
            nameCatalog: names,
            occurrenceFactory: () => ComponentApi.CreateNativeOccurrencePolicy(components, false),
            valuePolicy: ComponentApi.DefaultValuePolicy);

        var request = new ComponentProfileRequest(ProfileId, archetype, unlockComponentRoles: false);

        ComponentPackageApi.Register(new ComponentPackageRegistration(
            PackageId,
            request,
            profile,
            LocalizedTexts: null,
            KeywordUpgrades: null,
            Multiplicities: null,
            Valuations: null,
            IncludeInUltimateChaos: true,
            Keywords: null,
            Localizations: null));
    }

    /// <summary>第 2 步：名字部件（老接口，没有 ApiVersion 可查，按 mod 版本判断）。</summary>
    public static void RegisterNameParts(IEnumerable<ComponentCardNameParts> parts) =>
        CardNameGenerator.RegisterExternalParts(PackageId, parts);

    /// <summary>第 3 步：自定义 opcode 的执行路由（推荐原子包注册，精确 opcode+variant 优先于通配）。</summary>
    public static void RegisterRuntimeRoutes(IComponentRuntimeHandler handler) =>
        ComponentRuntimeApi.RegisterPackage(PackageId,
            [new ComponentRuntimeRoute("sts2modeexamples:op", "v1", handler)]);

    /// <summary>第 4 步：引用卡/命名机制的 hover tip 展示路由。</summary>
    public static void RegisterPresentationRoutes(IComponentHoverTipProvider provider) =>
        ComponentPresentationApi.RegisterPackage(PackageId,
            [new ComponentPresentationRoute("sts2modeexamples:op", "v1", provider)]);

    /// <summary>第 5 步：自定义关键词投影（冻结在首次卡查询）。</summary>
    public static void RegisterKeywordRuntime(IComponentKeywordRuntimeAdapter adapter) =>
        ComponentKeywordRuntimeApi.RegisterPackage(PackageId,
            [new ComponentKeywordRuntimeRegistration("sts2modeexamples:stance_locked", adapter)]);

    /// <summary>第 6 步：外部角色本体（EnergyIconPrefix 决定能量图标前缀，AncientRelics 可选）。</summary>
    public static void RegisterCharacter() =>
        ExternalComponentCharacterApi.Register(new ExternalComponentCharacterRegistration(
            ProfileId, GeneratedCharacter.Ironclad, "sts2modeexamples"));

    /// <summary>第 7 步：可选 runtime host —— 固定槽类型 / 活跃 run 谓词 / 生成池。</summary>
    public static void RegisterCharacterRuntime() =>
        ExternalComponentCharacterApi.RegisterRuntime(new ExternalComponentCharacterRuntimeRegistration(
            ProfileId,
            CardCount: 3,
            CardTypeForSlot: _ => typeof(CardModel),
            IsRunActive: () => true,
            CardPool: () => ModelDb.CardPool<ColorlessCardPool>()));

    /// <summary>运行期：整表替换定义（逐槽校验，任一条不满足直接抛，绝不半装）。</summary>
    public static void Install(IEnumerable<ChaosCardDefinition> definitions) =>
        ExternalComponentCharacterApi.InstallDefinitions(ProfileId, definitions);

    /// <summary>run 结束清理。</summary>
    public static void Clear() => ExternalComponentCharacterApi.ClearDefinitions(ProfileId);

    /// <summary>原生卡分解目录（只读）：按 nativeId 查全部 ModelId 变体，歧义时显式绑定模板。</summary>
    public static void Decompose(string nativeId)
    {
        foreach (var entry in NativeCardDecompositionApi.FindByNativeId(nativeId))
            Console.WriteLine($"{entry.CatalogId} referenceOnly={entry.ReferenceOnly}");

        if (NativeCardDecompositionApi.TryCreateBaseDefinition(nativeId, out var definition))
            Console.WriteLine($"materialized shell: cost={definition.Cost} operations={definition.Operations.Count}");
    }

    /// <summary>游戏侧单卡解析 + freeform 物化（preview 无副作用）。</summary>
    public static void Materialize(CardModel source, Player owner)
    {
        if (!AutoAnthonyNativeCardApi.TryResolve(source, out var decomposition)) return;
        if (!AutoAnthonyNativeCardApi.TryCreateBasePreview(owner, source, out var preview)) return;

        var generated = AutoAnthonyNativeCardApi.TryCreateDefinition(source, out var profileId, out var definition)
            ? definition
            : null;
        Console.WriteLine($"{decomposition.CatalogId} profile={profileId} preview={preview.Definition?.Card?.Cost} generated={generated?.Cost}");

        if (generated is not null && AutoAnthonyFreeformCardApi.Validate(generated).IsValid)
        {
            _ = AutoAnthonyFreeformCardApi.CreateForDeck(owner, generated);
            _ = AutoAnthonyFreeformCardApi.CreateForCombat(owner, ProfileId, generated, "res://cardgen/preview.png");
        }
    }

    /// <summary>只读设置快照 + 平衡调整监听。</summary>
    public static void ObserveSettings()
    {
        var snapshot = AutoAnthonySettingsApi.Current;
        Console.WriteLine($"enabled={snapshot.Enabled} ultimateChaos={snapshot.UltimateChaos} surprise={snapshot.AnySurpriseMode} addGenerated={snapshot.AddGeneratedCards}");

        AutoAnthonyBalanceAdjustmentApi.RegisterDefinitionChangedListener(model =>
            Console.WriteLine($"definition changed: {(model.HasEditorIdentity ? model.EditorIdentity?.Name?.Chinese : "<pool level>")}"));
    }

    /// <summary>自定义关键词的升级规则：满足上下文才允许挂载。</summary>
    private sealed class DemoKeywordRule : IComponentKeywordRule
    {
        public bool CanAttach(ComponentKeywordGenerationContext context) => true;
        public bool CanUpgradeAdd(GeneratedCard card) => true;
        public bool CanUpgradeRemove(GeneratedCard card) => true;
    }

    /// <summary>自定义关键词投影适配器：关键词身份是 ASCII 字符串，绝不制造枚举成员。</summary>
    private sealed class DemoKeywordAdapter : IComponentKeywordRuntimeAdapter
    {
        public IEnumerable<CardKeyword> CanonicalKeywords(ChaosCardModel card) => [];

        public IEnumerable<ChaosCardGenerator.CardTag> SemanticTags(ChaosCardModel card) => [];

        public IEnumerable<IHoverTip> BuildHoverTips(ChaosCardModel card) => [];

        public void OnUpgrade(ChaosCardModel card, bool added) { }
    }

    /// <summary>自定义 opcode 的执行体：返回 true=完成；false=继续走旧路由回退。</summary>
    private sealed class DemoRuntimeHandler : IComponentRuntimeHandler
    {
        public Task<bool> ExecuteAsync(ComponentRuntimeContext context)
        {
            context.RecordDamageDealt(context.Amount);
            return Task.FromResult(true);
        }
    }

    /// <summary>把三个内部实现串起来的注册入口（示例 mod 里从 Initialize 调一次）。</summary>
    public static void RegisterAll()
    {
        EnsureApiVersions();
        RegisterPackage();
        RegisterRuntimeRoutes(new DemoRuntimeHandler());
        RegisterKeywordRuntime(new DemoKeywordAdapter());
        RegisterCharacter();
        RegisterCharacterRuntime();
    }
}