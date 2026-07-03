using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Laboratories.Queries.GetLaboratories;

[SharedContract]
public class GetLaboratoriesResult
{
    public required IReadOnlyCollection<LaboratoryDto> Laboratories { get; set; }
    public required int Total { get; set; }
}
