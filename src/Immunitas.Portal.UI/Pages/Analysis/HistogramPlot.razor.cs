using Immunitas.Portal.UI.ApiProxies.Contracts;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.LayoutLib.XAxisLib;
using Plotly.Blazor.Interop;

namespace Immunitas.Portal.UI.Pages.Analysis;

public partial class HistogramPlot : ComponentBase
{
[Parameter]
    public IEnumerable<PointDto> Values { get; set; } = [];

    [Parameter]
    public string Color { get; set; } = "blue";

    /// <summary>
    /// Данные для графиков Гауссовых смесей
    /// </summary>
    [Parameter]
    public GmmOptionsDto? GmmOptions { get; set; }

    [Parameter]
    public HashSet<GmmPointDto> SelectedPoints { get; set; } = [];
    
    [Parameter]
    public EventCallback<HashSet<GmmPointDto>> SelectedPointsChanged { get; set; }

    // Цветовая палитра для кластеров, чтобы они автоматически различались
    private readonly List<string> _clusterPalette = ["#FF6B6B", "#f74de9", "#51CF66", "#FCC419", "#A61E4D", "#94D82D"];

    private PlotlyChart _chart = new();
    private readonly Config _chartConfig = new();
    private readonly Plotly.Blazor.Layout _chartLayout;
    private readonly List<ITrace> _chartData = new();

    private Scatter _selectedPoints = new()
    {
        Mode = ModeFlag.Markers,
        Marker = new()
        {
            Color = "red"
        },
        X = new List<object>(),
        Y = new List<object>(),
        HoverInfo = HoverInfoFlag.None,
        HoverOn = HoverOnFlag.Fills,
        Name = "Начальные приближения"
    };

    public HistogramPlot()
    {
        _chartLayout = new()
        {
            XAxis = new List<XAxis>
            {
                new()
                {
                    Range = new object[] { -1, 256 },
                    TickMode = TickModeEnum.Linear,
                    Tick0 = 0,
                    DTick = 32
                }
            },
            YAxis = new List<YAxis>
            {
                new()
                {
                    ZeroLine = true,
                    ZeroLineColor = "black",
                    ZeroLineWidth = 1
                }
            },       
            HoverMode = HoverModeEnum.X,
            AutoSize = true,
            Height = 400,
        };
    }

    /// <summary>
    /// Обновляет данные графика
    /// </summary>
    public async Task Reload()
    {
        StateHasChanged();
        await Task.Yield();

        _chartData.Clear();
        _selectedPoints.X = SelectedPoints.Select(p => p.X).Cast<object>().ToList();
        _selectedPoints.Y = SelectedPoints.Select(p => p.Y).Cast<object>().ToList();

        var points = Values.ToList();
        if (points.Count == 0)
            return;

        // 1. Базовый график: Исходная гистограмма распределения клеток
        var xVals = points.Select(p => (object)p.X).ToArray();
        var yVals = points.Select(p => (object)p.Y).ToArray();

        var baseHistogramTrace = new Bar
        {
            Name = "Измерение",
            X = xVals,
            Y = yVals,
            Marker = new Plotly.Blazor.Traces.BarLib.Marker
            {
                Color = Color
            },
            HoverTemplate = "Объем: %{x}<br>Количество: %{y:.4f}<br><extra></extra>",
            ShowLegend = true,
            Opacity = (decimal?)0.4
        };
        _chartData.Add(baseHistogramTrace);

        // 2. Добавление GMM кривых, если опции переданы
        if (GmmOptions is not null)
        {
            if (GmmOptions.ClusterCurves is not null)
            {
                for (int i = 0; i < GmmOptions.ClusterCurves.Count; i++)
                {
                    var cluster = GmmOptions.ClusterCurves[i];
                    if (cluster.X.Count == 0) continue;

                    string clusterName = GmmOptions.ClusterNames is not null && GmmOptions.ClusterNames.Count > i
                        ? GmmOptions.ClusterNames[i]
                        : $"Кластер {i + 1}";

                    var clusterTrace = new Scatter
                    {
                        Name = clusterName,
                        Mode = ModeFlag.Lines,
                        X = cluster.X.Select(x => (object)x).ToList(),
                        Y = cluster.Y.Select(y => (object)y).ToList(),
                        Line = new Line 
                        { 
                            Color = _clusterPalette[i % _clusterPalette.Count],
                            Width = 2,
                            Dash = "dash"
                        },
                        HoverInfo = HoverInfoFlag.None
                    };
                    _chartData.Add(clusterTrace);
                }
            }

            // Отрисовка главной суммирующей кривой (GMM сумма)
            if (GmmOptions.TotalCurve is not null && GmmOptions.TotalCurve.X.Count > 0)
            {
                var totalTrace = new Scatter
                {
                    Name = "GMM Сумма",
                    Mode = ModeFlag.Lines,
                    X = GmmOptions.TotalCurve.X.Select(x => (object)x).ToList(),
                    Y = GmmOptions.TotalCurve.Y.Select(y => (object)y).ToList(),
                    Line = new Line 
                    { 
                        Color = Color,
                        Width = 3 
                    },
                    HoverTemplate = "Общая плотность: %{y:.4f}<extra></extra>",
                };
                _chartData.Add(totalTrace);
            }

        }

        _chartData.Add(_selectedPoints);
        await _chart.React();
    }

    public async void ClickAction(IEnumerable<EventDataPoint> eventData)
    {
        try
        {
            var xString = eventData.FirstOrDefault()?.X.ToString();
            var yString = eventData.FirstOrDefault()?.Y.ToString();
            
            if (double.TryParse(xString, out var x) && double.TryParse(yString, out var y))
            {
                if (SelectedPoints.Add(new GmmPointDto(x, y)))
                {
                    _selectedPoints.X ??= [];
                    _selectedPoints.Y ??= [];
                    _selectedPoints.X.Add(x);
                    _selectedPoints.Y.Add(y);
                    await _chart.React();
                    await SelectedPointsChanged.InvokeAsync(SelectedPoints);
                    StateHasChanged();
                }
            }    
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    public async void SubscribeEvents()
    {
        await _chart.SubscribeClickEvent();
    }
}

public class GmmLineDataDto
{
    public List<double> X { get; set; } = [];
    public List<double> Y { get; set; } = [];
}

public record GmmPointDto(double X, double Y);

public class GmmOptionsDto
{
    /// <summary>
    /// Главная суммирующая аппроксимационная кривая
    /// </summary>
    public required GmmLineDataDto TotalCurve { get; set; }

    /// <summary>
    /// Кривые для каждого отдельного кластера (популяции клеток)
    /// </summary>
    public List<GmmLineDataDto> ClusterCurves { get; set; } = [];

    /// <summary>
    /// Названия для кластеров
    /// </summary>
    public List<string>? ClusterNames { get; set; }
}