using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private static readonly string connectionString = "YourAzureStorageConnectionString";

    static async Task Main(string[] args)
    {
        await BlobStorageOperations();

        await TableStorageOperations();

        await FileShareOperations();

        await QueueStorageOperations();
    }

    private static async Task BlobStorageOperations()
    {
        string containerName = "sample-container";
        string blobName = "sample.txt";
        string localFilePath = "sample.txt";

        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(localFilePath, overwrite: true);

        string downloadFilePath = "downloaded-sample.txt";
        await blobClient.DownloadToAsync(downloadFilePath);
        Console.WriteLine("Blob uploaded and downloaded successfully.");
    }

    private static async Task TableStorageOperations()
    {
        string tableName = "SampleTable";

        TableServiceClient tableServiceClient = new TableServiceClient(connectionString);

        TableClient tableClient = tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("PartitionKey", "RowKey")
        {
            { "Property1", "Value1" },
            { "Property2", "Value2" }
        };
        await tableClient.AddEntityAsync(entity);

        Pageable<TableEntity> queryResults = tableClient.Query<TableEntity>();
        foreach (TableEntity qEntity in queryResults)
        {
            Console.WriteLine($"{qEntity.RowKey}: {qEntity["Property1"]}, {qEntity["Property2"]}");
        }
        Console.WriteLine("Table created, data inserted and queried successfully.");
    }

    private static async Task FileShareOperations()
    {
        string shareName = "sampleshare";
        string fileName = "sample.txt";
        string localFilePath = "sample.txt";

        ShareServiceClient shareServiceClient = new ShareServiceClient(connectionString);

        ShareClient shareClient = shareServiceClient.GetShareClient(shareName);
        await shareClient.CreateIfNotExistsAsync();

        ShareDirectoryClient rootDir = shareClient.GetRootDirectoryClient();
        ShareFileClient fileClient = rootDir.GetFileClient(fileName);
        using FileStream uploadFileStream = File.OpenRead(localFilePath);
        await fileClient.CreateAsync(uploadFileStream.Length);
        await fileClient.UploadAsync(uploadFileStream);
        uploadFileStream.Close();

        string downloadFilePath = "downloaded-sample.txt";
        ShareFileDownloadInfo download = await fileClient.DownloadAsync();
        using FileStream downloadFileStream = File.OpenWrite(downloadFilePath);
        await download.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();
        Console.WriteLine("File uploaded and downloaded from file share successfully.");
    }

    private static async Task QueueStorageOperations()
    {
        string queueName = "samplequeue";

        QueueServiceClient queueServiceClient = new QueueServiceClient(connectionString);

        QueueClient queueClient = queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync();

        await queueClient.SendMessageAsync("First Message");
        await queueClient.SendMessageAsync("Second Message");

        QueueMessage[] retrievedMessages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);
        foreach (QueueMessage message in retrievedMessages)
        {
            Console.WriteLine($"Message: {message.MessageText}");
            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }
        Console.WriteLine("Messages added and read from queue successfully.");
    }
}
