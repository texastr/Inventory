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
            return View();
        }

        //
        // GET: /Home/Index2
        [HttpGet]
        public IActionResult Index2()//string arg1 = null, string arg2 = null)
        {
            //MessageBox.Show($"the first arg was {arg1} and the second one was {arg2}.");
            MessageBox.Show("hello from index2");
            return View("Index");
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
