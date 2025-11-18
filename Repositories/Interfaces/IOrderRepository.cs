using Lab08_JeanLazarinos.DTOs;
using Lab08_JeanLazarinos.Models;

namespace Lab08_JeanLazarinos.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<PedidoConDetallesDto?> GetPedidoConDetallesAsync(int orderId);
}