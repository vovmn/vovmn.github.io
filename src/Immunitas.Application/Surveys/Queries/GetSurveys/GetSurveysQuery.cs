using System;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Surveys.Queries.GetSurveys;

[SharedContract]
public class GetSurveysQuery
{
    /// <summary>
    /// Id пациента
    /// </summary>
    public required int PatientId { get; set; }

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Количество обследований
    /// </summary>
    public int Count { get; init; } = 10;

    /// <summary>
    /// Текст для поиска по названию обследования
    /// </summary>
    public string? SearchText { get; init; }
}
