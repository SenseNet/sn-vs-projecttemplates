using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.Configuration;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Services.Core.Authentication;
using SenseNet.Services.Core.Authentication.IdentityServer4;

namespace SnWebApplication.Mvc.Sql.Oidc
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
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // The following authentication setup does not allow SPA clients
            // connect to this app using token authentication. Use AddJwtBearer
            // for that.

            // [sensenet]: Authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration["sensenet:authentication:authority"];
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";

                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;

                    options.Scope.Add("sensenet");
                })
                .AddDefaultSenseNetIdentityServerClients(Configuration["sensenet:authentication:authority"])
                .AddSenseNetRegistration();

            // [sensenet]: add sensenet services
            services.AddSenseNet(Configuration, (repositoryBuilder, provider) =>
            {
                repositoryBuilder
                    .UseSecurityDataProvider(
                        new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                    .UseLucene29LocalSearchEngine(Path.Combine(System.Environment.CurrentDirectory, "App_Data",
                        "LocalIndex"));
            });
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

            app.UseRouting();

            // [sensenet]: custom CORS policy
            app.UseSenseNetCors();
            // [sensenet]: use Authentication and set User.Current
            app.UseSenseNetAuthentication();
            
            app.UseAuthorization();

            // [sensenet] Add the sensenet binary handler
            app.UseSenseNetFiles();

            // [sensenet]: OData middleware
            app.UseSenseNetOdata();
            // [sensenet]: WOPI middleware
            app.UseSenseNetWopi();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}")
                    .RequireAuthorization();
            });
        }
    }
}
