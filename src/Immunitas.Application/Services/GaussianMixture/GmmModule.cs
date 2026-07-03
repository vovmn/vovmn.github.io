using System;
using System.Collections.Generic;
using System.Linq;
using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Measurements;
using Python.Runtime;

namespace Immunitas.Application.Services.GaussianMixture;

/// <summary>
/// Критерий информационного отбора для определения оптимального количества компонент GMM.
/// </summary>
public enum GmmCriterion
{
    /// <summary>
    /// Bayesian Information Criterion (Байесовский информационный критерий). 
    /// Сильнее штрафует за избыточное количество параметров модели.
    /// </summary>
    BIC,

    /// <summary>
    /// Akaike Information Criterion (Информационный критерий Акаике). 
    /// Мягче штрафует за сложность модели, склонен выбирать чуть больше компонент.
    /// </summary>
    AIC
}

/// <summary>
/// Модуль для построения оптимальной модели смеси гауссиан (GMM) на основе гистограммы.
/// Использует интеграцию с Python (через библиотеку Python.Runtime) для вычислений в numpy, scikit-learn и scipy.
/// </summary>
public class GmmModule
{
    /// <summary>
    /// Строит оптимальную модель GMM по точкам гистограммы, автоматически подбирая число компонент (K) 
    /// по заданному критерию в диапазоне от <paramref name="minComponents"/> до <paramref name="maxComponents"/>.
    /// </summary>
    /// <param name="histogramPointDtos">Массив точек гистограммы (X - значение, Y - частота/количество).</param>
    /// <param name="minComponents">Минимальное количество компонент для проверки.</param>
    /// <param name="maxComponents">Максимальное количество компонент для проверки.</param>
    /// <param name="criterion">Информационный критерий для оценки качества модели (AIC или BIC).</param>
    /// <returns>Объект GmmResult, содержащий параметры модели и точки для отрисовки графиков.</returns>
    public static GmmResult BuildOptimalGmmFromHistogram(
        PointDto[] histogramPointDtos, 
        int minComponents = 1,
        int maxComponents = 10, 
        GmmCriterion criterion = GmmCriterion.BIC)
    {
        if (minComponents < 1)
            throw new ArgumentException("Минимальное количество компонент должно быть не меньше 1.", nameof(minComponents));

        if (maxComponents < minComponents)
            throw new ArgumentException("Максимальное количество компонент не может быть меньше минимального.", nameof(maxComponents));

        return ExecuteGmmWorkflow(
            histogramPointDtos, 
            minComponents: minComponents, 
            maxComponents: maxComponents, 
            criterion: criterion, 
            fixedComponentsCount: null, 
            initialMeans: null);
    }

    /// <summary>
    /// Строит модель GMM по точкам гистограммы, используя заданные начальные приближения средних значений (центров).
    /// Количество компонент (K) определяется автоматически по размеру массива <paramref name="initialMeans"/>.
    /// </summary>
    /// <param name="histogramPointDtos">Массив точек гистограммы (X - значение, Y - частота/количество).</param>
    /// <param name="initialMeans">Массив начальных приближений для средних значений (центров) гауссиан.</param>
    /// <returns>Объект GmmResult, содержащий параметры модели и точки для отрисовки графиков.</returns>
    public static GmmResult BuildOptimalGmmFromHistogram(
        PointDto[] histogramPointDtos,
        HashSet<double> initialMeans)
    {
        if (initialMeans == null || initialMeans.Count == 0)
            throw new ArgumentException("Массив начальных приближений не может быть пустым.", nameof(initialMeans));

        return ExecuteGmmWorkflow(
            histogramPointDtos, 
            minComponents: null, 
            maxComponents: null, 
            criterion: null, 
            fixedComponentsCount: null, 
            initialMeans: initialMeans);
    }

    /// <summary>
    /// Единственный внутренний рабочий процесс GMM, координирующий дегруппировку, работу с GIL,
    /// выбор стратегии обучения и последующую генерацию графиков.
    /// </summary>
    private static GmmResult ExecuteGmmWorkflow(
        PointDto[] histogramPointDtos,
        int? minComponents,
        int? maxComponents,
        GmmCriterion? criterion,
        int? fixedComponentsCount,
        HashSet<double>? initialMeans)
    {
        if (histogramPointDtos == null || histogramPointDtos.Length == 0)
            throw new ArgumentException("Массив точек не может быть пустым.", nameof(histogramPointDtos));

        // 1. ДЕГРУППИРОВКА: Превращаем гистограмму в сырой массив размеров клеток.
        List<double> rawSizesList = new List<double>();
        foreach (var pointDto in histogramPointDtos)
        {
            int count = (int)Math.Round(pointDto.Y);
            for (int i = 0; i < count; i++)
            {
                rawSizesList.Add((double)pointDto.X);
            }
        }
        
        double[] rawData = rawSizesList.ToArray();

        if (rawData.Length == 0)
            throw new InvalidOperationException("Не удалось сформировать выборку. Проверьте, что значения Y больше 0.");

        // Блокировка GIL для работы с Python
        using (Py.GIL())
        {
            dynamic np = Py.Import("numpy");
            dynamic mix = Py.Import("sklearn.mixture");

            // Форматируем данные под требования scikit-learn (N, 1)
            dynamic pyData = np.array(rawData).reshape(-1, 1);

            int optimalK;
            GmmResult result;

            // Выбираем сценарий обучения в зависимости от переданных аргументов
            if (initialMeans != null && initialMeans.Count > 0)
            {
                // Сценарий А: Обучение по начальным приближениям средних
                optimalK = initialMeans.Count;
                result = TrainGmmWithComponents(pyData, mix, optimalK, initialMeans);
            }
            else if (fixedComponentsCount.HasValue)
            {
                // Сценарий Б: Обучение для строго фиксированного K
                optimalK = fixedComponentsCount.Value;
                result = TrainGmmWithComponents(pyData, mix, optimalK, null);
            }
            else if (minComponents.HasValue && maxComponents.HasValue && criterion.HasValue)
            {
                // Сценарий В: Автоподбор K по информационному критерию
                optimalK = FindOptimalComponentsCount(pyData, mix, minComponents.Value, maxComponents.Value, criterion.Value);
                result = TrainGmmWithComponents(pyData, mix, optimalK, null);
            }
            else
            {
                throw new InvalidOperationException("Некорректная конфигурация параметров для обучения GMM.");
            }

            // Генерируем сглаженные кривые распределения для графика
            GeneratePlotPointDtos(
                rawData: rawData,
                trainedModel: result
            );

            return result;
        }
    }

