using Immunitas.Domain.Entities.Measurements;

namespace Immunitas.Application.Services.CytometerDataProcessor
{
    /// <summary>
    /// Интерфейс обработки цитометрических данных: нормировка, нелинейная интерполяция,
    /// поиск экстремумов, разбиение на сектора, вычисление разницы.
    /// </summary>
    public interface ICytometerDataProcessor
    {
        AnalysisResult Process(
            Point[] referenceRbcPoints,      
            Point[] testRbcPoints,
            Point[] referenceWbcPoints,
            Point[] testWbcPoints);

        AnalysisResult Process(
            Point[][] referenceRbcSamples,
            Point[][] testRbcSamples,
            Point[][] referenceWbcSamples,
            Point[][] testWbcSamples);
    }
}

