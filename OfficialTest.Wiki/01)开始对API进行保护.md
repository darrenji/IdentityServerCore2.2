## 基本概念

**什么是Client**？Client是指向IdentityServer4和API发出请求的客户端，它可以是手机断、电脑端，还可以是设备端。

**什么是Resources**？有关用户身份Identity的数据(用户的claims, email, names)和API都被看作是Resources。

**Client与IdentityServer4和API如何通讯**？Client首先向IdentityServer4发出一个验证Authentication请求，IdentityServer4返回有关验证的Token,Identity Token给Client;当Client第一次向API发送请求时，首先向IdentityServer4发送一个授权Authorization请求， IdentityServer4返回有关授权的Token, Authorization Token；Client再拿着这个Authorization Token向API发送请求。

**IdentityServer4是什么**？当对请求验证Authentication的时候，有OpenID Connect Protocol, 当对请求授权Authorization的时候，有OAuth Protocol, 实际上OpenID Connect Protocol是在OAuth Protocol上的扩展，而IdentityServer4是对OpenID Connect Protocol和OAuth Protocol的封装。IdentityServer4中包含Identity Provider, Authorization Server和Token Service。

**IdentityServer4的几张数据库表**：

- ApiClaims: 理解成与用户相关的Claim
- ApiResources:理解成API
- ApiScope: 与API相关的，比如：order.create, order.admin
- ApiScopeClaims:与API相关的Scope下的Claim
- ApiSecrets:用来查看Token

**OAuth**

基于token的协议，规范Authorization授权，管着谁使用什么资源。OAuth的几种endpoint:

- /authorize 请求token
- /token 请求token，刷新token,使用authorization code换取token
- /revocation 吊销token

**OpenID Connect**

在OAuth基础上的扩展，规范验证Authentication,管着查看是谁。OpenID Connect的几种endpoint:

- /userinfo
- /checksession
- /endsession
- /.well-known/openid-configuration
- /.well-known/jwks

**token的有效性**

client可以向验证服务器吊销token, 但需要api主动向验证服务器查询。

**token如何保证安全**

验证给出token的时候用到了private key, 同时给外部一个public key,当Client向API请求，API会向验证服务器申请public key或者把从Client端获取的token交给验证服务器。

**token包含了什么**？

private签名，email, 权限，到期时间，是哪个authorization server发布的。

**Flow**

- redirect flow, implicit grant, 当客户第一次向client发送请求，redirect到验证服务器
- credetial flow, 包括resource owner password credentials, client credential

**scope**

- OpenID Connect:openid, profile, email, address
- API级别的scope

## 开始实践

在vs2019中创建"ASP.NET Core Web应用程序"，选择SDK的版本"ASP.NET Core 2.2"，选择"空"模板。

安装`IdentityServer4`包。

IdentityServer需要对所有的Client和Resources进行管理。创建`Config.cs`用来管理ClientResourcess。

```
using IdentityServer4.Models;
using System.Collections.Generic;

namespace OfficialTest.AuthServer
{
    public static class Config
    {
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
                }
            };
        }


    }
}

```

在`Startup.cs`中配置。

```
public class Startup
{

    public void ConfigureServices(IServiceCollection services)
    {
        var builder = services.AddIdentityServer()
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiResources(Config.GetApis())
            .AddInMemoryClients(Config.GetClients());

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseIdentityServer();

    }
}
```

此时已经有了一个有关IdentityServer4的配置文件openid-configuration，地址是：`[openid-configuration](http://localhost:5000/.well-known/openid-configuration)`

