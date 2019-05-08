using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace OfficialTest.AuthServer
{
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
}
