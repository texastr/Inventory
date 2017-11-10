using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TexasTRInventory.Models
{
    public class ProductViewModel
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
		[DisplayName("Upload Your Product Photos - 上传产品图片")]
		[SufficientImages]
		public IFormFileCollection ImageFiles { get; set;} //Maybe rename?

		[DisplayName("Previously Uploaded Photos -- WTF is that in Chinese?")]
		public ICollection<FilePath> OldFilePaths { get; set; }


		
    }
	public class SufficientImagesAttribute : ValidationAttribute, IClientModelValidator
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			IFormFileCollection input = (IFormFileCollection)value;

			if (input?.Count >= GlobalCache.MinImgFilesCnt())
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult($"A minimum of {GlobalCache.MinImgFilesCnt()} images are required");
			}
		}

		void IClientModelValidator.AddValidation(ClientModelValidationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			MergeAttribute(context.Attributes, "data-val","true");//not sure what this line does.
			MergeAttribute(context.Attributes, "data-val-sufficentimages", "this is the error message I'll define");
			MergeAttribute(context.Attributes, "data-val-sufficientimagescnt", GlobalCache.MinImgFilesCnt().ToString());
		}

		private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
		{
			if (attributes.ContainsKey(key))
			{
				return false;
			}

			attributes.Add(key, value);
			return true;
		}
	}

}
