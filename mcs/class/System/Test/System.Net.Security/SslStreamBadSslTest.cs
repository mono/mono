#if UNITY	// Does this test work with Mono's default tls implementations?

using NUnit.Framework;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace MonoTests.System.Net.Security
{

[TestFixture]
public class SslStreamBadSslTest
{
        private SslStream sslStream;
        private TcpClient client;

        [SetUp]
        public void InitClient()
        {
            const string machineName = "badssl.com";
            client = new TcpClient(machineName, 443);
            sslStream = new SslStream(client.GetStream());
        }

        [TestCase("www.badssl.com")]
        [TestCase("sha256.badssl.com")]
        [TestCase("sha384.badssl.com")]
        [TestCase("sha512.badssl.com")]
        //[TestCase("1000-sans.badssl.com")]	// MBEDTLS_ERR_SSL_FEATURE_UNAVAILABLE
        //[TestCase("10000-sans.badssl.com")]
        [TestCase("ecc256.badssl.com")]
        [TestCase("ecc384.badssl.com")]
        [TestCase("rsa2048.badssl.com")]
        [TestCase("rsa8192.badssl.com")]
        [TestCase("dh2048.badssl.com")]
        [TestCase("tls-v1-0.badssl.com")]
        [TestCase("tls-v1-1.badssl.com")]
        [TestCase("long-extended-subdomain-name-containing-many-letters-and-dashes.badssl.com")]
        [TestCase("longextendedsubdomainnamewithoutdashesinordertotestwordwrapping.badssl.com")]
        public void SuccessFullHostProvider(string targetHost)
        {
            sslStream.AuthenticateAsClient(targetHost);
        }

        [ExpectedException(typeof(AuthenticationException))]
        [TestCase("expired.badssl.com")]
        [TestCase("wrong.host.badssl.com")]
        [TestCase("self-signed.badssl.com")]
        [TestCase("untrusted-root.badssl.com")]
        //[TestCase("revoked.badssl.com")]	// No support for revoking yet
        [TestCase("sha1-intermediate.badssl.com")]
        [TestCase("dh480.badssl.com")]
        [TestCase("dh512.badssl.com")]
        //[TestCase("dh1024.badssl.com")]
        //[TestCase("dh-small-subgroup.badssl.com")]
        //[TestCase("dh-composite.badssl.com")]
        //[TestCase("invalid-expected-sct.badssl.com")]
        [TestCase("superfish.badssl.com")]
        [TestCase("edellroot.badssl.com")]
        [TestCase("dsdtestprovider.badssl.com")]
        [TestCase("preact-cli.badssl.com")]
        [TestCase("webpack-dev-server.badssl.com")]
        [TestCase("sha1-2016.badssl.com")]
        [TestCase("sha1-2017.badssl.com")]
        public void FailingHostsProvider(string targetHost)
        {
            sslStream.AuthenticateAsClient(targetHost);
        }
    }
}

#endif