```
{
    "issuer": "http://localhost:5000",
    "authorization_endpoint": "http://localhost:5000/connect/authorize",
    "token_endpoint": "http://localhost:5000/connect/token",
    "userinfo_endpoint": "http://localhost:5000/connect/userinfo",
    "end_session_endpoint": "http://localhost:5000/connect/endsession",
    "check_session_iframe": "http://localhost:5000/connect/checksession",
    "revocation_endpoint": "http://localhost:5000/connect/revocation",
    "introspection_endpoint": "http://localhost:5000/connect/introspect",
    "device_authorization_endpoint": "http://localhost:5000/connect/deviceauthorization",
    "frontchannel_logout_supported": true,
    "frontchannel_logout_session_supported": true,
    "backchannel_logout_supported": true,
    "backchannel_logout_session_supported": true,
    "scopes_supported": [
        "openid",
        "api1",
        "offline_access"
    ],
    "claims_supported": [
        "sub"
    ],
    "grant_types_supported": [
        "authorization_code",
        "client_credentials",
        "refresh_token",
        "implicit",
        "urn:ietf:params:oauth:grant-type:device_code"
    ],
    "response_types_supported": [
        "code",
        "token",
        "id_token",
        "id_token token",
        "code id_token",
        "code token",
        "code id_token token"
    ],
    "response_modes_supported": [
        "form_post",
        "query",
        "fragment"
    ],
    "token_endpoint_auth_methods_supported": [
        "client_secret_basic",
        "client_secret_post"
    ],
    "subject_types_supported": [
        "public"
    ],
    "id_token_signing_alg_values_supported": [
        "RS256"
    ],
    "code_challenge_methods_supported": [
        "plain",
        "S256"
    ]
}
```

据说有一个`tempkey.rsa`文件，在哪里呢？暂时没找到。

在当前解决方案下创建一个API项目。把"Properties"下的`launchSettings.json`文件中的`applicationUrl`设置成：

```
"applicationUrl": "http://localhost:5002",
```

在API项目下创建一个控制器。

```
[Route("identity")]
[Authorize]
public class IdentityController : Controller
{
    public IActionResult Get()
    {
        return new JsonResult(from c in User.Claims select new { c.Type, c.Value});
    }
}
```

告诉这个API项目验证服务器在哪里，注入验证授权服务。在`Startup.cs`中设置。

```
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options=> {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.Audience = "api1"; //这里和验证服务器中的 new ApiResource("api1", "My Api")对应
                });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
```

在验证服务器项目的`bin\Debug\netcoreapp2.2`运行：

```
dotnet.exe OfficialTest.AuthServer.dll
```

在API项目的`bin\Debug\netcoreapp2.2`运行：
```
 dotnet.exe OfficialTest.API.dll
```

出现报错：

```
Failed to bind to address http://127.0.0.1:5000: address already in use.
```

修改`Program.cs`
```
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("127.0.0.1:5002");
```

再次报错：

```
System.FormatException: Invalid url: '127.0.0.1:5002'
```

再次修改`Program.cs`
```
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://127.0.0.1:5002");
```

现在请求`http://localhost:5002/identity`,会返回`401 Unauthorized`,这说明API已经受IdentityServer4保护了。

**以上有了验证服务器，有了API，接下来应该是Client了**。

在解决方案下创建一个控制台项目。添加对`IdentityServer4`的引用。

```
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
```

把API项目、IdentityServer4服务器开启，启用控制台程序：

```
dotnet.exe OfficialTest.Client.dll
```

出现报错：

```
keyset is missing
```

在验证服务器`Startup.cs`中修改配置如下：

```
public void ConfigureServices(IServiceCollection services)
{
    var builder = services.AddIdentityServer()
        .AddDeveloperSigningCredential()
        .AddInMemoryIdentityResources(Config.GetIdentityResources())
        .AddInMemoryApiResources(Config.GetApis())
        .AddInMemoryClients(Config.GetClients());

}
```

控制台返回的结果大致是：

```
{
    "access_token":"",
    "expires_in":3600,
    "token_type": "Bearer"
}
[
    {
        "type":"nbf",
        "value": ""
    },
    {
        "type":"exp",
        "value":""
    },
    {
        "type":"iss",
        "value":"http://localhost:5000"
    },
    {
        "type":"aud",
        "value":"http://localhost:5000/resources"
    },
    {
        "type":"aud",
        "value":"api1"
    },
    {
        "type":"client_id",
        "value": "client"
    },
    {
        "type":"scope",
        "value":"api1"
    }
]
```

以上说明：

- 验证服务器准备了`DiscoveryResponse`，从中可以获取到一些基本信息
- 验证服务器准本了`TokenResponse`，返回有关token的一切
- 在API的控制器方法中通过`User.Claims`获取有关用户的Claims
- Claim可以理解为有关用户的键值对








