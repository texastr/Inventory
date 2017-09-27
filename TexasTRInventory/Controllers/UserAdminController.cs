using TexasTRInventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Mvc;
using TexasTRInventory.Data;
using Microsoft.AspNetCore.Mvc;
using TexasTRInventory.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using TexasTRInventory.Services;

namespace TexasTRInventory.Controllers
{
    [Authorize(Roles = "InternalUser")]
    public class UsersAdminController : Controller
    {
        /* I don't know why they gave me this constructor
        public UsersAdminController()
        {
        }*/

        public UsersAdminController(UserManager<ApplicationUser> userManager, InventoryContext context, ILoggerFactory loggerFactory, IEmailSender emailSender)
        {
            UserManager = userManager;
            _context = context;
            _logger = loggerFactory.CreateLogger<UsersAdminController>();
            _emailSender = emailSender;
        }

        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private InventoryContext _context;
        private UserManager<ApplicationUser> _userManager;
        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager;//EXP 9.19.17. See if I can get away with this ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private RoleManager<IdentityRole> _roleManager;
        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager;//EXP 9.19.17 Hopefully this is good enough ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        //
        // GET: /UsersAdmin
        public async Task<ActionResult> Index()
        {
            return View(await _context.ApplicationUser.ToListAsync());
        }

        
        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user);

            return View(user);
        }

        /*Not going to user their create method. I already did all that work with the register methods.

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        */
        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        //
        // POST: /Users/Edit/5
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("Email,otherfirleds")] EditUserViewModel editUser)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(editUser.Email);
                if (user == null)
                {
                    return NotFound();
                }

                int supplierID=123456789; string removeRole; string newRole;
                //int.TryParse(editUser.SupplierList.First(s => s.Selected).Value, out supplierID);
                if (supplierID == GlobalCache.GetTexasTRCompanyID(_context))
                {
                    removeRole = Constants.Roles.Supplier;
                    newRole = Constants.Roles.InternalUser;
                    user.EmployerID = null;
                }
                else
                {
                    removeRole = Constants.Roles.InternalUser;
                    newRole = Constants.Roles.Supplier;
                    user.EmployerID = supplierID;
                }

                await UserManager.AddToRoleAsync(user, newRole);
                await UserManager.RemoveFromRoleAsync(user, removeRole);

                user.IsDisabled = editUser.IsDisabled;
                user.EmailConfirmed = editUser.EmailConfirmed;
                await UserManager.UpdateAsync(user);

                return RedirectToAction("Index");
            }
            return View();
        }*/
        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty,"Failed to delete user "+id);
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
