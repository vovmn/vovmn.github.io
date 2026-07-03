using Immunitas.Application.Exceptions;
using Immunitas.Domain.Entities.Measurements;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using System.Data;

namespace Immunitas.Application.Services.CytometerDataProcessor;

public class CytometerDataProcessor(ILogger<CytometerDataProcessor> logger) : ICytometerDataProcessor
{
    public AnalysisResult Process(
        Point[][] referenceRbcSamples,
        Point[][] testRbcSamples,
        Point[][] referenceWbcSamples,
        Point[][] testWbcSamples)
    {
        var referenceRbcPoints = ComputeAverage(referenceRbcSamples);
        var testRbcPoints = ComputeAverage(testRbcSamples);
        var referenceWbcPoints = ComputeAverage(referenceWbcSamples);
        var testWbcPoints = ComputeAverage(testWbcSamples);

        return ProcessInternal(
            referenceRbcPoints: referenceRbcPoints,
            testRbcPoints: testRbcPoints,
            referenceWbcPoints: referenceWbcPoints,
            testWbcPoints: testWbcPoints);

        List<Point> ComputeAverage(Point[][] samples)
        {
            var size = samples[0].Length;
            if (samples.Any(s => s.Length != size))
                throw new ArgumentException("Массивы образцов должны иметь равное количество элементов");

            var result = new List<Point>(size);
            for (int i = 0; i < size; i++)
            {
                var x = samples[0][i].X;
                var sumY = 0.0;
                foreach (var sample in samples)
                {
                    if (sample[i].X != x)
                        throw new ArgumentException("Значения X в соответствующих точках должны быть одинаковы");
                    sumY += sample[i].Y;
                }
                var averageY = sumY / samples.Length;
                result.Add(new Point(x, averageY));
            }

            return result;
        }
    }

    public AnalysisResult Process(
        Point[] referenceRbcPoints,
        Point[] testRbcPoints,
        Point[] referenceWbcPoints,
        Point[] testWbcPoints)
    {
        return Process(referenceRbcPoints, testRbcPoints, referenceWbcPoints, testWbcPoints);
    }

    private AnalysisResult ProcessInternal(List<Point> referenceRbcPoints, List<Point> testRbcPoints, List<Point> referenceWbcPoints, List<Point> testWbcPoints)
    {
        logger.LogDebug("Подготовка данных");

        logger.LogDebug("Нормализуем WBCa");
        var normalizedTestWbcPoints = NormalizeByRedBloodCells(referenceRbcPoints, testRbcPoints, referenceWbcPoints, testWbcPoints);

        logger.LogDebug("Ищем пики");
        var referenceExtrema = FindExtrema(referenceWbcPoints);

        logger.LogInformation("Minima: {Minima}", referenceExtrema.Minima.Select(p => p.ToString()));
        logger.LogInformation("Maxima: {Maxima}", referenceExtrema.Maxima.Select(p => p.ToString()));

        var (firstMaxima, globalMinima, secondMaxima) = SelectKeyExtrema(
            minima: referenceExtrema.Minima,
            maxima: referenceExtrema.Maxima);
        var firstNonZero = FindFirstNonZeroPoint(wbcePoints: referenceWbcPoints, wbcaPoints: normalizedTestWbcPoints);
        var lastNonZero = FindLastNonZeroPoint(wbcePoints: referenceWbcPoints, wbcaPoints: normalizedTestWbcPoints);

        logger.LogInformation("Пики: {Max1}, {Min}, {Max2}. Первая ненулевая: {FirstNonZero}. Последняя ненулевая: {LastNonZero}",
            firstMaxima,
            globalMinima,
            secondMaxima,
            firstNonZero,
            lastNonZero);

        try
        {

            var referenceSectors = new List<Sector>();
            referenceSectors.AddRange(new Sector(referenceWbcPoints, firstNonZero, firstMaxima.X).DivideSector(3));
            referenceSectors.AddRange(new Sector(referenceWbcPoints, firstMaxima.X, globalMinima.X).DivideSector(3));
            referenceSectors.AddRange(new Sector(referenceWbcPoints, globalMinima.X, secondMaxima.X).DivideSector(3));
            referenceSectors.AddRange(new Sector(referenceWbcPoints, secondMaxima.X, lastNonZero).DivideSector(5));

            var testSectors = new List<Sector>();
            testSectors.AddRange(new Sector(normalizedTestWbcPoints, firstNonZero, firstMaxima.X).DivideSector(3));
            testSectors.AddRange(new Sector(normalizedTestWbcPoints, firstMaxima.X, globalMinima.X).DivideSector(3));
            testSectors.AddRange(new Sector(normalizedTestWbcPoints, globalMinima.X, secondMaxima.X).DivideSector(3));
            testSectors.AddRange(new Sector(normalizedTestWbcPoints, secondMaxima.X, lastNonZero).DivideSector(5));

            List<SectorDifference> sectorFractionalDifferences = CalculateFractionalDifferencesForGroups(
                referenceSectors: referenceSectors,
                testSectors: testSectors
            );

            var stepDifferences = CalculateStepDifferences(
                originalReferenceWbc: referenceWbcPoints,
                normalizedTestWbc: normalizedTestWbcPoints);

            var result = new AnalysisResult(
                ProcessedReferenceData: referenceWbcPoints,
                ProcessedTestData: normalizedTestWbcPoints,
                StepDifferences: stepDifferences,
                SectorFractionalDifferences: sectorFractionalDifferences,
                ReferenceDataMinima: referenceExtrema.Minima,
                ReferenceDataMaxima: referenceExtrema.Maxima);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Произошла ошибка при обработке данных");
            var result = new AnalysisResult(
                ProcessedReferenceData: referenceWbcPoints,
                ProcessedTestData: normalizedTestWbcPoints,
                StepDifferences: [],
                SectorFractionalDifferences: [],
                ReferenceDataMinima: referenceExtrema.Minima,
                ReferenceDataMaxima: referenceExtrema.Maxima);

            return result;
        }

    }

