using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class SurveyDto
{
    public required int Id { get; init; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CytometerMeasurementDto[] CytometerMeasurements { get; set; } = [];
}
