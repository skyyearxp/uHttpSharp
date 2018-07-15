using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace uhttpsharp.Clients {
    public class ClientSslDecorator : IClient {
        private readonly IClient _child;
        private readonly SslStream _sslStream;

        public ClientSslDecorator(IClient child, X509Certificate certificate) {
            _child = child;
            _sslStream = new SslStream(_child.Stream);
            _sslStream.AuthenticateAsServer(certificate, false, SslProtocols.Tls, true);
        }

        public Stream Stream => _sslStream;

        public bool Connected => _child.Connected;

        public void Close() {
            _child.Close();
        }

        public EndPoint RemoteEndPoint => _child.RemoteEndPoint;
    }
}