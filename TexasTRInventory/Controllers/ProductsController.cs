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

			IQueryable<Product> products;
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
        public async Task<IActionResult> Create (ProductViewModel product)//Create(IFormFileCollection upload,[Bind("ID,Brand,SupplierID,SKU,PartNumber,AmazonASIN,Name,Inventory,Info,OurCost,Dealer,MAP,Dimensions,Weight,UPC,Website,PackageContents,Category")] Product product)
        {
			//EXP 9.26.17 Side effect: will mark the model stat as invalid if the file is bad
			//EXP 10.31.17 TODO: do all uploads in one shot, as a transaction
			//Intialize the ImageFilePath collection
			//product.ImageFilePaths = new Collection<FilePath>();

			if(!ModelState.IsValid)return ViewWithSupplierList(); ;

			foreach (IFormFile file in product.ImageFiles/*upload*/){
				if (!ModelState.IsValid)
				{
					break;
				}
				FilePath uploadedImage = new FilePath() { FileName = await UploadImageWrapper(file) };
				//product.ImageFilePaths.Add(uploadedImage);
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

        private async Task<string> UploadImageWrapper(IFormFile upload)
        {
            if (upload != null)
            {
                if (upload.ContentType.StartsWith("image/")) //TODO have an attribute on the model check that it's an image
                {
                    try
                    {
                        string newFileName = await UploadFile(upload);
                        return newFileName;
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Uploading the product's image file failed.");
                        //TODO. This is a good spot to log an error
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The file you attempted to upload is not an image file.");
                }
            }

            return null;
        }

        private async Task<string> UploadFile(IFormFile upload)
        {
            string newFileName = UniqueFileName(upload.FileName);
            CloudBlockBlob blob = await GlobalCache.GetBlob(newFileName);
            blob.Properties.ContentType = upload.ContentType;

            using (Stream intermediateMemory = new MemoryStream())
            {
                upload.CopyTo(intermediateMemory);
                intermediateMemory.Seek(0, SeekOrigin.Begin);
                blob.UploadFromStream(intermediateMemory);
            }

            return newFileName;
        }

        private string UniqueFileName(string fileName)
        {
            return String.Join("$", DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"), fileName);
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
        public async Task<IActionResult> Edit(int id, IFormFile upload, [Bind("ID,Brand,SupplierID,SKU,PartNumber,AmazonASIN,Name,Inventory,Info,OurCost,Dealer,MAP,Dimensions,Weight,UPC,Website,PackageContents,Category")] Product product)
        {
            Product existingProduct = await _context.Products
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


            string newFileName = await UploadImageWrapper(upload);

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

        private async Task<FilePath> GetEmptyFilePath(Product product)
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
                CloudBlockBlob blob = await GlobalCache.GetBlob(oldFileName); //delete the old file
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
                    CloudBlockBlob blob = await GlobalCache.GetBlob(fp.FileName);
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

        private IActionResult ViewWithSupplierList(Product currentProduct = null)
        {
            ViewData[Constants.KeyNames.SupplierID] = Utils.CompanyList(_context, currentProduct?.Supplier);
            return View();
        }

        private IActionResult HiddenProductError()
        {
            ViewData[Constants.KeyNames.ErrorDetails] = "You do not have the right to access that product.";
            return View("Error");
        }
    }

    static class ProductExtensions
    {
        public static bool IsOwnProduct(this ClaimsPrincipal user, Product product)
        {
            return Utils.IsInternalUser(user) || user.FindFirst(Constants.ClaimTypes.EmployerID).Value == product.SupplierID.ToString();
        }

    }
}
