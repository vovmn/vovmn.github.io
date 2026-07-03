using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Patients.Commands.CreatePatient;

[SharedContract]
public class CreatePatientCommandResult
{
    public required int Id { get; set; }
}