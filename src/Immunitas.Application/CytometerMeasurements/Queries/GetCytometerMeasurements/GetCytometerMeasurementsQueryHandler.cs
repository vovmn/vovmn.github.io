using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurements
{
    public class GetCytometerMeasurementsQueryHandler(
        IRepository<CytometerMeasurement> measurementsRepository) : IGetCytometerMeasurementsQueryHandler
    {
        public async Task<GetCytometerMeasurementsQueryResult> Handle(GetCytometerMeasurementsQuery query, CancellationToken cancellationToken)
        {
            var cytometerMeasurements = await measurementsRepository
                .Where(m => m.SurveyId == query.SurveyId)
                .OrderByDescending(m => m.ProccessedAt)
                .Select(m => new CytometerMeasurementDto
                {
                    Id = m.Id,
                    SampleBarcode = m.Sample.Barcode,
                    SampleId = m.SampleId,
                    CytometerName = m.Survey.Cytometer.Name,
                    ProccessedAt = m.ProccessedAt,
                    AntigenName = m.Antigen != null ? m.Antigen.Name : null,
                    SampleCollectedAt = m.Sample.CollectedAt
                })
                .ToListAsync(cancellationToken);

            return new GetCytometerMeasurementsQueryResult
            {
                CytometerMeasurements = cytometerMeasurements
            };
        }
    }
}
