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
