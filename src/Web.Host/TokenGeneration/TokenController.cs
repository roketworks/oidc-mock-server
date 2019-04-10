using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Web.Host.TokenGeneration
{
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TestUserStore _testUserStore;
        private readonly ITokenCreationService _tokenCreationService;
        private readonly IResourceStore _resourceStore;
        private readonly IClientStore _clientStore;

        public TokenController(IConfiguration configuration,
                                TestUserStore testUserStore,
                                ITokenCreationService tokenCreationService,
                                IResourceStore resourceStore,
                                IClientStore clientStore)
        {
            _configuration = configuration;
            _testUserStore = testUserStore;
            _tokenCreationService = tokenCreationService;
            _resourceStore = resourceStore;
            _clientStore = clientStore;
        }

        [HttpPost]
        public async Task<string> Generate([FromBody] GenerateToken createToken)
        {
            var user = _testUserStore.FindByUsername(createToken.Username);

            var userClaims = new List<Claim>(); 
            userClaims.AddRange(user.Claims);
            userClaims.Add(new Claim("sub", user.SubjectId));
                
            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                Issuer = _configuration["token:issuerUri"],
                Lifetime = int.Parse(_configuration["token:validFor"]),
                Audiences = createToken.Audiences,
                ClientId = createToken.ClientId,
                Claims = userClaims
            };

            return await _tokenCreationService.CreateTokenAsync(token);
        }
        
        [HttpPost]
        public async Task<ActionResult> GenerateAccess([FromBody] GenerateAccessToken accessTokenRequest)
        {
            var client = await _clientStore.FindClientByIdAsync(accessTokenRequest.ClientId);
            var scopes = accessTokenRequest.Scopes.Where(requestedScope => client.AllowedScopes.Contains(requestedScope)).ToList();

            if (!scopes.Any())
            {
                return Unauthorized();
            }
            
            foreach (var scope in scopes)
            {
                client.Claims.Add(new Claim(nameof(scope), scope));
            }

            var resources = await _resourceStore.FindApiResourcesByScopeAsync(scopes);

            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                Issuer = _configuration["token:issuerUri"],
                Lifetime = int.Parse(_configuration["token:validFor"]),
                Audiences = resources.Select(x => x.Name).ToArray(),
                ClientId = accessTokenRequest.ClientId,
                Claims = client.Claims
            };

            var generatedToken = await _tokenCreationService.CreateTokenAsync(token);
            return Ok(new{AccessToken = generatedToken});
        }
    }
}