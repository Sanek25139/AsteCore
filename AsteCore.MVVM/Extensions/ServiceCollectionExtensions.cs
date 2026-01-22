using AsteCore.MVVM.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AsteCore.MVVM.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVVMTransient<TView, TViewModel>(this IServiceCollection services) where TView : class where TViewModel : class, INotifyPropertyChanged
        {
            services.AddTransient<TView>();
            services.AddTransient<TViewModel>();
            return services;
        }
        public static IServiceCollection AddVVMSingleton<TView, TViewModel>(this IServiceCollection services) where TView : class where TViewModel : class, INotifyPropertyChanged
        {
            services.AddSingleton<TView>();
            services.AddSingleton<TViewModel>();
            return services;
        }
    }
}
