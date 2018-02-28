using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TexasTRInventory.Data;
using Microsoft.EntityFrameworkCore;
using TexasTRInventory.Models;
using TexasTRInventory.Constants;
using System.Web;
using System;
using Microsoft.AspNetCore.Authorization;

namespace TexasTRInventory.Controllers
{
    [Authorize(Policy=PolicyNames.IsInternal)]
    public class PicturesController : Controller
    {
        private InventoryContext _context;
      
        public PicturesController(InventoryContext context)
        {
            _context = context;
        }

        [Route("pics/{sku}/{supplier}/{num}")]
        public async Task<IActionResult> Index(string sku, string supplier, int num)
        {
            var products = await _context.Products.AsNoTracking()
                .Include(p => p.Supplier)
                .Include(p => p.ImageFilePaths)
                .Where(p => p.SKU.ToUpper() == sku.ToUpper())
                .Where(p => p.Supplier.ID.ToString() == supplier || p.Supplier.Name.ToUpper() == supplier.ToUpper())
                .ToListAsync();
            if(products.Count() == 0)
            {
                return Error($"No product was found whose SKU is <strong>{sku}</strong> and whose supplier is <strong>{supplier}</strong>.");
                   
            }
            else if (products.Count() == 1)
            {
                ProductDBModel product = products.First();
                string fileName=string.Empty;
                try
                {
                    fileName = product.ImageFilePaths.ElementAt(num-1).FileName; //subtract one, because users are indexing from 1 instead of 0.
                }
                catch(ArgumentOutOfRangeException)
                {
                    string message = $"It appears you were attempting to access product number <a href='/Products/Details/{product.ID}'>{product.ID}</a>. " +
                        "However you did not specify a valid image number";
                    return Error(message);
                }
                string url = (await GlobalCache.GetImageBlob(fileName)).Uri.AbsoluteUri;
                return Redirect(url);
            }
            else
            {
                string message = $"The following products from supplier {supplier} all have sku {sku}. Clean up the data before attempting to access photos.";
                foreach (Product product in products)
                {
                    message += $"<br/> <a href = '/Products/Details/{product.ID}' >{product.ID}</a>";
                }
                return Error(message);
            }
            

        }

        private IActionResult Error(string message)
        {
            string completeMessage = message + "<br/>" + HttpUtility.HtmlEncode("Remember: the correct form is /pics/<sku>/<supplier name or ID>/<image number>.");
            ViewData[KeyNames.ErrorDetails] = completeMessage;
            return View("Error");
        }
    }
}