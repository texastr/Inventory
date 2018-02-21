using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TexasTRInventory.Data;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TexasTRInventory.Services;
using Microsoft.Extensions.Logging;
using TexasTRInventory.Constants;

namespace TexasTRInventory.Controllers
{
    [Authorize(Policy = Constants.PolicyNames.IsInternal)]
    public class ApplicationUsersController : Controller
    {
        private readonly InventoryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        //12.7.17 adding more fields to do fancy authorization handling
        private readonly IAuthorizationService _authorizationService;

        public ApplicationUsersController(InventoryContext context, 
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory,
            IEmailSender emailSender,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<ApplicationUsersController>();
            _emailSender = emailSender;
            _authorizationService = authorizationService;
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

            //EXP 12.7.17. Make sure user has the right to edit this user
            bool isAuthorized = await _authorizationService.AuthorizeAsync(User, applicationUser, PolicyNames.OnlyAdminsEditAdmins);
            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

            return EditViewWithEmployerList(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, /*[Bind("IsDisabled,EmployerID")]*/ ApplicationUser applicationUser)
        {            
            if (ModelState.IsValid)
            {
                try
                {

                    ApplicationUser dbUser = await _userManager.FindByIdAsync(id);

                    //EXP 12.7.17. Make sure user has the right to edit this user
                    bool isAuthorized = await _authorizationService.AuthorizeAsync(User, dbUser,PolicyNames.OnlyAdminsEditAdmins);
                    if (!isAuthorized)
                    {
                        return new ChallengeResult();
                    }

                    dbUser.EmployerID = applicationUser.EmployerID;
                    dbUser.IsDisabled = applicationUser.IsDisabled;
                    dbUser.IsAdmin = applicationUser.IsAdmin && Utils.IsAdmin(User); //Sneaky how I made this an and, instead of an if. less readable, but so clever
                    await _userManager.UpdateAsync(dbUser);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.Id))
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
            return EditViewWithEmployerList(applicationUser);
        }

        private IActionResult EditViewWithEmployerList(ApplicationUser applicationUser)
        {
            ViewData["EmployerID"] = Utils.CompanyList(_context, applicationUser.Employer, false); //TODO I think I know why the employer isn't loading
            return View(applicationUser);
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

            bool isAuthorized = await _authorizationService.AuthorizeAsync(User, applicationUser, PolicyNames.OnlyAdminsEditAdmins);
            if (!isAuthorized)
            {
                return new ChallengeResult();
            }


            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationUser = await _context.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);

            //EXP 12.7.17. Make sure user has the right to edit this user
            bool isAuthorized = await _authorizationService.AuthorizeAsync(User, applicationUser, PolicyNames.OnlyAdminsEditAdmins);
            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

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
            ViewData["EmployerID"] = Utils.CompanyList(_context, excludeInternal: false);
            return View(model);
        }

        private async Task<string> EmailText(ApplicationUser user)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string callbackUrl = Url.Action(nameof(AccountController.SetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
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
