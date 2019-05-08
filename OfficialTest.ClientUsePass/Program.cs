using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OfficialTest.ClientUsePass
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var client = new HttpClient();
            var dis = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if(dis.IsError)
            {
                Console.WriteLine(dis.Error);
                return;
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest {
                Address=dis.TokenEndpoint,
                ClientId= "ro.client",
                ClientSecret="secret",
                UserName="alice",
                Password="password",
                Scope="api1"
            });

            if(tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("http://localhost:5002/identity");
            if(!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
