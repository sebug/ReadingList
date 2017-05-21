using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ReadingList
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

			app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

			app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
			{
				ClientId = Configuration["AzureAD:ClientId"],
				Authority = string.Format(CultureInfo.InvariantCulture, Configuration["AzureAd:AadInstance"], "common", "/v2.0"),
				ResponseType = OpenIdConnectResponseType.IdToken,
				PostLogoutRedirectUri = Configuration["AzureAd:PostLogoutRedirectUri"],
				Events = new OpenIdConnectEvents
				{
					OnRemoteFailure = RemoteFailure,
					OnTokenValidated = TokenValidated
				},
				TokenValidationParameters = new TokenValidationParameters
				{
					// Instead of using the default validation (validating against
					// a single issuer value, as we do in line of business apps), 
					// we inject our own multitenant validation logic
					ValidateIssuer = false,

					NameClaimType = "name"
				}
			});

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
        }

		private Task TokenValidated(TokenValidatedContext context)
		{
			/* ---------------------
            // Replace this with your logic to validate the issuer/tenant
               ---------------------       
            // Retriever caller data from the incoming principal
            string issuer = context.SecurityToken.Issuer;
            string subject = context.SecurityToken.Subject;
            string tenantID = context.Ticket.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            // Build a dictionary of approved tenants
            IEnumerable<string> approvedTenantIds = new List<string>
            {
                "<Your tenantID>",
                "9188040d-6c67-4c5b-b112-36a304b66dad" // MSA Tenant
            };
            if (!approvedTenantIds.Contains(tenantID))
                throw new SecurityTokenValidationException();
              --------------------- */

			return Task.FromResult(0);
		}

		// Handle sign-in errors differently than generic errors.
		private Task RemoteFailure(FailureContext context)
		{
			context.HandleResponse();
			context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
			return Task.FromResult(0);
		}
    }
}
