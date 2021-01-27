using System.Net.Sockets;
using Torrent.Helpers.Helpers;

namespace Torrent.System.Node.Impl
{
    public partial class HubNode
    {
        /// <summary>
        /// This method represents the function that will be called when a message arrives to a node
        /// </summary>
        /// <param name="messageSenderSocket">the socket opened with the client</param>
        private void HandleReceivedMessageCallback(Socket messageSenderSocket)
        {
            //get the message
            var message = SendHelpers.ReceiveMessage(messageSenderSocket);

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (message.Type)
            {
                //treat the upload request
                case Message.Types.Type.UploadRequest:
                    {
                        //get the upload request
                        var uploadResponse = _fileSystem
                            .UploadFileLocally(message.UploadRequest);

                        //send the upload response
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateUploadResponse(uploadResponse),
                                messageSenderSocket);

                        break;
                    }

                //treat the local search request
                case Message.Types.Type.LocalSearchRequest:
                    {
                        //get the local search request
                        var localSearchResponse = _fileSystem
                            .SearchFilesLocally(message.LocalSearchRequest);

                        //send the upload response
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateLocalSearchResponse(localSearchResponse),
                                messageSenderSocket);
                        break;
                    }

                //treat the download request
                case Message.Types.Type.DownloadRequest:
                    {
                        //get the download response
                        var downloadResponse = _fileSystem
                            .DownloadTheFileFromLocalFileSystem(message.DownloadRequest);

                        //send the download response
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateDownloadResponse(downloadResponse),
                                messageSenderSocket);
                        break;
                    }

                //treat the search request
                case Message.Types.Type.SearchRequest:
                    {
                        //get the search response
                        var searchResponse
                            = ProcessSearchRequest(message.SearchRequest);

                        //send it to the process
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateSearchResponse(searchResponse),
                                messageSenderSocket);
                        break;
                    }

                //handle the chunk request
                case Message.Types.Type.ChunkRequest:
                    {
                        //get the chunk response
                        var chunkResponse = _fileSystem
                            .SearchForChunkInLocalFileSystemAndGetResponse(message.ChunkRequest);

                        //send the response
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateChunkResponse(chunkResponse),
                                messageSenderSocket);

                        break;
                    }

                case Message.Types.Type.ReplicateRequest:
                    {
                        //get the replicate response
                        var replicateResponse =
                            ProcessReplicateRequest(message.ReplicateRequest);

                        //send the response
                        SendHelpers
                            .SendMessage(
                                MessageHelpers.CreateReplicateResponse(replicateResponse),
                                messageSenderSocket);
                        break;
                    }
            }

        }
    }
}
