using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Laboratories.Queries.GetLaboratories;

[SharedContract]
public class GetLaboratoriesQuery
{
    public required int Page { get; set; } = 1;

    public required int Count { get; set; } = 10;
}
