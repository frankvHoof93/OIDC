using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Client
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(config => {
                // Cookie Confirms Authentication (Cookie is created after token is received)
                config.DefaultAuthenticateScheme = "ClientCookie";
                // SignIn returns Cookie
                config.DefaultSignInScheme = "ClientCookie";
                // Default service for checking Authentication
                config.DefaultChallengeScheme = "Server";
                })
                .AddCookie("ClientCookie")
                .AddOAuth("Server", config =>
                {
                    // Specifies Endpoint IN THE MIDDLEWARE where callbacks are handled after authentication
                    // I.E Endpoint to return to after authentication. Setting to oauth/callback will return to initial request-uri (before redirect to login)
                    config.CallbackPath = "/oauth/callback";
                    // Identifies this client (application) as trustworthy
                    config.ClientId = "OAuthClient";
                    config.ClientSecret = "OAuthClientSecret";
                    // Redirect-Endpoint to Authenticate at for Authorization
                    config.AuthorizationEndpoint = "https://localhost:44379/oauth/login";
                    // Endpoint where Token is Access-Token can be attained
                    config.TokenEndpoint = "https://localhost:44379/oauth/token";

                    config.SaveTokens = true;

                    config.Events = new OAuthEvents()
                    { 
                        // Check Token and manually set Claims to Identity
                        // Use this if you want to check claims inside your client (use identity within client itself)
                        // See Basics/CustomRequireClaim for more on this
                        OnCreatingTicket = context =>
                        {
                            var accessToken = context.AccessToken;
                            var payload = accessToken.Split('.')[1];
                            var bytes = Convert.FromBase64String(payload);
                            string json = Encoding.UTF8.GetString(bytes);

                            var claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                            foreach (var claim in claims)
                            {
                                context.Identity.AddClaim(new Claim(claim.Key, claim.Value));
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddHttpClient();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Who are you?
            app.UseAuthentication();
            // Are you allowed?
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
