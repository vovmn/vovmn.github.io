using System;

namespace Immunitas.Application.CytometerMeasurements.Queries.PerformGmmAnalysis;

public interface IPerformGmmAnalysisQueryHandler : IHandler
{
    Task<PerformGmmAnalysisQueryResult> Handle(PerformGmmAnalysisQuery query, CancellationToken cancellationToken = default);
}
