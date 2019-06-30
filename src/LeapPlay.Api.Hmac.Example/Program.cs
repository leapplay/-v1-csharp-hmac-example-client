using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LeapPlay.Api.Hmac.Example
{
    /// <summary>
    /// Example for Leap Play HMAC Authentication with Station bound APIKey / Secret 
    /// </summary>
    class Program
    {
        //The Authentication Scheme in the Header
        static string AuthorizationScheme = "amx";

        //Your API Public Key
        const string ApiPublicKey = "PublicKey goes here";

        //Your API Secret
        const string ApiSecret = "Api Secret goes here";

        //The Url that gets called with all route parameters
        const string ApiRouteUrl = "https://localhost:5001/api/v1/station/settings";

        static async Task Main(string[] args)
        {
            //Send a Get
            await Send(HttpVerb.HttpGet, ApiRouteUrl);

            //Send a Post
            await Send(
                    HttpVerb.HttpPost,
                    ApiRouteUrl,
                    new RequestStationSettingsDto()
                    {
                            DisplayName = "My Station Name",
                            Mode = StationControlMode.RemoteWithQrCode,
                            QrCode = "http://mygames.arcade.com/login&station=637d41f0-3763-4718-89de-207bc9f56a59"
                    });

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
        
        /// <summary>
        /// Creates the Authentication Header
        /// </summary>
        private class AuthenticationHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                    HttpRequestMessage request, CancellationToken cancellationToken)
            {
                string requestContentMd5AsBase64 = string.Empty;
                string requestUri = HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());
                string requestHttpMethod = request.Method.Method;

                //Timestamp as UNIX Time in Milliseconds
                string requestTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                //Create a random nonce for each request
                string nonce = Guid.NewGuid().ToString("N");

                //Checking if the request contains a body a
                if(request.Content != null)
                {
                    byte[] content = await request.Content.ReadAsByteArrayAsync();

                    //Secure message integrity by Hashing its Content and including this Hash during signature creation
                    requestContentMd5AsBase64 = Convert.ToBase64String(MD5.Create().ComputeHash(content));
                }

                //Create the string we have to sign with our secret
                string signatureString =
                        $"{ApiPublicKey}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentMd5AsBase64}";

                string authorizationValue;

                //Calculating the HMAC SHA256 and setting the Authorization Value for the Header
                using(var hmacSha256 = new HMACSHA256(Convert.FromBase64String(ApiSecret)))
                {
                    string signatureHmacSha256AsBase64 = Convert.ToBase64String(hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(signatureString)));

                    //Multiple Values for Server side authorization are required and separated by a colon
                    //Important:
                    //1. Keep the order exactly as here
                    //2. Provide the timestamp as Unix Timestamp in milliseconds
                    //3. Synchronize the senders clock, the clock-skew between sender and receiver has to be minimal  
                    authorizationValue = $"{ApiPublicKey}:{signatureHmacSha256AsBase64}:{nonce}:{requestTimeStamp}";
                }

                //Set the Authorization in the Header
                Console.WriteLine("Header Authorization Value:");
                Console.WriteLine($"{AuthorizationScheme} {authorizationValue}");
                request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, authorizationValue);

                //Send the Request and receive a Response
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
        }

        private static async Task Send<T>(HttpVerb method, string apiRouteUrl, T value)
        {
            var client = HttpClientFactory.Create(new AuthenticationHandler());
            HttpResponseMessage response;
            PrintRequest(method, apiRouteUrl);
            switch (method)
            {
                case HttpVerb.HttpGet:
                    throw new NotImplementedException("Get with body is not supported, use non generic send method");
                case HttpVerb.HttpPost:
                    response = await client.PostAsJsonAsync(apiRouteUrl, value);
                    break;
                case HttpVerb.HttpPut:
                    response = await client.PutAsJsonAsync(apiRouteUrl, value);
                    break;
                case HttpVerb.HttpDelete:
                    throw new NotImplementedException("Delete with body is not supported, use non generic send method");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await PrintResponse(response);
        }

        private static async Task Send(HttpVerb method, string apiRouteUrl)
        {
            var client = HttpClientFactory.Create(new AuthenticationHandler());
            HttpResponseMessage response;
            PrintRequest(method, apiRouteUrl);
            switch (method)
            {
                case HttpVerb.HttpGet:
                    response = await client.GetAsync(apiRouteUrl);
                    break;
                case HttpVerb.HttpPost:
                    throw new NotImplementedException("Post without body is not supported, use generic send method");
                case HttpVerb.HttpPut:
                    throw new NotImplementedException("Put without body is not supported, use generic send method");
                case HttpVerb.HttpDelete:
                    response = await client.DeleteAsync(apiRouteUrl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await PrintResponse(response);
        }

        private static void PrintRequest(HttpVerb httpMethod, string apiRouteUrl)
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("---------------------------------START-----------------------------------------");
            Console.WriteLine("Calling the API");
            Console.WriteLine($"Method: {httpMethod} | Route: {apiRouteUrl}");
        }

        private static async Task PrintResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("HTTP Status: {0}, Reason {1}.", response.StatusCode, response.ReasonPhrase);
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine(
                        "API Call Failed. HTTP Status: {0}, Reason {1}",
                        response.StatusCode,
                        response.ReasonPhrase);
            }
            Console.WriteLine("----------------------------------END------------------------------------------");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private enum HttpVerb
        {
            HttpGet,
            HttpPost,
            HttpPut,
            HttpDelete
        }
    }
}