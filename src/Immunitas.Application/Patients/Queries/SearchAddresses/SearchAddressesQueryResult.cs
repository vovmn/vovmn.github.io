using Immunitas.Generators.Attributes;
using Immunitas.Application.DTOs;

namespace Immunitas.Application.Patients.Queries.SearchAddresses;

[SharedContract]
public class SearchAddressesQueryResult
{
    public required IReadOnlyList<AddressDto> Addresses { get; set; }
}