    /// <summary>
    /// Перебирает количество компонент от minComponents до maxComponents и выбирает наилучшее K, минимизируя выбранный критерий (AIC или BIC).
    /// </summary>
    private static int FindOptimalComponentsCount(
        dynamic pyData, 
        dynamic mix, 
        int minComponents, 
        int maxComponents, 
        GmmCriterion criterion)
    {
        int bestK = minComponents;
        double bestScore = double.MaxValue;

        for (int k = minComponents; k <= maxComponents; k++)
        {
            dynamic testGmm = mix.GaussianMixture(n_components: k, random_state: 42);
            testGmm.fit(pyData);

            // Вычисляем значение метрики в зависимости от выбранного критерия
            double score = criterion == GmmCriterion.AIC 
                ? (double)testGmm.aic(pyData) 
                : (double)testGmm.bic(pyData);

            if (score < bestScore)
            {
                bestScore = score;
                bestK = k;
            }
        }

        return bestK;
    }

    /// <summary>
    /// Обучает финальную модель GMM с заданным количеством кластеров K, используя (при наличии) начальные приближения средних значений.
    /// </summary>
    private static GmmResult TrainGmmWithComponents(dynamic pyData, dynamic mix, int k, HashSet<double>? initialMeans)
    {
        var result = new GmmResult { OptimalComponents = k };
        dynamic finalGmm;

        if (initialMeans != null && initialMeans.Count > 0)
        {
            dynamic np = Py.Import("numpy");
            // scikit-learn ожидает матрицу средних значений размерности (n_components, n_features)
            dynamic pyMeansInit = np.array(initialMeans.ToArray()).reshape(-1, 1);
            
            finalGmm = mix.GaussianMixture(n_components: k, means_init: pyMeansInit, random_state: 42);
        }
        else
        {
            finalGmm = mix.GaussianMixture(n_components: k, random_state: 42);
        }

        finalGmm.fit(pyData);

        // 1. Извлекаем веса (Weights)
        dynamic pyWeights = finalGmm.weights_;
        result.Weights = (double[])pyWeights.AsManagedObject(typeof(double[]));

        // 2. Извлекаем средние (Means)
        dynamic pyMeans = finalGmm.means_.flatten();
        result.Means = (double[])pyMeans.AsManagedObject(typeof(double[]));

        // 3. Извлекаем ковариации (Covariances)
        dynamic pyCovs = finalGmm.covariances_.flatten();
        result.Covariances = (double[])pyCovs.AsManagedObject(typeof(double[]));

        return result;
    }

    /// <summary>
    /// Генерирует массивы точек для графиков на основе УЖЕ ОБУЧЕННОЙ модели GMM.
    /// </summary>
    private static void GeneratePlotPointDtos(
        double[] rawData, 
        GmmResult trainedModel, 
        int gridSize = 500, 
        double binWidth = 1.0)
    {
        if (trainedModel == null || trainedModel.OptimalComponents == 0)
            throw new ArgumentException("Передана неинициализированная или необученная модель.");

        double gmmTotal = rawData.Length;
        int bestK = trainedModel.OptimalComponents;

        trainedModel.ClusterPointDtos = new List<List<PointDto>>(bestK);
        trainedModel.TotalCurvePointDtos = new List<PointDto>(gridSize);

        dynamic np = Py.Import("numpy");
        dynamic stats = Py.Import("scipy.stats");

        double xMin = rawData.Min();
        double xMax = rawData.Max();
        dynamic pyXGrid = np.linspace(xMin, xMax, gridSize);
        double[] csharpXGrid = (double[])pyXGrid.AsManagedObject(typeof(double[]));

        double[] totalYGrid = new double[gridSize]; 

        for (int k = 0; k < bestK; k++)
        {
            double mu = trainedModel.Means[k];
            double sigma = Math.Sqrt(trainedModel.Covariances[k]); 
            double w = trainedModel.Weights[k];
            double area = w * gmmTotal;

            dynamic normDist = stats.norm(loc: mu, scale: sigma);
            dynamic pyPdf = normDist.pdf(pyXGrid);
            double[] pdfValues = (double[])pyPdf.AsManagedObject(typeof(double[]));

            var clusterPointDtos = new List<PointDto>(gridSize);
            for (int i = 0; i < gridSize; i++)
            {
                double yValue = pdfValues[i] * area / binWidth;
                clusterPointDtos.Add(new PointDto(csharpXGrid[i], yValue));
                totalYGrid[i] += yValue;
            }
            
            trainedModel.ClusterPointDtos.Add(clusterPointDtos);
        }

        for (int i = 0; i < gridSize; i++)
        {
            trainedModel.TotalCurvePointDtos.Add(new PointDto(csharpXGrid[i], totalYGrid[i]));
        }
    }
}