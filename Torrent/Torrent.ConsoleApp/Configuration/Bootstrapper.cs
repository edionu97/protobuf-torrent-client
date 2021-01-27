using System;
using Torrent.System.Files;
using Torrent.System.Node;
using Torrent.System.Node.Impl;
using Torrent.Helpers.AppConfig;
using Torrent.System.Files.Impl;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Torrent.ConsoleApp.Configuration
{
    public static class Bootstrapper
    {
        public static IServiceProvider Load()
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appSettings.json", optional: false);
                })
                .ConfigureServices((context, services) =>
                {
                    //register the app settings
                    services
                        .AddSingleton(context.Configuration.Get<AppSettings>());

                    //register nodes file system as transient (new instance every time)
                    services
                        .AddTransient<IFileSystem, LocalFileSystem>();

                    //add the node component as transient(new instance every time)
                    services
                        .AddTransient<INode, HubNode>();

                })
                .Build()
                .Services;
        }
    }
}
