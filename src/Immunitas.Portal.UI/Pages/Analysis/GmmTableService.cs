using System;

namespace Immunitas.Portal.UI.Pages.Analysis;

/// <summary>
/// Представляет строку итоговой таблицы сравнительного анализа фракций клеток.
/// </summary>
public class GmmComparisonRow
{
    /// <summary>
    /// Название фракции
    /// </summary>
    public string Fraction { get; set; } = string.Empty;

    /// <summary>
    /// Средний размер в эталоне (Mat. expectation)
    /// </summary>
    public double MuEt { get; set; }

    /// <summary>
    /// Средний размер с антигеном
    /// </summary>
    public double MuAg { get; set; }

    /// <summary>
    /// Стандартное отклонение в эталоне
    /// </summary>
    public double SigmaEt { get; set; }

    /// <summary>
    /// Стандартное отклонение с антигеном
    /// </summary>
    public double SigmaAg { get; set; }

    /// <summary>
    /// Процент популяции в эталоне
    /// </summary>
    public double PercentEt { get; set; }

    /// <summary>
    /// Процент популяции с антигеном
    /// </summary>
    public double PercentAg { get; set; }

    /// <summary>
    /// Отношение весов (процентов)
    /// </summary>
    public double RatioWeights { get; set; }

    /// <summary>
    /// Интегральное изменение (процент * сдвиг центра)
    /// </summary>
    public double WeightMuProduct { get; set; }

    /// <summary>
    /// Разница в процентах (Antigen - Ethalone)
    /// </summary>
    public double DiffPercent { get; set; }

    /// <summary>
    /// Разница средних размеров (сдвиг пика)
    /// </summary>
    public double DiffMu { get; set; }

    /// <summary>
    /// RGBA-строка цвета заднего фона для подсветки изменений
    /// </summary>
    public string? GradientBg { get; set; }
}

/// <summary>
/// Результат сравнительного анализа двух GMM моделей.
/// </summary>
public class GmmComparisonResult
{
    /// <summary>
    /// Информация по каждому ряду таблицы
    /// </summary>
    public List<GmmComparisonRow> Rows { get; set; } = [];

    /// <summary>
    /// Максимальное зафиксированное отклонение для шкалы
    /// </summary>
    public double MaxValue { get; set; }
}

