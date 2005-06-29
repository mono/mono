//
// postecho.cs: TLS/SSL Post Echo Test Program
//
// Authors:
//	Gonzalo Paniagua Javier  <gonzalo@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2005 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;

class PostEcho {

	static void Help ()
	{
		Console.WriteLine ("postecho url [size] [--web | --ssl3 | --tls1]");
		Console.WriteLine ("  default size is 1024 (bytes)");
		Console.WriteLine ("  default mode is --tls1");
		Console.WriteLine ("* a server side script must be installed to run postecho");
	}

	static string PostWeb (string url, byte[] buffer)
	{
		ServicePointManager.CertificatePolicy = new TestCertificatePolicy ();

		string postdata = "TEST=";
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
		req.Method = "POST";
		req.ContentType = "application/x-www-form-urlencoded";
		req.ContentLength = 5 + buffer.Length;
		Stream output = req.GetRequestStream ();
		byte [] bytes = Encoding.Default.GetBytes (postdata);
		output.Write (bytes, 0, bytes.Length);
		output.Write (buffer, 0, buffer.Length);
		output.Close ();
		HttpWebResponse response = (HttpWebResponse) req.GetResponse ();
		StreamReader reader = new StreamReader (response.GetResponseStream ());
		return reader.ReadToEnd ();
	}

	static string PostStream (Mono.Security.Protocol.Tls.SecurityProtocolType protocol, string url, byte[] buffer)
	{
		Uri uri = new Uri (url);
		string post = "POST " + uri.AbsolutePath + " HTTP/1.0\r\n";
		post += "Content-Type: application/x-www-form-urlencoded\r\n";
		post += "Content-Length: " + (buffer.Length + 5).ToString () + "\r\n";
		post += "Host: " + uri.Host + "\r\n\r\n";
		post += "TEST=";
		byte[] bytes = Encoding.Default.GetBytes (post);

		IPHostEntry host = Dns.Resolve (uri.Host);
		IPAddress ip = host.AddressList [0];
		Socket socket = new Socket (ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		socket.Connect (new IPEndPoint (ip, uri.Port));
		NetworkStream ns = new NetworkStream (socket, false);
		SslClientStream ssl = new SslClientStream (ns, uri.Host, false, protocol);
		ssl.ServerCertValidationDelegate += new CertificateValidationCallback (CertificateValidation);

		ssl.Write (bytes, 0, bytes.Length);
		ssl.Write (buffer, 0, buffer.Length);
		ssl.Flush ();

		StreamReader reader = new StreamReader (ssl, Encoding.UTF8);
		string result = reader.ReadToEnd ();
		int start = result.IndexOf ("\r\n\r\n") + 4;
		start = result.IndexOf ("\r\n\r\n") + 4;
		return result.Substring (start);
	}

	static int Main (string[] args)
	{
		if (args.Length < 1) {
			Help ();
			return 2;
		}

		string url = args [0];
		int size = 1024;
		bool ssl = true;
		Mono.Security.Protocol.Tls.SecurityProtocolType protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Tls;

		if (args.Length > 1) {
			for (int i=1; i < args.Length; i++) {
				switch (args [i].ToLower ()) {
				case "--ssl3":
					ssl = true;
					protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl3;
					break;
				case "--tls":
				case "--tls1":
					ssl = true;
					protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Tls;
					break;
				case "--web":
					ssl = false;
					break;
				default:
					size = Int32.Parse (args [i]);
					break;
				}
			}
		}

		// prepare test buffer
		byte[] data = new byte[size];
		for (int i = 0; i < size; i++)
			data[i] = 65;

		string result = (ssl ? PostStream (protocol, url, data) : PostWeb (url, data));

		if (data.Length != result.Length) {
			Console.WriteLine ("Invalid length {0}. Expected {1}", result.Length, data.Length);
			return 1;
		}
		for (int i = 0; i < result.Length; i++) {
			if (result[i] != 'A') {
				Console.WriteLine ("Error at position #{0} - received '{1}'", i, result[i]);
				return 1;
			}
		}
		Console.WriteLine ("Result OK (length: {0})", result.Length);
		return 0;
	}

	private static void ShowCertificateError (int error)
	{
		string message = null;
		switch (error)
		{
			case -2146762490:
				message = "CERT_E_PURPOSE 0x800B0106";
				break;
			case -2146762481:
				message = "CERT_E_CN_NO_MATCH 0x800B010F";
				break;
			case -2146869223:
				message = "TRUST_E_BASIC_CONSTRAINTS 0x80096019";
				break;
			case -2146869232:
				message = "TRUST_E_BAD_DIGEST 0x80096010";
				break;
			case -2146762494:
				message = "CERT_E_VALIDITYPERIODNESTING 0x800B0102";
				break;
			case -2146762495:
				message = "CERT_E_EXPIRED 0x800B0101";
				break;
			case -2146762486:
				message = "CERT_E_CHAINING 0x800B010A";
				break;
			case -2146762487:
				message = "CERT_E_UNTRUSTEDROOT 0x800B0109";
				break;
			default:
				message = "unknown (try WinError.h)";
				break;
		}
		Console.WriteLine ("Error #{0}: {1}", error, message);
	}

	private static bool CertificateValidation (X509Certificate certificate, int[] certificateErrors)
	{
		if (certificateErrors.Length > 0)
		{
			Console.WriteLine (certificate.ToString (true));
			// X509Certificate.ToString(true) doesn't show dates :-(
			Console.WriteLine ("\tValid From:  {0}", certificate.GetEffectiveDateString ());
			Console.WriteLine ("\tValid Until: {0}{1}", certificate.GetExpirationDateString (), Environment.NewLine);
			// multiple errors are possible using SslClientStream
			foreach (int error in certificateErrors)
			{
				ShowCertificateError (error);
			}
		}
		// whatever the reason we do not stop the SSL connection
		return true;
	}

	public class TestCertificatePolicy : ICertificatePolicy {

		public bool CheckValidationResult (ServicePoint sp, X509Certificate certificate, WebRequest request, int error)
		{
			if (error != 0) {
				Console.WriteLine (certificate.ToString (true));
				// X509Certificate.ToString(true) doesn't show dates :-(
				Console.WriteLine ("\tValid From:  {0}", certificate.GetEffectiveDateString ());
				Console.WriteLine ("\tValid Until: {0}{1}", certificate.GetExpirationDateString (), Environment.NewLine);

				ShowCertificateError (error);
			}
			// whatever the reason we do not stop the SSL connection
			return true;
		}
	}
}

