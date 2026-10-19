using Web.Application.Contracts.Orders;

namespace Web.Application.Interfaces.Repository;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderResponse>> GetByUserIdAsync(Guid userId);
    Task<OrderResponse?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid userId, Guid addressId, decimal totalPrice, List<OrderItemResponse> items);

    Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<Guid?> GetUserIdByOrderIdAsync(Guid orderId);
}
