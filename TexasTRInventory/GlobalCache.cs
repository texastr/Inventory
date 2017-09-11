using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
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
        private static KeyVaultClient keyVaultClient;
        private static Dictionary<string, string> secrets;
        
        static GlobalCache()
        {
            secrets = new Dictionary<string, string>();
            keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
        }


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

        static async public Task<String> GetSendGridKey()
        {
            return await GetSecret("SendGridKey");
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
