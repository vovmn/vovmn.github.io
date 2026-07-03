using System;
using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Surveys.Queries.GetSurveys;

[SharedContract]
public class GetSurveysQueryResult
{
    public required SurveyDto[] Surveys { get; init; } = [];
    public required int Total { get; set; }
}
