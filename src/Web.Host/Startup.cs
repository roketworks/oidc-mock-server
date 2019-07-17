using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace Web.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddCors()
                .AddApiExplorer()
                .AddJsonFormatters(); 

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info { Title = "Token Generation Api", Version = "v1" }));
            
            var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.Endpoints.EnableAuthorizeEndpoint = false;
                options.Endpoints.EnableDiscoveryEndpoint = true;
                options.Endpoints.EnableIntrospectionEndpoint = false;
                options.Endpoints.EnableTokenEndpoint = false;
                options.Endpoints.EnableCheckSessionEndpoint = false;
                options.Endpoints.EnableEndSessionEndpoint = false;
                options.Endpoints.EnableTokenRevocationEndpoint = false;
                options.Endpoints.EnableUserInfoEndpoint = false;

                options.IssuerUri = Configuration["token:issuerUri"];
            });
               
            identityServerBuilder
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryClients(new List<Client>())
                .AddTestUsers(LoadUsers());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePathBase(Configuration["pathBase"]);
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseIdentityServer();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Token Generation API"));
        }

        private List<TestUser> LoadUsers()
        {
            var users = Configuration.GetSection("users").Get<List<User>>();
            return users.Select(user => new TestUser
            {
                Username = user.Username,
                SubjectId =  user.SubjectId,
                Claims = user.Claims.Select(c => new Claim(c.Type, c.Value)).ToList()
            }).ToList();
        }

        private class User
        {
            public string Username { get; set; }
            public string SubjectId { get; set; }
            public List<UserClaim> Claims { get; set; }
        }

        private class UserClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}