#if UNITY	// Does this test work with Mono's default tls implementations?

using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MonoTests.Helpers;

namespace MonoTests.System.Net.Security
{
	[TestFixture]
	public class SslStreamTest2
	{
		byte[] m_serverCertRaw = { 48, 130, 5, 165, 2, 1, 3, 48, 130, 5, 95, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 5, 80, 4, 130, 5, 76, 48, 130, 5, 72, 48, 130, 2, 87, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 6, 160, 130, 2, 72, 48, 130, 2, 68, 2, 1, 0, 48, 130, 2, 61, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 211, 176, 234, 3, 252, 26, 32, 15, 2, 2, 7, 208, 128, 130, 2, 16, 183, 149, 35, 180, 127, 95, 163, 122, 138, 244, 29, 177, 220, 173, 46, 73, 208, 217, 211, 190, 164, 183, 21, 110, 33, 122, 98, 163, 251, 16, 23, 106, 154, 14, 52, 177, 3, 12, 248, 226, 48, 123, 211, 6, 216, 6, 192, 175, 203, 142, 141, 143, 252, 178, 7, 162, 81, 232, 159, 42, 56, 177, 191, 53, 7, 146, 189, 236, 75, 140, 210, 143, 11, 103, 64, 58, 10, 73, 123, 39, 97, 119, 166, 114, 123, 65, 68, 214, 42, 17, 156, 122, 8, 58, 184, 134, 255, 48, 64, 20, 229, 247, 196, 12, 130, 56, 176, 69, 179, 254, 216, 45, 25, 244, 240, 116, 88, 137, 66, 13, 18, 202, 199, 59, 200, 245, 19, 175, 232, 217, 211, 12, 191, 222, 26, 162, 253, 73, 201, 48, 61, 3, 248, 117, 16, 71, 233, 183, 90, 110, 91, 116, 56, 133, 223, 148, 19, 78, 140, 123, 159, 203, 78, 15, 172, 39, 190, 39, 71, 180, 155, 48, 156, 116, 212, 52, 1, 231, 201, 196, 73, 87, 68, 104, 208, 40, 104, 32, 218, 235, 245, 84, 136, 168, 51, 9, 93, 126, 46, 80, 180, 240, 144, 79, 88, 87, 159, 24, 108, 186, 9, 20, 48, 100, 148, 250, 4, 163, 115, 131, 44, 13, 38, 222, 117, 196, 196, 128, 114, 149, 97, 93, 37, 191, 3, 192, 231, 88, 80, 218, 147, 8, 192, 165, 27, 206, 56, 42, 157, 230, 223, 130, 253, 169, 182, 245, 192, 181, 18, 212, 133, 168, 73, 92, 66, 197, 117, 245, 107, 127, 23, 146, 249, 41, 66, 219, 210, 207, 221, 205, 205, 15, 110, 92, 12, 207, 76, 239, 4, 13, 129, 127, 170, 205, 253, 148, 208, 24, 129, 24, 210, 220, 85, 45, 179, 137, 66, 134, 142, 22, 112, 48, 160, 236, 232, 38, 83, 101, 55, 51, 18, 110, 99, 69, 41, 173, 107, 233, 11, 199, 23, 61, 135, 222, 94, 74, 29, 219, 80, 128, 167, 186, 254, 235, 42, 96, 134, 5, 13, 90, 59, 231, 137, 195, 207, 28, 165, 12, 218, 5, 72, 102, 61, 135, 198, 73, 250, 97, 89, 214, 179, 244, 194, 23, 142, 157, 4, 243, 90, 69, 54, 10, 139, 76, 95, 40, 225, 219, 59, 15, 54, 182, 206, 142, 228, 248, 79, 156, 129, 246, 63, 6, 6, 236, 44, 67, 116, 213, 170, 47, 193, 186, 139, 25, 80, 166, 57, 99, 231, 156, 191, 117, 65, 76, 7, 243, 244, 127, 225, 210, 190, 164, 141, 46, 36, 99, 111, 203, 133, 127, 80, 28, 61, 160, 36, 132, 182, 16, 41, 39, 185, 232, 123, 32, 57, 189, 100, 152, 38, 205, 5, 189, 240, 65, 3, 191, 73, 85, 12, 209, 180, 1, 194, 70, 124, 57, 71, 48, 230, 235, 122, 175, 157, 35, 233, 83, 40, 20, 169, 224, 14, 11, 216, 48, 194, 105, 25, 187, 210, 182, 6, 184, 73, 95, 85, 210, 227, 113, 58, 10, 186, 175, 254, 25, 102, 39, 3, 2, 200, 194, 197, 200, 224, 77, 164, 8, 36, 114, 48, 130, 2, 233, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 2, 218, 4, 130, 2, 214, 48, 130, 2, 210, 48, 130, 2, 206, 6, 11, 42, 134, 72, 134, 247, 13, 1, 12, 10, 1, 2, 160, 130, 2, 166, 48, 130, 2, 162, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 178, 13, 52, 135, 85, 49, 79, 105, 2, 2, 7, 208, 4, 130, 2, 128, 21, 84, 227, 109, 230, 144, 140, 170, 117, 250, 179, 207, 129, 100, 126, 126, 29, 231, 94, 140, 45, 26, 168, 45, 240, 4, 170, 73, 98, 115, 109, 96, 177, 206, 6, 80, 170, 22, 237, 144, 58, 95, 59, 26, 85, 135, 178, 69, 184, 44, 122, 81, 213, 135, 149, 198, 246, 83, 68, 129, 2, 186, 118, 33, 44, 214, 227, 240, 220, 51, 175, 220, 220, 180, 113, 216, 101, 138, 81, 54, 38, 0, 216, 30, 29, 187, 213, 230, 12, 181, 130, 21, 241, 98, 120, 41, 150, 176, 69, 37, 169, 249, 123, 212, 254, 135, 154, 214, 127, 39, 105, 149, 180, 218, 41, 207, 75, 70, 105, 169, 185, 169, 132, 173, 188, 82, 251, 71, 234, 136, 5, 254, 110, 223, 34, 4, 145, 7, 19, 51, 123, 140, 75, 226, 0, 21, 220, 228, 223, 218, 8, 169, 210, 194, 139, 93, 218, 55, 40, 174, 50, 238, 38, 166, 222, 103, 0, 209, 88, 131, 51, 222, 154, 217, 18, 172, 73, 17, 133, 54, 173, 208, 118, 104, 167, 113, 153, 223, 251, 154, 120, 176, 18, 127, 51, 206, 164, 77, 86, 9, 82, 212, 86, 162, 206, 230, 79, 217, 178, 42, 217, 162, 152, 188, 217, 59, 212, 117, 200, 135, 75, 74, 43, 1, 42, 79, 180, 164, 250, 122, 103, 103, 157, 11, 14, 33, 48, 8, 108, 155, 46, 124, 223, 204, 169, 124, 104, 11, 246, 213, 226, 16, 125, 17, 228, 15, 178, 141, 79, 78, 115, 76, 131, 122, 166, 124, 154, 1, 174, 178, 176, 213, 208, 188, 71, 118, 220, 168, 64, 218, 176, 134, 38, 229, 14, 109, 162, 125, 16, 57, 249, 201, 180, 17, 182, 143, 184, 12, 248, 113, 65, 70, 109, 79, 249, 34, 170, 35, 228, 219, 121, 202, 228, 121, 127, 255, 22, 173, 202, 171, 33, 232, 4, 240, 142, 216, 80, 56, 177, 83, 93, 123, 217, 213, 157, 99, 34, 194, 61, 228, 239, 194, 20, 27, 9, 53, 132, 79, 19, 97, 107, 31, 51, 39, 176, 223, 90, 88, 67, 138, 194, 169, 176, 144, 202, 119, 146, 74, 27, 118, 63, 129, 230, 101, 104, 75, 116, 49, 223, 254, 225, 70, 206, 183, 11, 134, 148, 10, 55, 57, 50, 178, 144, 164, 139, 233, 169, 109, 186, 211, 95, 123, 75, 111, 192, 187, 127, 240, 45, 226, 194, 240, 128, 10, 79, 178, 192, 66, 21, 197, 24, 171, 141, 255, 185, 230, 84, 206, 151, 9, 93, 115, 162, 12, 115, 129, 218, 103, 219, 183, 142, 123, 3, 110, 139, 208, 4, 146, 76, 99, 246, 240, 32, 169, 148, 16, 146, 172, 230, 36, 56, 145, 23, 94, 209, 92, 38, 244, 127, 70, 121, 253, 66, 55, 36, 140, 98, 105, 233, 112, 24, 23, 230, 112, 62, 244, 12, 48, 30, 51, 0, 18, 244, 139, 66, 245, 234, 203, 195, 52, 119, 255, 84, 82, 204, 100, 176, 167, 24, 224, 8, 127, 214, 148, 115, 242, 56, 190, 72, 221, 68, 252, 36, 74, 254, 57, 52, 96, 20, 173, 32, 236, 87, 15, 16, 76, 9, 48, 3, 61, 2, 137, 137, 9, 68, 213, 99, 163, 63, 201, 83, 241, 98, 7, 117, 108, 4, 123, 170, 18, 10, 19, 198, 31, 170, 15, 247, 216, 145, 172, 239, 137, 181, 80, 160, 24, 11, 35, 131, 58, 218, 22, 250, 215, 52, 160, 246, 197, 183, 92, 137, 0, 245, 63, 49, 183, 246, 195, 58, 63, 4, 75, 10, 92, 131, 181, 59, 78, 247, 44, 150, 49, 49, 107, 211, 62, 71, 62, 222, 159, 161, 118, 236, 55, 219, 49, 0, 3, 82, 236, 96, 20, 83, 39, 245, 208, 240, 245, 174, 218, 49, 21, 48, 19, 6, 9, 42, 134, 72, 134, 247, 13, 1, 9, 21, 49, 6, 4, 4, 1, 0, 0, 0, 48, 61, 48, 33, 48, 9, 6, 5, 43, 14, 3, 2, 26, 5, 0, 4, 20, 30, 154, 48, 126, 198, 239, 114, 62, 12, 58, 129, 172, 67, 156, 76, 214, 62, 205, 89, 28, 4, 20, 135, 177, 105, 83, 79, 93, 181, 149, 169, 49, 112, 201, 70, 212, 153, 79, 198, 163, 137, 90, 2, 2, 7, 208 };
		const string m_serverHostName = "server";

