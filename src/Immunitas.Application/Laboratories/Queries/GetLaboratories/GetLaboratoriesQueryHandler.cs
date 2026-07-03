using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Laboratories;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Laboratories.Queries.GetLaboratories;

public class GetLaboratoriesQueryHandler(
    IRepository<Laboratory> laboratoriesRepository) : IGetLaboratoriesQueryHandler
{
    public async Task<GetLaboratoriesResult> Handle(GetLaboratoriesQuery query, CancellationToken cancellationToken)
    {
        var laboratories = await laboratoriesRepository
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.Count)
            .Take(query.Count)
            .Select(p => new LaboratoryDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync(cancellationToken);

        // Получаем общее количество лабораторий
        var total = await laboratoriesRepository.CountAsync(cancellationToken);

        // Возвращаем результат
        return new GetLaboratoriesResult
        {
            Laboratories = laboratories,
            Total = total
        };
    }
}
