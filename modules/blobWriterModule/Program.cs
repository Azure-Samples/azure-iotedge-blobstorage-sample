namespace blobWriterModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Azure.Devices.Client;

    class Program
    {
        static int counter;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                StoreMessageToLocalBlobstore(moduleClient, message, messageString);
 
                var pipeMessage = new Message(messageBytes);
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }

        private static async void StoreMessageToLocalBlobstore(ModuleClient moduleClient, Message message, string messageString)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");
            string storageContainername = Environment.GetEnvironmentVariable("storageContainername");

            BlobServiceClient blobServiceClient = null;
            BlobContainerClient Container = null;
            string sourceFile = null;

            try
            {
                blobServiceClient = new BlobServiceClient(storageConnectionString);
            }catch(Exception)
            {
                Console.WriteLine(
                   "A connection string has not been defined in the system environment variables. " +
                   "Add a environment variable named 'storageconnectionstring' with your storage " +
                   "connection string as a value.");
                return;
            }
            // Check whether the connection string can be parsed.
           
                // We definetly should check for a valid container name. Very common error is to use upper case characters
            if (storageContainername == null || storageContainername.Length == 0 /*| storageContainername.Any(char.IsUpper)*/) {
                storageContainername = "samplecontainer";
            }

            try
            {
                // Create a container called 'samplecontainer' and append a GUID value to it to make the name unique. 
                Container = blobServiceClient.GetBlobContainerClient(storageContainername);
                await Container.CreateIfNotExistsAsync();  
                Console.WriteLine("Successfully created container '{storageContainername}'");
                await Container.SetAccessPolicyAsync(PublicAccessType.Blob).ConfigureAwait(false);
                Console.WriteLine($"Permissions set");
                  

                // Create a file in your local MyDocuments folder to upload to a blob.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = "MessageContents_" + DateTime.UtcNow.ToString("yyyy-MM-ddTHHmmss") + ".txt";
                sourceFile = Path.Combine(localPath, localFileName);
                // Write text to the file.
                File.WriteAllText(sourceFile, messageString);

                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);

                    // Get a reference to the blob address, then upload the file to the blob.
                    // Use the value of localFileName for the blob name.
                BlobClient blobClient = Container.GetBlobClient(localFileName);
                await blobClient.UploadAsync(sourceFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            } finally
            {
                File.Delete(sourceFile);
            }
        }
    }
}
