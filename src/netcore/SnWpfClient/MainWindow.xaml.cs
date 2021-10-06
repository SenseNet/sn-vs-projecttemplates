using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using SenseNet.Client;

namespace SnWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            const string repositoryUrl = "https://localhost:44362";

            // the clientid must refer to a client that supports the hybrid flow
            var options = new OidcClientOptions()
            {
                Authority = "https://localhost:44311/",
                ClientId = "spa",
                Scope = "openid profile sensenet",
                RedirectUri = "http://127.0.0.1/sample-wpf-app",
                Browser = new WpfEmbeddedBrowser(),
                Policy = new Policy
                {
                    RequireIdentityTokenSignature = false
                }
            };

            var oidcClient = new OidcClient(options);

            LoginResult loginResult;
            try
            {
                loginResult = await oidcClient.LoginAsync(new LoginRequest
                {
                    FrontChannelExtraParameters = new Parameters(new List<KeyValuePair<string, string>>
                    {
                        // sensenet IdentityServer requires the repository information
                        new KeyValuePair<string, string>("snrepo", repositoryUrl)
                    })
                });
            }
            catch (Exception exception)
            {
                tbTitle.Text = $"Unexpected Error: {exception.Message}";
                return;
            }

            if (loginResult.IsError)
            {
                tbTitle.Text = loginResult.Error == "UserCancel" ? "The sign-in window was closed before authorization was completed." : loginResult.Error;
                return;
            }
            else
            {
                tbTitle.Text = "Signed in";
            }

            var userIdString = loginResult.User.Claims.FirstOrDefault(cl => cl.Type == "sub")?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                tbTitle.Text = $"Unknown user id: {userIdString}.";
                return;
            }

            var server = new ServerContext
            {
                Url = repositoryUrl
            };
            server.Authentication.AccessToken = loginResult.AccessToken;

            dynamic user = await SenseNet.Client.Content.LoadAsync(userId, server);
            if (user == null)
            {
                tbTitle.Text = $"Unknown user: {userIdString}.";
                return;
            }

            lblUserIdValue.Content = (int)user.Id;
            lblUsernameValue.Content = (string)user.LoginName;
            lblPathValue.Content = (string)user.Path;

            var rootContents = await SenseNet.Client.Content.LoadCollectionAsync(new ODataRequest(server)
            {
                Path = "/Root",
                OrderBy = new []{"Path"}
            }, server);

            tbContentPathList.Text = string.Join(Environment.NewLine, rootContents.Select(c => c.Path));
        }
    }
}
