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
using Microsoft.WindowsAzure.Storage.Blob;
using System.Reflection;
using System.Text;

namespace TexasTRInventory.Controllers
{

	public class CSVController : Controller
	{
		//We need some blob stuff, and that should be put in using DI. But we cannot. I forgot why. But I remember that it was a formidible technical challenge
		private readonly InventoryContext _context;

		public CSVController(InventoryContext context)
		{
			_context = context;
		}




		// GET: CSV
		public async Task<IActionResult> Index()
		{

			List<CSVViewModel> CSVs = new List<CSVViewModel>();
			CloudBlobContainer blobs = await GlobalCache.GetCSVContainer();
			foreach (IListBlobItem blob in blobs.ListBlobs())
			{
				if (blob.GetType() == typeof(CloudBlockBlob))
				{
					CSVs.Add(new CSVViewModel((CloudBlockBlob)blob));
				}
			}

			CSVs.Sort((x,y) => DateTimeOffset.Compare((DateTimeOffset)(x.Modified), (DateTimeOffset)(y.Modified)));
			CSVList csvsInTheTypeYouFuckersWant = new CSVList() { TheOnlyField = CSVs }; //This line is stupid, but necessary. If it makes the code work, it'll be worth it.
			return View(csvsInTheTypeYouFuckersWant);
		}
	
		// GET: CSVs/Create
		public async Task<IActionResult> Create()
		{
			StringBuilder file = new StringBuilder();
			//Get the column headers

			foreach(PropertyInfo pi in typeof(Product).GetProperties())
			{
				file.Append(pi.Name);
				file.Append(',');
			}

			file.Append('\n');

			foreach (Product product in _context.Products.AsNoTracking())
			{
				AppendProduct(product, ref file);
				file.Append('\n');
			}

			file.Remove(file.Length - 1, 1);

			//Got the file in a long string. let's get it into a file
			CloudBlockBlob blob = await GlobalCache.GetCSVBlob(CSVFileName());
			blob.UploadText(file.ToString());
			
			return RedirectToAction("Index");
		}

		private void AppendProduct(Product product, ref StringBuilder sb)
		{
			foreach (PropertyInfo pi in product.GetType().GetProperties())
			{
				var val = pi.GetValue(product);
				sb.Append(val);
				sb.Append(',');
			}

			sb.Remove(sb.Length - 1, 1);
		}

		private string CSVFileName()
		{
			return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
		}

		//POST: CSV
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(CSVList model)
		{
			List<CSVViewModel> CSVs = new List<CSVViewModel>();
			/*CloudBlobContainer blobs = await GlobalCache.GetCSVContainer();
			foreach (IListBlobItem blob in blobs.ListBlobs())
			{
				if (blob.GetType() == typeof(CloudBlockBlob))
				{
					CSVs.Add(new CSVViewModel((CloudBlockBlob)blob));
				}
			}

			CSVs.Sort((x, y) => DateTimeOffset.Compare((DateTimeOffset)(x.Modified), (DateTimeOffset)(y.Modified)));
			*/
			return View(CSVs);
		}
	}
}
