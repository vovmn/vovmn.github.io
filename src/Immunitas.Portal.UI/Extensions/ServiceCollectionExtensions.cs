using Immunitas.Portal.UI.ApiProxies.ApiServices;
using Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;
using System.Reflection;

namespace Immunitas.Portal.UI.Extensions
{
    /// <summary>
    /// Расширения для коллекции сервисов приложения
    /// </summary>
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует API-сервисы, которые используются для обращения к серверу
        /// </summary>
        /// <param name="services">Коллекция сервисов приложения</param>
        /// <returns>Обновленная коллекция сервисов приложения</returns>
        /// <exception cref="InvalidOperationException">Не найдена сборка с api-сервисами</exception>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Получаем все реализации маркерного интерфейса
            var apiServicesImplementations = Assembly.GetAssembly(typeof(PatientsApiService))?
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && t.IsAssignableTo(typeof(IApiService)))
                .ToList() ?? throw new InvalidOperationException("Не найдена сборка с api-сервисами");

            foreach (var implementationType in apiServicesImplementations)
            {
                // Получаем конкретный интерфейс сервиса (например, IPatientsApiService)
                var interfaceType = implementationType
                    .GetInterfaces()
                    .FirstOrDefault(i => i != typeof(IApiService)
                                         && i.IsAssignableTo(typeof(IApiService)));

                // Регистрируем реализацию интерфейса (например, PatientsApiService будет
                // реализовывать IPatientsApiService
                if (interfaceType != null)
                    services.AddScoped(interfaceType, implementationType);
            }

            return services;
        }
    }
}
