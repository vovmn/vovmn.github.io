using Immunitas.Generators.Attributes;

namespace Immunitas.Application.CytometerMeasurements.Queries.PerformGmmAnalysis;

[SharedContract]
public class PerformGmmAnalysisQuery
{
    public required int CytometerMeasurementId { get; init; }
    public int? MinComponents { get; init; }
    public int? MaxComponents { get; set; }
    public HashSet<double> InitialsMeans { get; set; } = [];
    public GmmCriterion? Criterion { get; set; }
}

[SharedContract]
public enum GmmCriterion
{
    BIC,
    AIC
}
