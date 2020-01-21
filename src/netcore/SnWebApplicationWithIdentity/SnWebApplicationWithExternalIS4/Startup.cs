using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.Configuration;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage.Data.MsSqlClient;
using SenseNet.Diagnostics;
using SenseNet.Identity.Experimental;
using SenseNet.OData;
using SenseNet.Search.Lucene29;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Services.Core;

namespace SnWebApplicationWithExternalIS4
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        private IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // [sensenet]: Identity store
            services.AddSenseNetIdentity(
                "/Root/IMS/BuiltIn/Portal",
                new[] {"/Root/IMS/BuiltIn/Portal/Administrators"})
                .AddDefaultUI();

            services.AddRazorPages();
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

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
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    //options.ClientId = "mvc";
                    options.ClientId = "localhost44317";
                    options.ClientSecret = "secret";
                    //options.ResponseType = "code";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;

                    options.Scope.Add("sensenet");
                });

            // using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
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

            // [sensenet]: Authentication
            app.UseSenseNetAuthentication();

            app.UseAuthorization();

            // [sensenet]: OData
            app.UseSenseNetOdata();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                //.RequireAuthorization();
                endpoints.MapRazorPages();
            });
        }

        internal static RepositoryBuilder GetRepositoryBuilder(IConfiguration configuration, string currentDirectory)
        {
            var repositoryBuilder = new RepositoryBuilder()
                .UseConfiguration(configuration)
                .UseLogger(new SnFileSystemEventLogger())
                .UseTracer(new SnFileSystemTracer())
                .UseAccessProvider(new UserAccessProvider())
                .UseDataProvider(new MsSqlDataProvider())
                .UseSecurityDataProvider(new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                .UseLucene29LocalSearchEngine($"{currentDirectory}\\App_Data\\LocalIndex")
                .StartWorkflowEngine(false)
                .DisableNodeObservers()
                .UseTraceCategories("Event", "Custom", "System") as RepositoryBuilder;

            Providers.Instance.PropertyCollector = new EventPropertyCollector();

            return repositoryBuilder;
        }
    }
}
