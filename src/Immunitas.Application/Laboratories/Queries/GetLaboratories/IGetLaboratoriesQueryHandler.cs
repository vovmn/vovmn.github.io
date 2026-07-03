namespace Immunitas.Application.Laboratories.Queries.GetLaboratories;

public interface IGetLaboratoriesQueryHandler : IHandler
{
    Task<GetLaboratoriesResult> Handle(GetLaboratoriesQuery query, CancellationToken cancellationToken);
}
