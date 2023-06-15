using System.Collections.Generic;
using IdentityServer4.Models;

namespace WebStore.Authentication
{
    public class Config
    {
        public static IEnumerable<Client> GetClients() 
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "1000000",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("client-secret".Sha256())
                    },
                    AllowedScopes = { "api-resource"}
                }
            };
        }
        public static IEnumerable<ApiResource> GetApiResources() 
        {
            return new List<ApiResource>
            {
                new ApiResource("api-resource", "API Resource")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId()
            };
        }
    }
}
