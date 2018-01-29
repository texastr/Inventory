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
using Microsoft.AspNetCore.Authorization;

namespace TexasTRInventory.Controllers
{
    [Authorize(Policy = Constants.PolicyNames.IsInternal)]
    public class CSVsController : Controller
	{
		//We need some blob stuff, and that should be put in using DI. But we cannot. I forgot why. But I remember that it was a formidible technical challenge
		private readonly InventoryContext _context;

		public CSVsController(InventoryContext context)
		{
			_context = context;
		}




		// GET: CSVs
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

            return View(CSVs);
		}
	
		// GET: CSVs/Create
		public async Task<IActionResult> Create()
		{
			StringBuilder file = new StringBuilder();
            //Get the column headers
            
			foreach(PropertyInfo pi in typeof(ProductDBModel).GetProperties())
			{
                //When it starts with capital ID, it makes Excel throw errors. I don't want to make my code ugly because MS Excel is stupid. But I will.
                if (pi.Name=="ID")
                {
                    file.Append("Product ID,");
                    continue;
                }

                file.Append(pi.Name);
				file.Append(',');
			}
            file.Remove(file.Length - 1, 1);

            file.Append('\n');

			foreach (ProductDBModel product in _context.Products.AsNoTracking())
			{
				AppendProduct(product, ref file);
				file.Append('\n');
			}

			file.Remove(file.Length - 1, 1);

			//Got the file in a long string. let's get it into a file
			CloudBlockBlob blob = await GlobalCache.GetCSVBlob(CSVFileName());
            blob.Properties.ContentType = "text/csv";
            blob.UploadText(file.ToString());
            
			return RedirectToAction("Index");
		}

		private void AppendProduct(ProductDBModel product, ref StringBuilder sb)
		{
			foreach (PropertyInfo pi in product.GetType().GetProperties())
			{
				var val = pi.GetValue(product);
                AppendCell(sb, val.ToString());
			}

			sb.Remove(sb.Length - 1, 1);
		}

        //Copied from https://stackoverflow.com/questions/6377454/escaping-tricky-string-to-csv-format
        private void AppendCell(StringBuilder sb, string value)
        {
            bool mustQuote = (value.Contains(',')
                            || value.Contains('\"')
                            || value.Contains('\r')
                            || value.Contains('\n'));

            if (mustQuote)
            {
                sb.Append('\"');
                foreach (char nextChar in value)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"') sb.Append('\"');
                }
                sb.Append('\"');
            }
            else
            {
                sb.Append(value);
            }
        }

        private string CSVFileName()
		{
			return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")+".csv";
		}

		//POST: CSVs
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(List<CSVViewModel> CSVList)
		{
			for(int i=CSVList.Count-1; i>=0; i--)//I'm so clever. If I iterate backwards, I can modify the list while I'm iterating.
            {
                CSVViewModel file = CSVList[i];
                if (file.ShouldBeDeleted)
                {
                    CloudBlockBlob blob = await GlobalCache.GetCSVBlob(file.Name);
                    await blob.DeleteIfExistsAsync();
                    CSVList.RemoveAt(i);
                }
            }
            return View(CSVList);
		}
	}
}
