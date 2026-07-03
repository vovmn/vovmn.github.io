using Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;
using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurement;

public class GetCytometerMeasurementQueryHandler(
    IRepository<CytometerMeasurement> measurementsRepository) : IGetCytometerMeasurementQuery
{
    public async Task<CytometerMeasurementDto> Handle(int id, CancellationToken cancellationToken = default)
    {
        var cytometerMeasurement = await measurementsRepository
            .GetById(id)
            .Select(m => new
            {
                Id = m.Id,
                SampleBarcode = m.Sample.Barcode,
                SampleId = m.SampleId,
                CytometerName = m.Survey.Cytometer.Name,
                ProccessedAt = m.ProccessedAt,
                AntigenName = m.Antigen != null ? m.Antigen.Name : null,
                SampleCollectedAt = m.Sample.CollectedAt,
                WbcDistribution = m.WbcDistribution,
                RbcDistribution = m.RbcDistribution,
                PltDistribution = m.PltDistribution,
                Parameters = m.Parameters
                    .Select(p => new BloodParameterDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Value = p.Value
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken) ?? throw new EntityNotFoundException(nameof(CytometerMeasurement), id);

        var cytometerMeasurementDto = new CytometerMeasurementDto
        {
            Id = cytometerMeasurement.Id,
            SampleBarcode = cytometerMeasurement.SampleBarcode,
            SampleId = cytometerMeasurement.SampleId,
            CytometerName = cytometerMeasurement.CytometerName,
            ProccessedAt = cytometerMeasurement.ProccessedAt,
            AntigenName = cytometerMeasurement.AntigenName,
            SampleCollectedAt = cytometerMeasurement.SampleCollectedAt,
            WbcDistribution = cytometerMeasurement.WbcDistribution.Select(PointDto.FromDomain).ToArray(),
            RbcDistribution = cytometerMeasurement.RbcDistribution.Select(PointDto.FromDomain).ToArray(),
            PltDistribution = cytometerMeasurement.PltDistribution.Select(PointDto.FromDomain).ToArray(),
            Parameters = cytometerMeasurement.Parameters
        };

        return cytometerMeasurementDto;
    }
}
