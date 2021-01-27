using System.Collections.Generic;

namespace Torrent.System.Files
{
    public interface IFileSystem
    {
        /// <summary>
        /// Store locally the given file and return the details about the file.
        /// </summary>
        /// <param name="uploadRequest">a structure containing the name of the file and the file data</param>
        /// <returns>
        /// an instance of upload response and the status can have one of these values
        ///   - SUCCESS if all went well, even if you have the file already
        ///   - MESSAGE_ERROR if the filename is empty (it is OK for the data to be empty)
        ///   - PROCESSING_ERROR in all other cases
        /// </returns>
        public UploadResponse UploadFileLocally(UploadRequest uploadRequest);

        /// <summary>
        /// Search locally for file names matching the regex
        /// </summary>
        /// <param name="localSearchRequest"></param>
        ///   - SUCCESS if all went well, even if there are no results
        ///   - MESSAGE_ERROR if the request regexp is invalid
        ///   - PROCESSING_ERROR in all other cases
        /// <returns>a new local search response</returns>
        public LocalSearchResponse SearchFilesLocally(LocalSearchRequest localSearchRequest);

        /// <summary>
        /// Download a file from the local node based on its hash
        /// </summary>
        /// <param name="downloadRequest">the download request</param>
        /// <returns>
        ///   - SUCCESS if you have the file
        ///   - MESSAGE_ERROR if the file hash is not 16 bytes long
        ///   - UNABLE_TO_COMPLETE if you do not have the file
        ///   - PROCESSING_ERROR in all other cases
        /// </returns>
        public DownloadResponse DownloadTheFileFromLocalFileSystem(DownloadRequest downloadRequest);

        /// <summary>
        /// Download the specified chunk.
        /// </summary>
        /// <param name="chunkRequest">the chunk request</param>
        /// <returns>
        ///   - SUCCESS if you have the chunk
        ///   - MESSAGE_ERROR if the file hash is not 16 bytes long or the index is less than zero
        ///   - UNABLE_TO_COMPLETE if you do not have the chunk
        ///   - PROCESSING_ERROR in all other cases
        /// </returns>
        public ChunkResponse SearchForChunkInLocalFileSystemAndGetResponse(ChunkRequest chunkRequest);

        /// <summary>
        ///     Replicate this file locally, if not present already.
        ///     Asks for chunks from the other nodes using ChunkRequest.
        ///     If a ChunkRequest fails, send it to another node until you tried al nodes. 
        /// </summary>
        /// <param name="fileParts">the parts that </param>
        /// <param name="filInfoMetadata"></param>
        /// <returns>
        /// Response status:
        ///   - SUCCESS if all went well, even if you have the file already
        ///   - MESSAGE_ERROR if the filename in the FileInfo is empty
        ///   - UNABLE_TO_COMPLETE if you cannot receive all the chunks from the other nodes
        ///   - PROCESSING_ERROR in all other cases
        /// NodeReplication status:
        ///   - NETWORK_ERROR if you cannot connect to the node
        ///   - MESSAGE_ERROR if the response is not parseable or has the wrong type
        ///   - Otherwise, use ChunkResponse.status
        /// </returns>
        public ReplicateResponse ReconstructFileFromChunksAndGetReplicationResponse(
            IEnumerable<(NodeReplicationStatus replicationStatus, ChunkResponse chunkResponse)> fileParts, FileInfo filInfoMetadata);

    }
}
