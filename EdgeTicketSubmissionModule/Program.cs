using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System.Diagnostics.Metrics;
using System.Runtime.Loader;
using System.Text;
using System.Security.Cryptography.Xml;

namespace EdgeTicketSubmissionService

{
    public class Program
    {

        static int counter;
        private static ModuleClient ioTHubModuleClient;

        internal static ModuleClient IoTHubModuleClient { get => ioTHubModuleClient; }

        public static void Main(string[] args)
        {
            // Initialize IoT Edge's module client
            Init().Wait();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();


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
            ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await IoTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register a callback to enable direct methods being called from the cloud
            await ioTHubModuleClient.SetMethodHandlerAsync("DirectMethod1", DirectMethod1, ioTHubModuleClient);


            // Register callback to be called when a message is received by the module
            await IoTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, IoTHubModuleClient);
        }

        private static Task<MethodResponse> DirectMethod1(MethodRequest methodRequest, object userContext)
        {

            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes("{\"status\": \"ok\"}"), 200);
            return Task.FromResult(methodResponse);

        }

        /// <summary>
        /// We're not using module to module messaging here yet, just simply
        /// exposing a REST api to the local device host (EFLOW vm and Windows host). 
        /// 
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