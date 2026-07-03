using Immunitas.Generators.Attributes;


namespace Immunitas.Application.Patients.Queries.SearchAddresses;

[SharedContract]
public class SearchAddressesQuery
{
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public int Take { get; set; } = 10;
}
