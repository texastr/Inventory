using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory
{
    public static class GlobalCache
    {
        private static NameValueCollection localConfig;
        private static IConfiguration onlineConfig;
        private static bool isOnline;
        private static CloudBlobContainer BlobContainer;

        public static void Initialize(NameValueCollection config)
        {
            localConfig = config;
            isOnline = false;
        }

        public static void Initialize(IConfiguration config)
        {
            onlineConfig = config;
            isOnline = true;
        }

        //Will make this private later, and enforce that all access is done through fields.
        public static string Indexer(string key)
        {
            return isOnline ? onlineConfig[key] : localConfig[key];
        }
        
        static async public Task<CloudBlockBlob> GetBlob(string fileName)
        {
           if (BlobContainer == null)
            {
                //TODO should be able to save this off in a field
                var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));

                var sec = await kv.GetSecretAsync("https://expkeyvault.vault.azure.net:443/secrets/AzureBlobKey/010bc658fe9442d8b43bc5621c6e5551"/*Indexer("SecretUri")*/);

                StorageCredentials credentials = new StorageCredentials(Secrets.AzureName, Secrets.AzureKey);
                CloudStorageAccount account = new CloudStorageAccount(credentials, true);
                CloudBlobClient client = account.CreateCloudBlobClient();
                BlobContainer = client.GetContainerReference("images"); //TODO I don't like hardcoding the container name
            }

            return BlobContainer.GetBlockBlobReference(fileName);
        }

        //EXP copied off the internet
        //the method that will be provided to the KeyVaultClient
        public static async Task<string> GetToken(string authority, string resource, string scope)
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
