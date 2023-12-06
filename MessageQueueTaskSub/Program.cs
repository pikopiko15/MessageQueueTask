using Azure.Storage.Queues;
using Azure.Identity;
using Azure.Storage.Queues.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string queueName = "mqtaskqueues-8435f45b-c848-4e42-854f-4e69b5418c46";
        string storageAccountName = "messageueuetaskstorage";

        QueueClient queueClient = new QueueClient(
                new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
                new DefaultAzureCredential());


        Console.WriteLine("\nReceiving messages from the queue...");

        // Get messages from the queue
        QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);

        Console.WriteLine("\nPress Enter key to 'process' messages and delete them from the queue...");
        Console.ReadLine();

        // Process and delete messages from the queue
        foreach (QueueMessage message in messages)
        {
            // "Process" the message
            Console.WriteLine($"Message: {message}");

            File.WriteAllBytes(@"C:\Users\Kemal_Maralov\source\repos\MessageQueueTask\MessageQueueTaskSub\DataCapture\result.txt", Convert.FromBase64String(message.Body.ToString()));

            // Let the service know we're finished with
            // the message and it can be safely deleted.
            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }
    }
}