using Immunitas.Application;
using Immunitas.Application.Patients.Queries.GetPatients;

namespace Immunitas.Portal.Bff.Extensions
{
    /// <summary>
    /// Методы расширения коллекции сервисов приложения
    /// </summary>
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует все обработчики комманд и запросов
        /// </summary>
        /// <param name="services">Коллекция сервисов приложения</param>
        /// <returns>Обновленная коллекция сервисов приложения</returns>
        public static IServiceCollection AddCommandAndQueryHandlers(this IServiceCollection services)
        {
            // Получаем все реализации маркерного интерфейса
            var handlersImplementations = typeof(GetPatientsQueryHandler).Assembly
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && t.IsAssignableTo(typeof(IHandler)))
                .ToList();

            foreach (var implementationType in handlersImplementations)
            {
                // Получаем конкретный интерфейс обработчика (например, IGetPatientsQueryHandler)
                var interfaceType = implementationType
                    .GetInterfaces()
                    .FirstOrDefault(i => i != typeof(IHandler) && i.IsAssignableTo(typeof(IHandler)));

                // Регистрируем реализацию конкретного интерфейса (например, GetPatientsQueryHandler будет
                // реализовывать IGetPatientsQueryHandler
                if (interfaceType != null)
                    services.AddScoped(interfaceType, implementationType);
            }

            return services;
        }
    }
}
