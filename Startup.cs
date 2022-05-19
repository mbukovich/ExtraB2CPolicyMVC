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

            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                /*options.Events.OnRedirectToIdentityProvider = context =>
                {
                    var policyID = context.Properties.Items.FirstOrDefault(x => x.Key == "policy").Value;

                    if (policyID == "B2C_1A_DEMO_CHANGESIGNINNAME")
                    {
                        options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
                    }
                    else if (policyID == "B2C_1_signupsignin1")
                    {
                        options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_signupsignin1";
                    }
                    else if (policyID == "B2C_1_PasswordReset1")
                    {
                        options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_PasswordReset1";
                    }
                    else if (policyID == "B2C_1_editProfileTest1")
                    {
                        options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_editProfileTest1";
                    }
                    return Task.CompletedTask;
                };*/
                
            });

            // Create another authentication scheme to handle extra custom policy
            services.AddAuthentication()
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C2"), "B2C2", "cookiesB2C");
            
            services.Configure<OpenIdConnectOptions>("B2C2", options =>
                {
                    // Configuration.Bind("AzureAdB2C", options);
                    options.CallbackPath = "/signin-oidc-custom";
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
    }
}
