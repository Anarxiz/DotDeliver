namespace Web.Application.Interfaces.Service;

public interface INotificationChannel
{
    string Name { get; }
    Task SendAsync(Guid userId, string message, CancellationToken cancellationToken);
}
