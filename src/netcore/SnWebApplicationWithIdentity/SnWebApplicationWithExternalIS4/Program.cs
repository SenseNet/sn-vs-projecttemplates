using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SnWebApplicationWithExternalIS4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();
            var config = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;

            var repositoryBuilder = Startup.GetRepositoryBuilder(config, Environment.CurrentDirectory);

            using (Repository.Start(repositoryBuilder))
            {
                SnTrace.EnableAll();
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
