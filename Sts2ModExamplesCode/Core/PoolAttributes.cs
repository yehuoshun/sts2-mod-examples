namespace Sts2ModExamples.Core;

/// <summary>
/// 池注册 Attribute（纯原生实现，对照 skill baselib/design-patterns-core.md 模式1）。
/// 标记在模型类上，ContentRegistry 扫描后调用 ModHelper.AddModelToPool 自动入池。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CardPoolAttribute(Type poolType) : Attribute
{
    public Type PoolType { get; } = poolType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RelicPoolAttribute(Type poolType) : Attribute
{
    public Type PoolType { get; } = poolType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class PotionPoolAttribute(Type poolType) : Attribute
{
    public Type PoolType { get; } = poolType;
}
