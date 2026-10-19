using Web.Application.Interfaces.Service;

namespace Web.Infrastructure.Notifications;

public class PushNotificationChannel : INotificationChannel
{
    public string Name => "push";

    public Task SendAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        // TODO: реальный push-провайдер пока не подключён
        return Task.CompletedTask;
    }
}
