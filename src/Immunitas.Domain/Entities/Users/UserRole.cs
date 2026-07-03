using Immunitas.Generators.Attributes;

namespace Immunitas.Domain.Entities.Users;

[SharedContract]
public enum UserRole
{
    Admin = 1,
    Doctor,
    Nurse,
    Anonymous
}
