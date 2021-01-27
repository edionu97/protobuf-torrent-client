using System;
using Torrent.System.Node;
using System.Threading.Tasks;
using Torrent.Helpers.AppConfig;
using System.Collections.Generic;
using Torrent.ConsoleApp.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Torrent.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //get the service provider
            var serviceProvider = Bootstrapper.Load();

            //get the app settings
            var appSettings = serviceProvider.GetService<AppSettings>();

            //register the nodes
            var listenerList = new List<Task>();
            for (var nodeIndex = 1; nodeIndex <= appSettings?.NodeCount; ++nodeIndex)
            {
                //get the node instance
                var node = serviceProvider.GetService<INode>()
                           ?? throw new ArgumentNullException();

                //set the node port and node index
                node.NodeIndex = nodeIndex;
                node.NodePort = appSettings.Nodes.NodesStartingPort + nodeIndex;

                //start the listening
                listenerList.Add(node.StartListening());

                //register the node to the hub
                node.RegisterNodeToHub(appSettings.Nodes.NodesOwner);
            }

            //block the thread to wait indefinitely
            Task.WaitAll(listenerList.ToArray());
        }
    }
}
