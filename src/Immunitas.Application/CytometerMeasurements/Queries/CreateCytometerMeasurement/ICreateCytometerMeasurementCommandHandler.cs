namespace Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;

public interface ICreateCytometerMeasurementCommandHandler : IHandler
{
    Task Handle(CreateCytometerMeasurementCommand command, CancellationToken cancellationToken);
}
