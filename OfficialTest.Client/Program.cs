using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OfficialTest.Client
{
    public class Program
    {
        private static async Task Main()
        {
            var client = new HttpClient();

            //获取验证服务器的文档
            var disc = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (disc.IsError)
            {
                Console.WriteLine(disc.Error);
                return;
            }

            //获取验证服务器的token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disc.TokenEndpoint, //比如说，对应验证服务器的http://localhost:5000/connect/token
                ClientId = "client", //对应验证服务器中的配置：new ClientClientId = "client",
                ClientSecret = "secret", //对应验证服务器中的配置 new Secret("secret".Sha256())
                Scope = "api1" //对应验证服务器中的配置AllowedScopes = { "api1" }
            });

            if(tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            //请求API
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
