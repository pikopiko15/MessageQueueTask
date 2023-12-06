using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

internal class Program
{
    static string queueName = "mqtaskqueues-" + Guid.NewGuid().ToString();
    static string storageAccountName = "messageueuetaskstorage";

    static QueueClient queueClient = new QueueClient(
            new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
            new DefaultAzureCredential());

    private static async Task Main(string[] args)
    {
        Console.WriteLine($"Creating queue: {queueName}");

        await queueClient.CreateAsync();

        using var watcher = new FileSystemWatcher(@"C:\Users\Kemal_Maralov\Desktop\images");

        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;

        watcher.Filter = "*.txt";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }

    private static async void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"Changed: {e.FullPath}");
        Console.WriteLine("\nAdding messages to the queue...");

        string filePath = e.FullPath;
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                byte[] byteArray = reader.ReadBytes((int)fileStream.Length);
                string fileContent = Convert.ToBase64String(byteArray);
                Console.WriteLine(fileContent);
                await queueClient.SendMessageAsync(fileContent);
            }
        }
        Console.WriteLine("Done");
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        Console.WriteLine(value);
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e) =>
        Console.WriteLine($"Deleted: {e.FullPath}");

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Renamed:");
        Console.WriteLine($"    Old: {e.OldFullPath}");
        Console.WriteLine($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e) =>
        PrintException(e.GetException());

    private static void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            PrintException(ex.InnerException);
        }
    }
}