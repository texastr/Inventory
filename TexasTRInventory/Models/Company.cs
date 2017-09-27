using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TexasTRInventory.Models
{
    public class Company
    {
        public int ID { get; set; }

        //Details regarding this field are in InventoryContext, since I needed fluent API
        public string Name { get; set; }
        
        public bool IsInternal { get; set; }
       
        public ICollection<Product> Products { get; set; }
    }
}