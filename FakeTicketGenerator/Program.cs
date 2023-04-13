using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using SQLSyncModule2.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeTicketGenerator
{
    internal class Program
    {
        static int counter;
        static System.Timers.Timer timer2 = new System.Timers.Timer();

        static string _fakeJson;
     
        static void Main(string[] args)
        {
            // Initialize IoT Module Client
            Init().Wait();

            // load fake json from file
            _fakeJson = File.ReadAllText("fake.json");


            // Start the timer for generating fake tickets
            timer2.Interval = 5000;
            timer2.Elapsed += (sender, e) => GenerateFakeTickets(sender);   

            // host a wep api to toggle the fake ticket generation
            var app = WebApplication.Create(args);
            app.MapGet("/toggle", () => Toggle());
            app.Run("http://localhost:15959");
            
            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }


        /// <summary>
        /// Simply toggles the fake ticket generation timer on or off based on current state
        /// </summary>
        /// <returns></returns>
        private static string Toggle()
        {
            if (timer2.Enabled) {
                timer2.Stop();
                return "Fake ticket generation stopped";
            }
            else
            {
                timer2.Start();
                return "Fake ticket generation started";
            }
        }


        /// <summary>
        /// Inserts fake tickets into the database 
        /// </summary>
        /// <param name="state"></param>
        private static void GenerateFakeTickets(object state)
        {                    
            // Create random number of new tickets
            TicketContext ticketContext = new TicketContext();
            var rand = new Random().Next(1, 20);
            for (int i = 0; i < rand; i++)
            {
                // for now, just create a new ticket record
                ticketContext.Tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    StoreId = 1,
                    Json = _fakeJson,
                    Status = "New"
                });
                ticketContext.SaveChangesAsync();
            }            
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
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            //// Register callback to be called when a message is received by the module
            //await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
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
    }
}
