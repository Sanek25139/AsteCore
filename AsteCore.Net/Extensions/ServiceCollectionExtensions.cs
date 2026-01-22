using Microsoft.Extensions.DependencyInjection;

namespace AsteCore.MVVM.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientServicesthis(this IServiceCollection services) => services.AddSingleton(services);
    }
}
