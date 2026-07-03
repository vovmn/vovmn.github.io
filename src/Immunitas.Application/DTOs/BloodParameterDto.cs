using System;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

/// <summary>
/// Модель параметра крови, измеряемый на цитометре
/// </summary>
[SharedContract]
public class BloodParameterDto
{
    /// <summary>
    /// Id параметра
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Название параметра
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Значение параметра
    /// </summary>
    public required double Value { get; set; }
}
