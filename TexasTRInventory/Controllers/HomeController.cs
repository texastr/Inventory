using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web.Configuration;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace TexasTRInventory.Controllers
{
    public class HomeController : Controller
    {
        //EXP 9.7.17 from jpasnders blog post.
        //private readonly IConfiguration Config;

        /*public HomeController(IConfiguration c)
        {
            Config = c;
        }*/

        public IActionResult Index()
        {

            ViewData["Message"] = "testing";

            ViewData["EXPTESTKEY"] = GlobalCache.Indexer("EXPTESTKEY");
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
