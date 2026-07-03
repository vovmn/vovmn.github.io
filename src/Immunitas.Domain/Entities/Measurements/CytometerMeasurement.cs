using Immunitas.Domain.Entities.Antigens;
using Immunitas.Domain.Entities.Samples;
using Immunitas.Domain.Entities.Surveys;
namespace Immunitas.Domain.Entities.Measurements;

/// <summary>
/// Измерение, полученное в результате анализа образца крови пациента на проточном цитометре
/// </summary>
public class CytometerMeasurement : EntityBase
{
    /// <summary>
    /// Id образца крови пациента, который был проанализирован на проточном цитометре
    /// </summary>
    public required int SampleId { get; set; }

    /// <summary>
    /// Образец крови пациента, который был проанализирован на проточном цитометре
    /// </summary>
    public Sample Sample { get; set; } = null!;

    /// <summary>
    /// Дата и время обработки результата анализа образца крови пациента на проточном цитометре
    /// </summary>
    public DateTime ProccessedAt { get; init; }

    /// <summary>
    /// Id антигена, который был использован для анализа образца крови пациента на проточном цитометре
    /// </summary>
    /// <remarks>null - чистый образец</remarks>
    public int? AntigenId { get; set; }

    /// <summary>
    /// Антиген, который был использован для анализа образца крови пациента на проточном цитометре
    /// </summary>
    /// <remarks>null - чистый образец</remarks>
    public Antigen? Antigen { get; set; }

    /// <summary>
    /// Распределение лейкоцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public Point[] WbcDistribution { get; set; } = [];

    /// <summary>
    /// Распределение эритроцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public Point[] RbcDistribution { get; set; } = [];

    /// <summary>
    /// Распределение тромбоцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public Point[] PltDistribution { get; set; } = [];

    /// <summary>
    /// Набор параметров, полученных в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public ICollection<BloodParameter> Parameters { get; set; } = [];

    /// <summary>
    /// Id обследования пациента, в рамках которого было проведено измерение на проточном цитометре
    /// </summary>
    public required int SurveyId { get; set; }

    /// <summary>
    /// Обследование пациента, в рамках которого было проведено измерение на проточном цитометре
    /// </summary>
    public Survey Survey { get; set; } = null!;
}
