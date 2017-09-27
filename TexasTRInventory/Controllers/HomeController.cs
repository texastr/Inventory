using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web.Configuration;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace TexasTRInventory.Controllers
{
    [AllowAnonymous] //EXP 9.11.17
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
