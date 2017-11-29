using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TexasTRInventory.Data;

namespace TexasTRInventory
{
    public static class GlobalCache
    {
        private static NameValueCollection secondConfig;
        private static IConfiguration firstConfig;
        private static Dictionary<string,CloudBlobContainer> BlobContainers;
        private static int? TexasTRCompanyID;
        private static KeyVaultClient keyVaultClient;
        private static Dictionary<string, string> secrets;
        
        static GlobalCache()
        {
            secrets = new Dictionary<string, string>();
            keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
        }
        
        public static void Initialize(IConfiguration firstConfigParam, NameValueCollection secondConfigParam)
        {
            firstConfig = firstConfigParam;
            secondConfig = secondConfigParam;

        }

        private static string Indexer(string key)
        {
            string possibleRet = firstConfig[key];
            return !string.IsNullOrWhiteSpace(possibleRet) ? possibleRet : secondConfig[key];
        }

        public static EmailAddress GetSystemEmailAddress()
        {
            return new EmailAddress(Indexer("EmailSenderAddress"), Indexer("EmailSenderName"));
        }

        public static string GetAdminUsername()
        {
            return Indexer("AdminUsername");
        }

        static async public Task<String> GetSendGridKey()
        {
            return await GetSecret("SendGridKey");
        }

        public static bool IsDevelopment()
        {
            return Indexer("ASPNETCORE_ENVIRONMENT") == "Development";
        }

		public static int? MinImgFilesCnt()
		{
			if (int.TryParse(Indexer("MinImgFilesCnt"), out int ret))
			{
				return ret;
			}
			return 0;
		}

        static public int GetTexasTRCompanyID(InventoryContext context)
        {
            if(TexasTRCompanyID == null)
            {
                TexasTRCompanyID = context.Companies.Where(s => s.IsInternal)
                    .First().ID;
            }
            //TODO will this crash if the database is corrupted and no supplier is returned?
            return (int)TexasTRCompanyID;
        }

		//erstwhile way to get container name
		//BlobContainer = client.GetContainerReference(Indexer("ContainerName"));
		static async public Task<CloudBlockBlob> GetImageBlob(string fileName)
        {
			string containerName = Indexer("ImageContainerName");
			return await GetBlob(fileName, containerName);
        }

		static async public Task<CloudBlockBlob> GetCSVBlob(string fileName)
		{
			string containerName = Indexer("CSVContainerName");
			return await GetBlob(containerName, fileName);

		}

		static async public Task<CloudBlobContainer> GetCSVContainer()
		{
			return await GetContainer(Indexer("CSVContainerName"));
		}


		static async private Task<CloudBlobContainer> GetContainer(string containerName)
		{
			if (BlobContainers == null)
			{
				BlobContainers = new Dictionary<string, CloudBlobContainer>();
			}

			if (!BlobContainers.ContainsKey(containerName))
			{
				string blobKey = await GetSecret("AzureBlobKey");
				StorageCredentials credentials = new StorageCredentials(Indexer("AzureName"), blobKey);
				CloudStorageAccount account = new CloudStorageAccount(credentials, true);
				CloudBlobClient client = account.CreateCloudBlobClient();
				CloudBlobContainer ret = client.GetContainerReference(containerName);
				BlobContainers.Add(containerName, ret);
			}

			return BlobContainers[containerName];
		}

		static async private Task<CloudBlockBlob> GetBlob(string containerName, string fileName)
		{
			CloudBlobContainer container = await GetContainer(containerName);
			return container.GetBlockBlobReference(fileName);
		}

        //EXP 9.2.17
        static async public Task<string> GetSecret(string secretName)
        {
            //EXP 9.13.17. Handling the disabling of our vault :(
            if (secretName == Constants.SecretNames.AdminInitializer) return "fireworks";

            if (!secrets.ContainsKey(secretName))
            {
                string secretUri = "https://" + Indexer("keyVaultName") + ".vault.azure.net/secrets/" + secretName + "/";
                SecretBundle secret = await keyVaultClient.GetSecretAsync(secretUri);
                secrets.Add(secretName, secret.Value);
            }

            return secrets[secretName];
        }

        //EXP copied off the internet
        //the method that will be provided to the KeyVaultClient
        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(Indexer("ClientId"), Indexer("ClientSecret"));
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

    }
}