public class GmmTableService
{
    /// <summary>
    /// Сопоставляет результаты двух обученных моделей GMM (Эталон и Антиген) и готовит данные для таблицы.
    /// </summary>
    /// <param name="ethalone">Обученная модель эталонного замера.</param>
    /// <param name="antigen">Обученная модель замера с добавлением антигена.</param>
    /// <param name="fractionNames">Необязательные пользовательские названия фракций.</param>
    /// <returns>Данные сравнения для построения таблицы или null, если сравнение невозможно.</returns>
    public static GmmComparisonResult? PrepareTableData(
        GmmParameters ethalone, 
        GmmParameters antigen, 
        List<string>? fractionNames = null)
    {
        int kEt = ethalone.OptimalComponents;
        int kAg = antigen.OptimalComponents;

        // Сравнение возможно только при одинаковом количестве найденных популяций (кластеров)
        if (kEt != kAg || kEt == 0)
            return null;

        // Переводим веса в проценты (0.0 - 1.0 -> 0.0 - 100.0)
        double[] percentEt = ethalone.Weights.Select(w => w * 100.0).ToArray();
        double[] percentAg = antigen.Weights.Select(w => w * 100.0).ToArray();

        // Извлекаем среднеквадратичное (стандартное) отклонение из дисперсий (ковариаций)
        double[] sigmaEt = ethalone.Covariances.Select(Math.Sqrt).ToArray();
        double[] sigmaAg = antigen.Covariances.Select(Math.Sqrt).ToArray();

        // Поиск максимального значения отклонения для нормализации градиента цвета
        var valuesForGradient = new List<double>();
        for (int i = 0; i < kEt; i++)
        {
            double ratio = percentEt[i] > 0 ? percentAg[i] / percentEt[i] : 1.0;
            double product = percentAg[i] * (antigen.Means[i] - ethalone.Means[i]);
            double diffPct = percentAg[i] - percentEt[i];
            double diffMu = antigen.Means[i] - ethalone.Means[i];

            if (Math.Abs(ratio - 1.0) > 0.01)
                valuesForGradient.Add(Math.Abs(ratio - 1.0));
            if (Math.Abs(product) > 0.01)
                valuesForGradient.Add(Math.Abs(product));
            if (Math.Abs(diffPct) > 0.01)
                valuesForGradient.Add(Math.Abs(diffPct));
            if (Math.Abs(diffMu) > 0.01)
                valuesForGradient.Add(Math.Abs(diffMu));
        }

        double maxValue = valuesForGradient.Count > 0 ? valuesForGradient.Max() : 1.0;

        var result = new GmmComparisonResult { MaxValue = maxValue };

        // Заполнение строк сравнительной таблицы
        for (int i = 0; i < kEt; i++)
        {
            double ratioWeights = percentEt[i] > 0 ? percentAg[i] / percentEt[i] : 1.0;
            double weightMuProduct = percentAg[i] * (antigen.Means[i] - ethalone.Means[i]);
            double diffPercent = percentAg[i] - percentEt[i];
            double diffMu = antigen.Means[i] - ethalone.Means[i];

            // Определение имени фракции
            string fractionName = fractionNames != null && i < fractionNames.Count 
                ? fractionNames[i] 
                : $"C{i + 1}";

            // Вычисляем подсветку на основе изменения отношения весов фракций
            string? gradientBg = GetGradientBg(ratioWeights - 1.0, maxValue, isPositive: true);

            result.Rows.Add(new GmmComparisonRow
            {
                Fraction = fractionName,
                MuEt = ethalone.Means[i],
                MuAg = antigen.Means[i],
                SigmaEt = sigmaEt[i],
                SigmaAg = sigmaAg[i],
                PercentEt = percentEt[i],
                PercentAg = percentAg[i],
                RatioWeights = ratioWeights,
                WeightMuProduct = weightMuProduct,
                DiffPercent = diffPercent,
                DiffMu = diffMu,
                GradientBg = gradientBg
            });
        }

        return result;
    }

    /// <summary>
    /// Генерирует CSS-совместимый цвет RGBA для подсветки изменений в ячейках таблицы.
    /// </summary>
    /// <param name="value">Текущее отклонение (например, разница в соотношении весов).</param>
    /// <param name="maxVal">Глобальный максимум отклонений для нормирования шкалы.</param>
    /// <param name="isPositive">Если true, положительное отклонение подсвечивается зеленым, отрицательное - красным.</param>
    private static string? GetGradientBg(double value, double maxVal, bool isPositive = true)
    {
        if (Math.Abs(value) < 0.01)
            return null;

        // Рассчитываем интенсивность цвета от 50 до 205 (плавное нарастание яркости)
        int intensity = (int)(50 + Math.Min(Math.Abs(value) / maxVal, 1.0) * 155);

        // Полупрозрачный альфа-канал (0.4), чтобы цвет не перекрывал текст в таблице
        double alpha = 0.4; 

        if ((value > 0 && isPositive) || (value < 0 && !isPositive))
        {
            // Положительное изменение (рост популяции) — зеленый оттенок
            return $"rgba(100, {100 + intensity}, 100, {alpha.ToString("G", System.Globalization.CultureInfo.InvariantCulture)})";
        }
        else
        {
            // Отрицательное изменение (сокращение популяции) — красный оттенок
            return $"rgba({100 + intensity}, 100, 100, {alpha.ToString("G", System.Globalization.CultureInfo.InvariantCulture)})";
        }
    }
}

public class GmmParameters
{
    public required int OptimalComponents { get; set; }
    public required double[] Weights { get; set; }
    public required double[] Means { get; set; }
    public required double[] Covariances { get; set; }
}