// DTOs/CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace Lab08_JeanLazarinos.DTOs;

public class CreateOrderDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderDetailDto> Items { get; set; } = new();
}

public class OrderDetailDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}