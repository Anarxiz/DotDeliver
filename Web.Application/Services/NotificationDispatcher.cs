using Web.Application.Contracts;
using Web.Application.Interfaces.Service;

namespace Web.Application.Services;

public class NotificationDispatcher(IEnumerable<INotificationChannel> channels) : INotificationDispatcher
{
    public async Task<IReadOnlyList<ChannelResult>> DispatchAsync(Guid userId ,string message, CancellationToken cancellationToken)
    {
        var tasks = channels.Select(c => SendSafelyAsync(c, userId, message, cancellationToken));
        
        return await Task.WhenAll(tasks);
    }

    private static async Task<ChannelResult> SendSafelyAsync(INotificationChannel channel, Guid userId, string message, CancellationToken cancellationToken)
    {
        try
        {
            await channel.SendAsync(userId, message, cancellationToken);
            return new ChannelResult(channel.Name, true);
        }
        catch (Exception ex)
        {
            return new ChannelResult(channel.Name, false, ex.Message);
        }
    }
}
