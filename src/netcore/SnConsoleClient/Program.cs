using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SenseNet.Client;
using SenseNet.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SnConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()               
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            
            var provider = new ServiceCollection()
                .AddSenseNetRepository(options =>
                {
                    config.Bind("sensenet", options);
                })
                .AddLogging(logging => logging.AddConsole())
                .BuildServiceProvider();

            var sf = provider.GetRequiredService<IServerContextFactory>();
            var server = await sf.GetServerAsync();
            
            try
            {
                //var folders = (await Content.LoadCollectionAsync("/Root/Content/SampleWorkspace", server)).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
