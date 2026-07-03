namespace Immunitas.Application.Services.Hashing;

/// <summary>
/// Интерфейс сервиса по работе с хэшем
/// </summary>
public interface IHashingService
{
    /// <summary>
    /// Вычисляет хэш строки
    /// </summary>
    /// <param name="input">Исходная строка</param>
    /// <returns>Результат хэширования строки</returns>
    string GenerateHash(string input);

    /// <summary>
    /// Вычисляет хэш исходной строки и сравнивает его с переданным хэшем
    /// </summary>
    /// <param name="inputClear">Исходная строка</param>
    /// <param name="inputHash">Хэш, с которым необходимо провести сравнение</param>
    /// <returns>True, если вычисленный хэш исходной строки и переданный хэш совпадают. Иначе - False</returns>
    bool VerifyHash(string inputClear, string inputHash);
}
