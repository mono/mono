using System.Net.Sockets;
using System.Net.Test.Common;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace System.Net.Security.Tests
{
    using Configuration = System.Net.Test.Common.Configuration;

    public class SslStreamNetworkStreamTest
    {
        [Fact]
        public async Task SslStream_SendReceiveOverNetworkStream_Ok()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);

            using (X509Certificate2 serverCertificate = Configuration.Certificates.GetServerCertificate())
            using (TcpClient client = new TcpClient())
            {
                listener.Start();

                Task clientConnectTask = client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
                Task<TcpClient> listenerAcceptTask = listener.AcceptTcpClientAsync();

                await Task.WhenAll(clientConnectTask, listenerAcceptTask);

                TcpClient server = listenerAcceptTask.Result;
                using (SslStream clientStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null,
                    EncryptionPolicy.RequireEncryption))
                using (SslStream serverStream = new SslStream(
                    server.GetStream(),
                    false,
                    null,
                    null,
                    EncryptionPolicy.RequireEncryption))
                {

                    Task clientAuthenticationTask = clientStream.AuthenticateAsClientAsync(
                        serverCertificate.GetNameInfo(X509NameType.SimpleName, false),
                        null,
                        SslProtocols.Tls12,
                        false);

                    Task serverAuthenticationTask = serverStream.AuthenticateAsServerAsync(
                        serverCertificate,
                        false,
                        SslProtocols.Tls12,
                        false);

                    await Task.WhenAll(clientAuthenticationTask, serverAuthenticationTask);

                    byte[] writeBuffer = new byte[256];
                    Task writeTask = clientStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                    byte[] readBuffer = new byte[256];
                    Task<int> readTask = serverStream.ReadAsync(readBuffer, 0, readBuffer.Length);

                    await TestConfiguration.WhenAllOrAnyFailedWithTimeout(writeTask, readTask);

                    Assert.InRange(readTask.Result, 1, 256);
                }
            }

            listener.Stop();
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)] // This only applies where OpenSsl is used.
        public async Task SslStream_SendReceiveOverNetworkStream_AuthenticationException()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);

            using (X509Certificate2 serverCertificate = Configuration.Certificates.GetServerCertificate())
            using (TcpClient client = new TcpClient())
            {
                listener.Start();

                Task clientConnectTask = client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
                Task<TcpClient> listenerAcceptTask = listener.AcceptTcpClientAsync();

                await TestConfiguration.WhenAllOrAnyFailedWithTimeout(clientConnectTask, listenerAcceptTask);

                TcpClient server = listenerAcceptTask.Result;
                using (SslStream clientStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null,
                    EncryptionPolicy.RequireEncryption))
                using (SslStream serverStream = new SslStream(
                    server.GetStream(),
                    false,
                    null,
                    null,
                    EncryptionPolicy.RequireEncryption))
                {

                    Task clientAuthenticationTask = clientStream.AuthenticateAsClientAsync(
                        serverCertificate.GetNameInfo(X509NameType.SimpleName, false),
                        null,
                        SslProtocols.Tls11,
                        false);

                    AuthenticationException e = await Assert.ThrowsAsync<AuthenticationException>(() => serverStream.AuthenticateAsServerAsync(
                        serverCertificate,
                        false,
                        SslProtocols.Tls12,
                        false));

                    Assert.NotNull(e.InnerException);
                    Assert.True(e.InnerException.Message.Contains("SSL_ERROR_SSL"));
                    Assert.NotNull(e.InnerException.InnerException);
                    Assert.True(e.InnerException.InnerException.Message.Contains("protocol"));
                }
            }

            listener.Stop();
        }

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate retrievedServerPublicCertificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Accept any certificate.
            return true;
        }
    }
}