    private static List<Point> NormalizeByRedBloodCells(
        List<Point> referenceRbcSample,
        List<Point> testRbcSample,
        List<Point> referenceWbcSample,
        List<Point> testWbcSample)
    {
        // Вычисляем суммарные интенсивности (аналогично количеству клеток)
        double totalRbce = referenceRbcSample.Sum(p => p.Y);
        double totalRbca = testRbcSample.Sum(p => p.Y);
        double totalWbce = referenceWbcSample.Sum(p => p.Y);
        double totalWbca = testWbcSample.Sum(p => p.Y);

        // Коэффициент коррекции объема
        double volumeFactor = totalRbca / totalRbce;

        // Ожидаемая суммарная интенсивность WBC с антигеном
        double expectedTotalWbca = totalWbce * volumeFactor;

        // Коэффициент масштабирования
        double scalingFactor = expectedTotalWbca / totalWbca;

        // Масштабируем данные
        List<Point> normalizedWbca = testWbcSample.Select(p => p with { Y = p.Y * scalingFactor }).ToList();

        return normalizedWbca;
    }

    private List<Point> InterpolateWhiteBloodCellsViaPython(List<Point> sample)
    {
        double[] x = sample.Select(p => p.X).ToArray();
        double[] y = sample.Select(p => p.Y).ToArray();

        logger.LogDebug("Подключаем питон");
        using (Py.GIL())
        {
            logger.LogDebug("Импортируем библиотеки");
            dynamic np = Py.Import("numpy");
            dynamic interp = Py.Import("scipy.interpolate");

            logger.LogDebug("Создаем массивы нампай");
            dynamic npX = np.array(x);
            dynamic npY = np.array(y);

            logger.LogDebug("Вызываем интерполяцию");
            dynamic spl = interp.UnivariateSpline(x, y, k: 2);
            logger.LogDebug("Интерполируем");
            dynamic result = spl(x);

            logger.LogDebug("Подготовка результата");
            // numpy.ndarray -> Python list -> C# List<double>
            dynamic pyList = result.tolist();
            var list = ((PyObject)pyList).As<double[]>();

            if (list.Length != x.Length)
                throw new CytometerDataProcessingException("После интерполяции массивы имеют разные длины");

            return x.Zip(list).Select(p => new Point(p.First, p.Second)).ToList();
        }
    }

    private (List<Point> Minima, List<Point> Maxima) FindExtrema(List<Point> originalReferenceWbc)
    {
        var interpolatedReferenceWbc = InterpolateWhiteBloodCellsViaPython(originalReferenceWbc);
        logger.LogInformation("Интерполяция эталона: {Interpolated}",
            string.Join(", ", interpolatedReferenceWbc.Select(p => p.ToString())));

        List<Point> maxima = [];
        List<Point> minima = [];

        for (int i = 1; i < interpolatedReferenceWbc.Count - 1; i++)
        {
            var current = interpolatedReferenceWbc[i];
            var prev = interpolatedReferenceWbc[i - 1];
            var next = interpolatedReferenceWbc[i + 1];
            var originalCurrent = current with { Y = originalReferenceWbc[i].Y };
            if (current.Y > prev.Y && current.Y > next.Y)
                maxima.Add(originalCurrent);
            else if (current.Y < prev.Y && current.Y < next.Y)
                minima.Add(originalCurrent);
        }

        return (minima, maxima);
    }

    private static (Point FirstMaxima, Point GlobalMinima, Point SecondMaxima) SelectKeyExtrema(
        List<Point> minima, List<Point> maxima)
    {
        var globalMinima = minima.Where(p => p.Y >= 1).MinBy(p => p.Y)
            ?? throw new CytometerDataProcessingException("Глобальный минимум не найден");

        var firstMaxima = maxima.TakeWhile(p => p.X < globalMinima.X).MaxBy(p => p.Y) 
            ?? throw new CytometerDataProcessingException("Максимум перед глобальным минимумом не найден");

        var secondMaxima = maxima.SkipWhile(p => p.X < globalMinima.X).MaxBy(p => p.Y)
            ?? throw new CytometerDataProcessingException("Максимум после глобального минимума не найден");

        return (firstMaxima, globalMinima, secondMaxima);
    }

