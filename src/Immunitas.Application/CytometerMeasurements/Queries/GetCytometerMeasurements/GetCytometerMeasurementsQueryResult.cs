using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurements
{
    [SharedContract]
    public class GetCytometerMeasurementsQueryResult
    {
        public required List<CytometerMeasurementDto> CytometerMeasurements { get; set; }
    }
}
