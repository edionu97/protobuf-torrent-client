using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using Torrent.Helpers.Exceptions;
using Torrent.Helpers.Helpers;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode
    {
        /// <summary>
        /// Sends a message to hub and receives back the response
        /// </summary>
        /// <param name="message">the message that will be send</param>
        /// <returns>the hubs response</returns>
        private Message SendMessageToHubAndGetResponse(Message message)
        {
            //create the socket
            using var hubSocket =
                new Socket(
                    _hubIpAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = true
                };

            //connect to the endpoint
            hubSocket.Connect(_ipEndpoint);

            //send the message
            SendHelpers.SendMessage(message, hubSocket);

            //return the message
            return SendHelpers.ReceiveMessage(hubSocket);
        }

        /// <summary>
        /// * It sends the chunk requests to all the nodes from the subnet (in a round robin manner)
        /// * Also if a node does not have a particular chunk, the algorithm tries to download the chunk
        ///     from the other nodes, and if the chunk cannot be downloaded from any node, then a ChunkNotFoundException    
        ///     will be thrown
        /// </summary>
        /// <param name="chunks">the chunks that need to be downloaded</param>
        /// <param name="nodes">the nodes whom we are talking with</param>
        /// <param name="fileHash">the hash of the file that contains all the chunks</param>
        /// <param name="chunksMissingFromNodes">
        /// * Represents a dictionary in which the key is the chunkIdx and the value is all the nodes (from the subnet)
        /// that does not have the desired chunk
        /// </param>
        /// <returns>a list of tasks that contain all the responses for all the concurrent requests</returns>
        private IEnumerable<Task<Tuple<NodeReplicationStatus, ChunkResponse>>> SendChunkRequestToAllAvailableNodes(
            IEnumerable<ChunkInfo> chunks,
            ICollection<NodeId> nodes,
            ByteString fileHash,
            IDictionary<int, ISet<int>> chunksMissingFromNodes)
        {
            //initialize the pending queue of chunks
            var chunkDownloadQueue = new Queue<ChunkInfo>(chunks);

            //as long as there are some items pending in the queue
            while (chunkDownloadQueue.Any())
            {
                //store the size before processing
                var beforeDownloadQueueSize = chunkDownloadQueue.Count;

                //iterate through each node
                foreach (var nodeId in nodes)
                {
                    //if there are no items in queue then quit the cycle
                    if (!chunkDownloadQueue.Any())
                    {
                        break;
                    }

                    //get the chunk index
                    var chunkIndexToProcessByNode =
                        (int)chunkDownloadQueue.Peek().Index;

                    //if the node already processed the chunk pass it to another node
                    if (chunksMissingFromNodes.ContainsKey(chunkIndexToProcessByNode)
                        && chunksMissingFromNodes[chunkIndexToProcessByNode].Contains(nodeId.Port))
                    {
                        continue;
                    }

                    //return the task
                    yield return SendChunkRequestToNode(nodeId, fileHash, (int)chunkDownloadQueue.Dequeue().Index);
                }

                //if no other node can process the chunk then the chunk is not available for the nodes from current subnet
                if (chunkDownloadQueue.Count == beforeDownloadQueueSize)
                {
                    throw new ChunkNotFoundException(
                        $"The chunk from file {fileHash} cannot be found on any nodes");
                }
            }
        }


        /// <summary>
        /// This method it is used for sending a chunk request to a specific node 
        /// </summary>
        /// <param name="node">the destination node</param>
        /// <param name="fileHash">the hash of the file</param>
        /// <param name="chunkIndex">the chunk index</param>
        /// <returns>a task with the result as a tuple of nodeReplicationStatus and chunkResponse</returns>
        private Task<Tuple<NodeReplicationStatus, ChunkResponse>> SendChunkRequestToNode(NodeId node, ByteString fileHash, int chunkIndex)
        {
            var message = MessageHelpers
                .CreateChunkRequest(chunkIndex, fileHash);

            //if the node is the current one execute the search locally
            if (node.Port == NodePort)
            {
                //chunk response
                var chunkResponse = _fileSystem
                    .SearchForChunkInLocalFileSystemAndGetResponse(message.ChunkRequest);

                //create the replication status
                var nodeReplicationStatus = new NodeReplicationStatus
                {
                    Node = node.Clone(),
                    ChunkIndex = (uint)chunkIndex,
                    ErrorMessage = chunkResponse.ErrorMessage,
                    Status = chunkResponse.Status
                };

                //create the response
                return Task
                    .FromResult(
                        Tuple.Create(nodeReplicationStatus, chunkResponse));
            }

            //get the chunk response
            return Task.Run(() =>
            {
                try
                {
                    // get the chunk response
                    var chunkResponse =
                        SendMessageToNodeAndGetResponse(message, node).ChunkResponse;

                    //create the replication status
                    var nodeReplicationStatus = new NodeReplicationStatus
                    {
                        Node = node.Clone(),
                        ChunkIndex = (uint)chunkIndex,
                        ErrorMessage = chunkResponse.ErrorMessage,
                        Status = chunkResponse.Status
                    };

                    return Task
                        .FromResult(Tuple.Create(nodeReplicationStatus, chunkResponse));
                }
                catch (Exception e)
                {
                    //treat the communication error
                    return Task
                        .FromResult(Tuple.Create<NodeReplicationStatus, ChunkResponse>(
                            new NodeReplicationStatus
                            {
                                Status = Status.NetworkError,
                                ErrorMessage = e.Message
                            }, null));
                }
            });
        }

        /// <summary>
        /// Sends message to a node and gets back the response
        /// </summary>
        /// <param name="message">the message that will be send to the node</param>
        /// <param name="toNode">the destination node</param>
        /// <returns>the response of the node</returns>
        private static Message SendMessageToNodeAndGetResponse(Message message, NodeId toNode)
        {
            //get the address and endpoint for the node
            var ipAddress = IPAddress.Parse(toNode.Host);
            var ipEndpoint = new IPEndPoint(ipAddress, toNode.Port);

            //create the node socket
            using var nodeSocket =
                new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = true
                };

            //connect to node
            nodeSocket.Connect(ipEndpoint);

            //send the message to socket
            SendHelpers.SendMessage(message, nodeSocket);

            //get the message from socket
            return SendHelpers.ReceiveMessage(nodeSocket);
        }
    }
}
