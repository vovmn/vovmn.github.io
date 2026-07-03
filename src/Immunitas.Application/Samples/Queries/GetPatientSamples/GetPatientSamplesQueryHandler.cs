using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Samples;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Samples.Queries.GetPatientSamples;

public class GetPatientSamplesQueryHandler(
    IRepository<Sample> samplesRepository) : IGetPatientSamplesQueryHandler
{
    public async Task<GetPatientSamplesQueryResult> Handle(GetPatientSamplesQuery query, CancellationToken cancellationToken)
    {
        var samplesQuery = samplesRepository
            .Where(s => s.PatientId == query.PatientId)
            .Skip((query.Page - 1) * query.Count)
            .Take(query.Count)
            .Select(s => new SampleDto
            {
                Id = s.Id,
                Barcode = s.Barcode,
                PatientId = s.PatientId,
                CollectedAt = s.CollectedAt
            });

        var samples = await samplesQuery.ToListAsync(cancellationToken);
        var totalCount = await samplesQuery.CountAsync(cancellationToken);

        return new GetPatientSamplesQueryResult
        {   
            Samples = samples,
            TotalCount = totalCount
        };
    }
}
