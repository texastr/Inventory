using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Linq;
using AutoMapper;

namespace TexasTRInventory.Models
{
    public class ProductViewModel : Product
    {

		/*the retailer specific information I'll do later
        B&H,Adorama,Sammys,eBay,New Egg, Walmart, Walmart DSV,Jet.com,Vendor - Drop ship, Vendor USA,Vendor Canada
        */
		[DisplayName("Upload Your Product Photos - 上传产品图片")]
		[SufficientImages]
		public IFormFileCollection ImageFiles { get; set;} //Maybe rename?

		[DisplayName("Previously Uploaded Photos -- WTF is that in Chinese?")]
		public IList<string> OldFileURLs { get; set; }

        public ProductViewModel()
        {
            //We already learned the hard way that you need a parameterless constructor
        }

        public static async Task<ProductViewModel> FromDBProduct (ProductDBModel product)
        {

            ProductViewModel ret = Mapper.Map<ProductDBModel, ProductViewModel>(product);
            //This is not done. This may even crash.
            //Now to convert the IFP objects to strings.
            ret.OldFileURLs = await GetImageURLs(product);

            return ret;
        }

        public static async Task<IList<string>> GetImageURLs(ProductDBModel product)
        {
            int size = product.ImageFilePaths.Count;
            IList<string> ret = new List<string>(size);
            for (int i = 0; i < size; i++)
            {
                string fileName = product.ImageFilePaths?.ElementAt(i)?.FileName;
                if (!String.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        ret[i] = (await GlobalCache.GetImageBlob(fileName)).Uri.AbsoluteUri;
                    }
                    catch
                    {
                        //If we can't get the image, just return null. NBD
                        ret[i] = "";
                    }
                }
            }
            return ret;
        }

        public async Task<ProductDBModel> ToProductDB()
        {
            ProductDBModel ret = Mapper.Map<ProductViewModel, ProductDBModel>(this);//new ProductDBModel();
                        
            //now to map the IFormFiles to ImageFilePaths
            ret.ImageFilePaths = new List<FilePath>();                                                                                                     

            foreach (IFormFile file in this.ImageFiles)
            {
                FilePath uploadedImage = new FilePath() { FileName = await UploadImageWrapper(file) };
                ret.ImageFilePaths.Add(uploadedImage);
            }

            return ret;
        }

        #region uploaderHelpers
        private async Task<string> UploadImageWrapper(IFormFile upload)
        {
            if (upload != null)
            {
                if (upload.ContentType.StartsWith("image/")) //TODO have an attribute on the model check that it's an image
                {
                    try
                    {
                        string newFileName = await UploadFile(upload);
                        return newFileName;
                    }
                    catch(Exception e)
                    {
                        throw new FileUploadFailedException("Uploading the product's image files failed.", e);
                    }
                }
                else
                {
                    throw new FileUploadFailedException("You attempted to upload a file that is not an image file.");
                }
            }

            return null;
        }

        private async Task<string> UploadFile(IFormFile upload)
        {
            string newFileName = UniqueFileName(upload.FileName);
            CloudBlockBlob blob = await GlobalCache.GetImageBlob(newFileName);
            blob.Properties.ContentType = upload.ContentType;

            using (Stream intermediateMemory = new MemoryStream())
            {
                upload.CopyTo(intermediateMemory);
                intermediateMemory.Seek(0, SeekOrigin.Begin);
                blob.UploadFromStream(intermediateMemory);
            }

            return newFileName;
        }

        private string UniqueFileName(string fileName)
        {
            return String.Join("$", DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"), fileName);
        }
        #endregion


    }
    public class SufficientImagesAttribute : ValidationAttribute, IClientModelValidator
	{
		protected override ValidationResult IsValid(object value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
			IFormFileCollection input = (IFormFileCollection)value;

			if (input?.Count >= GlobalCache.MinImgFilesCnt())
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult($"A minimum of {GlobalCache.MinImgFilesCnt()} images are required.");
			}
		}

		void IClientModelValidator.AddValidation(ClientModelValidationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			MergeAttribute(context.Attributes, "data-val","true");//not sure what this line does.
			MergeAttribute(context.Attributes, "data-val-sufficientimages", "this is the error message defined in the view model class");
			MergeAttribute(context.Attributes, "data-val-sufficientimages-cnt", GlobalCache.MinImgFilesCnt().ToString());
		}

		private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
		{
			if (attributes.ContainsKey(key))
			{
				return false;
			}

			attributes.Add(key, value);
			return true;
		}
	}

    public class FileUploadFailedException : Exception
    {
        public FileUploadFailedException()
        {
        }

        public FileUploadFailedException(string message)
            : base(message)
        {
        }

        public FileUploadFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
