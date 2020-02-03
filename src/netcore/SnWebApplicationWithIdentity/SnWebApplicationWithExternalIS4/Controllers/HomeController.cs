using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SnWebApplicationWithExternalIS4.Models;
using SnContent = SenseNet.ContentRepository.Content;

namespace SnWebApplicationWithExternalIS4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Protected()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult HelloEdwin()
        {
            var name = "edwin";

            using (new SystemAccount())
            {
                var parentPath = "/Root/IMS/BuiltIn/Temp";
                if (!Node.Exists($"{parentPath}/{name}"))
                {
                    var parent = RepositoryTools.CreateStructure(parentPath, "OrganizationalUnit")
                                 ?? SnContent.Load(parentPath);

                    var user = new User(parent.ContentHandler)
                    {
                        Name = name,
                        LoginName = name,
                        Password = name,
                        Email = $"{name}@example.com"
                    };
                    user.Save();
                }
            }

            ViewData["Name"] = name;
            ViewData["Email"] = $"{name}@example.com";
            return View();
        }
    }
}
