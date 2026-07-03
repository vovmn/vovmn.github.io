using Immunitas.Generators.Attributes;

namespace Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurements;

[SharedContract]
public class GetCytometerMeasurementsQuery
{
    public required int SurveyId { get; set; }
}
