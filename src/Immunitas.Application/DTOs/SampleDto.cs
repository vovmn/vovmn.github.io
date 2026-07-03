using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class SampleDto
{
    /// <summary>
    /// Id образца крови пациента
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Код пробирки с образцом крови пациента
    /// </summary>
    public required string Barcode { get; set; }

    /// <summary>
    /// Id пациента, которому принадлежит образец крови
    /// </summary>
    public required int PatientId { get; set; }

    /// <summary>
    /// Дата и время сбора образца крови у пациента
    /// </summary>
    public required DateTime CollectedAt { get; set; }
}
