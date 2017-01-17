using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobDotNetCoreWebApp.Models
{
    public class BlobUtility
    {
        public CloudStorageAccount storageAccount;

        public BlobUtility(string AccountName, string AccountKey)
        {
            string UserConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", AccountName, AccountKey);
            storageAccount = CloudStorageAccount.Parse(UserConnectionString);
        }

        public async Task<CloudBlockBlob> UploadBlob(string BlobName, string ContainerName, Stream stream)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName.ToLower());
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
            try
            {
                await blockBlob.UploadFromStreamAsync(stream);
                return blockBlob;
            }
            catch (Exception e)
            {
                var r = e.Message;
                return null;
            }
        }

        public async void DeleteBlob(string BlobName, string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
            await blockBlob.DeleteAsync();
        }

        public CloudBlockBlob DownloadBlob(string BlobName, string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
            // blockBlob.DownloadToStream(Response.OutputStream);
            return blockBlob;
        }
    }
}