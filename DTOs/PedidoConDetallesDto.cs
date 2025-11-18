namespace Lab08_JeanLazarinos.DTOs;

public class PedidoConDetallesDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public ClienteBasicoDto Cliente { get; set; } = new();
    public List<DetalleProductoDto> Detalles { get; set; } = new();
    public decimal TotalPedido { get; set; }
}

public class ClienteBasicoDto
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class DetalleProductoDto
{
    public int OrderDetailId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; } // Precio * Cantidad
}