		private X509Certificate2 m_serverCert;
		private IPEndPoint m_ipEndPoint;
		private TcpClient m_tcpClient;
		private TcpClient m_tcpServer;
		private MemoryStream m_stream;

		class TestException : Exception
		{
		}

		[SetUp()]
		public void GetReady()
		{
			m_serverCert = new X509Certificate2(m_serverCertRaw, m_serverHostName);
			m_ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkHelpers.FindFreePort ());
		}

		private void SetupClientServerConnection()
		{
			m_tcpClient = new TcpClient();
			var tcpListener = new TcpListener(m_ipEndPoint);
			tcpListener.Start();
			Task.Run(() => m_tcpClient.Connect(m_ipEndPoint.Address, m_ipEndPoint.Port));
			m_tcpServer = tcpListener.AcceptTcpClient();
			tcpListener.Stop();
		}

		[TestCase]
		public void HandshakeVerification_CorrectPolicyError_ForSelfSignedCertificate()
		{
			SetupClientServerConnection();
			var clientStream = new SslStream(m_tcpClient.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) =>
				{
					Assert.AreEqual (SslPolicyErrors.RemoteCertificateChainErrors, sslPolicyErrors); 
					return true;
				});
			var serverStream = new SslStream(m_tcpServer.GetStream(), false, RemoteCertificateValidationCallback_AlwaysSucceed);
			DoHandshake(clientStream, serverStream);
		}

		[TestCase]
		public void HandshakeVerification_CorrectPolicyError_ForSelfSignedCertificateWithWrongCN()
		{
			SetupClientServerConnection();
			var clientStream = new SslStream(m_tcpClient.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) =>
				{
					Assert.AreEqual (SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch, sslPolicyErrors); 
					return true;
				});
			var serverStream = new SslStream(m_tcpServer.GetStream(), false, RemoteCertificateValidationCallback_AlwaysSucceed);
			DoHandshake(clientStream, serverStream, "test");
		}

		[TestCase]
		[ExpectedException(typeof(AuthenticationException))]
		public void HandshakeVerification_FailureClient_ByCallback()
		{
			SetupClientServerConnection();
			var clientStream = new SslStream(m_tcpClient.GetStream(), false, RemoteCertificateValidationCallback_AlwaysFail);
			var serverStream = new SslStream(m_tcpServer.GetStream(), false, RemoteCertificateValidationCallback_AlwaysSucceed);
			DoHandshake(clientStream, serverStream);
		}

