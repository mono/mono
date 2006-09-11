using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Mono.Security.Authenticode;
using Mono.Security.Protocol.Tls;
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
						using (SslServerStream s = new SslServerStream (ns, Certificate, false, false)) {
							s.PrivateKeyCertSelectionDelegate += new PrivateKeySelectionCallback (GetPrivateKey);
							s.ClientCertValidationDelegate += new CertificateValidationCallback (VerifyClientCertificate);
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
								s.ClientCertificate == null ? "World" : s.ClientCertificate.GetName ());

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
				if (_certificate == null)
					_certificate = X509Certificate.CreateFromCertFile (certfile);
				return _certificate;
			}
		}

		// note: makecert creates the private key in the PVK format
		private static AsymmetricAlgorithm GetPrivateKey (X509Certificate certificate, string targetHost)
		{
			PrivateKey key = PrivateKey.CreateFromFile (keyfile);
			return key.RSA;
		}

		private static bool VerifyClientCertificate (X509Certificate certificate, int[] certificateErrors)
		{
			if (certificate != null) {
				Console.WriteLine (certificate.ToString (true));
			} else {
				Console.WriteLine ("No client certificate provided.");
			}

			foreach (int error in certificateErrors)
				Console.WriteLine ("\terror #{0}", error);
			return true;
		}
	}
}
