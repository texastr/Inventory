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
            _logger = loggerFactory.CreateLogger<UsersAdminController>();
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
            ViewData["EmployerID"] = Utils.CompanyList(_context, applicationUser.Employer, false);
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EmailConfirmed,IsDisabled,EmployerID")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(user);
                    //await _context.SaveChangesAsync();
                    //EXP 9.22.17 the above is the standard scaffolded code and throws a concurrency exceptions
                    ApplicationUser dbUser = await _userManager.FindByIdAsync(id);
                    dbUser.EmployerID = user.EmployerID;
                    dbUser.EmailConfirmed = user.EmailConfirmed;
                    dbUser.IsDisabled = user.IsDisabled;
                    await _userManager.UpdateAsync(dbUser);
                    //await _userManager.UpdateAsync(user);
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
                    IsDisabled = model.IsDisabled,
                    EmailConfirmed = model.EmailConfirmed
                };

                IdentityResult result; //EXP 9.15.17 We can now create users without passwords
                if (String.IsNullOrWhiteSpace(model.Password))
                {
                    result = await _userManager.CreateAsync(user);
                }
                else
                {
                    result = await _userManager.CreateAsync(user, model.Password);
                }



                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //Line below came with the code. But I'll replace it with what's on the site
                    var callbackUrl = Url.Action(nameof(AccountController.ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                        $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    //await _signInManager.SignInAsync(user, isPersistent: false); <=comment out so that users aren't logged in until they verify they're email address
                    _logger.LogInformation(3, "User created a new account");


                    return RedirectToAction(nameof(Index));

                }
                Utils.AddErrors(result, ModelState);
            }

            // If we got this far, something failed, redisplay form
            //Must also repopulate the list of suppliers. Maybe I should refactor it to a different tag
            return View(model);
        }



        private bool ApplicationUserExists(string id)
        {
            return _context.ApplicationUser.Any(e => e.Id == id);
        }
    }
}
