using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Patients.Commands.CreateAddress;

[SharedContract]
public class CreateAddressCommandResult
{
    public required int Id { get; set; }
    public required string Country { get; set; }
    public required string Region { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string House { get; set; }
    public string? Apartment { get; set; }
}
