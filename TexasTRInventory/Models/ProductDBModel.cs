using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace TexasTRInventory.Models
{
    public class ProductDBModel : Product
    {
		public virtual ICollection<FilePath> ImageFilePaths { get; set;}

		public ProductDBModel() { }

    }
}
