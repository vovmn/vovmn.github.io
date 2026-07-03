using Immunitas.Application.Services.Hashing;
using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.Users.Commands;

public class CreateUserCommandHandler(
    IRepository<User> usersRepository,
    IChangesSaver changesSaver,
    IHashingService hashingService,
    ILogger<CreateUserCommandHandler> logger) : ICreateUserCommandHandler
{
    public async Task Handle(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            CreatedAt = DateTime.UtcNow,
            Email = command.Email,
            FullName = command.FullName,
            LaboratoryId = command.LaboratoryId,
            PasswordHash = hashingService.GenerateHash(command.Password),
            Role = command.Role,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        usersRepository.Add(user);
        await changesSaver.CommitAsync(cancellationToken);

        logger.LogInformation("Добавлен пользователь с Id {UserId}", user.Id);
    }
}
