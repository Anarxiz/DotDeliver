using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Application.Interfaces.Service;

namespace Web.Infrastructure.Notifications;

public static class Extensions
{
    public static IServiceCollection AddNotificationChannels(this IServiceCollection services)
    {
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();
        services.AddScoped<INotificationChannel, PushNotificationChannel>();

        return services;
    }
}
