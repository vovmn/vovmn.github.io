namespace Immunitas.Application.Samples.Queries.GetPatientSamples;

public interface IGetPatientSamplesQueryHandler : IHandler
{
    Task<GetPatientSamplesQueryResult> Handle(GetPatientSamplesQuery query, CancellationToken cancellationToken);
}
