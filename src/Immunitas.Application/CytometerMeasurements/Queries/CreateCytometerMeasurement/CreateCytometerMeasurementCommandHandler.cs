using Immunitas.Domain.Entities.Antigens;
using Immunitas.Domain.Entities.Cytometers;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Entities.Samples;
using Immunitas.Domain.Entities.Surveys;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;

public class CreateCytometerMeasurementCommandHandler(
    IRepository<CytometerMeasurement> cytometerMeasurementsRepository,
    IRepository<Patient> patientsRepository,
    IRepository<Sample> samplesRepository,
    IRepository<Antigen> antigensRepository,
    IRepository<Survey> surveysRepository,
    IChangesSaver changesSaver,
    ILogger<CreateCytometerMeasurementCommandHandler> logger) : ICreateCytometerMeasurementCommandHandler
{
    public async Task Handle(CreateCytometerMeasurementCommand command, CancellationToken cancellationToken)
    {
        var patientExists = await patientsRepository
            .GetById(command.PatientId)
            .AnyAsync(cancellationToken);

        if (!patientExists)
            throw new EntityNotFoundException(nameof(Patient), command.PatientId);

        var sampleExists = await samplesRepository
            .GetById(command.SampleId)
            .AnyAsync(cancellationToken);

        if (!sampleExists)
            throw new EntityNotFoundException(nameof(Sample), command.SampleId);

        var surveyExists = await surveysRepository
            .GetById(command.SurveyId)
            .AnyAsync(cancellationToken);

        if (!surveyExists)
            throw new EntityNotFoundException(nameof(Cytometer), command.SurveyId);

        if (command.AntigenId.HasValue)
        {
            var antigenExists = await antigensRepository
                .GetById(command.AntigenId.Value)
                .AnyAsync(cancellationToken);

            if (!antigenExists)
                throw new EntityNotFoundException(nameof(Antigen), command.AntigenId.Value);
        }

        var wbcDistribution = ConvertToPoints(command.Histograms.WBC);
        var rbcDistribution = ConvertToPoints(command.Histograms.RBC);
        var pltDistribution = ConvertToPoints(command.Histograms.PLT);

        var measurement = new CytometerMeasurement
        {
            AntigenId = command.AntigenId,
            SampleId = command.SampleId,
            SurveyId = command.SurveyId,
            ProccessedAt = command.ReceivedAt ?? DateTime.UtcNow,
            Parameters = command.Parameters
                .Select(p => new BloodParameter
                {
                    Name = p.Name,
                    Value = p.Value,
                })
                .ToArray()
        };

        cytometerMeasurementsRepository.Add(measurement);
        await changesSaver.CommitAsync(cancellationToken);

        logger.LogInformation("Добавлено измерение цитометра с id {MeasurementId} для образца с id {SampleId} пациента с id {PatientId}", measurement.Id, command.SampleId, command.PatientId);
    }

    private static Point[] ConvertToPoints(IEnumerable<int> distribution)
    {
        return distribution
            .Select((value, index) => new Point(index + 9, value))
            .ToArray();
    }
}
