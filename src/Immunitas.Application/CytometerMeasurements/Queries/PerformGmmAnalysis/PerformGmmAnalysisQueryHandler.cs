using System.Diagnostics;
using Immunitas.Application.DTOs;
using Immunitas.Application.Services.GaussianMixture;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.CytometerMeasurements.Queries.PerformGmmAnalysis;

/// <summary>
/// Обработчик запроса на проведение GMM-анализа цитометрического измерения.
/// Анализирует входящие параметры запроса и маршрутизирует вызов к соответствующей перегрузке GmmModule.
/// </summary>
public class PerformGmmAnalysisQueryHandler(
    IRepository<CytometerMeasurement> cytometerMeasurementsRepository,
    ILogger<PerformGmmAnalysisQueryHandler> logger
) : IPerformGmmAnalysisQueryHandler
{
    public async Task<PerformGmmAnalysisQueryResult> Handle(PerformGmmAnalysisQuery query, CancellationToken cancellationToken = default)
    {
        // Извлекаем измерение по ID
        var measurement = await cytometerMeasurementsRepository
            .GetById(query.CytometerMeasurementId)
            .SingleOrDefaultAsync(cancellationToken) 
            ?? throw new EntityNotFoundException(nameof(CytometerMeasurement), query.CytometerMeasurementId);

        // Превращаем доменное распределение в массив DTO точек для анализа
        var points = measurement.WbcDistribution.Select(PointDto.FromDomain).ToArray();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        GmmResult gmmResult;

        if (query.InitialsMeans is { Count: > 0 })
        {
            gmmResult = GmmModule.BuildOptimalGmmFromHistogram(points, query.InitialsMeans);
        }
        else
        {
            int min = query.MinComponents ?? 1;
            int max = query.MaxComponents ?? 10;
            
            var serviceCriterion = query.Criterion == GmmCriterion.AIC
                ? Services.GaussianMixture.GmmCriterion.AIC
                : Services.GaussianMixture.GmmCriterion.BIC;

            gmmResult = GmmModule.BuildOptimalGmmFromHistogram(points, min, max, serviceCriterion);
        }

        stopwatch.Stop();

        // Формируем результат выполнения запроса
        var result = new PerformGmmAnalysisQueryResult
        {
            ClusterPoints = gmmResult.ClusterPointDtos,
            Covariances = gmmResult.Covariances,
            Means = gmmResult.Means,
            OptimalComponents = gmmResult.OptimalComponents,
            TotalCurvePoints = gmmResult.TotalCurvePointDtos,
            Weights = gmmResult.Weights,
        };

        logger.LogInformation("Gmm анализ проведен. Найдено оптимальное K: {K}. Анализ занял {Elapsed} мс", 
            gmmResult.OptimalComponents, 
            stopwatch.ElapsedMilliseconds);
            
        return result;
    }
}