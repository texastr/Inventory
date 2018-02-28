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
		//[SufficientImages]
		public IFormFileCollection ImageFiles { get; set;} //Maybe rename?

		[DisplayName("Previously Uploaded Photos -- WTF is that in Chinese?")]
        //[SufficientImages]
        public IList<OldImage> OldFileURLs { get; set; }

        public string IsAdminApprovedStr
        {
            get
            {
                if (this.IsAdminApproved)
                {
                    return "Approved -- TODO Get Chinese";
                }
                else
                {
                    return "Pedning Admin Approval -- CHINESE?";
                }
            }
        }

        public ProductViewModel()
        {
            //We already learned the hard way that you need a parameterless constructor
            OldFileURLs = new List<OldImage>(); //Maybe this will help?
        }

        public static async Task<ProductViewModel> FromDBProduct (ProductDBModel product)
        {

            ProductViewModel ret = Mapper.Map<ProductDBModel, ProductViewModel>(product);
            ret.OldFileURLs = await GetImageURLs(product);

            return ret;
        }

        public static async Task<IList<OldImage>> GetImageURLs(ProductDBModel product)
        {
            int size = product?.ImageFilePaths?.Count ?? 0;
            IList<OldImage> ret = new List<OldImage>(size);
            for (int i = 0; i < size; i++)
            {
                FilePath filePath = product.ImageFilePaths?.ElementAt(i);
                string fileName = filePath?.FileName;
                if (!String.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        //ret[i] = (await GlobalCache.GetImageBlob(fileName)).Uri.AbsoluteUri;
                        string URL = (await GlobalCache.GetImageBlob(fileName)).Uri.AbsoluteUri;
                        ret.Add(new OldImage() { URL = URL, FPid = filePath.FilePathId});
                    }
                    catch
                    {
                        //If we can't get the image, just return null. NBD
                        ret.Add(new OldImage(""));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Has the side effects of uploading images that shoud be uploaded
        /// </summary>
        /// <returns></returns>
        public async Task<ProductDBModel> ToProductDB(ProductDBModel oldProduct = null)
        {
            ProductDBModel ret = Mapper.Map<ProductViewModel, ProductDBModel>(this);

            //now to map the IFormFiles to ImageFilePaths
            ret.ImageFilePaths = new List<FilePath>();

            if(this.ImageFiles !=null)
            {
                foreach (IFormFile file in this.ImageFiles)
                {
                    FilePath uploadedImage = new FilePath() { FileName = await UploadImageWrapper(file) };
                    ret.ImageFilePaths.Add(uploadedImage);
                }
            }

            if (oldProduct?.ImageFilePaths != null) {
                foreach (OldImage oi in this.OldFileURLs)
                { 
                    if (!oi.ShouldBeDeleted)
                    {
                        FilePath fp = oldProduct.ImageFilePaths.FirstOrDefault(x => x.FilePathId == oi.FPid);
                        ret.ImageFilePaths.Add(fp);
                    }
                }
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

    public class OldImage
    {
        public string URL { get; set; }
        public int FPid { get; set; }
        public bool ShouldBeDeleted { get; set; }

        public OldImage(string url)
        {
            URL = url;
        }

        //Pretty sure this is never used
        /*
        public OldImage(string url, bool sd)
        {
            URL = url;
            ShouldBeDeleted = sd;
        }*/

        public OldImage() { }//I will never forget that we need an argumentless constructor
    }

    public class SufficientImagesAttribute : ValidationAttribute, IClientModelValidator
	{
        public static string ErrMsg
        {
            get { return $"A minimum of {GlobalCache.MinImgFilesCnt()} images are required."; }
        }
        protected override ValidationResult IsValid(object value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
            ProductViewModel input = (ProductViewModel)validationContext.ObjectInstance;

            int filesCnt = 0;
            if (input.OldFileURLs != null)
            {
                var nonDeletedFiles = input.OldFileURLs.Where(p => !p.ShouldBeDeleted);
                filesCnt += nonDeletedFiles.Count();
            }


            if(input.ImageFiles != null)
            {
                filesCnt += input.ImageFiles.Count();
            }

			if (filesCnt >= GlobalCache.MinImgFilesCnt())
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult(ErrMsg);
			}
		}

		void IClientModelValidator.AddValidation(ClientModelValidationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			MergeAttribute(context.Attributes, "data-val","true");//not sure what this line does.
			MergeAttribute(context.Attributes, "data-val-sufficientImages", ErrMsg);
			MergeAttribute(context.Attributes, "data-val-sufficientImages-cnt", GlobalCache.MinImgFilesCnt().ToString());
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
