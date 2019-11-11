using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.Configuration;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage.Data.MsSqlClient;
using SenseNet.Diagnostics;
using SenseNet.OData;
using SenseNet.Search.Lucene29;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Services.Core;

namespace SnWebApplication
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();

            // sensenet section---------------------------------------------------------------------------------

            StartRepository();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSenseNetAuthentication();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
            app.UseSenseNetOdata();

            // sensenet: stop repository and release index lock file
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        private static RepositoryInstance _repository;
        private void StartRepository()
        {
            var repositoryBuilder = new RepositoryBuilder()
                .UseConfiguration(Configuration)
                .UseLogger(new SnFileSystemEventLogger())
                //.UseTracer(new SnFileSystemTracer())
                .UseAccessProvider(new UserAccessProvider())
                .UseDataProvider(new MsSqlDataProvider())
                .UseSecurityDataProvider(new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                .UseLucene29LocalSearchEngine($"{Environment.ContentRootPath}\\App_Data\\LocalIndex")
                .StartWorkflowEngine(false)
                .DisableNodeObservers()
                .UseTraceCategories("Event", "Custom", "System") as RepositoryBuilder;

            Providers.Instance.PropertyCollector = new EventPropertyCollector();

            _repository = Repository.Start(repositoryBuilder);
        }

        internal static void OnShutdown()
        {
            SnTrace.System.Write("Shutting down web application...");
            _repository?.Dispose();
        }
    }
}
