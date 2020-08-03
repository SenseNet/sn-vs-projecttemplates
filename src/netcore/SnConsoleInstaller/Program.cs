using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SenseNet.Configuration;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Data.MsSqlClient;
using SenseNet.Diagnostics;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Services.Core.Install;

namespace SnConsoleInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            var builder = new RepositoryBuilder()
                .SetConsole(Console.Out)
                .UseLogger(new SnFileSystemEventLogger())
                .UseTracer(new SnFileSystemTracer())
                .UseTraceCategories("System", "Event", "Repository")
                .UseConfiguration(config)
                .UseDataProvider(new MsSqlDataProvider())
                .UseSecurityDataProvider(
                    new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                .UseLucene29LocalSearchEngine(Path.Combine(Environment.CurrentDirectory, "App_Data", "LocalIndex")) as RepositoryBuilder;

            var installer = new SenseNet.Packaging.Installer(builder)
                .InstallSenseNet();

            // optional configured import folders
            foreach (var importPath in config.GetSection("sensenet:install:import").GetChildren().Select(c => c.Value))
            {
                installer.Import(importPath);
            }

            // optional configured install folders
            foreach (var installPath in config.GetSection("sensenet:install:packages").GetChildren().Select(c => c.Value))
            {
                installer.InstallPackage(installPath);
            }

        }
    }
}
