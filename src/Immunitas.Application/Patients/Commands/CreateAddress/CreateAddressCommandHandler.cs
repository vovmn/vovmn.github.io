using Immunitas.Domain.Entities.Geography;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;


namespace Immunitas.Application.Patients.Commands.CreateAddress;

public class CreateAddressCommandHandler(
    IRepository<Country> countryRepository,
    IRepository<Region> regionRepository,
    IRepository<City> cityRepository,
    IRepository<Street> streetRepository,
    IRepository<Address> addressRepository,
    IChangesSaver changesSaver) : ICreateAddressCommandHandler
{

    public async Task<CreateAddressCommandResult> Handle(
        CreateAddressCommand command,
        CancellationToken cancellationToken)
    {
        var country = await countryRepository
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Name, command.Country), cancellationToken);

        if (country == null)
        {
            // Если страны нет в БД - создаем
            country = new Country
            {
                Name = command.Country,
            };
            countryRepository.Add(country);
            await changesSaver.CommitAsync(cancellationToken);
        }

        var region = await regionRepository
            .FirstOrDefaultAsync(c => c.CountryId == country.Id &&
                EF.Functions.ILike(c.Name, command.Region), cancellationToken);

        if (region == null)
        {
            // Если страны нет в БД - создаем
            region = new Region
            {
                Name = command.Region,
                CountryId = country.Id,
            };
            regionRepository.Add(region);
            await changesSaver.CommitAsync(cancellationToken);
        }


        var city = await cityRepository
            .FirstOrDefaultAsync(c => c.RegionId == region.Id &&
                EF.Functions.ILike(c.Name, command.City), cancellationToken);

        if (city == null)
        {
            // Если города нет в БД - создаем
            city = new City
            {
                Name = command.City,
                RegionId = region.Id
            };

            cityRepository.Add(city);
            await changesSaver.CommitAsync(cancellationToken);
        }

        var street = await streetRepository
            .FirstOrDefaultAsync(s => s.CityId == city.Id &&
                EF.Functions.ILike(s.Name, command.Street), cancellationToken);

        if (street == null)
        {
            // Если улицы нет в БД - создаем
            street = new Street
            {
                Name = command.Street,
                CityId = city.Id
            };
            streetRepository.Add(street);
            await changesSaver.CommitAsync(cancellationToken);
        }

        // 4. Ищем адрес в БД
        var existingAddress = await addressRepository
            .FirstOrDefaultAsync(a =>
                a.StreetId == street.Id &&
                a.House == command.House &&
                a.Apartment == command.Apartment,
                cancellationToken);

        if (existingAddress != null)
        {
            // Если адрес уже есть в БД - возвращаем его

            return new CreateAddressCommandResult
            {
                Id = existingAddress.Id,
                Country = country.Name,
                Region = region.Name,
                City = city.Name,
                Street = street.Name,
                House = existingAddress.House,
                Apartment = existingAddress.Apartment,
            };
        }

        var address = new Address
        {
            StreetId = street.Id,
            House = command.House,
            Apartment = command.Apartment,
            PostalCode = command.PostalCode
        };

        addressRepository.Add(address);
        await changesSaver.CommitAsync(cancellationToken);

        return new CreateAddressCommandResult
        {
            Id = address.Id,
            Country = country.Name,
            Region = region.Name,
            City = city.Name,
            Street = street.Name,
            House = address.House,
            Apartment = address.Apartment,
        };
    }
}