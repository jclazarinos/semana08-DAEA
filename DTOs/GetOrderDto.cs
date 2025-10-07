// DTOs/GetOrderDto.cs
namespace Lab08_JeanLazarinos.DTOs;

public class GetOrderDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string ClientName { get; set; }
    public List<GetOrderDetailDto> Items { get; set; } = new();
}

public class GetOrderDetailDto
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}