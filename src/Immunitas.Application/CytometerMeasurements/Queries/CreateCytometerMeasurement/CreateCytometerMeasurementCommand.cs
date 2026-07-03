namespace Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;

public class CreateCytometerMeasurementCommand
{
    public required int PatientId { get; set; }
    public required int SampleId { get; set; }
    public required int SurveyId { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? AntigenId { get; set; }
    public required HistogramsDto Histograms { get; set; }
    public required ParameterDto[] Parameters { get; set; }
}

public class HistogramsDto
{
    public int[] PLT { get; set; } = [];
    public int[] RBC { get; set; } = [];
    public int[] WBC { get; set; } = [];
}

public class ParameterDto
{
    public required string Name { get; set; }
    public required double Value { get; set; }
}