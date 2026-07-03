using Immunitas.Application.Services.Hashing;
using Immunitas.Domain.Entities.Antigens;
using Immunitas.Domain.Entities.Cytometers;
using Immunitas.Domain.Entities.Geography;
using Immunitas.Domain.Entities.Laboratories;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Entities.Samples;
using Immunitas.Domain.Entities.Surveys;
using Immunitas.Domain.Entities.Users;
using Immunitas.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Immunitas.Application.Services.DataSeeding;

public class TestSeedDataGenerator(IHashingService hashingService) : ISeedDataGenerator
{
    public void GenerateSeedData(DbContext context, bool _)
    {
        SeedData(context);        
        context.SaveChanges();
    }

    public async Task GenerateSeedDataAsync(DbContext context, bool _, CancellationToken cancellationToken)
    {
        SeedData(context);
        await context.SaveChangesAsync(cancellationToken);
    }

    private void SeedData(DbContext context)
    {
        // Базовые сущности
        var country = new Country { Id = -1, Name = "Россия" };
        var region = new Region { Id = -1, Name = "", CountryId = country.Id, Country = country };
        var city = new City { Id = -1, Name = "Москва", RegionId = region.Id, Region = region };
        var street = new Street { Id = -1, Name = "Ленина", CityId = city.Id, City = city };
        var address = new Address { Id = -1, StreetId = street.Id, Street = street, House = "1", Apartment = "1", PostalCode = "101000" };
        var address2 = new Address { Id = -2, StreetId = street.Id, Street = street, House = "2", Apartment = "2", PostalCode = "101000" };
        var laboratory = new Laboratory { Id = -1, Name = "Лаборатория 1", AddressId = address.Id, Address = address, PhoneNumber = "+7 495 000-00-00", IsDeleted = false };
        
        var patient = new Patient
        {
            Id = -1,
            FirstName = "Иван",
            LastName = "Иванов",
            MiddleName = "Иванович",
            Gender = Gender.Male,
            BirthDate = new DateOnly(1991, 1, 1),
            PhoneNumber = "+79000000000",
            AddressId = address2.Id,
            CreatedAt = DateTime.UtcNow,
        };
        var user = new User
        {
            Id = -1,
            Email = "admin@test.com",
            PasswordHash = hashingService.GenerateHash("Password123"),
            FullName = "Админ",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            LaboratoryId = laboratory.Id,
        };
        var cytometer = new Cytometer { Id = -1, Name = "Цитометр 1", SerialNumber = "SN-001", Model = "Model X", };

        var immuneSystem = new ImmuneSystem { Id = -1, Name = "Врожденный иммунитет", CreatedAt = DateTime.UtcNow };
        var organ = new Organ { Id = -1, Name = "Селезенка", CreatedAt = DateTime.UtcNow, ImmuneSystemId = immuneSystem.Id };
        var antigen = new Antigen { Id = -1, Name = "Антиген 1", OrganId = organ.Id};

        var samples = new List<Sample>();
        var measurements = new List<CytometerMeasurement>();
        var surveys = new List<Survey>();
        string filePath = SeedDataConfig.SeedDataFilePath;
        if (File.Exists(filePath))
        {
            string[] allLines = File.ReadAllLines(filePath);
            var measurementId = -1;
            var sampleId = -1;
            var surveyId = -1;

            for (int i = 0; i < allLines.Length;)
            {
                // Пропускаем пустые строки
                if (string.IsNullOrWhiteSpace(allLines[i]))
                {
                    i++;
                    continue;
                }
                // Считываем 4 строки блока
                if (i + 3 >= allLines.Length)
                    break;
                var wbcELine = allLines[i++].Trim();
                var wbcALine = allLines[i++].Trim();
                var rbcELine = allLines[i++].Trim();
                var rbcALine = allLines[i++].Trim();

                var wbcEData = ParseDistributionData(wbcELine);
                var wbcAData = ParseDistributionData(wbcALine);
                var rbcEData = ParseDistributionData(rbcELine);
                var rbcAData = ParseDistributionData(rbcALine);

                // Создаем Sample
                var sample = new Sample
                {
                    Id = sampleId--,
                    PatientId = patient.Id,
                    Patient = patient,
                    Barcode = Math.Abs(sampleId+1).ToString("D6"),
                    CollectedAt = DateTime.UtcNow.AddMinutes(-measurementId * 30 - 10)
                };
                samples.Add(sample);

                if (measurements.Count % 5 == 0)
                {
                    var survey = new Survey
                    {
                        Id = surveyId--,
                        PatientId = patient.Id,
                        Patient = patient,
                        LaboratoryId = laboratory.Id,
                        Laboratory = laboratory,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-measurementId * 30 - 15),
                        CytometerId = cytometer.Id,
                        Cytometer = cytometer,
                    };
                    surveys.Add(survey);
                }

                var cleanMeasurement = new CytometerMeasurement
                {
                    Id = measurementId--,
                    SurveyId = surveys[^1].Id,
                    SampleId = sample.Id,
                    Sample = sample,
                    ProccessedAt = DateTime.UtcNow.AddMinutes(-measurementId * 30 - 5),
                    RbcDistribution = rbcEData.Select((y, idx) => new Point(idx, y)).ToArray(),
                    WbcDistribution = wbcEData.Select((y, idx) => new Point(idx, y)).ToArray(),
                };

                var antigenMeasurement = new CytometerMeasurement
                {
                    Id = measurementId--,
                    SurveyId = surveys[^1].Id,
                    SampleId = sample.Id,
                    Sample = sample,
                    AntigenId = antigen.Id,
                    Antigen = antigen,
                    ProccessedAt = DateTime.UtcNow.AddMinutes(-measurementId * 30 - 3),
                    RbcDistribution = rbcAData.Select((y, idx) => new Point(idx, y)).ToArray(),
                    WbcDistribution = wbcAData.Select((y, idx) => new Point(idx, y)).ToArray(),
                };

                measurements.Add(cleanMeasurement);
                measurements.Add(antigenMeasurement);
            }
        }

        context.AddRange(country);
        context.AddRange(city);
        context.AddRange(street);
        context.AddRange([address, address2]);
        context.AddRange(laboratory);
        context.AddRange(patient);
        context.AddRange(user);
        context.AddRange(cytometer);
        context.AddRange(antigen);
        context.AddRange(samples);
        context.AddRange(measurements);
        context.AddRange(organ);
        context.AddRange(immuneSystem);
        context.AddRange(surveys);
    }

    private static List<double> ParseDistributionData(string line)
    {
        var parts = line.Split(',')
            .Skip(1)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => double.Parse(x.Trim(), CultureInfo.InvariantCulture))
            .ToList();
        return parts;
    }
}
