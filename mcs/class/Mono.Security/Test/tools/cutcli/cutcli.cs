//
// CutCli.cs: A TLS/SSL Test Program that can cut the communication after
//	'x' read bytes and/or 'y' bytes written.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2005 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Mono.Security.Protocol.Tls;
using Mono.Test;

public class TlsTest {

	public static void Usage (string message) 
	{
		Console.WriteLine ("{0}tlstest - Copyright (c) 2004 Novell", Environment.NewLine);
		if (message != null) {
			Console.WriteLine ("{0}{1}{0}", Environment.NewLine, message);
		}
		Console.WriteLine ("Usage:");
		Console.WriteLine ("tlstest [protocol] [class] [credentials] [--x:x509 [--x:x509]] [--time] [--show] url [...]");
		Console.WriteLine ("{0}protocol (only applicable when using stream)", Environment.NewLine);
		Console.WriteLine ("\t--any   \tNegotiate protocol [default]");
		Console.WriteLine ("\t--ssl   \tUse SSLv3");
		Console.WriteLine ("\t--ssl3  \tUse SSLv3");
		Console.WriteLine ("\t--tls   \tUse TLSv1");
		Console.WriteLine ("\t--tls1  \tUse TLSv1");
		Console.WriteLine ("{0}credentials", Environment.NewLine);
		Console.WriteLine ("\t--basic:username:password:domain\tBasic Authentication");
		Console.WriteLine ("\t--digest:username:password:domain\tDigest Authentication");
		Console.WriteLine ("{0}options", Environment.NewLine);
		Console.WriteLine ("\t--x:x509\tX.509 client certificate (multiple entries allowed");
		Console.WriteLine ("\t--time  \tShow the time required for each page load");
		Console.WriteLine ("\t--show  \tShow the web page content on screen");
		Console.WriteLine ("{0}\turl [...]\tOne, or more, URL to download{0}", Environment.NewLine);
	}

	private static bool show;
	private static bool time;
	private static Mono.Security.Protocol.Tls.SecurityProtocolType protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Default;
	private static int read = -1;
	private static int write = -1;
	private static bool readloop = false;
	private static bool writeloop = false;

	public static void Main (string[] args) 
	{
		if (args.Length == 0) {
			Usage ("Missing arguments");
			return;
		}

		ArrayList urls = new ArrayList ();
		foreach (string arg in args) {
			switch (arg) {
				// protocol
				case "--any":
					protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Default;
					break;
				case "--ssl":
				case "--ssl3":
					protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl3;
					break;
				case "--tls":
				case "--tls1":
					protocol = Mono.Security.Protocol.Tls.SecurityProtocolType.Tls;
					break;
				// options
				case "--time":
					time = true;
					break;
				case "--show":
					show = true;
					break;
				case "--help":
					Usage (null);
					return;
				// credentials, certificates, urls or bad options
				default:
					if (arg.StartsWith ("--read:")) {
						string rval = arg.Substring (7);
						if (rval == "loop")
							readloop = true;
						else
							read = Int32.Parse (rval);
						continue;
					}
					else if (arg.StartsWith ("--write:")) {
						string wval = arg.Substring (8);
						if (wval == "loop")
							writeloop = true;
						else
							write = Int32.Parse (wval);
						continue;
					}
					else if (arg.StartsWith ("--")) {
						Usage ("Invalid option " + arg);
						return;
					}
					urls.Add (arg);
					break;
			}
		}

		if (readloop && writeloop) {
			Usage ("Can't loop on both read and write");
			return;
		}
		int loop = 1;
		if (readloop || writeloop) {
			// this is it meant to be stopped manually 
			loop = Int32.MaxValue;
		}

		if (urls.Count == 0) {
			Usage ("no URL were specified");
			return;
		}

		for (int i = 0; i < loop; i++) {
			if (readloop || writeloop)
				Console.WriteLine ("*** LOOP {0} ***", i);

			foreach (string url in urls) {
				Console.WriteLine ("{0}{1}", Environment.NewLine, url);
				string content = null;
				DateTime start = DateTime.Now;
				
				Uri uri = new Uri (url);
				if (uri.Scheme != Uri.UriSchemeHttps)
					throw new NotSupportedException ("Stream only works with HTTPS protocol");
				ControlledNetworkStream ns = null;

				try {
					IPHostEntry host = Dns.Resolve (uri.Host);
					IPAddress ip = host.AddressList [0];
					Socket socket = new Socket (ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					socket.Connect (new IPEndPoint (ip, uri.Port));
					ns = new ControlledNetworkStream (socket, false);
					ns.MaximumRead = (readloop) ? i : read;
					ns.MaximumWrite = (writeloop) ? i : write;
					SslClientStream ssl = new SslClientStream (ns, uri.Host, false, protocol);
					ssl.ServerCertValidationDelegate += new CertificateValidationCallback (CertificateValidation);

					StreamWriter sw = new StreamWriter (ssl);
					sw.WriteLine ("GET {0}{1}", uri.AbsolutePath, Environment.NewLine);
					sw.Flush ();

					StreamReader sr = new StreamReader (ssl, Encoding.UTF8);
					content = sr.ReadToEnd ();
				}
				catch (Exception e) {
					// HResult is protected - but very useful in debugging
					PropertyInfo pi = e.GetType ().GetProperty ("HResult", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);
					Console.WriteLine ("FAILED: #{0}", (int)pi.GetValue (e, null));
					Console.WriteLine (e.ToString ());
					if (ns != null) {
						Console.WriteLine ("Bytes Read:  {0}", ns.CurrentRead);
						Console.WriteLine ("Max Read:    {0}", ns.MaximumRead);
						Console.WriteLine ("Bytes Write: {0}", ns.CurrentWrite);
						Console.WriteLine ("Max Write:   {0}", ns.MaximumWrite);
					}
				}

				TimeSpan ts = (DateTime.Now - start);
				if ((show) && (content != null)) {
					Console.WriteLine ("{0}{1}{0}", Environment.NewLine, content);
				}
				if (time) {
					Console.WriteLine ("Time: " + ts.ToString ());
				}
			}
		}
	}

	private static void ShowCertificateError (int error) 
	{
		string message = null;
		switch (error) {
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
		if (certificateErrors.Length > 0) {
			Console.WriteLine (certificate.ToString (true));
			// X509Certificate.ToString(true) doesn't show dates :-(
			Console.WriteLine ("\tValid From:  {0}", certificate.GetEffectiveDateString ());
			Console.WriteLine ("\tValid Until: {0}{1}", certificate.GetExpirationDateString (), Environment.NewLine);
			// multiple errors are possible using SslClientStream
			foreach (int error in certificateErrors) {
				ShowCertificateError (error);
			}
		}
		// whatever the reason we do not stop the SSL connection
		return true;
	}
}

