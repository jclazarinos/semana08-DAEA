using Lab08_JeanLazarinos.DTOs;
using Lab08_JeanLazarinos.Models;

namespace Lab08_JeanLazarinos.Repositories.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<ClienteConPedidosDto?> GetClienteConPedidosAsync(int clientId);
    Task<IEnumerable<ClienteConTotalProductosDto>> GetClientesConTotalProductosAsync();
    
    Task<IEnumerable<VentasPorClienteDto>> GetVentasPorClienteAsync(
        DateTime? fechaInicio = null, 
        DateTime? fechaFin = null, 
        decimal? montoMinimo = null);
}