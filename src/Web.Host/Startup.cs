using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .AddInMemoryApiResources(LoadResources())
                .AddInMemoryClients(LoadClients())
                .AddTestUsers(LoadUsers());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePathBase(Configuration["pathBase"]);
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseIdentityServer();
            app.UseMvc(ConfigureRoutes);
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Token Generation API"));
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            var routes = Configuration.GetSection("routes")?.Get<List<ConfiguredRoute>>();

            if (routes != null)
            {
                foreach (var configuredRoute in routes)
                {
                    var action = configuredRoute.AccessToken ? "GenerateAccess" : "Generate";
                    routeBuilder.MapRoute(configuredRoute.Name, configuredRoute.Url, new {controller = "Token", action});
                }
            }
            
            routeBuilder.MapRoute("Default", "{controller}/{action}", new {controller = "Token", action = "Generate"});
        }

        private List<TestUser> LoadUsers()
        {
            var users = Configuration.GetSection("users").Get<List<ConfiguredUser>>();
            return users.Select(user => new TestUser
            {
                Username = user.Username,
                SubjectId =  user.SubjectId,
                Claims = user.Claims.Select(c => new Claim(c.Type, c.Value)).ToList()
            }).ToList();
        }

        private List<ApiResource> LoadResources()
        {
            var resources = Configuration.GetSection("resources").Get<List<ConfiguredResource>>();
            return resources.Select(resource => new ApiResource
            {
                Name = resource.Name,
                Scopes = resource.Scopes.Select(x => new Scope(x)).ToList()
            }).ToList();
        }

        private List<Client> LoadClients()
        {
            var clients = Configuration.GetSection("clients").Get<List<ConfiguredClient>>();
            return clients.Select(client => new Client
            {
                ClientId = client.ClientId,
                AllowedScopes = client.AllowedScopes
            }).ToList();
        }
        
        private class ConfiguredRoute
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public bool AccessToken { get; set; }
        }

        private class ConfiguredClient
        {
            public string ClientId { get; set; }
            public List<string> AllowedScopes { get; set; }
        }

        private class ConfiguredResource
        {
            public string Name { get; set; }
            public List<string> Scopes { get; set; }
        }

        private class ConfiguredUser
        {
            public string Username { get; set; }
            public string SubjectId { get; set; }
            public List<ConfiguredUserClaim> Claims { get; set; }
        }

        private class ConfiguredUserClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }

}