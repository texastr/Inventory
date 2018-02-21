using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TexasTRInventory.Models
{
    /// <summary>
    /// This class contains the intersection of what belongs in ProductDBModel and in ProductViewModel.
    /// A purist would say it's bad practice to define a class solely in terms of conincidences between other classes.
    /// But it's better than having nearly identical fields in two classes and having complicated or duplicitive code to map between them.
    /// </summary>
    public abstract class Product
    {
        public int? SupplierID { get; set; }
        public Company Supplier { get; set; }

        public int ID { get; set; }

        [DisplayName("Brand Name - 产品品牌")]
        public string Brand { get; set; }

        [DisplayName("Stock Keeping Unit (SKU) - 库存单位(SKU)")]
        public string SKU { get; set; }

        [DisplayName("Item Name - 产品名称")]
        public string Name { get; set; }

        [DisplayName("Product Description - 产品描述")]
        [DataType(DataType.MultilineText)]
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
        [RestrictedField(Constants.ClaimNames.IsInternal)]
        public string Category { get; set; }

        [DisplayName("Quantity on hand")]
        [RestrictedField(Constants.ClaimNames.IsInternal)]
        public int? Inventory { get; set; }

        [DisplayName("Part Number")]
        [RestrictedField(Constants.ClaimNames.IsInternal)]
        public string PartNumber { get; set; }

        [DisplayName("Amazon ASIN")]
        [RestrictedField(Constants.ClaimNames.IsInternal)]
        public string AmazonASIN { get; set; }

        [RestrictedField(Constants.ClaimNames.IsInternal)]
        public string Dealer { get; set; }

        [DisplayName("Has an Admin Approved?")]
        [RestrictedField(Constants.ClaimNames.IsAdmin)]
        public bool IsAdminApproved { get; set; }

        public static void MapperInitializer()
        {
            Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<ProductViewModel, ProductDBModel>();
                    cfg.CreateMap<ProductDBModel, ProductViewModel>();
                });
        }
        
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class RestrictedFieldAttribute: Attribute
    {
        public string RequiredClaim { get; set; }

        public RestrictedFieldAttribute(string requiredClaim)
        {
            RequiredClaim = requiredClaim;
        }

        public bool CanUserEdit(ClaimsPrincipal user)
        {
            return user.HasClaim(RequiredClaim, true.ToString());
        }
    }

}