// We don't support client authentification, so we can't fail the handshake on the server side
// Note that in contrast to .Net we don't call the verification callback (with a null certificate) if verification is disabled.

		// [TestCase]
		// [ExpectedException(typeof(AuthenticationException))]
		// public void HandshakeVerification_FailureServer_ByCallback()
		// {
		// 	SetupClientServerConnection();
		// 	var clientStream = new SslStream(m_tcpClient.GetStream(), false, RemoteCertificateValidationCallback_AlwaysSucceed);
		// 	var serverStream = new SslStream(m_tcpServer.GetStream(), false, RemoteCertificateValidationCallback_AlwaysFail);
		// 	DoHandshake(clientStream, serverStream);
		// }

// TODO: Can we support passing on the exception?
		
		// [TestCase]
		// [ExpectedException(typeof(TestException))]
		// public void HandshakeVerification_FailureClient_ByException()
		// {
		// 	SetupClientServerConnection();
		// 	var clientStream = new SslStream(m_tcpClient.GetStream(), false, RemoteCertificateValidationCallback_ThrowException);
		// 	var serverStream = new SslStream(m_tcpServer.GetStream(), false, RemoteCertificateValidationCallback_AlwaysSucceed);
		// 	DoHandshake(clientStream, serverStream);
		// }


		private void DoHandshake(SslStream clientStream, SslStream serverStream, string expectedCN = m_serverHostName)
		{
			try
			{
                Task.WaitAll(
				    Task.Run(() => {
					    try {
							if (clientStream != null)
						    	clientStream.AuthenticateAsClient(expectedCN);
					    }
					    catch {
						    m_tcpServer.Close();
						    throw;
					    }}),
				    Task.Run(() => {
					    try {
							if (serverStream != null)
						    	serverStream.AuthenticateAsServer(m_serverCert);
					    }
					    catch  {
                            m_tcpClient.Close();
						    throw;
					    }})
				);
			}
			// We're intersted in the "first" exception.
			catch (AggregateException e)
			{
				if (e.InnerException is AggregateException)
					throw e.InnerException.InnerException;
				else
					throw e.InnerException;
			}
		}

		private bool RemoteCertificateValidationCallback_AlwaysSucceed(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private bool RemoteCertificateValidationCallback_AlwaysFail(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return false;
		}

		private bool RemoteCertificateValidationCallback_ThrowException(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			throw new TestException();
		}
	}
}

#endif