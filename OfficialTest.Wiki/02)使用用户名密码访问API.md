在上一篇中，客户端通过发送`ClientCredentialsTokenRequest`把ClientId, ClientSecret, Scope等发送给验证服务器，验证服务器返回`TokenResponse`给Client。

在IdentityServer4中的token service实现了OAuth2.0 resource owner password grant机制。当客户端发送用户名和密码给验证服务器，验证服务器返回access token。

在验证服务器的`Config.cs`中添加两个`TestUser`类型的用户，再添加新的Client。

```
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser> {
                new TestUser{SubjectId="1", Username="alice", Password="password"},
                new TestUser{SubjectId="2", Username="bob", Password="password"}
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[] { new IdentityResources.OpenId() };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource> { new ApiResource("api1", "My Api") };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",

                    //对于Client没有互动的情况，使用clientid/secret进行验证
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // Client验证时用的密码
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // Client允许的Scopes
                    AllowedScopes = { "api1" }
                },
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedScopes={"api1"}
                }
            };
        }


    }
```

以上新的Client的AllowedGrantTypes属性值变成了`GrantTypes.ResourceOwnerPassword`。


在验证服务器的`Startup.cs`中配置：

```
var builder = services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());
```

在解决方案中添加新的控制台程序。

```
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
```