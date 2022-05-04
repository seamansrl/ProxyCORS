using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CorsProxy
{
    public class MainController : ApiController
    {

        private HttpResponseMessage setCorsHeaders(HttpResponseMessage response)
        {
            response.Headers.Remove("Access-Control-Allow-Origin");
            response.Headers.Remove("Access-Control-Allow-Credentials");
            response.Headers.Remove("Origin");
            response.Headers.Remove("Connection");
            response.Headers.Remove("Keep-Alive");

            response.Headers.Add("Access-Control-Allow-Methods", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Max-Age", "86400");
            

            return response;
        }

        public HttpResponseMessage Options()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return setCorsHeaders(response);
        }

        public async Task<HttpResponseMessage> Get()
        {
            return await Proxy();
        }

        public async Task<HttpResponseMessage> Delete()
        {
            return await Proxy();
        }


        public async Task<HttpResponseMessage> Put()
        {
            return await Proxy();
        }

        public async Task<HttpResponseMessage> Head()
        {
            return await Proxy();
        }

        public async Task<HttpResponseMessage> Patch()
        {
            return await Proxy();
        }

        private Stream fixWsdl(Stream body, String host)
        {
            StreamReader reader = new StreamReader(body);
            string wsdl = reader.ReadToEnd();
            
            wsdl = wsdl.Replace("\"" + host, "\"http://" + Program.SERVER_ADDRESS + ":" + Program.SERVER_PORT + "?url=" + host);
            
            byte[] byteArray = Encoding.ASCII.GetBytes(wsdl);
            MemoryStream stream = new MemoryStream(byteArray);

            return stream;
        }

        private async Task<HttpResponseMessage> MakeRequest()
        {

            HttpRequestMessage proxyRequest;
            HttpResponseMessage proxyResponse;
            var query = HttpUtility.ParseQueryString(Request.RequestUri.Query);


            var proxyMethod = Request.Method;
            var proxyUri = new Uri(query["url"]);
            proxyRequest = new HttpRequestMessage(proxyMethod, proxyUri);

            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower().Contains("user-agent"))
                {
                    if(!Program.KEEP_USER_AGENT) continue;
                }
                var value = header.Value;
                proxyRequest.Headers.Add(header.Key, value);
            }

            proxyRequest.Headers.Host = proxyUri.Host;

            if (proxyMethod != HttpMethod.Get)
            {
                var body = await Request.Content.ReadAsStreamAsync();


                proxyRequest.Content = new StreamContent(body);
                proxyRequest.Content.Headers.ContentType = Request.Content.Headers.ContentType;
            }

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            DecompressionMethods dm = DecompressionMethods.None;

            foreach (var e in Request.Headers.AcceptEncoding)
            {
                if (e.Value.ToLower() == "gzip")
                    dm |= DecompressionMethods.GZip;

                if (e.Value.ToLower() == "deflate")
                    dm |= DecompressionMethods.Deflate;
            }

            HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = dm };
            HttpClient client = new HttpClient(handler);

            proxyResponse = await client.SendAsync(proxyRequest);

            return proxyResponse;
        }


        private async Task<HttpResponseMessage> PrepareResponse(HttpResponseMessage proxyResponse)
        {
            HttpResponseMessage response = Request.CreateResponse();

            response.StatusCode = HttpStatusCode.OK;

            foreach (var header in proxyResponse.Headers)
            {
                if (header.Key.ToLower().StartsWith("Access-Control") || header.Key.ToLower() == ("accepts")) continue;

                var value = header.Value;
                response.Headers.Add(header.Key, value);
            }

            var contenttype = proxyResponse.Content.Headers.ContentType;
            var proxyRepsonseStream = await proxyResponse.Content.ReadAsStreamAsync();

            if (proxyResponse.RequestMessage.RequestUri.AbsoluteUri.EndsWith("?wsdl"))
                proxyRepsonseStream = fixWsdl(proxyRepsonseStream, proxyResponse.RequestMessage.RequestUri.Scheme + "://" + proxyResponse.RequestMessage.RequestUri.Host);

            response.Content = new StreamContent(proxyRepsonseStream);
            response.Content.Headers.ContentType = contenttype;

            return response;

        }

        public async Task<HttpResponseMessage> Proxy()
        {
            try
            {
                var query = HttpUtility.ParseQueryString(Request.RequestUri.Query);

                if (query["url"] == null || query["url"] == "")
                {
                    var res = Request.CreateResponse();
                    var toolbox = File.ReadAllText("toolbox.html");
                    res.Content = new StringContent(toolbox);
                    res.Headers.Add("content-type", "text/html");
                    return res;
                }

                Console.WriteLine("Preparando respuesta para " + query["url"]);

                var proxyResponse = await MakeRequest();
                var response = await PrepareResponse(proxyResponse);

                Console.WriteLine("Respondiendo");

                return setCorsHeaders(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.Write(ex.StackTrace);
                Console.WriteLine("----------------------------------------");
                var res = Request.CreateResponse(HttpStatusCode.InternalServerError);

                return setCorsHeaders(res);
            }
        }
    }
}
