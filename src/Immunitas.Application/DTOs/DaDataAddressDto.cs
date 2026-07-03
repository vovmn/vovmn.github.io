using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs.DaData;

/// <summary>
/// Ответ от DaData API
/// </summary>
[SharedContract]
public class DaDataDto
{
    public List<DaDataSuggestion> Suggestions { get; set; } = new();
}

/// <summary>
/// Предложение адреса от DaData
/// </summary>
[SharedContract]
public class DaDataSuggestion
{
    public string Value { get; set; } = string.Empty;
    public string UnrestrictedValue { get; set; } = string.Empty;
    public DaDataAddressData Data { get; set; } = new();
}

/// <summary>
/// Данные адреса от DaData
/// </summary>
[SharedContract]
public class DaDataAddressData
{
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? Settlement { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Flat { get; set; }
}



