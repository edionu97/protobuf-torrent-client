using System;
using System.Net;
using System.Net.Sockets;
using Torrent.System.Files;
using System.Threading.Tasks;
using Torrent.Helpers.AppConfig;
using Torrent.Helpers.Helpers;
using Torrent.Helpers.ExtensionMethods;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode : INode
    {
        private readonly IFileSystem _fileSystem;

        private readonly IPAddress _hubIpAddress;
        private readonly IPEndPoint _ipEndpoint;
       
        private readonly string _nodeAddress;

        public int NodeIndex { private get; set; }

        public int NodePort { private get; set; }

        /// <summary>
        /// Constructor for the node
        /// </summary>
        /// <param name="appSettings">the app settings</param>
        /// <param name="fileSystem">the file system instance</param>
        public HubNode(AppSettings appSettings, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            //destruct the app settings object
            var ( (hubAddress, hubPort), (nodeAddress, _)) 
                = appSettings;

            //set the node address
            _nodeAddress = nodeAddress;

            //get the address and the port
            _hubIpAddress = IPAddress.Parse(hubAddress);
            _ipEndpoint = new IPEndPoint(_hubIpAddress, hubPort);
        }

        public Task StartListening()
        {
            return Task.Factory.StartNew(() =>
            {
                //parse the address 
                var ipAddress = IPAddress.Parse(_nodeAddress);

                //get the listener
                var clientListener = new TcpListener(ipAddress, NodePort);

                //start the client listener
                clientListener.Start();

                //display the message
                Console.WriteLine($"Node {NodeIndex} started listening on port: {NodePort}...");

                //handle the message
                while (true)
                {
                    //accept the client
                    using var messageSocket = clientListener.AcceptSocket();
                    //process the message socket
                    HandleReceivedMessageCallback(messageSocket);
                }

                // ReSharper disable once FunctionNeverReturns
            }, TaskCreationOptions.LongRunning);
        }

        public void RegisterNodeToHub(string nodeOwner)
        {
            //create the registration message
            var registrationMessage = MessageHelpers
                .CreateRegistrationRequest(nodeOwner, NodeIndex, NodePort);

            //get the response
            var registrationResponse
                = SendMessageToHubAndGetResponse(registrationMessage);

            //get the response
            var response = registrationResponse.As<RegistrationResponse>();
            if (response.Status != Status.Success)
            {
                throw new Exception($"{response.Status} due to: {response.ErrorMessage}");
            }

            //write the message
            Console.WriteLine($"Node {NodeIndex} owned by {nodeOwner} registered to hub.\n");
        }
    }
}
