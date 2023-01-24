using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace EdgeTicketSubmissionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketSubmissionController : ControllerBase
    {
        // GET: api/<TicketSubmissionController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
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

            // TODO:  Do we need to added a time stamp for when the ticket
            // was submitted to this local ticket submission service?

            // TODO: should add authentication to this api so that only intended
            // clients can post to this API. 

            string strPayload = System.Text.Json.JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(strPayload);
            var message = new Message(bytes);

            message.ContentEncoding = "utf-8";
            message.ContentType = "application/json";

            await Program.IoTHubModuleClient.SendEventAsync("output1", message);            
        }

        // PUT api/<TicketSubmissionController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TicketSubmissionController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
