using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.Configuration;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Security.Messaging.RabbitMQ;

namespace SnWebApplication.Mvc.Sql.SearchService.LocalUserStore
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // [sensenet]: Identity store
            services.AddSenseNetIdentity("/Root/IMS/BuiltIn/Portal")
                .AddDefaultUI();

            services.AddControllersWithViews();
            services.AddRazorPages();

            // [sensenet]: add sensenet services
            services.AddSenseNet(Configuration, (repositoryBuilder, provider) =>
            {
                repositoryBuilder
                    .UseSecurityDataProvider(
                        new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                    .UseSecurityMessageProvider(new RabbitMQMessageProvider())
                    .UseLucene29CentralizedSearchEngineWithGrpc(Configuration["sensenet:search:service:address"], options =>
                    {
                        if (!Environment.IsDevelopment())
                            return;

                        // trust the server in a development environment
                        options.HttpClient = new HttpClient(new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                        });
                        options.DisposeHttpClient = true;
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
