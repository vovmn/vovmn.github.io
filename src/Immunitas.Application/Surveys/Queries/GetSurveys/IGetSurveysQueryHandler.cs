using System;

namespace Immunitas.Application.Surveys.Queries.GetSurveys;

public interface IGetSurveysQueryHandler : IHandler
{
    Task<GetSurveysQueryResult> Handle(GetSurveysQuery query, CancellationToken cancellationToken = default);
}
