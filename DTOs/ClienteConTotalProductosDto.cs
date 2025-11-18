namespace Lab08_JeanLazarinos.DTOs;

public class ClienteConTotalProductosDto
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPedidos { get; set; }
    public int TotalProductosComprados { get; set; }
    public decimal MontoTotalGastado { get; set; }
}