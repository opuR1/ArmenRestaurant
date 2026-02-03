using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmenRestauran.Models;

namespace ArmenRestauran.Services
{
    public static class CartService
    {
        public static List<CartItem> Items { get; set; } = new List<CartItem>();

        public class CartItem
        {
            public MenuItem Product { get; set; }
            public int Quantity { get; set; }
            public decimal Total => Product.Price * Quantity;
        }
    }
}
