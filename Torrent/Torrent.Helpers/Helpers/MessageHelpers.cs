namespace Torrent.Helpers.Helpers
{
    public static partial class MessageHelpers
    {
        /// <summary>
        /// Wraps the upload response into a message
        /// </summary>
        /// <param name="response">the upload response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateUploadResponse(UploadResponse response)
        {
            return new Message
            {
                UploadResponse = response,
                Type = Message.Types.Type.UploadResponse
            };
        }

        /// <summary>
        /// Wraps the local search response into a message
        /// </summary>
        /// <param name="response">the local search response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateLocalSearchResponse(LocalSearchResponse response)
        {
            return new Message
            {
                LocalSearchResponse = response,
                Type = Message.Types.Type.LocalSearchResponse
            };
        }

        /// <summary>
        /// Wraps the download response into a message
        /// </summary>
        /// <param name="response">the download response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateDownloadResponse(DownloadResponse response)
        {
            return new Message
            {
                DownloadResponse = response,
                Type = Message.Types.Type.DownloadResponse
            };
        }

        /// <summary>
        /// Wraps the search response into a message
        /// </summary>
        /// <param name="response">the upload response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateSearchResponse(SearchResponse response)
        {
            return new Message
            {
                SearchResponse = response,
                Type = Message.Types.Type.SearchResponse
            };
        }

        /// <summary>
        /// Wraps the chunk response into a message
        /// </summary>
        /// <param name="response">the chunk response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateChunkResponse(ChunkResponse response)
        {
            return new Message
            {
                ChunkResponse = response,
                Type = Message.Types.Type.ChunkResponse
            };
        }

        /// <summary>
        /// Wraps the replicate response into a message
        /// </summary>
        /// <param name="response">the replicate response</param>
        /// <returns>the wrapped message</returns>
        public static Message CreateReplicateResponse(ReplicateResponse response)
        {
            return new Message
            {
                ReplicateResponse = response,
                Type = Message.Types.Type.ReplicateResponse
            };
        }
    }
}
