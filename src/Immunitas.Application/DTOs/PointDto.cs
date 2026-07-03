using Immunitas.Domain.Entities.Measurements;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class PointDto(double x, double y)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public static PointDto FromDomain(Point p) => new(p.X, p.Y);
}