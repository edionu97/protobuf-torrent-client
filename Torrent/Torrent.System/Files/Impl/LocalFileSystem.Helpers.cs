using System;
using Google.Protobuf;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Torrent.System.Files.Impl
{
    public partial class LocalFileSystem
    {
        /// <summary>
        /// Creates an instance of the file info
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        /// <param name="fileData">the file data</param>
        /// <param name="chunks">the file chunks</param>
        /// <returns>a new instance of file that contains all the previous pieces of information</returns>
        private static FileInfo CreateFileInfo(
            string fileName, byte[] fileData, IEnumerable<ChunkInfo> chunks)
        {
            //compute the md5
            using var md5 = MD5.Create();

            //create the fileInfo object
            var fileInfo = new FileInfo
            {
                Filename = fileName,
                Size = (uint)fileData.Length,
                Hash = ByteString
                    .CopyFrom(md5.ComputeHash(fileData))
            };

            //add items in repeated field
            fileInfo.Chunks.AddRange(chunks);
            return fileInfo;
        }

        /// <summary>
        /// Splits the bytes into chunks using the ChunkSize 
        /// </summary>
        /// <param name="fileData">the data that will be chunked</param>
        /// <returns>a list of chunks that represents the file chunks</returns>
        public IEnumerable<ChunkInfo> ChunkData(List<byte> fileData)
        {
            //get the number of chunks
            var numberOfChunks = Math.Ceiling((fileData.Count + .0) / ChunkSize);

            //iterate through chunks
            for (var chunkIdx = 0; chunkIdx < numberOfChunks; ++chunkIdx)
            {
                //get the starting and ending point for the current chunk
                var chunkStartsAt = chunkIdx * ChunkSize;
                var chunkEndsAt = Math.Min(fileData.Count, (chunkIdx + 1) * ChunkSize);

                //get the chunk
                var chunk = fileData
                    .GetRange(chunkStartsAt, chunkEndsAt - chunkStartsAt);

                //get the chunk info
                using var md5 = MD5.Create();
                yield return new ChunkInfo
                {
                    Index = (uint)chunkIdx,
                    Size = (uint)chunk.Count,
                    Hash = ByteString
                        .CopyFrom(md5.ComputeHash(chunk.ToArray()))
                };
            }
        }
    }
}
