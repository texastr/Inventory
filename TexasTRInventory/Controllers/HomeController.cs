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

            ViewData["Message"] = "testing";

            ViewData["EXPTESTKEY"] = GlobalCache.Indexer("EXPOnlyOnLocal");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
