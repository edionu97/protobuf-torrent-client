using System.Threading.Tasks;

namespace Torrent.System.Node
{
    public interface INode
    {
        public int NodeIndex { set; }

        public int NodePort {  set; }

        /// <summary>
        /// This method it is used for starting the listening for incoming requests
        /// </summary>
        /// <returns>the task that is associated with the thread that listens for incoming requests</returns>
        public Task StartListening();

        /// <summary>
        /// This method registers the node into the hub
        /// </summary>
        public void RegisterNodeToHub(string nodeOwner);
    }
}
