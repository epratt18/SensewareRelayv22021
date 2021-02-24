using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SensewareRelayv22021
{
    public static class Function1
    {
        [FunctionName("SW")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Site API Variables
            //string APIClinetID = "xxxx";
            //string APIClinetSec = "XXXXXX";
            //string BOSGatewayID = "xxxx";

            //Localt time offset 1 hr = 3600 set
            //long epoctOffset = 3600 * 3;
            long epoctOffset = 0;

            //Variables
            string bOSToken;
            string Front = "", middle = "", end = "";
            //.NET logging
            log.LogInformation("C# HTTP trigger function processed a request.");

            //Get the Paramater from message
            //example ?BOSUUID=12345
            string BOSUUID = req.Query["UUID"];
            string APIClinetID = req.Query["CID"];
            string APIClinetSec = req.Query["CSec"];
            string BOSGatewayID = req.Query["GID"];
            string epoctOffsetSt = req.Query["tOffset"];
            long LSepoctoffset = Convert.ToInt64(epoctOffsetSt);
            epoctOffset = LSepoctoffset + epoctOffset;

            //string name1 = req.Query["name1"];
            //string name2 = req.Query["name2"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            /*
            name1 = name1 ?? data?.data[1].pkt;
            name1 = name1 ?? data?.data[1].value;
            name1 = data.data[1].value;
            */
            string SensorValue = data.data[0].value;
            //string SensorType = data.name;
            long epoct = data.data[0].ts;

            epoct = epoct + epoctOffset;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(epoct);
            string DateandTime = dateTimeOffset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            RESTClient rClient = new RESTClient();
            rClient.endPoint = "https://api.buildingos.com/o/token/";
            rClient.httpMethod = httpVerb.POST;
            rClient.ContentTType = "application/x-www-form-urlencoded";
            rClient.postJSON = @"client_id=" + APIClinetID + @"&client_secret=" + APIClinetSec + @"&grant_type=client_credentials";

            //Displaying the response message
            string strJSON = string.Empty;
            strJSON = rClient.makeRequest();
            //deserilzie json message
            dynamic bosdata = JsonConvert.DeserializeObject<dynamic>(strJSON);
            bOSToken = bosdata.access_token;

            //post data to gateway
            rClient.endPoint = @"https://api.buildingos.com/gateways/" + BOSGatewayID + @"/data";
            //rClient.endPoint = "http://ptsv2.com";
            rClient.httpMethod = httpVerb.POST;
            rClient.ContentTType = "application /json";
            rClient.HeaderPayload = bOSToken;

            //Constructing message to send to bOS
            Front = @"{""meta"": {""naive_timestamp_utc"": true},""data"": {""";
            middle = @""": [[""2019-10-17T13:00:0:"", 70.5],[""";
            end = @"]]}}";
            rClient.postJSON = Front + BOSUUID + middle + DateandTime + @"""," + SensorValue + end;

            //Displaying the response message
            strJSON = string.Empty;
            strJSON = rClient.makeRequest();
            //txtResponse.Text = strJSON;
            bosdata = JsonConvert.DeserializeObject<dynamic>(strJSON);

            // Response to initial message
            return (SensorValue != null)// && (name2 != null)
                ? (ActionResult)new OkObjectResult($"SensorValue / Time, {SensorValue}, {DateandTime}, {bOSToken},{bosdata}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");

        }
    }
}
