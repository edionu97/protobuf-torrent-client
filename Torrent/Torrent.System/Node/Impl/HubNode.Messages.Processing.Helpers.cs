using System;
using System.Linq;
using System.Collections.Generic;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode
    {
        /// <summary>
        /// This method tries to download all the required files
        /// </summary>
        /// <param name="fileInfo">the file info that contains information about all the other parts</param>
        /// <param name="nodes">the nodes that are in the same subnet as the current node</param>
        /// <returns>a list of tuples (NodeReplicationStatus and ChunkResponse)</returns>
        private IEnumerable<(NodeReplicationStatus, ChunkResponse)> DownloadFileByParts(FileInfo fileInfo, ICollection<NodeId> nodes)
        {
            //convert chunks to dictionary (to be used latter when retries the downloading)
            var chunksIdxToChunk =
                fileInfo
                    .Chunks
                    .ToDictionary(x => (int)x.Index);

            //keep track of all nodes that does not have a particular chunk
            var chunksMissingFromNodes = new Dictionary<int, ISet<int>>();

            //put all the chunks in pending list
            var pendingChunks = new List<ChunkInfo>(fileInfo.Chunks);

            //search the file chunks through all the nodes from subnet
            var downloadedFileParts = new List<(NodeReplicationStatus, ChunkResponse)>();
            do
            {
                //send the request in round robin manner to nodes from the subnet
                var taskList =
                    SendChunkRequestToAllAvailableNodes(
                        pendingChunks,
                        nodes,
                        fileInfo.Hash,
                        chunksMissingFromNodes);

                //wait until all the tasks are completed
                var nodesResponses = taskList.Select(x => x.Result).ToList();

                //assume that all the chunks will be downloaded
                pendingChunks.Clear();
                foreach (var (nodeReplicationResponse, chunkResponse) in nodesResponses)
                {
                    //get the chunk idx and the node
                    var chunkIdx = (int)nodeReplicationResponse.ChunkIndex;
                    var nodeId = nodeReplicationResponse.Node;

                    //treat the network error response
                    if (nodeReplicationResponse.Status == Status.NetworkError)
                    {
                        pendingChunks.Add(chunksIdxToChunk[chunkIdx]);
                        continue;
                    }

                    //if the status is unable to complete this means that the chunk does not exist on the node
                    if (chunkResponse.Status == Status.UnableToComplete)
                    {
                        //put back into queue the chunk
                        pendingChunks.Add(chunksIdxToChunk[chunkIdx]);

                        //mark the chunks as not present on node 
                        if (!chunksMissingFromNodes.ContainsKey(chunkIdx))
                        {
                            chunksMissingFromNodes.Add(chunkIdx, new HashSet<int>());
                        }

                        chunksMissingFromNodes[chunkIdx].Add(nodeId.Port);
                        continue;
                    }

                    //process the other cases
                    if (chunkResponse.Status != Status.Success)
                    {
                        pendingChunks.Add(chunksIdxToChunk[chunkIdx]);
                        continue;
                    }

                    //add the chunk into list
                    downloadedFileParts.Add((nodeReplicationResponse, chunkResponse));
                }

            } while (pendingChunks.Any());

            //return the file parts
            return downloadedFileParts;
        }
    }
}
