﻿using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using SenseNet.Client;
using System;
using System.Net.Http;
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
                .Build();

            var server = new ServerContext()
            {
                Url = config["sensenet:RepositoryUrl"]
            };

            server.Authentication.AccessToken = await GetTokenAsync(config);

            ClientContext.Current.AddServer(server);

            // use client Content or RESTCaller apis to manage the repository
            // var content = await Content.LoadAsync(path);
            foreach (var child in await Content.LoadCollectionAsync("/Root"))
            {
                Console.WriteLine(child.Path);
            }
        }

        private static async Task<string> GetTokenAsync(IConfiguration config)
        {
            var authority = config["sensenet:Authentication:Authority"];

            Console.WriteLine($"Requesting auth token from {authority}");

            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(authority);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return string.Empty;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = config["sensenet:Authentication:ClientId"],
                ClientSecret = config["sensenet:Authentication:ClientSecret"],
                Scope = config["sensenet:Authentication:Scope"]
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return string.Empty;
            }

            Console.WriteLine("Token request was successful.");

            return tokenResponse.AccessToken;
        }
    }
}
