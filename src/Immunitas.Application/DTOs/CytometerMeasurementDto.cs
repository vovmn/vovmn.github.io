using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class CytometerMeasurementDto
{
    /// <summary>
    /// Id измерения
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Штрихкод пробирки с образцом крови пациента, который был проанализирован на проточном цитометре
    /// </summary>
    public required string SampleBarcode { get; set; }

    /// <summary>
    /// Id образца крови пациента, который был проанализирован на проточном цитометре
    /// </summary>
    public required int SampleId { get; set; }

    /// <summary>
    /// Название проточного цитометра, на котором был проанализирован образец крови пациента
    /// </summary>
    public required string CytometerName { get; set; }

    /// <summary>
    /// Дата и время получения образца крови пациента, который был проанализирован на проточном цитометре
    /// </summary>
    public required DateTime SampleCollectedAt { get; set; }

    /// <summary>
    /// Дата и время обработки результата анализа образца крови пациента на проточном цитометре
    /// </summary>
    public required DateTime ProccessedAt { get; set; }

    /// <summary>
    /// Название антигена, который был использован при проведении измерения (null - чистый образец)
    /// </summary>
    public string? AntigenName { get; set; }

    /// <summary>
    /// Распределение лейкоцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public PointDto[] WbcDistribution { get; set; } = [];

    /// <summary>
    /// Распределение эритроцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public PointDto[] RbcDistribution { get; set; } = [];

    /// <summary>
    /// Распределение тромбоцитов в виде массива точек, полученное в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public PointDto[] PltDistribution { get; set; } = [];

    /// <summary>
    /// Набор параметров, полученных в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public ICollection<BloodParameterDto> Parameters { get; set; } = [];
}
