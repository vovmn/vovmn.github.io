using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Samples.Queries.GetPatientSamples;

[SharedContract]
public class GetPatientSamplesQuery
{
    /// <summary>
    /// Id пациента, для которого необходимо получить список образцов крови
    /// </summary>
    public int PatientId { get; init; }

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Количество образцов
    /// </summary>
    public int Count { get; init; } = 10;
}
