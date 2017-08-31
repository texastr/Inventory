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
//EXP This is me, testing the git hub bullshit. Testing again.
namespace TexasTRInventory.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InventoryContext _context;
        private int IHopeThisIsPersistant = 0;

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
                //TODO get the right new path, check it's an image, throw exception handling everywhere
                FilePath productPhoto = new FilePath { FileName = "bad File" };
                string newPath = uniquePath(upload.FileName);
                using (FileStream fileStream = new FileStream(newPath, FileMode.CreateNew))
                {
                    await upload.CopyToAsync(fileStream);
                }
                productPhoto.FileName = newPath;            
                product.ImageFilePath = productPhoto;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ManufacturerID"] = new SelectList(_context.Manufacturers, "ID", "ID", product.ManufacturerID);
            return View(product);
        }

        private string uniquePath(string fileName)
        {
            const string IMAGE_FOLDER = "Images";  
            string newFileName = String.Join("$", DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"), IHopeThisIsPersistant++, fileName);
            string root = HttpRuntime.AppDomainAppPath;
            return Path.Combine(root,IMAGE_FOLDER,newFileName);
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
