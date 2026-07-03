using System;
using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.CytometerMeasurements.Queries.PerformGmmAnalysis;

[SharedContract]
public class PerformGmmAnalysisQueryResult
{
    public int OptimalComponents { get; set; }
    public double[] Weights { get; set; } = [];
    public double[] Means { get; set; } = [];
    public double[] Covariances { get; set; } = [];

    public List<PointDto> TotalCurvePoints { get; set; } = [];
    public List<List<PointDto>> ClusterPoints { get; set; } = [];
}