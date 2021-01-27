using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Torrent.Helpers.AppConfig;
using Torrent.Helpers.ExtensionMethods;

namespace Torrent.System.Files.Impl
{
    public partial class LocalFileSystem : IFileSystem
    {
        private readonly IDictionary<string, Tuple<FileInfo, ByteString>> _localFiles
            = new Dictionary<string, Tuple<FileInfo, ByteString>>();

        public int ChunkSize { get; }

        public LocalFileSystem(AppSettings settings)
        {
            ChunkSize = settings.ChunkSize;
        }

        public UploadResponse UploadFileLocally(UploadRequest uploadRequest)
        {
            //get the name of the file and the byte fileData
            var fileName = uploadRequest.Filename;
            if (string.IsNullOrEmpty(fileName))
            {
                return new UploadResponse
                {
                    Status = Status.MessageError,
                    ErrorMessage = "The filename is null or empty"
                };
            }

            try
            {
                //get the chunk data
                var chunks = ChunkData(uploadRequest.Data.ToByteArray().ToList()).ToList();

                //create the file info
                var fileInfo = CreateFileInfo(fileName, uploadRequest.Data.ToByteArray(), chunks);

                //upload the file
                if (!_localFiles.ContainsKey(fileName))
                {
                    _localFiles.Add(fileName, Tuple.Create(fileInfo, uploadRequest.Data));
                }

                //get the upload response
                return new UploadResponse
                {
                    Status = Status.Success,
                    ErrorMessage = string.Empty,
                    FileInfo = _localFiles[fileName].Item1
                };
            }
            catch (Exception e)
            {
                return new UploadResponse
                {
                    Status = Status.ProcessingError,
                    ErrorMessage = e.Message
                };
            }
        }

        public LocalSearchResponse SearchFilesLocally(LocalSearchRequest localSearchRequest)
        {
            //check if the regex is valid
            if (!localSearchRequest.Regex.IsRegexValid())
            {
                return new LocalSearchResponse
                {
                    Status = Status.MessageError,
                    ErrorMessage = "The regex is invalid"
                };
            }

            try
            {
                //get the regex
                var searchRegex = new Regex(localSearchRequest.Regex);

                //get the file names of those that respect the regex
                var fileNames = _localFiles
                    .Keys
                    .Where(x => searchRegex.IsMatch(x));

                //create the response
                var response = new LocalSearchResponse
                {
                    Status = Status.Success,
                    ErrorMessage = string.Empty
                };
                foreach (var fileName in fileNames)
                {
                    var (fileInfo, _) = _localFiles[fileName];
                    response.FileInfo.Add(fileInfo.Clone());
                }

                //return the response
                return response;
            }
            catch (Exception e)
            {
                return new LocalSearchResponse
                {
                    Status = Status.ProcessingError,
                    ErrorMessage = e.Message
                };
            }
        }

        public DownloadResponse DownloadTheFileFromLocalFileSystem(DownloadRequest downloadRequest)
        {
            //get the file bytes
            var fileBytes = downloadRequest.FileHash.ToByteArray();
            if (fileBytes.Length != 16)
            {
                return new DownloadResponse
                {
                    Status = Status.MessageError,
                    ErrorMessage = "The length of the file hash must be 16"
                };
            }

            try
            {
                //get the desired file
                var file = _localFiles
                    .Values
                    .FirstOrDefault(x
                        => x.Item1.Hash.ToByteArray().SequenceEqual(fileBytes));

                //treat the case in which the file does not exist
                if (file == null)
                {
                    return new DownloadResponse
                    {
                        Status = Status.UnableToComplete,
                        ErrorMessage = "The file is not present on the local file system"
                    };
                }

                //unpack the file
                var (_, data) = file;

                //return the file content
                return new DownloadResponse
                {
                    Status = Status.Success,
                    ErrorMessage = string.Empty,
                    Data = data
                };
            }
            catch (Exception e)
            {
                //treat the other cases
                return new DownloadResponse
                {
                    Status = Status.ProcessingError,
                    ErrorMessage = e.Message
                };
            }
        }

        public ChunkResponse SearchForChunkInLocalFileSystemAndGetResponse(ChunkRequest chunkRequest)
        {
            //get the hash
            var fileHash =
                chunkRequest.FileHash.ToByteArray();

            //check the hash length
            if (fileHash.Length != 16)
            {
                return new ChunkResponse
                {
                    Status = Status.MessageError,
                    ErrorMessage = "Incorrect hash size."
                };
            }

            try
            {
                //get the file info that have the desired hash
                var (fileInfo, fileData) =
                    _localFiles
                       .Values
                       .FirstOrDefault(
                           x => x.Item1.Hash.ToByteArray().SequenceEqual(fileHash))
                   ?? throw new FileNotFoundException("File is not present locally");

                //check if the chunk exists
                if (chunkRequest.ChunkIndex >= fileInfo.Chunks.Count)
                {
                    throw new FileNotFoundException("Chunk is not present locally");
                }

                //get the starting and ending point for the current chunk
                var chunkStartsAt = (int)chunkRequest.ChunkIndex * ChunkSize;
                var chunkEndsAt = (int)Math.Min(fileData.Length, (chunkRequest.ChunkIndex + 1) * ChunkSize);

                //get the chunk
                var chunk = fileData
                    .ToList()
                    .GetRange(chunkStartsAt, chunkEndsAt - chunkStartsAt);

                //get the chunk response
                return new ChunkResponse
                {
                    Status = Status.Success,
                    ErrorMessage = string.Empty,
                    Data = ByteString.CopyFrom(chunk.ToArray())
                };
            }
            catch (FileNotFoundException e)
            {
                return new ChunkResponse
                {
                    ErrorMessage = e.Message,
                    Status = Status.UnableToComplete
                };
            }
            catch (Exception e)
            {
                return new ChunkResponse
                {
                    Status = Status.ProcessingError,
                    ErrorMessage = e.Message
                };
            }
        }

        public ReplicateResponse ReconstructFileFromChunksAndGetReplicationResponse(
            IEnumerable<(NodeReplicationStatus replicationStatus, ChunkResponse chunkResponse)> fileParts, FileInfo filInfoMetadata)
        {
            //order the chunks by their index
            var orderedChunks = fileParts
                .OrderBy(x =>
                    x.replicationStatus?.ChunkIndex ?? throw new Exception("Null not accepted"))
                .ToList();

            //create the replication response
            var replicationResponse = new ReplicateResponse
            {
                ErrorMessage = string.Empty,
                Status = Status.Success
            };

            //set the responses
            replicationResponse
                .NodeStatusList
                .AddRange(orderedChunks.Select(x => x.replicationStatus));

            //save the file only if is not present locally
            if (_localFiles.ContainsKey(filInfoMetadata.Filename))
            {
                return replicationResponse;
            }

            //iterate through each chunk
            var byteArray = new List<byte>();
            foreach (var (_, chunkResponse) in orderedChunks)
            {
                //append the bytes to file
                byteArray.AddRange(chunkResponse.Data.ToByteArray());
            }

            //upload the file
            var _ = UploadFileLocally(new UploadRequest
            {
                Filename = filInfoMetadata.Filename,
                Data = ByteString.CopyFrom(byteArray.ToArray())
            });

            //return the result
            return replicationResponse;
        }
    }
}
