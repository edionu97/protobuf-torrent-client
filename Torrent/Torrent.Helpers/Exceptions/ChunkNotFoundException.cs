using System;

namespace Torrent.Helpers.Exceptions
{
    public class ChunkNotFoundException : Exception
    {
        public ChunkNotFoundException(string message)
            : base(message)
        {
            
        }
    }
}
