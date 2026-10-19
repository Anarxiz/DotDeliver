using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Web.Application.Contracts.Orders;
using Web.Application.Interfaces.Repository;
using Web.Application.Interfaces.Service;
using Web.Domain.Common;

namespace Web.Application.Features.Orders;

public sealed record UpdateOrderStatusCommand(Guid OrderId, UpdateOrderStatusRequest Request) : IRequest<Result<bool>>;

public sealed class UpdateOrderStatusHandler(
    IOrderRepository orderRepository,
    IBackgroundTaskQueue backgroundTaskQueue,
    ILogger<UpdateOrderStatusHandler> logger) 
    : IRequestHandler<UpdateOrderStatusCommand, Result<bool>> {
    public async ValueTask<Result<bool>> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken) {
        bool update = await orderRepository.UpdateStatusAsync(command.OrderId, command.Request);
        if (!update)
        {
            logger.LogWarning("Не удалось обновить статус заказа {OrderId}", command.OrderId);
            return Result.Failure<bool>("Не удалось обновить заказ");
        }

        var userId = await orderRepository.GetUserIdByOrderIdAsync(command.OrderId);

        if (userId is not null)
        {
            string message = $"Статус заказа изменён на {command.Request.Status}";
            backgroundTaskQueue.Enqueue(async (sp, ct) =>
            {
                var dispatcher = sp.GetRequiredService<INotificationDispatcher>();
                await dispatcher.DispatchAsync(userId.Value, message, ct);
            });
        }

        logger.LogInformation("Статус заказа {OrderId} изменён на {Status}", command.OrderId, command.Request.Status);

        return Result.Success(update);
    }
}