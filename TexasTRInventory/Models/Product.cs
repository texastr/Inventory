using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TexasTRInventory.Models
{
    public class Product
    {
        public int ID { get; set; }
        public int? SupplierID { get; set; }
        public Company Supplier { get; set; }

		[DisplayName("Brand Name - 产品品牌")]
		public string Brand { get; set; }

		[DisplayName("Stock Keeping Unit (SKU) - 库存单位(SKU)")]
		public string SKU { get; set; }

		[DisplayName("Item Name - 产品名称")]
        public string Name { get; set; }

        [DisplayName("Product Description - 产品描述")]
        public string Info { get; set; }

		[DisplayName("Package Contents - 包装内容(盒子里内容)")]
		public string PackageContents { get; set; }

		[DisplayName("Cost - 批发价")]
        public decimal? OurCost { get; set; }
   
        [DisplayName("MAP - Lowest Advertised Price - 最低零售价(不能低于它)")]
        public decimal? MAP { get; set; }
        
		[DisplayName("Dimensions - 尺寸")]
        public string Dimensions { get; set; }

		[DisplayName("Weight - 重量")]
		public string Weight { get; set; }

		[DisplayName("Universal Product Code (UPC) - 通用产品代码 (条形码)")]
		public string UPC { get; set; }

		[DisplayName("Website URL For Item - 产品网站 / 网址")]
		public string Website { get; set; }


		//Internal-only fields

		[DisplayName("Tags")]
		public string Category { get; set; }

		[DisplayName("Quantity on hand")]
		public int? Inventory { get; set; }

		[DisplayName("Part Number")]
		public string PartNumber { get; set; }

		[DisplayName("Amazon ASIN")]
		public string AmazonASIN { get; set; }

		public string Dealer { get; set; }

		/*the retailer specific information I'll do later
        B&H,Adorama,Sammys,eBay,New Egg, Walmart, Walmart DSV,Jet.com,Vendor - Drop ship, Vendor USA,Vendor Canada
        */
		public virtual ICollection<FilePath> ImageFilePaths { get; set;}
    }
}
