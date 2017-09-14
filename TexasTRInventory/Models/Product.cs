using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory.Models
{
    public class Product
    {
        public int ID { get; set; } //EXP 8.8.17 I guess we should have special primary key field
        public int? SupplierID { get; set; }
        public Supplier Supplier { get; set; }
        public string SKU { get; set; }
        public string PartNumber { get; set; }
        public string AmazonASIN { get; set; }
        public string Name { get; set; }
        public int? Inventory { get; set; }
        public string Info { get; set; }
        public decimal? OurCost { get; set; }
        // is this field used?
        public string Dealer { get; set; }
        public decimal? MAP { get; set; }
        //TODO make dimensions into its own class
        public string Dimentions { get; set; }
        public decimal? Weight { get; set; }
        public string UPC { get; set; }
        public string Website { get; set; }
        public string PackageContents { get; set; }
        //Is this field used?
        public string Category { get; set; }
        /*the retailer specific information I'll do later
        B&H,Adorama,Sammys,eBay,New Egg, Walmart, Walmart DSV,Jet.com,Vendor - Drop ship, Vendor USA,Vendor Canada
        */

        //8.30.17 putting in the file
        //[ForeignKey("FilePath")]
        //public int? FilePathID { get; set; }
        public virtual FilePath ImageFilePath { get; set;}
    }
}
