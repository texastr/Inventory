using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TexasTRInventory.Data;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Windows.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using TexasTRInventory.Constants;

namespace TexasTRInventory.Controllers
{
    
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly InventoryContext _context;

        public ProductsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string sortOrder)
        {

			IQueryable<ProductDBModel> products;
            if (Utils.IsInternalUser(User))
            {
                products = _context.Products.Include(p => p.Supplier);
            }
            else
            {
                int? supplierID = Utils.SupplierID(User);
                products = _context.Products.Where(p => p.SupplierID == supplierID);
            }

            //EXP 9.28.17. Interpreting the sortOrderParam
            ViewData["SKUSortParm"] = String.IsNullOrEmpty(sortOrder) ? "SKU_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";

            switch (sortOrder)
            {
                case "SKU_desc":
                    products = products.OrderByDescending(p => p.SKU);
                    break;
                case "Name":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(s => s.SKU);
                    break;
            }

            return View(await products.AsNoTracking().ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.ImageFilePaths)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!User.IsOwnProduct(product))
            {
                return HiddenProductError();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return ViewWithSupplierList();
        }
        
        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (ProductViewModel pvm)
        {
			if(!ModelState.IsValid)return ViewWithSupplierList(); ;

            ProductDBModel product = null;
            try
            {
                product = await pvm.ToProductDB();
            }
            catch (FileUploadFailedException e)
            {
                Expression<Func<ProductViewModel, IFormFileCollection>> expression = p => p.ImageFiles;
                string key = ExpressionHelper.GetExpressionText(expression);
                ModelState.AddModelError(key, e.Message);
            }

            if (ModelState.IsValid)
            {
                product.SupplierID = Utils.IsInternalUser(User) ? product.SupplierID : Utils.SupplierID(User);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return ViewWithSupplierList();
        }



        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.ImageFilePaths)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!User.IsOwnProduct(product))
            {
                return HiddenProductError();
            }

            ViewData["SupplierID"] = Utils.CompanyList(_context, product.Supplier);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile upload, [Bind("ID,Brand,SupplierID,SKU,PartNumber,AmazonASIN,Name,Inventory,Info,OurCost,Dealer,MAP,Dimensions,Weight,UPC,Website,PackageContents,Category")] ProductDBModel product)
        {
            ProductDBModel existingProduct = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.ImageFilePaths)
                .SingleOrDefaultAsync(m => m.ID == id);
			_context.Entry(existingProduct).State = EntityState.Detached; //If all works well, the filepath will be tracked, but not the product

            if (id != product.ID || existingProduct == null)
            {
                return NotFound();
            }

            if (!User.IsOwnProduct(existingProduct))
            {
                return HiddenProductError();
            }

			//EXP 9.28.17. Handle SupplierID
			if (!Utils.IsInternalUser(User))
			{
				product.SupplierID = existingProduct.SupplierID;
			}


            string newFileName = null; //TODO EXP breaking everything! await ProductViewModel.UploadImageWrapper(upload);

            if (ModelState.IsValid)
            {
                try
                {
                    if (newFileName != null)
                    {
                        FilePath filePath = await GetEmptyFilePath(existingProduct);

                        filePath.FileName = newFileName; //then wire in the new file
                    }
                    
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["SupplierID"] = new SelectList(_context.Companies, "ID", "ID", product.SupplierID);
            return View(product);
        }

        private async Task<FilePath> GetEmptyFilePath(ProductDBModel product)
        {
			FilePath oldFilePath = null;//just want to to get this to compile product.ImageFilePath;
            
            if (oldFilePath == null)//hitherto there was no image saved
            {
                FilePath ret = new FilePath() { ProductID = product.ID };
                _context.Add(ret);
                return ret;
            }
            //If there was an image, let's delete it and return the existing object
            string oldFileName = oldFilePath.FileName;
            if (!string.IsNullOrWhiteSpace(oldFileName))
            {
                CloudBlockBlob blob = await GlobalCache.GetImageBlob(oldFileName); //delete the old file
                await blob.DeleteIfExistsAsync();
            }

            return oldFilePath;
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
				.Include(p => p.ImageFilePaths)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!User.IsOwnProduct(product))
            {
                return HiddenProductError();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.SingleOrDefaultAsync(m => m.ID == id);

            if (!User.IsOwnProduct(product))
            {
                return HiddenProductError();
            }
            
            //EXP 9.6.17
            var filePaths = _context.FilePaths.Where(f => f.ProductID == id);
            foreach (FilePath fp in filePaths)
            {
                //This will be wasteful. Only significant if we have many images for a product
                if (!String.IsNullOrEmpty(fp.FileName))
                {
                    CloudBlockBlob blob = await GlobalCache.GetImageBlob(fp.FileName);
                    await blob.DeleteIfExistsAsync();
                }
            }
            _context.FilePaths.RemoveRange(filePaths);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ID == id);
        }

        private IActionResult ViewWithSupplierList(ProductDBModel currentProduct = null)
        {
            ViewData[KeyNames.SupplierID] = Utils.CompanyList(_context, currentProduct?.Supplier);
            return View();
        }

        private IActionResult HiddenProductError()
        {
            ViewData[KeyNames.ErrorDetails] = "You do not have the right to access that product.";
            return View("Error");
        }
    }

    static class ProductExtensions
    {
        public static bool IsOwnProduct(this ClaimsPrincipal user, ProductDBModel product)
        {
            return Utils.IsInternalUser(user) || user.FindFirst(ClaimNames.EmployerID).Value == product.SupplierID.ToString();
        }

    }
}
