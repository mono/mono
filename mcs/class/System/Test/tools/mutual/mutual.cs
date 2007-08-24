using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
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

		SslProtocols protocol = SslProtocols.Tls;
		if (args.Length > 1) {
			switch (args [1].ToUpper ()) {
			case "SSL":
				protocol = SslProtocols.Ssl3;
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
				certificates.Add(new X509Certificate2(args [2], password));
				break;
			}
		}

		TcpClient client = new TcpClient ();
		client.Connect (host, 4433);
 
 		SslStream ssl = new SslStream (client.GetStream(), false, new RemoteCertificateValidationCallback (CertificateValidation), new LocalCertificateSelectionCallback (ClientCertificateSelection));

		ssl.AuthenticateAsClient (host, certificates, protocol, false); 	
		StreamWriter sw = new StreamWriter (ssl, System.Text.Encoding.ASCII);
		sw.WriteLine ("GET /clientcert.aspx{0}", Environment.NewLine);
		sw.Flush ();

		StreamReader sr = new StreamReader (ssl);
		Console.WriteLine (sr.ReadToEnd ());
	}

	static bool CertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors certificateErrors)
	{
		Console.WriteLine ("CertificateValidation");
		Console.WriteLine (certificate.ToString (true));
		Console.WriteLine ("Chain");
		Console.WriteLine (chain);
		Console.WriteLine ("\tError(s)");
		Console.WriteLine (certificateErrors);
		Console.WriteLine ();
		return true;
	}

	static X509Certificate ClientCertificateSelection (object sender, string targetHost, X509CertificateCollection clientCertificates,
		X509Certificate serverCertificate, string [] acceptableIssuers)
	{
		Console.WriteLine ("ClientCertificateSelection");
		Console.WriteLine ("\tClient Certificates ({0})", clientCertificates.Count);
		int i = 1;
		foreach (X509Certificate client in clientCertificates)
			Console.WriteLine ("#{0} - {1}", i++, client.ToString (true));
		Console.WriteLine ("\tHost: {0}", targetHost);
		Console.Write ("SERVER {0}", serverCertificate != null ? serverCertificate.ToString (true) : null);
		Console.WriteLine ();
		if (i == 1)
			return null;
		X509Certificate2 cc = new X509Certificate2 (clientCertificates [0]);
		cc.PrivateKey = PrivateKeySelection (cc, targetHost);
		return cc;
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
