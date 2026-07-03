using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Samples.Queries.GetPatientSamples;

[SharedContract]
public class GetPatientSamplesQueryResult
{
    public IReadOnlyList<SampleDto> Samples { get; set; } = [];
    public int TotalCount { get; set; }
}
