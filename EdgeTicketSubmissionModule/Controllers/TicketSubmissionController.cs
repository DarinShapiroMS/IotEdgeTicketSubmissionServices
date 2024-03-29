﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Client;
using System.Text.Json.Nodes;


namespace EdgeTicketSubmissionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketSubmissionController : ControllerBase
    {
        // GET: api/<TicketSubmissionController>
        // This is to verify that the controller is reachable from the edge device
        [HttpGet]
        public IEnumerable<string> Get()
        {            
            return new string[] { "TicketSubmissionController", "ConnectivityVerified" };
        }

        // GET api/<TicketSubmissionController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Posting a valid JSON document to this controller sends the JSON
        /// document to IoT Hub, where further message routing will send
        /// the JSON document to Cosmos DB. 
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async void Post([FromBody] JsonObject value)
        {
            // as long as we got a json document, send
            // upstream through IoT Hub
            
            // TODO: should do JSON validation against expected schema
            
            // TODO: should add authentication to this api so that only intended
            // clients can post to this API. 

            string strPayload = System.Text.Json.JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(strPayload);
            var message = new Message(bytes)
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json" ,                
                Properties =
                {
                    { "SubmissionTime", DateTime.UtcNow.ToString("O") }                    
                }
            };

            // added a custom property SubmissionTime above because CreationTimeUtc formats strangely

            await Program.IoTHubModuleClient.SendEventAsync("output1", message);            
        }
      
    }
}
