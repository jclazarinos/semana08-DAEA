using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.DTOs;
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab08_JeanLazarinos.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(StoreLdbContext context) : base(context)
    {
    }

    public async Task<ClienteConPedidosDto?> GetClienteConPedidosAsync(int clientId)
    {
        var clienteDto = await _context.Clients
            .AsNoTracking() 
            .Where(c => c.Clientid == clientId)
            .Select(c => new ClienteConPedidosDto
            {
                ClientId = c.Clientid,
                Name = c.Name,
                Email = c.Email,
                Pedidos = c.Orders.Select(o => new PedidoDto
                {
                    OrderId = o.Orderid,
                    OrderDate = o.Orderdate,
                    TotalProductos = o.Orderdetails.Count()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return clienteDto;
    }
    
        
    public async Task<IEnumerable<ClienteConTotalProductosDto>> GetClientesConTotalProductosAsync()
    {
        var clientes = await _context.Clients
            .AsNoTracking()
            .Select(c => new
            {
                c.Clientid,
                c.Name,
                c.Email
            })
            .ToListAsync();
        
        var estadisticasPorCliente = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Orderdetails)
                .ThenInclude(od => od.Product)
            .GroupBy(o => o.Clientid)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalPedidos = g.Count(),
                TotalProductosComprados = g.SelectMany(o => o.Orderdetails)
                                           .Sum(od => od.Quantity),
                MontoTotalGastado = g.SelectMany(o => o.Orderdetails)
                                     .Sum(od => od.Quantity * od.Product.Price)
            })
            .ToListAsync();
        
        var resultado = from cliente in clientes
                        join stats in estadisticasPorCliente
                        on cliente.Clientid equals stats.ClientId into clienteStats
                        from stats in clienteStats.DefaultIfEmpty()
                        select new ClienteConTotalProductosDto
                        {
                            ClientId = cliente.Clientid,
                            Name = cliente.Name,
                            Email = cliente.Email,
                            TotalPedidos = stats?.TotalPedidos ?? 0,
                            TotalProductosComprados = stats?.TotalProductosComprados ?? 0,
                            MontoTotalGastado = stats?.MontoTotalGastado ?? 0
                        };

        return resultado.ToList();
    }
    
    
    public async Task<IEnumerable<VentasPorClienteDto>> GetVentasPorClienteAsync(
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        decimal? montoMinimo = null)
    {
        // Consulta base con AsNoTracking
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Client)
            .Include(o => o.Orderdetails)
                .ThenInclude(od => od.Product)
            .AsQueryable();

        // FILTRO 1: Fecha de inicio
        if (fechaInicio.HasValue)
        {
            query = query.Where(o => o.Orderdate >= fechaInicio.Value);
        }

        // FILTRO 2: Fecha de fin
        if (fechaFin.HasValue)
        {
            query = query.Where(o => o.Orderdate <= fechaFin.Value);
        }

        // AGRUPACIÓN Y CÁLCULO DE VENTAS
        var ventasPorCliente = await query
            .GroupBy(o => new
            {
                o.Client.Clientid,
                o.Client.Name,
                o.Client.Email
            })
            .Select(g => new
            {
                ClientId = g.Key.Clientid,
                ClientName = g.Key.Name,
                ClientEmail = g.Key.Email,
                TotalPedidos = g.Count(),
                TotalProductos = g.SelectMany(o => o.Orderdetails)
                                  .Sum(od => od.Quantity),
                TotalVentas = g.SelectMany(o => o.Orderdetails)
                              .Sum(od => od.Quantity * od.Product.Price),
                FechaPrimerPedido = g.Min(o => o.Orderdate),
                FechaUltimoPedido = g.Max(o => o.Orderdate)
            })
            .ToListAsync();

        // FILTRO 3: Monto mínimo (se aplica después de la agrupación en memoria)
        var resultado = ventasPorCliente
            .Where(v => !montoMinimo.HasValue || v.TotalVentas >= montoMinimo.Value)
            .OrderByDescending(v => v.TotalVentas) // Ordenar por ventas de mayor a menor
            .Select(v => new VentasPorClienteDto
            {
                ClientId = v.ClientId,
                ClientName = v.ClientName,
                ClientEmail = v.ClientEmail,
                TotalPedidos = v.TotalPedidos,
                TotalProductos = v.TotalProductos,
                TotalVentas = v.TotalVentas,
                PromedioVentaPorPedido = v.TotalPedidos > 0 
                    ? v.TotalVentas / v.TotalPedidos 
                    : 0,
                FechaPrimerPedido = v.FechaPrimerPedido,
                FechaUltimoPedido = v.FechaUltimoPedido
            })
            .ToList();

        return resultado;
    }
}