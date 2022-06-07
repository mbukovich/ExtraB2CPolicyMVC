using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace ExtraB2CPolicyMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();

                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, Constants.AzureAdB2C);

            services.AddControllersWithViews().
                AddMicrosoftIdentityUI();

            services.AddRazorPages();

            //Configuring appsettings section AzureAdB2C, into IOptions
            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureAdB2C"));

            /*services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                *//*options.Events ??= new OpenIdConnectEvents();
                options.Events.OnRedirectToIdentityProvider += OnRedirectToIdentityProviderFunc;
                options.Events.OnTokenResponseReceived += OnTokenResponseReceivedFunc;
                options.Events.OnMessageReceived += OnMessageReceivedFunc;*//*
            });*/

            // Create another authentication scheme to handle extra custom policy
            services.AddAuthentication()
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C2"), "B2C2", "cookiesB2C");

            services.Configure<OpenIdConnectOptions>("B2C2", options =>
                {
                    // Configuration.Bind("AzureAdB2C", options);
                    // options.CallbackPath = "/signin-oidc-custom";
                    options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                });

            /*services.Configure<CookiePolicyOptions>("cookiesB2C", options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                    options.HandleSameSiteCookieCompatibility();

                    options.Secure = CookieSecurePolicy.Always;
                });*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Add the Microsoft Identity Web cookie policy
            app.UseCookiePolicy();
            app.UseRouting();
            // Add the ASP.NET Core authentication service
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Add endpoints for Razor pages
                endpoints.MapRazorPages();
            });
        }

        private async Task OnRedirectToIdentityProviderFunc(RedirectContext context)
        {
            // var policyID = context.Properties.Items.FirstOrDefault(x => x.Key == "policy").Value;
            var issuerAddress = context.ProtocolMessage.IssuerAddress;

            if (issuerAddress == "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/b2c_1a_demo_changesigninname/oauth2/v2.0/authorize")
            {
                context.Options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                // context.Options.ConfigurationManager.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                context.Options.Authority = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/B2C_1A_DEMO_CHANGESIGNINNAME/v2.0";
                // options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                var metaDataAddressProperty = context.Options.ConfigurationManager.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals("MetadataAddress"));
                if (metaDataAddressProperty != null)
                {
                    metaDataAddressProperty.SetValue(context.Options.ConfigurationManager, "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME");
                }
                /*if (context.Options.ConfigurationManager.GetType().GetProperty("MetadataAddress") != null) 
                {
                    context.Options.ConfigurationManager.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                }*/
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task OnTokenResponseReceivedFunc(TokenResponseReceivedContext context)
        {
            var c = context;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task OnMessageReceivedFunc(MessageReceivedContext context)
        {
            var c = context;
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
