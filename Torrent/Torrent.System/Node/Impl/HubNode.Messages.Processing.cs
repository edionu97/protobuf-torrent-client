using System;
using System.Linq;
using System.Threading.Tasks;
using Torrent.Helpers.Helpers;
using Torrent.Helpers.Exceptions;
using Torrent.Helpers.ExtensionMethods;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode
    {
        /// <summary>
        /// Process the search request and returns the search response
        /// </summary>
        /// <param name="request">the search request</param>
        /// <returns>a new instance of search response</returns>
        private SearchResponse ProcessSearchRequest(SearchRequest request)
        {
            //treat the case in which the regex is invalid
            if (!request.Regex.IsRegexValid())
            {
                return new SearchResponse
                {
                    Status = Status.MessageError,
                    ErrorMessage = "The regex is invalid"
                };
            }

            try
            {
                //get the nodes that need to talk to (excepting the current one)
                var nodes =
                    GetAllNodesFromSubnet(request.SubnetId).ToList();

                //send the request to all the nodes (excepting the current node)
                var nodeJobs = nodes
                    .Where(x => x.Port != NodePort)
                    .Select(nodeId => Task.Run(() =>
                    {
                        //get the local search response
                        var localSearchResponse =
                            SendMessageToNodeAndGetResponse(MessageHelpers.CreateLocalSearchRequest(request.Regex), nodeId)
                                .As<LocalSearchResponse>();

                        //create the result
                        var nodeSearchResult = new NodeSearchResult
                        {
                            Node = nodeId,
                            Status = localSearchResponse.Status,
                            ErrorMessage = localSearchResponse.ErrorMessage,
                        };

                        //add the file infos into search result
                        nodeSearchResult
                            .Files
                            .AddRange(
                                localSearchResponse.FileInfo.Select(x => x.Clone()));

                        return nodeSearchResult;
                    }))
                    .ToList();

                //create the search response
                var searchResponse
                    = InitializeSearchResponseWithNodeLocalFileSystemValues(
                        nodes.First(x => x.Port == NodePort),
                        request.Regex);

                //wait all the tasks to finish
                foreach (var nodeJob in nodeJobs)
                {
                    try
                    {
                        searchResponse
                            .Results
                            .Add(nodeJob.Result);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }

                //return the response
                return searchResponse;
            }
            catch (Exception e)
            {
                return new SearchResponse
                {
                    Status = Status.ProcessingError,
                    ErrorMessage = e.Message
                };
            }
        }

        /// <summary>
        /// This method processes a replicate request and saves the file locally
        /// </summary>
        /// <param name="replicateRequest">the replication request</param>
        /// <returns>an instance of replication response</returns>
        private ReplicateResponse ProcessReplicateRequest(ReplicateRequest replicateRequest)
        {
            //get the subnet id and the file info
            var subnetId = replicateRequest.SubnetId;
            var fileInfo = replicateRequest.FileInfo;

            try
            {
                //get all nodes from the subnet without the current node
                var nodes =
                    GetAllNodesFromSubnet(subnetId)
                        .ToList();

                //get the file parts ordered by chunk's indexes
                var fileParts =
                    DownloadFileByParts(fileInfo, nodes);

                return _fileSystem
                    .ReconstructFileFromChunksAndGetReplicationResponse(
                        fileParts, fileInfo);
            }
            catch (ChunkNotFoundException e)
            {
                return new ReplicateResponse
                {
                    Status = Status.UnableToComplete,
                    ErrorMessage = e.Message
                };
            }
            catch (Exception e)
            {
                return new ReplicateResponse
                {
                    ErrorMessage = e.Message,
                    Status = Status.ProcessingError
                };
            }
        }
    }
}
