using Web.Application.Interfaces.Service;

namespace Web.API.Tests.Extensions;

public class FakeNotificationChannel : INotificationChannel
{
    public string Name => "fake";

    public Task SendAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
