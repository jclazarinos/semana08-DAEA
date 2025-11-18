namespace Lab08_JeanLazarinos.DTOs;

public class ClienteConPedidosDto
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<PedidoDto> Pedidos { get; set; } = new();
}

public class PedidoDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int TotalProductos { get; set; }
}