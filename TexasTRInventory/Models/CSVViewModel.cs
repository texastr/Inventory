using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory.Models
{

	public class CSVViewModel
    {
		[DisplayName("Delete?")]
		public bool ShouldBeDeleted { get; set; }

		public string Name { get; set; }

		[DisplayName("Download")]
        [DataType(DataType.Url)]
		public string URL { get; set; }

		[DisplayName("Last Modified")]
        [DisplayFormat(DataFormatString = "O"/*"{0:MMM dd, yyyy hh:mm:ss zzz}"*/)]
        public DateTimeOffset? Modified { get; set; }

		[DisplayName("File Size")]
		public string PrettySize { get; set; }

        public CSVViewModel()
        {
            //https://www.red-gate.com/simple-talk/dotnet/asp-net/model-binding-asp-net-core/
            //Maybe throwing in a parameterless constructor will help
        }

        public CSVViewModel(CloudBlockBlob blob)
		{
			Name = blob.Name;
            URL = blob.Uri.AbsoluteUri;
			Modified = blob.Properties.LastModified;
			PrettySize = BytesToString(blob.Properties.Length);
		}

		//https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
		static String BytesToString(long byteCount)
		{
			string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
			if (byteCount == 0)
				return "0" + suf[0];
			long bytes = Math.Abs(byteCount);
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return (Math.Sign(byteCount) * num).ToString() + suf[place];
		}
	}
}
