using API.AuthRequirement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            // To return 401
            services.AddAuthentication(config =>
            {
                config.RequireAuthenticatedSignIn = false;
                config.DefaultChallengeScheme = "Server";
            })
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
                });

            services.AddAuthorization(config =>
            {
                AuthorizationPolicyBuilder defaultAuthBuilder = new AuthorizationPolicyBuilder();
                AuthorizationPolicy defaultAuthPolicy = defaultAuthBuilder
                    .AddRequirements(new JwtRequirement())
                    .Build();
                config.DefaultPolicy = defaultAuthPolicy;
            });

            // To make call to Auth-service
            services.AddHttpClient()
                .AddHttpContextAccessor(); // To access HttpContext

            services.AddScoped<IAuthorizationHandler, JwtRequirementHandler>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            // To return 401
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
