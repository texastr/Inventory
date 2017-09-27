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
        private static CloudBlobContainer BlobContainer;
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

        static async public Task<CloudBlockBlob> GetBlob(string fileName)
        {
           if (BlobContainer == null)
            {
                string blobKey = await GetSecret("AzureBlobKey");
                StorageCredentials credentials = new StorageCredentials(Indexer("AzureName"), blobKey);
                CloudStorageAccount account = new CloudStorageAccount(credentials, true);
                CloudBlobClient client = account.CreateCloudBlobClient();
                BlobContainer = client.GetContainerReference(Indexer("ContainerName"));
            }

            return BlobContainer.GetBlockBlobReference(fileName);
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
