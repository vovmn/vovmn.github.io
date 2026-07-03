using System.ComponentModel.DataAnnotations;

namespace Immunitas.Application.Services.TokensGeneration;

public class TokenCreationOptions
{
    public const string SectionName = "TokenCreation";

    [Required(AllowEmptyStrings = false), MinLength(32)]
    public required string Key { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Issuer { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Audience { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int AccessTokenExpirationMinutes { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int RefreshTokenExpirationDays { get; init; }
}
