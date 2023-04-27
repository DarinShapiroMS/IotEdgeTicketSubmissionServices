using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SQLSyncModule2.Models;
using System;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLSyncModule2
{
    internal class Program
    {
        static int counter;

        static ModuleClient _ioTHubModuleClient;

        // used to poll the database for changes on a timer
        static Timer _syncTimer;


        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Initialize IoT Module Client
            Init().Wait();

            // setup a timer to query the database for changes
            // defaulting to 60 second interval until a module twin update changes this
            _syncTimer = new Timer(SyncDatabaseChanges, null, 0, 10000);

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }



        /// <summary>
        /// 1. Queries database for new records.
        /// 2. Sends message through IoT hub for each
        /// 3. Updates high water mark for each message sent
        /// </summary>
        /// <param name="state"></param>
        private static void SyncDatabaseChanges(object state)
        {

            TicketContext ticketContext = new();

            // query the database for changes start with last synced record time
            var lastSynced = ticketContext.LastSynced.FirstOrDefault();
            if(lastSynced == null)
            {
                // if no records have been synced, get all records
                try
                {
                    var tickets = ticketContext.Tickets.ToList();
                    var ticketCount = tickets.Count;
                    foreach (var ticket in tickets)
                    {
                        using Message message = SendIoTHubMessage(ticket);
                        
                        lastSynced = new LastSynced();
                        lastSynced.LastSyncedDateTime = ticket.TransactionDate;
                        ticketContext.LastSynced.Add(lastSynced);
                        ticketContext.SaveChanges();
                    }

                    Console.WriteLine($"Synced {ticketCount} records");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error writing to database when lastsynced == null \r\n{e.Message}");
                }
            }
            else
            {
                try
                {
                    var tickets = ticketContext.Tickets.Where(t => t.TransactionDate > lastSynced.LastSyncedDateTime).ToList();
                    var ticketCount = tickets.Count;
                    foreach (var ticket in tickets)
                    {
                        using Message message = SendIoTHubMessage(ticket);

                        lastSynced.LastSyncedDateTime = ticket.TransactionDate;
                        //ticketContext.LastSynced.Update(lastSynced);
                        ticketContext.SaveChanges();
                    }
                    Console.WriteLine($"synced {ticketCount} records");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error writing to database when lastsynced != null \r\n{e.Message}");
                }
            }                      
        }

        private static Message SendIoTHubMessage(Ticket ticket)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ticket)))
            {
                ContentEncoding = Encoding.UTF8.ToString(),
                ContentType = "application/json",

            };

            _ioTHubModuleClient.SendEventAsync("output1", message).Wait();
            return message;
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
        /// Initializes the ModuleClient and sets up the callbacks for module twin
        /// and input messages.
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            _ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await _ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await _ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, _ioTHubModuleClient);

            // Register callback when a module twin update is received
            await _ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

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
                using (var pipeMessage = new Message(messageBytes))
                {
                    foreach (var prop in message.Properties)
                    {
                        pipeMessage.Properties.Add(prop.Key, prop.Value);
                    }
                    await moduleClient.SendEventAsync("output1", pipeMessage);

                    Console.WriteLine("Received message sent");
                }
            }
            return MessageResponse.Completed;
        }


        /// <summary>
        /// This method is called whenever the module twin's desired properties are updated.  Currently,
        /// the only desired property supported is SYNC_INTERVAL_SECONDS, which is used to update the 
        /// interval the module polls the database for changes.
        /// </summary>
        /// <param name="desiredProperties"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        private static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties["SYNC_INTERVAL_SECONDS"] != null)

                    // ensure data type and range (int between 1 and 3600)
                    if (desiredProperties["SYNC_INTERVAL_SECONDS"] is int syncIntervalSeconds && syncIntervalSeconds >= 1 && syncIntervalSeconds <= 3600)
                    {
                        // update the timer interval
                        int intervalInMs = syncIntervalSeconds * 1000;
                        _syncTimer.Change(0, intervalInMs);
                        Console.WriteLine($"Module twin update set sync interval to {syncIntervalSeconds} seconds");
                    }
                    else
                    {
                        Console.WriteLine("Invalid desired property value for SYNC_INTERVAL_SECONDS. Valid range is 1-3600");
                    }
                    

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }

        private static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<TicketContext>(ServiceLifetime.Singleton);
            
        }
    }
}
