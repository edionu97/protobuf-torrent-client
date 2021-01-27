using Google.Protobuf;

namespace Torrent.Helpers.Helpers
{
    public partial class MessageHelpers
    {
        /// <summary>
        /// Creates a new registration message
        /// </summary>
        /// <param name="nodeOwner">the owner of the node</param>
        /// <param name="nodeIndex">the node index</param>
        /// <param name="nodePort">the node port</param>
        /// <returns>a message of type registration request</returns>
        public static Message
            CreateRegistrationRequest(
                string nodeOwner, int nodeIndex, int nodePort)
        {
            //create a registration message
            return new Message
            {
                RegistrationRequest = new RegistrationRequest
                {
                    Index = nodeIndex,
                    Owner = nodeOwner,
                    Port = nodePort
                },
                Type = Message.Types.Type.RegistrationRequest
            };
        }

        /// <summary>
        /// Create the subnet response
        /// </summary>
        /// <param name="subnetId">the subnet id</param>
        /// <returns>the subnet response</returns>
        public static Message CreateSubnetRequest(int subnetId)
        {
            return new Message
            {
                Type = Message.Types.Type.SubnetRequest,
                SubnetRequest = new SubnetRequest
                {
                    SubnetId = subnetId
                }
            };
        }

        /// <summary>
        /// Create the local search request
        /// </summary>
        /// <param name="regex">the regex of file identification</param>
        /// <returns>the local search request</returns>
        public static Message CreateLocalSearchRequest(string regex)
        {
            return new Message
            {
                Type = Message.Types.Type.LocalSearchRequest,
                LocalSearchRequest = new LocalSearchRequest
                {
                    Regex = regex
                }
            };
        }

        /// <summary>
        /// Create the chunk request message
        /// </summary>
        /// <param name="chunkIdx">the index of the chunk</param>
        /// <param name="fileHash">the hash of the chunk</param>
        /// <returns></returns>
        public static Message CreateChunkRequest(int chunkIdx, ByteString fileHash)
        {
            return new Message
            {
                ChunkRequest = new ChunkRequest
                {
                    ChunkIndex = (uint)chunkIdx,
                    FileHash = fileHash
                },
                Type = Message.Types.Type.ChunkRequest
            };
        }
    }
}
