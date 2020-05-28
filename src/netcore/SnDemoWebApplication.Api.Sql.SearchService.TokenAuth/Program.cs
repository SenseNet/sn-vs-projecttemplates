using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.ContentRepository;

namespace SnDemoWebApplication.Api.Sql.SearchService.TokenAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();
            var config = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            var environment = host.Services.GetService(typeof(IHostEnvironment)) as IHostEnvironment;

            var repositoryBuilder = Startup.GetRepositoryBuilder(config, environment);

            using (Repository.Start(repositoryBuilder))
            {
                host.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
