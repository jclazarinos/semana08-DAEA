using System;
using System.Collections.Generic;

namespace Lab08_JeanLazarinos.Models;

public partial class Order
{
    public int Orderid { get; set; }

    public int Clientid { get; set; }

    public DateTime Orderdate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
}
