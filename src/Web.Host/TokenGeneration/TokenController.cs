using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Web.Host.TokenGeneration
{
    [Route("[controller]")]
    public class TokenController
    {
        private readonly IConfiguration _configuration;
        private readonly TestUserStore _testUserStore;
        private readonly ITokenCreationService _tokenCreationService;

        public TokenController(IConfiguration configuration, TestUserStore testUserStore, ITokenCreationService tokenCreationService)
        {
            _configuration = configuration;
            _testUserStore = testUserStore;
            _tokenCreationService = tokenCreationService;
        }

        [HttpPost("generate")]
        public async Task<string> GenerateToken([FromBody] GenerateToken createToken)
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
    }
}