using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Mono.Security.Authenticode;
//using Mono.Security.Protocol.Tls;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SslHttpServer
{
	class SslHttpServer
	{
		private static X509Certificate _certificate = null;
		private static string certfile;
		private static string keyfile;

		static void Main (string [] args)
		{
			certfile = (args.Length > 1) ? args [0] : "ssl.cer";
			keyfile = (args.Length > 1) ? args [1] : "ssl.pvk";

			Socket listenSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint localEndPoint = new IPEndPoint (IPAddress.Any, 4433);
			Socket requestSocket;

			listenSocket.Bind (localEndPoint);
			listenSocket.Listen (10);

			while (true) {
				try {
					requestSocket = listenSocket.Accept ();
					using (NetworkStream ns = new NetworkStream (requestSocket, FileAccess.ReadWrite, true)) {
						using (SslStream s = new SslStream (ns, false, new RemoteCertificateValidationCallback (VerifyClientCertificate))) {
							s.AuthenticateAsServer (Certificate, false, SslProtocols.Default, false);
							StreamReader reader = new StreamReader (s);
							StreamWriter writer = new StreamWriter (s, Encoding.ASCII);

							string line;
							// Read request header
							do {
								line = reader.ReadLine ();
								if (line != null)
									Console.WriteLine (line);
							}
							while (line != null && line.Length > 0);

							string answer = String.Format ("HTTP/1.0 200{0}Connection: close{0}" +
								"Content-Type: text/html{0}Content-Encoding: {1}{0}{0}" +
								"<html><body><h1>Hello {2}!</h1></body></html>{0}",
								"\r\n", Encoding.ASCII.WebName,
								s.RemoteCertificate == null ? "World" : s.RemoteCertificate.GetName ());

							// Send response
							writer.Write (answer);

							writer.Flush ();
							s.Flush ();
							ns.Flush ();
						}
					}
				}
				catch (Exception ex) {
					Console.WriteLine ("---------------------------------------------------------");
					Console.WriteLine (ex.ToString ());
				}
			}
		}

		private static X509Certificate Certificate {
			get {
				if (_certificate == null) {
					X509Certificate2 ccc = new X509Certificate2 (certfile);
					ccc.PrivateKey = PrivateKey.CreateFromFile (keyfile).RSA;
					//_certificate = new X509Certificate2 (ccc.Export (X509ContentType.Pkcs12, "mono"), "mono");
					_certificate = ccc;
				}
				return _certificate;
			}
		}

/*
		// note: makecert creates the private key in the PVK format
		private static AsymmetricAlgorithm GetPrivateKey (X509Certificate certificate, string targetHost)
		{
			PrivateKey key = PrivateKey.CreateFromFile (keyfile);
			return key.RSA;
		}
*/

		private static bool VerifyClientCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors certificateErrors)
		{
			if (certificate != null) {
				Console.WriteLine (certificate.ToString (true));
			} else {
				Console.WriteLine ("No client certificate provided.");
			}

			Console.WriteLine (chain);

//			foreach (int error in certificateErrors)
				Console.WriteLine ("\terror #{0}", certificateErrors);
			return true;
		}
	}
}
