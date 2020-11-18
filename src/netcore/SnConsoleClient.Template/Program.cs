using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Client;
using SenseNet.Client.Authentication;
using SenseNet.Extensions.DependencyInjection;

namespace SnConsoleClient
{
    class Program
    {
        private static ServiceProvider Services;
        private static SenseNetOptions Configuration;
        private static TokenStore TokenStore;
        private static ILogger Logger;

        static async Task Main(string[] args)
        {
            await InitializeAsync();

            Console.WriteLine("Hello sensenet!");

            try
            {
                // access the repository:
                // await Content.LoadAsync()
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }
        }

        private static async Task InitializeAsync()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Services = new ServiceCollection()
                .Configure<SenseNetOptions>(config.GetSection("sensenet"))
                .AddSenseNetClientTokenStore()
                .AddLogging(logging => logging.AddConsole())
                .BuildServiceProvider();

            Configuration = Services.GetService<IOptions<SenseNetOptions>>().Value;
            TokenStore = Services.GetService<TokenStore>();
            Logger = Services.GetService<ILogger<Program>>();

            if (string.IsNullOrEmpty(Configuration.RepositoryUrl))
            {
                Logger.LogWarning("Repository url is not configured.");
                return;
            }

            var server = new ServerContext
            {
                Url = Configuration.RepositoryUrl,
                IsTrusted = true
            };
            
            server.Authentication.AccessToken = await TokenStore.GetTokenAsync(server,
                Configuration.Authentication.ClientId,
                Configuration.Authentication.ClientSecret).ConfigureAwait(false);

            if (string.IsNullOrEmpty(server.Authentication.AccessToken))
                Logger.LogWarning("Access token could not be retrieved for the repository.");

            ClientContext.Current.AddServer(server);
        }
    }
}
