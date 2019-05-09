上面几节经历了：

- client credential
- password
- OpenID Connect implicit flow, 获取到了identity token
- Hybrid Flow：同时获取identity token和access token


在验证服务器：

```
new Client
{
    ClientId = "mvc",
    ClientName = "MVC Client",
    AllowedGrantTypes = GrantTypes.Hybrid,

    ClientSecrets =
    {
        new Secret("secret".Sha256())
    },

    RedirectUris           = { "http://localhost:5002/signin-oidc" },
    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

    AllowedScopes =
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        "api1"
    },
    AllowOfflineAccess = true
};
```

在web站点`Startup.cs`中配置：

```
.AddOpenIdConnect("oidc", options =>
{
    options.SignInScheme = "Cookies";

    options.Authority = "http://localhost:5000";
    options.RequireHttpsMetadata = false;

    options.ClientId = "mvc";
    options.ClientSecret = "secret";
    options.ResponseType = "code id_token";

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Add("api1");
    options.Scope.Add("offline_access");
    options.ClaimActions.MapJsonKey("website", "website");
});
```

在web站点使用token:
```
public async Task<IActionResult> CallApi()
{
    var accessToken = await HttpContext.GetTokenAsync("access_token");

    var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var content = await client.GetStringAsync("http://localhost:5001/identity");

    ViewBag.Json = JArray.Parse(content).ToString();
    return View("json");
}
```