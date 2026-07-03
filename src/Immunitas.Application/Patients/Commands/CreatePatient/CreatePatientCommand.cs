using Immunitas.Domain.Entities.Patients;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Patients.Commands.CreatePatient;

[SharedContract]
public class CreatePatientCommand
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public required Gender Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public required int AddressId { get; set; }
}   
