using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SenseNet.Client;
using SnWebApplication.Mvc.Client.Models;

namespace SnWebApplication.Mvc.Client.Controllers
{
    public class ContentController : Controller
    {
        private readonly IServerContextFactory _serverFactory;
        private readonly ClientOptions _options;

        public ContentController(IServerContextFactory serverFactory, IOptions<ClientOptions> options)
        {
            _serverFactory = serverFactory;
            _options = options.Value;
        }

        public async Task<IActionResult> Index(int id = 0)
        {
            Content content;

            // get the configured and authenticated server
            var server = await _serverFactory.GetServerAsync();

            if (id == 0)
            {
                // display the root
                content = await SenseNet.Client.Content.LoadAsync(_options.RootPath, server);
            }
            else
            {
                // load the current content
                content = await SenseNet.Client.Content.LoadAsync(id, server);
            }

            var children = await SenseNet.Client.Content.LoadCollectionAsync(content.Path, server);
            
            return View(new SnContent
            {
                Content = content,
                Children = children
            });
        }
    }
}
