using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Surveys;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Surveys.Queries.GetSurveys;

public class GetSurveysQueryHandler(
    IRepository<Survey> surveysRepository
) : IGetSurveysQueryHandler
{
    public async Task<GetSurveysQueryResult> Handle(GetSurveysQuery query, CancellationToken cancellationToken = default)
    {
        var surveysQuery = surveysRepository
            .Where(s => s.PatientId == query.PatientId);

        var surveys = await surveysQuery
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SurveyDto
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CytometerMeasurements = s.CytometerMeasurements
                    .Select(m => new CytometerMeasurementDto
                    {
                        Id = m.Id,
                        SampleBarcode = m.Sample.Barcode,
                        SampleId = m.SampleId,
                        CytometerName = m.Survey.Cytometer.Name,
                        SampleCollectedAt = m.Sample.CollectedAt,
                        ProccessedAt = m.ProccessedAt,
                        AntigenName = m.Antigen == null
                            ? null
                            : m.Antigen.Name
                    }).ToArray()
            })
            .ToArrayAsync(cancellationToken);

        var surveysCount = await surveysQuery.CountAsync(cancellationToken);

        return new GetSurveysQueryResult
        {
            Surveys = surveys,
            Total = surveysCount
        };
    }
}
