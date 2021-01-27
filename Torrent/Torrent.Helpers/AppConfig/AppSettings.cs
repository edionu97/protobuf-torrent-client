using Torrent.Helpers.AppConfig.Parts;

namespace Torrent.Helpers.AppConfig
{
    public class AppSettings
    {
        public HubSettings Hub { get; set; }

        public NodesSettings Nodes { get; set; }

        public int ChunkSize { get; set; }

        public int NodeCount { get; set; }

        public void Deconstruct(out HubSettings hub, out NodesSettings nodes)
        {
            hub = Hub;
            nodes = Nodes;
        }
    }
}
