using System.IO;
using System.Net;
using System.Net.Sockets;

namespace uhttpsharp.Clients {
    public class TcpClientAdapter : IClient {
        private readonly TcpClient _client;

        public TcpClientAdapter(TcpClient client) {
            _client = client;
        }

        public Stream Stream => _client.GetStream();

        public bool Connected => _client.Connected;

        public void Close() {
            _client.Close();
        }

        public EndPoint RemoteEndPoint => _client.Client.RemoteEndPoint;
    }
}