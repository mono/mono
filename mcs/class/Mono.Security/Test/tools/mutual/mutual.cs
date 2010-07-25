using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls;

class TestSslClientStream {

	static Mono.Security.X509.PKCS12 p12;

	[STAThread]
	static void Main(string[] args)
	{
		string host = "localhost";
		if (args.Length > 0)
			host = args[0];

		SecurityProtocolType protocol = SecurityProtocolType.Tls;
		if (args.Length > 1) {
			switch (args [1].ToUpper ()) {
			case "SSL":
				protocol = SecurityProtocolType.Ssl3;
				break;
			}
		}

		X509CertificateCollection certificates = null;
		if (args.Length > 2) {
			string password = null;
			if (args.Length > 3)
				password = args [3];

			p12 = Mono.Security.X509.PKCS12.LoadFromFile(args [2], password);

			certificates = new X509CertificateCollection ();
			foreach (Mono.Security.X509.X509Certificate cert in p12.Certificates) {
				certificates.Add(new X509Certificate(cert.RawData));
			}
		}

		TcpClient client = new TcpClient ();
		client.Connect (host, 4433);
 
 		SslClientStream ssl = new SslClientStream (client.GetStream(), host, false, protocol, certificates);
 		ssl.ServerCertValidationDelegate += new CertificateValidationCallback (CertificateValidation);
 		ssl.ClientCertSelectionDelegate += new CertificateSelectionCallback (ClientCertificateSelection);
 		ssl.PrivateKeyCertSelectionDelegate += new PrivateKeySelectionCallback (PrivateKeySelection);
	
		StreamWriter sw = new StreamWriter (ssl, System.Text.Encoding.ASCII);
		sw.WriteLine ("GET /clientcert.aspx{0}", Environment.NewLine);
		sw.Flush ();

		StreamReader sr = new StreamReader (ssl);
		Console.WriteLine (sr.ReadToEnd ());
	}

	static bool CertificateValidation (X509Certificate certificate, int[] certificateErrors)
	{
		Console.WriteLine ("CertificateValidation");
		Console.WriteLine (certificate.ToString (true));
		Console.WriteLine ("\tError(s)");
		foreach (int error in certificateErrors)
			Console.WriteLine ("\t\t#{0}", error);
		Console.WriteLine ();
		return true;
	}

	static X509Certificate ClientCertificateSelection (X509CertificateCollection clientCertificates,
		X509Certificate serverCertificate, string targetHost, X509CertificateCollection serverRequestedCertificates)
	{
		Console.WriteLine ("ClientCertificateSelection");
		Console.WriteLine ("\tClient Certificates ({0})", clientCertificates.Count);
		int i = 1;
		foreach (X509Certificate client in clientCertificates)
			Console.WriteLine ("#{0} - {1}", i++, client.ToString (true));
		Console.WriteLine ("\tHost: {0}", targetHost);
		Console.Write ("SERVER {0}", serverCertificate.ToString (true));
		Console.WriteLine ();
		return clientCertificates [0];
	}

	static AsymmetricAlgorithm PrivateKeySelection (X509Certificate certificate, string targetHost)
	{
		Console.WriteLine ("PrivateKeySelection");
		Console.WriteLine ("\tHost: {0}", targetHost);
		Console.WriteLine (certificate.ToString (true));
		Console.WriteLine ("\tPrivateKeySelection ({0})", p12.Keys.Count);
		Console.WriteLine ();
		return (AsymmetricAlgorithm) p12.Keys [0];
	}
}
