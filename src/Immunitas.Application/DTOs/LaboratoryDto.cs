using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class LaboratoryDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}
