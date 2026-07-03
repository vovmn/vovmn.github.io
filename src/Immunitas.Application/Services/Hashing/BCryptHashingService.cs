namespace Immunitas.Application.Services.Hashing;

/// <summary>
/// Реализация сервиса по работе с хэшем при помощи библиотеки BCrypt
/// </summary>
public class BCryptHashingService : IHashingService
{
    /// <inheritdoc/>
    public string GenerateHash(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }

    /// <inheritdoc/>
    public bool VerifyHash(string inputClear, string inputHash)
    {
        return BCrypt.Net.BCrypt.Verify(inputClear, inputHash);
    }
}
