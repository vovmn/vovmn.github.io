using System;
using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Measurements;

namespace Immunitas.Application.Services.GaussianMixture;

public class GmmResult
{
    public int OptimalComponents { get; set; }
    public double[] Weights { get; set; } = [];
    public double[] Means { get; set; } = [];
    public double[] Covariances { get; set; } = [];

    public List<PointDto> TotalCurvePointDtos { get; set; } = [];
    public List<List<PointDto>> ClusterPointDtos { get; set; } = [];
}