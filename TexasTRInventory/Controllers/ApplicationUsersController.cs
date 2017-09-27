using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TexasTRInventory.Data;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Ajax.Utilities;
using TexasTRInventory.Services;
using Microsoft.Extensions.Logging;
using TexasTRInventory.Models.AccountViewModels;

namespace TexasTRInventory.Controllers
{
    [Authorize(Policy = Constants.PolicyNames.IsInternal)]
    public class ApplicationUsersController : Controller
    {
        private readonly InventoryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public ApplicationUsersController(InventoryContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<ApplicationUsersController>();
            _emailSender = emailSender;
        }

        // GET: ApplicationUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApplicationUser.Include(p => p.Employer).ToListAsync());
        }
        
        // GET: ApplicationUsers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            ViewData["EmployerID"] = Utils.CompanyList(_context, applicationUser.Employer, false); //TODO I think I know why the employer isn't loading
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IsDisabled,EmployerID")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    ApplicationUser dbUser = await _userManager.FindByIdAsync(id);
                    dbUser.EmployerID = user.EmployerID;
                    dbUser.IsDisabled = user.IsDisabled;
                    await _userManager.UpdateAsync(dbUser);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(user.Id))
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
            return View(user);
        }

        // GET: ApplicationUsers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.ApplicationUser
                .AsNoTracking()
                .Include(u => u.Employer)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationUser = await _context.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);
            _context.ApplicationUser.Remove(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        //
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {

            ViewData["EmployerID"] = Utils.CompanyList(_context, excludeInternal: false);
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email,
                    Email = model.Email,
                    EmployerID = model.EmployerID,
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                    string msgBody = await EmailText(user);
                    await _emailSender.SendEmailAsync(model.Email, "Welcome to Texas TR!",msgBody);
                    _logger.LogInformation(3, "User created a new account");
                    return RedirectToAction(nameof(Index));

                }
                Utils.AddErrors(result, ModelState);
            }

            // If we got this far, something failed, redisplay form
            //Must also repopulate the list of suppliers. Maybe I should refactor it to a different tag
            return View(model);
        }

        private async Task<string> EmailText(ApplicationUser user)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string callbackUrl = Url.Action(nameof(ManageController.SetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            Company employer = await _context.Companies.FirstOrDefaultAsync(c => c.ID == user.EmployerID);

            string explanation = employer.IsInternal ?
                "An account has been created for you in our inventory management system." :
                $"You have been invited to record and track which products {employer.Name} sells with Texas TR.";

            string ret =
                $"Hello!<br><br>{explanation}<br>" +
                $"Please activate your account and set a password by clicking <a href='{callbackUrl}'>here</a>.<br><br>" +
                "Once your account is active, you can access the inventory management system at " +
                "<a href='https://texastrinventory.azurewebsites.net'>texastrinventory.azurewebsites.net</a>." +
                "<br><br> Sincerely,<br>Texas TR";
            return ret;
        }



        private bool ApplicationUserExists(string id)
        {
            return _context.ApplicationUser.Any(e => e.Id == id);
        }
    }
}
