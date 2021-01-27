namespace Torrent.Helpers.AppConfig.Parts
{
    public class NodesSettings
    {
        public int NodesStartingPort { get; set; }

        public string NodesAddress { get; set; }

        public string NodesOwner { get; set; }

        public void Deconstruct(out string nodesAddress, out int nodesStartingPort)
        {
            nodesAddress = NodesAddress;
            nodesStartingPort = NodesStartingPort;
        }
    }
}