    private static double FindFirstNonZeroPoint(List<Point> wbcePoints, List<Point> wbcaPoints)
    {
        var wbceNonZero = wbcePoints.FirstOrDefault(p => p.Y > 0)?.X
            ?? throw new CytometerDataProcessingException("Не найдено точек отличных от нуля");

        var wbcaNonZero = wbcaPoints.FirstOrDefault(p => p.Y > 0)?.X
            ?? throw new CytometerDataProcessingException("Не найдено точек отличных от нуля");

        return wbceNonZero < wbcaNonZero ? wbceNonZero : wbcaNonZero;
    }

    private static double FindLastNonZeroPoint(List<Point> wbcePoints, List<Point> wbcaPoints)
    {
        var wbceNonZero = wbcePoints.LastOrDefault(p => p.Y > 0)?.X
            ?? throw new CytometerDataProcessingException("Не найдено точек отличных от нуля");

        var wbcaNonZero = wbcaPoints.LastOrDefault(p => p.Y > 0)?.X
            ?? throw new CytometerDataProcessingException("Не найдено точек отличных от нуля");

        return wbceNonZero < wbcaNonZero ? wbceNonZero : wbcaNonZero;
    }

    private static List<SectorDifference> CalculateFractionalDifferencesForGroups(
        List<Sector> referenceSectors,
        List<Sector> testSectors)
    {
        if (referenceSectors.Count != testSectors.Count)
            throw new ArgumentException("Переданные массивы имеют разные длины");

        var referenceSumSquare = referenceSectors.Sum(s => s.Square);
        var sectorDifferences = new List<SectorDifference>();
        for (var i = 0; i < referenceSectors.Count; i++)
        {
            var fractionalDifference = (testSectors[i].Square - referenceSectors[i].Square) / referenceSumSquare;
            var sectorDifference = new SectorDifference(
                StartX: referenceSectors[i].StartX,
                EndX: referenceSectors[i].EndX,
                FractionalDifference: fractionalDifference);
            sectorDifferences.Add(sectorDifference);
        }

        return sectorDifferences;
    }

    private static List<Point> CalculateStepDifferences(
        List<Point> originalReferenceWbc,
        List<Point> normalizedTestWbc)
    {
        if (originalReferenceWbc.Count != normalizedTestWbc.Count)
            throw new ArgumentException("Переданные массивы имеют разные длины");

        // Из теста вычитаем соответствующие значения эталона
        return normalizedTestWbc.Zip(originalReferenceWbc)
            .Select(pair => new Point(pair.Second.X, pair.First.Y - pair.Second.Y))
            .ToList();
    }
}

#region Data structures

/// <summary>
/// Процентная разница по сектору.
/// </summary>
public record SectorDifference(double StartX, double EndX, double FractionalDifference);

/// <summary>
/// Результаты анализа: список различий по каждому сектору.
/// </summary>
public record AnalysisResult(
    IEnumerable<Point> ProcessedReferenceData,
    IEnumerable<Point> ProcessedTestData,
    IEnumerable<Point> StepDifferences,
    IEnumerable<SectorDifference> SectorFractionalDifferences,
    IEnumerable<Point> ReferenceDataMinima,
    IEnumerable<Point> ReferenceDataMaxima);

public class Sector
{
    private List<Point> _points = [new Point(0, 0)];
    private double? _square;
    public double StartX => _points[0].X;
    public double EndX => _points[^1].X;
    public double Square => _square ??= CalculateSquare();
    public Sector(IEnumerable<Point> points)
    {
        _points = points.ToList();
    }
    public Sector(IEnumerable<Point> points, double startX, double EndX)
    {
        _points = points.Where(p => p.X >= startX && p.X < EndX).ToList();
    }
    public IEnumerable<Sector> DivideSector(int numberOfParts)
    {
        if (numberOfParts > _points.Count)
            throw new ArgumentException("Количество подсекторов не может быть больше числа точек в исходном секторе");

        if (numberOfParts <= 0)
            throw new ArgumentException("Количество частей должно быть положительным числом");

        if (numberOfParts == 1)
            return [this];

        int totalPoints = _points.Count;
        int baseSize = totalPoints / numberOfParts;
        int remainder = totalPoints % numberOfParts;

        var sectors = new List<Sector>();
        int currentIndex = 0;

        for (int i = 0; i < numberOfParts; i++)
        {
            // Определяем размер текущей части
            int partSize = baseSize + (i < remainder ? 1 : 0);

            // Берем срез точек для нового сектора
            var sectorPoints = _points.GetRange(currentIndex, partSize);
            sectors.Add(new Sector(sectorPoints));

            // Сдвигаем индекс для следующей части
            currentIndex += partSize;
        }

        return sectors;
    }
    private double CalculateSquare() => _points.Sum(p => p.Y);
}

#endregion