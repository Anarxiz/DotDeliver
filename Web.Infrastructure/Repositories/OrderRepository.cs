using Microsoft.EntityFrameworkCore;
using Web.Application.Contracts.Address;
using Web.Application.Contracts.Orders;
using Web.Application.Interfaces.Repository;
using Web.Domain.Entities;

namespace Web.Infrastructure.Repositories;

public class OrderRepository(ShopDbContext db) : IOrderRepository
{
    public async Task<IReadOnlyList<OrderResponse>> GetByUserIdAsync(Guid userId) {
        return await db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Select(o => new OrderResponse(
                o.Id,
                new AddressResponse(
                           o.Address.Id,
                           o.Address.City,
                           o.Address.Street,
                           o.Address.House,
                           o.Address.Apartment,
                           o.Address.Floor
                        ),
                o.CreatedAt,
                o.Status,
                o.Items.Sum(i => i.PriceAtOrder * i.Quantity),
                o.Items.Select(i => new OrderItemResponse(
                    i.Dish.Id,
                    i.Dish.Name,
                    i.Quantity,
                    i.PriceAtOrder
                    ))
                .ToList()
            ))
            .ToListAsync();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id) {
        return await db.Orders
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(o => new OrderResponse(
                o.Id,
                new AddressResponse(
                           o.Address.Id,
                           o.Address.City,
                           o.Address.Street,
                           o.Address.House,
                           o.Address.Apartment,
                           o.Address.Floor
                        ),
                o.CreatedAt,
                o.Status,
                o.Items.Sum(i => i.PriceAtOrder * i.Quantity),
                o.Items.Select(i => new OrderItemResponse(
                    i.Dish.Id,
                    i.Dish.Name,
                    i.Quantity,
                    i.PriceAtOrder
                    ))
                .ToList()
            ))
            .FirstOrDefaultAsync();
    }
    public async Task<Guid> CreateAsync(Guid userId, Guid addressId, decimal totalPrice, List<OrderItemResponse> items) {
        var orderId = Guid.NewGuid();

        var entity = new OrderEntity {
            Id = orderId,
            TotalPrice = totalPrice,
            UserId = userId,
            AddressId = addressId,
            Items = items.Select(i => new OrderItemEntity {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                DishId = i.DishId,
                Quantity = i.Quantity,
                PriceAtOrder = i.PriceAtOrder
            }).ToList()
        };

        db.Orders.Add(entity);
        await db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request) {
        var affected = await db.Orders
            .Where(o => o.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, request.Status));

        return affected > 0;
    }

    public async Task<Guid?> GetUserIdByOrderIdAsync(Guid id)
    {
        return await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(u => (Guid?)u.UserId)
            .FirstOrDefaultAsync();
    }
}
