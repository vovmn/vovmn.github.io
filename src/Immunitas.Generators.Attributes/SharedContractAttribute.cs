namespace Immunitas.Generators.Attributes;

/// <summary>
/// Маркерный аттрибут, которым должны быть помечены все DTO, запросы и комманды для
/// переноса их на фронтенд приложение
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public sealed class SharedContractAttribute : Attribute
{
}
