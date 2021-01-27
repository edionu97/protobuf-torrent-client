using System.Linq;
using Torrent.Helpers.Helpers;
using System.Collections.Generic;
using Torrent.Helpers.ExtensionMethods;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode
    {
        /// <summary>
        /// This method it is used for creating a search response with the local files that match the regex
        /// </summary>
        /// <param name="currentNode">the value of the current node</param>
        /// <param name="regex">the search regex</param>
        /// <returns>all the files that are matching the search condition (the regex)</returns>
        private SearchResponse 
            InitializeSearchResponseWithNodeLocalFileSystemValues(NodeId currentNode, string regex)
        {
            //get the files from local system
            var localSearchResult = _fileSystem
                .SearchFilesLocally(new LocalSearchRequest
                {
                    Regex = regex
                });

            //create the search response
            var searchResponse = new SearchResponse
            {
                Status = Status.Success,
                ErrorMessage = string.Empty
            };

            //create the node search result
            var nodeSearchResult = new NodeSearchResult
            {
                ErrorMessage = localSearchResult.ErrorMessage,
                Node = currentNode,
                Status = localSearchResult.Status
            };

            //add the files into the node search result
            nodeSearchResult
                .Files
                .AddRange(
                    localSearchResult
                        .FileInfo
                        .Select(x => x.Clone()));

            //add the search result into the search response
            searchResponse.Results.Add(nodeSearchResult);

            //return the response
            return searchResponse;
        }

        /// <summary>
        /// This method it is used for getting all the nodes from a the hub that are part of a subnet
        /// </summary>
        /// <param name="subnetId">the id of the subnet</param>
        /// <returns>a list of nodes that are present into the subnet</returns>
        private IEnumerable<NodeId> GetAllNodesFromSubnet(int subnetId)
        {
            //get the subnet response
            var subnetResponse =
                SendMessageToHubAndGetResponse(
                        MessageHelpers.CreateSubnetRequest(subnetId))
                    .As<SubnetResponse>();

            //get the list of nodes from that subnet
            return subnetResponse.Nodes;
        }
    }
}
