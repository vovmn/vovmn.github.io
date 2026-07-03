namespace Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurements
{
    public interface IGetCytometerMeasurementsQueryHandler : IHandler
    {
        Task<GetCytometerMeasurementsQueryResult> Handle(GetCytometerMeasurementsQuery query, CancellationToken cancellation);
    }
}
