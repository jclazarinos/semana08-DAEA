using Lab08_JeanLazarinos.DTOs; // Importa tus DTOs
using Lab08_JeanLazarinos.Models;

namespace Lab08_JeanLazarinos.Services
{
public interface IExcelService
{
    // Reporte 1: Ventas por Cliente
    byte[] GenerateVentasPorClienteReport(IEnumerable<VentasPorClienteDto> ventas);

    // Reporte 2: Detalle de un Pedido
    byte[] GeneratePedidoDetalladoReport(PedidoConDetallesDto pedido);
}
}