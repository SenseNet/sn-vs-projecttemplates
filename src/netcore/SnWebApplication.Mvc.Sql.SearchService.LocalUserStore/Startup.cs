using System.ServiceModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
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
using SenseNet.Security.Messaging.RabbitMQ;
using SenseNet.Services.Core.Cors;
using SenseNet.Services.Core.Virtualization;
using SenseNet.Services.Core;
using SenseNet.Services.Wopi;

namespace SnWebApplication.Mvc.Sql.SearchService.LocalUserStore
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
            // [sensenet]: Identity store
            services.AddSenseNetIdentity("/Root/IMS/BuiltIn/Portal")
                .AddDefaultUI();

            services.AddControllersWithViews();
            services.AddRazorPages();
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

        internal static RepositoryBuilder GetRepositoryBuilder(IConfiguration configuration, string currentDirectory)
        {
            // assemble a SQL-specific repository

            var repositoryBuilder = new RepositoryBuilder()
                .UseConfiguration(configuration)
                .UseLogger(new SnFileSystemEventLogger())
                .UseTracer(new SnFileSystemTracer())
                .UseAccessProvider(new UserAccessProvider())
                .UseDataProvider(new MsSqlDataProvider())
                .UseSecurityDataProvider(new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                .UseSecurityMessageProvider(new RabbitMQMessageProvider())
                .UseLucene29CentralizedSearchEngine(
                    new NetTcpBinding(),
                    new EndpointAddress(configuration["sensenet:search:service:address"]))
                .StartWorkflowEngine(false)
                .UseTraceCategories("Event", "Custom", "System") as RepositoryBuilder;

            Providers.Instance.PropertyCollector = new EventPropertyCollector();

            return repositoryBuilder;
        }
    }
}
