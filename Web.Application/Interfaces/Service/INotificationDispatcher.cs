using Web.Application.Contracts;

namespace Web.Application.Interfaces.Service;

public interface INotificationDispatcher
{
    Task<IReadOnlyList<ChannelResult>> DispatchAsync(Guid userId, string message, CancellationToken cancellationToken);
}
