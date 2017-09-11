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
using System.Web;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Windows.Forms;
using Microsoft.Azure.KeyVault;
using System.Web.Configuration;
using System.Configuration;

namespace TexasTRInventory.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InventoryContext _context;
  
        public ProductsController(InventoryContext context)
        {
            _context = context;    
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var inventoryContext = _context.Products.Include(p => p.Manufacturer);
            return View(await inventoryContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.ImageFilePath)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["ManufacturerID"] = new SelectList(_context.Manufacturers, "ID", "ID");
            return View();
        }
        
        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile upload,[Bind("ID,ManufacturerID,SKU,PartNumber,AmazonASIN,Name,Inventory,Info,OurCost,Dealer,MAP,Dimentions,Weight,UPC,Website,PackageContents,Category")] Product product)
        {
            if (ModelState.IsValid)
            {
                //EXP 8.30.17 adding in stuff regarding the file
                //TODO throw exception handling everywhere. handle files that are too big (e.g. Jared's tiff)
                if (upload != null)
                {
                    if (upload.ContentType.StartsWith("image/"))
                    {
                        product.ImageFilePath = await UploadFile(upload);
                    }
                    else
                    {
                        //TODO
                        MessageBox.Show("The file you chose wasn't an image and won't be saved. Posner! Find a better way to deal with this");
                    }
                                    }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ManufacturerID"] = new SelectList(_context.Manufacturers, "ID", "ID", product.ManufacturerID);
            return View(product);
        }

        private async Task<FilePath> UploadFile(IFormFile upload)
        {
            FilePath ret = new FilePath { FileName = "bad File" };
            string newFileName = UniqueFileName(upload.FileName);
            CloudBlockBlob blob = await GlobalCache.GetBlob(newFileName);
            blob.Properties.ContentType = upload.ContentType;

            using (Stream intermediateMemory = new MemoryStream())
            {
                upload.CopyTo(intermediateMemory);
                intermediateMemory.Seek(0, SeekOrigin.Begin);
                blob.UploadFromStream(intermediateMemory);
            }

            ret.FileName = newFileName;


            return ret;
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

            var product = await _context.Products.SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ManufacturerID"] = new SelectList(_context.Manufacturers, "ID", "ID", product.ManufacturerID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ManufacturerID,SKU,PartNumber,AmazonASIN,Name,Inventory,Info,OurCost,Dealer,MAP,Dimentions,Weight,UPC,Website,PackageContents,Category")] Product product)
        {
            if (id != product.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["ManufacturerID"] = new SelectList(_context.Manufacturers, "ID", "ID", product.ManufacturerID);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.SingleOrDefaultAsync(m => m.ID == id);
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
    }
}
