using System;
using System.Collections.Generic;

namespace Lab08_JeanLazarinos.Models;

public partial class Product
{
    public int Productid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
}
