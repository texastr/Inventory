using System;
using System.Collections.Generic;

namespace TexasTRInventory.Models
{
    public class Manufacturer
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}