using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class SectorDifferenceDto(double startX, double endX, double fractionalDifference)
{
    public double StartX { get; set; } = startX;
    public double EndX { get; set; } = endX;
    public double FractionalDifference { get; set; } = fractionalDifference;
}
