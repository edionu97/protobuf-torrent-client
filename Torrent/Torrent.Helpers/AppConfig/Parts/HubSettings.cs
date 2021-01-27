namespace Torrent.Helpers.AppConfig.Parts
{
    public class HubSettings
    {
        public string HubAddress { get; set; }

        public int HubPort { get; set; }

        public void Deconstruct(out string hubAddress, out int hubPort)
        {
            hubAddress = HubAddress;
            hubPort = HubPort;
        }
    }
}
