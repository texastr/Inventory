using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web.Configuration;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using TexasTRInventory.Models;

namespace TexasTRInventory.Controllers
{
    [AllowAnonymous] //EXP 9.11.17
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
			//Good stop to put trash that I want to trip the debugger
			ProductViewModel pvm = new ProductViewModel() { Brand = "brand string", SKU = "sku string" };
			Product product = new Product(pvm);

			return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
