using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SensewareRelayv22021
{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }
    class RESTClient
    {
        public string endPoint { get; set; }
        public httpVerb httpMethod { get; set; }
        public string postJSON { get; set; }
        public string ContentTType { get; set; }
        public string HeaderPayload { get; set; }

        //Default Constructor
        public RESTClient()
        {
            //DEFAULT SETTTINGS
            endPoint = "";
            httpMethod = httpVerb.GET;
            ContentTType = "application/json";
            HeaderPayload = null;

        }

        public string makeRequest()
        {
            string strResponseValue = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(endPoint);

            request.Method = httpMethod.ToString();

            //this is where we write to API 22:18
            //we are adding header to 
            if (HeaderPayload != null)
            {
                //request.Headers.Add("header", HeaderPayload);
                request.Headers.Add("Authorization", "Bearer " + HeaderPayload);
                HeaderPayload = null;
            }

            //request.Method == "POST" && postJSON != string.Empty
            if ((request.Method == "PUT" || request.Method == "PATCH" || request.Method == "POST") && postJSON != string.Empty)
            {
                //request.ContentType = "application/json";
                //request.ContentType = "application/x-www-form-urlencoded";
                request.ContentType = ContentTType;

                using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
                {
                    swJSONPayload.Write(postJSON);
                    //close is not needed
                    swJSONPayload.Close();
                }

            }
            //Lets read the message response
            //https://dotnetplaybook.com/posting-to-a-rest-api-with-c/
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();


                //Proecess the resopnse stream... (could be JSON, XML or HTML etc..._

                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //We catch non Http 200 responses here.
                strResponseValue = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";
            }
            finally
            {
                if (response != null)
                {
                    ((IDisposable)response).Dispose();
                }
            }

            return strResponseValue;

        }
    }

}
