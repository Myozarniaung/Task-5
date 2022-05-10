using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orderconsumerapi.Models
{
    public class orderconsumer
    {
        public int cartId { get; set; }
        public int total { get; set; }
        public string orderId { get; set; }
        public string Status { get; set; }
        public List<product> products { get; set; }
    }
    public class product
    {
        public string productId { get; set; }
        public double price { get; set; }
    }
}
