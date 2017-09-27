using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TexasTRInventory.Data;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;//EXP 9.13.17. From here to the end, all bullshit
using Microsoft.AspNetCore.Identity;

namespace TexasTRInventory.Controllers
{
    
    public class CompaniesController : Controller
    {
        private readonly InventoryContext _context;
        
        public CompaniesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
           
            //This is the only line that is not shit.
            return View(await _context.Companies.ToListAsync());
        }
 
        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Company supplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id == GlobalCache.GetTexasTRCompanyID(_context))
            {
                return DontTouchTexasTR();
            }

            var supplier = await _context.Companies.SingleOrDefaultAsync(m => m.ID == id);
            if (supplier == null) //TODO -- also exclude internal supplier. But first let me deal with the post method
            {
                return NotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Company supplier)
        {
            if (id != supplier.ID)
            {
                return NotFound();
            }

            //EXP 9.22.17. Don't let people edit TexasTR
            if (id==GlobalCache.GetTexasTRCompanyID(_context))
            {
                return DontTouchTexasTR();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (Utils.IsUniqueKeyViolation(ex.InnerException))
                    {
                        ModelState.AddModelError(string.Empty, "The database could not accept your edits. Each company must have a unique name.");
                        return View(supplier);
                    }

                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

/*            if (id == GlobalCache.GetTexasTRCompanyID(_context))
            {
                return DontTouchTexasTR();
            }
            */
            var supplier = await _context.Companies
                .SingleOrDefaultAsync(m => m.ID == id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == GlobalCache.GetTexasTRCompanyID(_context))
            {
                return DontTouchTexasTR();
            }
            var supplier = await _context.Companies.SingleOrDefaultAsync(m => m.ID == id);
            _context.Companies.Remove(supplier);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool SupplierExists(int id)
        {
            return _context.Companies.Any(e => e.ID == id);
        }

        private IActionResult DontTouchTexasTR()
        {
            ViewData[Constants.KeyNames.ErrorDetails] = "You cannot edit or delete the Texas TR record.";
            return View("Error");
        }
    }
}
