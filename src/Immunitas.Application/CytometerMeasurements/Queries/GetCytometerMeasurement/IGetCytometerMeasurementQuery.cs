using System;
using Immunitas.Application.DTOs;

namespace Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;

public interface IGetCytometerMeasurementQuery : IHandler
{
    Task<CytometerMeasurementDto> Handle(int id, CancellationToken cancellationToken = default);
}
