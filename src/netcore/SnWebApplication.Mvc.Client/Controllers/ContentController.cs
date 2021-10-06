using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SenseNet.Client;
using SenseNet.Client.Authentication;
using SnWebApplication.Mvc.Client.Models;

namespace SnWebApplication.Mvc.Client.Controllers
{
    public class ContentController : Controller
    {
        private readonly ITokenStore _tokenStore;

        public ContentController(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        public async Task<IActionResult> Index(int id = 0)
        {
            Content content;

            var server = await GetSnServer();

            if (id == 0)
            {
                // display the root
                content = await SenseNet.Client.Content.LoadAsync("/Root/Content/SampleWorkspace", server);
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

        private async Task<ServerContext> GetSnServer()
        {
            // define sensenet service url
            var server = new ServerContext
            {
                Url = "https://example.sensenet.cloud"
            };

            // request and set the access token
            server.Authentication.AccessToken = await _tokenStore.GetTokenAsync(server,
                "",
                "");

            return server;
        }
    }
}
