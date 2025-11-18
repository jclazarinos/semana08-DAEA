using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.DTOs;
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab08_JeanLazarinos.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(StoreLdbContext context) : base(context)
    {
    }

    public async Task<PedidoConDetallesDto?> GetPedidoConDetallesAsync(int orderId)
    {
        var pedidoDto = await _context.Orders
            .AsNoTracking() 
            .Include(o => o.Orderdetails) 
            .ThenInclude(od => od.Product) 
            .Include(o => o.Client) 
            .Where(o => o.Orderid == orderId)
            .Select(o => new PedidoConDetallesDto
            {
                OrderId = o.Orderid,
                OrderDate = o.Orderdate,
                Cliente = new ClienteBasicoDto
                {
                    ClientId = o.Client.Clientid,
                    Name = o.Client.Name,
                    Email = o.Client.Email
                },
                Detalles = o.Orderdetails.Select(od => new DetalleProductoDto
                {
                    OrderDetailId = od.Orderdetailid,
                    ProductId = od.Product.Productid,
                    ProductName = od.Product.Name,
                    ProductDescription = od.Product.Description,
                    ProductPrice = od.Product.Price,
                    Quantity = od.Quantity,
                    Subtotal = od.Product.Price * od.Quantity
                }).ToList(),
                TotalPedido = o.Orderdetails.Sum(od => od.Product.Price * od.Quantity)
            })
            .FirstOrDefaultAsync();

        return pedidoDto;
    }
}