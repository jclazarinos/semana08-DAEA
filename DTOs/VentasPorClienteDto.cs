namespace Lab08_JeanLazarinos.DTOs;

public class VentasPorClienteDto
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public int TotalPedidos { get; set; }
    public int TotalProductos { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioVentaPorPedido { get; set; }
    public DateTime? FechaPrimerPedido { get; set; }
    public DateTime? FechaUltimoPedido { get; set; }
}