OfficalTest.Web暂时无法运行下去。

官方文档不完整：https://github.com/IdentityServer/IdentityServer4.Quickstart.UI

运行程序报错：

```
InvalidOperationException: Unable to resolve service for type 'IdentityServer4.Services.IIdentityServerInteractionService'
```

在`Startup.cs`中添加：

```
services.AddIdentityServer();
```

运行继续报错：
```
InvalidOperationException: Unable to resolve service for type 'IdentityServer4.Stores.IClientStore'
```

