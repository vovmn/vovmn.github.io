using System;
using Immunitas.Portal.UI.ApiProxies.ApiServices.CytometerMeasurements;
using Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;
using Immunitas.Portal.UI.ApiProxies.ApiServices.Surveys;
using Immunitas.Portal.UI.ApiProxies.Contracts;
using Microsoft.AspNetCore.Components;
using Immunitas.Portal.UI.ApiProxies.Contracts.Enums;

namespace Immunitas.Portal.UI.Pages.Analysis;

public partial class AnalysisTab(
    IPatientsApiService patientsApiService,
    ISurveysApiService surveysApiService,
    ICytometerMeasurementsApiService cytometerMeasurementsApiService) : ComponentBase
{
    private PatientDto? _selectedPatient;
    private async Task<IEnumerable<PatientDto>> SearchPatient(string fullName, CancellationToken cancellationToken)
    {
        var patientsResult = await patientsApiService.GetPatients(new()
        {
            Count = 10,
            OrderBy = nameof(PatientDto.FullName),
            Page = 1,
            SearchText = fullName
        }, cancellationToken);
        return patientsResult.Patients;
    }
    private async Task OnPatientChanged(PatientDto patient)
    {
        _selectedPatient = patient;
        await LoadSurveys();
        if (_surveys.Length != 0)
            await OnSurveyChanged(_surveys[0]);
    }

    private SurveyDto? _selectedSurvey;
    private SurveyDto[] _surveys = [];
    private async Task LoadSurveys()
    {
        if (_selectedPatient is null)
            return;

        var surveysResult = await surveysApiService.GetSurveys(new()
        {
            Count = 10,
            Page = 1,
            PatientId = _selectedPatient.Id,
            SearchText = null 
        });
        _surveys = surveysResult.Surveys;
    }
    private async Task OnSurveyChanged(SurveyDto survey)
    {
        _selectedSurvey = survey;
        await LoadMeasurements();
        var ethaloneMeasurement = _cytometerMeasurements.FirstOrDefault(m => m.AntigenName == null);
        if (ethaloneMeasurement != null)
            await OnEthaloneMeasurementChanged(ethaloneMeasurement);
        var antigenMeasurement = _cytometerMeasurements.FirstOrDefault(m => m.AntigenName != null);
        if (antigenMeasurement != null)
            await OnAntigenMeasurementChanged(antigenMeasurement);
        StateHasChanged();
    }

    private CytometerMeasurementDto[] _cytometerMeasurements = [];
    private CytometerMeasurementDto? _selectedEthaloneMeasurement;
    private CytometerMeasurementDto? _selectedAntigenMeasurement;
    private HistogramPlot? _ethaloneHistogramRef;
    private HistogramPlot? _antigenHistogramRef;
    private GmmOptionsDto? _gmmEthaloneOptions;
    private GmmOptionsDto? _gmmAntigenOptions;
    private async Task LoadMeasurements()
    {
        if (_selectedSurvey is null)
            return;

        var measurementsResult = await cytometerMeasurementsApiService.GetCytometerMeasurements(new()
        {
            SurveyId = _selectedSurvey.Id
        });
        _cytometerMeasurements = measurementsResult.CytometerMeasurements.ToArray();
    }
    private async Task OnEthaloneMeasurementChanged(CytometerMeasurementDto measurement)
    {
        _selectedEthaloneMeasurement = measurement;
        var fetchedMeasurement = await cytometerMeasurementsApiService.GetCytometerMeasurement(measurement.Id);
        _selectedEthaloneMeasurement.RbcDistribution = fetchedMeasurement.RbcDistribution;
        _selectedEthaloneMeasurement.WbcDistribution = fetchedMeasurement.WbcDistribution;
        _selectedEthaloneMeasurement.PltDistribution = fetchedMeasurement.PltDistribution;
        _selectedEthaloneMeasurement.Parameters = fetchedMeasurement.Parameters;
        _gmmEthaloneOptions = null;
        StateHasChanged();
        if (_ethaloneHistogramRef is not null)
            await _ethaloneHistogramRef.Reload();
    }
    private async Task OnAntigenMeasurementChanged(CytometerMeasurementDto measurement)
    {
        _selectedAntigenMeasurement = measurement;
        var fetchedMeasurement = await cytometerMeasurementsApiService.GetCytometerMeasurement(measurement.Id);
        _selectedAntigenMeasurement.RbcDistribution = fetchedMeasurement.RbcDistribution;
        _selectedAntigenMeasurement.WbcDistribution = fetchedMeasurement.WbcDistribution;
        _selectedAntigenMeasurement.PltDistribution = fetchedMeasurement.PltDistribution;
        _selectedAntigenMeasurement.Parameters = fetchedMeasurement.Parameters;
        _gmmAntigenOptions = null;
        StateHasChanged();
        if (_antigenHistogramRef is not null)
            await _antigenHistogramRef.Reload();
    }
    private static IEnumerable<CytometerMeasurementDto> ExlcudeSelectedMeasurement(
        IEnumerable<CytometerMeasurementDto> measurements,
        CytometerMeasurementDto? selectedMeasurement)
    {
        return selectedMeasurement == null 
            ? measurements 
            : measurements.Where(m => m.Id != selectedMeasurement.Id);
    }

    private HashSet<GmmPointDto> _selectedEthalonePoints = [];
    private HashSet<GmmPointDto> _selectedAntigenPoints = [];

    private async Task RunGmmAnalysis()
    {
        Task<PerformGmmAnalysisResponse>? ethaloneTask = null;
        Task<PerformGmmAnalysisResponse>? antigenTask = null;

        if (_selectedEthaloneMeasurement is not null)
        {
            ethaloneTask = cytometerMeasurementsApiService.PerformGmmAnalysis(new()
            {
                CytometerMeasurementId = _selectedEthaloneMeasurement.Id,
                Criterion = GmmCriterion.AIC,
                InitialsMeans = _selectedEthalonePoints.Select(p => p.X).ToHashSet(),
                MinComponents = _selectedAntigenPoints.Count > 0
                    ? _selectedAntigenPoints.Count 
                    : 4,
                MaxComponents = _selectedAntigenPoints.Count > 0
                    ? _selectedAntigenPoints.Count 
                    : 20
            });
        }

        if (_selectedAntigenMeasurement is not null)
        {
            antigenTask = cytometerMeasurementsApiService.PerformGmmAnalysis(new()
            {
                CytometerMeasurementId = _selectedAntigenMeasurement.Id,
                Criterion = GmmCriterion.AIC,
                InitialsMeans = _selectedAntigenPoints.Select(p => p.X).ToHashSet(),
                MinComponents = _selectedEthalonePoints.Count > 0
                    ? _selectedEthalonePoints.Count 
                    : 4,
                MaxComponents = _selectedEthalonePoints.Count > 0
                    ? _selectedEthalonePoints.Count 
                    : 20
            });
        }

        var activeTasks = new[] { ethaloneTask, antigenTask }.Where(t => t != null).Cast<Task>();

        if (activeTasks.Any())
        {
            await Task.WhenAll(activeTasks);
            var ethaloneResult = ethaloneTask != null 
                ? await ethaloneTask 
                : null;
            var antigenResult = antigenTask != null 
                ? await antigenTask 
                : null;
            _gmmEthaloneOptions = ethaloneResult != null 
                ? MapToOptions(ethaloneResult) 
                : null;
            _gmmAntigenOptions = antigenResult != null 
                ? MapToOptions(antigenResult) 
                : null;

            if (ethaloneResult != null && antigenResult != null)
            {
                var ethaloneParameters = MapToParameters(ethaloneResult);
                var antigenParameters = MapToParameters(antigenResult);

                GmmComparisonModel = GmmTableService.PrepareTableData(
                    ethalone: ethaloneParameters,
                    antigen: antigenParameters);
            }

            StateHasChanged();
            
            if (_ethaloneHistogramRef is not null)
                await _ethaloneHistogramRef.Reload();

            if (_antigenHistogramRef is not null)
                await _antigenHistogramRef.Reload();
        }
    }

    public static GmmOptionsDto MapToOptions(PerformGmmAnalysisResponse serverResult, List<string>? customClusterNames = null)
    {
        var options = new GmmOptionsDto
        {
            TotalCurve = new GmmLineDataDto
            {
                X = serverResult.TotalCurvePoints.Select(p => p.X).ToList(),
                Y = serverResult.TotalCurvePoints.Select(p => p.Y).ToList()
            },
            ClusterCurves = []
        };

        foreach (var clusterPointsList in serverResult.ClusterPoints)
        {
            var lineData = new GmmLineDataDto
            {
                X = clusterPointsList.Select(p => p.X).ToList(),
                Y = clusterPointsList.Select(p => p.Y).ToList()
            };
            
            options.ClusterCurves.Add(lineData);
        }

        if (customClusterNames != null && customClusterNames.Count >= serverResult.OptimalComponents)
        {
            options.ClusterNames = customClusterNames;
        }
        else if (serverResult.Means != null)
        {
            // Автоматически сортируем имена по размеру клеток (mu), чтобы в легенде они шли по порядку
            options.ClusterNames = serverResult.Means
                .Select((mu, index) => new { Mean = mu, OriginalIndex = index })
                .OrderBy(item => item.Mean)
                .Select((item, sortedIndex) => $"Популяция {sortedIndex + 1} (d ≈ {item.Mean:F1})")
                .ToList();
                
            // Важно: если мы отсортировали имена для легенды, 
            // кривые в ClusterCurves тоже должны соответствовать этому порядку.
            // Поэтому сделаем сортировку и самих кривых по значению Means:
            var sortedCurves = serverResult.Means
                .Select((mu, index) => new { Mean = mu, Index = index })
                .OrderBy(item => item.Mean)
                .Select(item => options.ClusterCurves[item.Index])
                .ToList();

            options.ClusterCurves = sortedCurves;
        }

        return options;
    }

    private GmmParameters MapToParameters(PerformGmmAnalysisResponse serverResult)
    {
        return new GmmParameters
        {
            Covariances = serverResult.Covariances,
            Means = serverResult.Means,
            OptimalComponents = serverResult.OptimalComponents,
            Weights = serverResult.Weights
        };
    }

    private async Task ResetEthalone()
    {
        _gmmEthaloneOptions = null;
        _selectedEthalonePoints.Clear();
        if (_ethaloneHistogramRef is not null)
            await _ethaloneHistogramRef.Reload();
        if (GmmComparisonModel != null)
        {
            GmmComparisonModel = null;
            StateHasChanged();
        }
    }

    private async void ResetAntigen()
    {
        _gmmAntigenOptions = null;
        _selectedAntigenPoints.Clear();
        if (_antigenHistogramRef is not null)
            await _antigenHistogramRef.Reload();
        if (GmmComparisonModel != null)
        {
            GmmComparisonModel = null;
            StateHasChanged();
        }
    }

    public GmmComparisonResult? GmmComparisonModel { get; set; }
}
