using Microsoft.Extensions.DependencyInjection;
using Web.Application.Interfaces.Service;

namespace Web.Infrastructure.BackgroundTasks;

public static class Extensions
{
    public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        return services;
    }
}