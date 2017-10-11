using System;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;

namespace TlsCompat
{
	public class Validator
	{
		String CACertPrint;
		X509Certificate2 CACert;

		public Validator (X509Certificate2 CACert)
		{
			this.CACert = CACert;
			this.CACertPrint = CACert.Thumbprint;
		}

		public bool Callback (object sender, X509Certificate certificate,
		                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{

			chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
			chain.ChainPolicy.ExtraStore.Add (this.CACert);

			var success = true;
			try {
				success = chain.Build (new X509Certificate2 (certificate));

				if (!success) {
					foreach (var stat in chain.ChainStatus) {
						if (stat.Status == X509ChainStatusFlags.UntrustedRoot) {
							success = false;
							foreach (var elem in chain.ChainElements) {
								success = success || elem.Certificate.Thumbprint == CACertPrint; 
							}
						} else {
							Console.WriteLine ("Cert had error: {0}", stat.StatusInformation);
							success = false;
						}
					}
				}
				return success;
			} catch (ArgumentException e) {
				Console.WriteLine ("Cert was unreadable {0}", e.ToString ());
				success = false;
			}

			return success;
		}
	}

	public class ClientRunner
	{

		public static void Usage ()
		{
			Console.WriteLine ("Usage: mono Harness.exe client " +
			"< host >" +
			"< port >" +
			"< number of clients >" +
			"< CA cert path(.pfx) >" +
			"< CA cert pass >");
		}

		public static async Task Exec (String[] args)
		{
			if (args.Length < 5) {
				Usage ();
				return;
			}
			var host = args [1];
			var port = Convert.ToInt32 (args [2]);
			var numClients = Convert.ToInt32 (args [3]);

			var pass = args.Length > 5 ? args [5] : "";
			var cert = new X509Certificate2 (args [4], pass);
			var validator = new Validator (cert);

			Console.WriteLine ("Running sync");
			RunSync (host, port, validator);

			Console.WriteLine ("Running async");
			var waiters = new List<Task> ();

			for (int i = 0; i < numClients; i++) {
				waiters.Add (RunAsync (host, port, validator));
			}

			for (int i = 0; i < numClients; i++) {
				RunSync (host, port, validator);
			}

			waiters.ForEach (w => {
				w.Wait ();
			});
		}

		public static async Task RunAsync (String host, int port, Validator validator)
		{
			using (var client = new TcpClient ()) {
				client.Connect (host, port);

				using (SslStream secureStream = new SslStream (client.GetStream (), false, 
					                                new RemoteCertificateValidationCallback (validator.Callback))) {

					var done = secureStream.AuthenticateAsClientAsync (host);

					using (var writer = new StreamWriter (secureStream)) {
						await done;
						await writer.WriteLineAsync ("Client connected;");
					}
				}
			}
		}

		public static void RunSync (String host, int port, Validator validator)
		{
			using (var client = new TcpClient ()) {
				client.Connect (host, port);

				using (SslStream secureStream = new SslStream (client.GetStream (), false, 
					                                new RemoteCertificateValidationCallback (validator.Callback))) {

					secureStream.AuthenticateAsClient (host);
					using (var writer = new StreamWriter (secureStream)) {
						writer.WriteLine ("Client connected;");
					}
				}
			}
		}

	}

	public class ServerRunner
	{
		public static void Usage ()
		{
			Console.WriteLine ("Usage: mono Harness.exe server " +
			"< host >" +
			"< port >" +
			"< number of clients to stay running >" +
			"< cert path(.pfx) >" +
			"< cert pass >");
		}

		public static async Task Exec (String[] args)
		{
			if (args.Length < 5) {
				Usage ();
				return;
			}
			var ip = IPAddress.Parse (args [1]);
			var port = Convert.ToInt32 (args [2]);
			var clients = Convert.ToInt32 (args [3]);

			var pass = args.Length > 5 ? args [5] : "";
			var cert = new X509Certificate2 (args [4], pass);

			RunSync (ip, port, clients, cert);

			var aRunner = RunAsync (ip, port, clients, cert);
			await aRunner;
		}

		public static void RunSync (IPAddress ip, int port, int numClients, X509Certificate2 cert)
		{
			var listener = new TcpListener (ip, port);
			listener.Start ();
			Console.WriteLine ("started sync! numclients: {0}", numClients);

			while (numClients > 0) {
				using (var server = listener.AcceptTcpClient ()) {
					using (var secureStream = new SslStream (server.GetStream ())) {
						secureStream.AuthenticateAsServer (cert);

						using (var writer = new StreamWriter (secureStream)) {
							Console.WriteLine ("Console: Connected!");
							numClients--;
						}
					}
					server.Close ();
				}
			}

			Console.WriteLine ("sync listener stopped");
			listener.Stop ();
		}

		public static async Task RunAsync (IPAddress ip, int port, int numClients, X509Certificate2 cert)
		{
			var listener = new TcpListener (ip, port);

			listener.Start ();
			Console.WriteLine ("started async! numclients: {0}", numClients);

			while (numClients > 0) {
				using (var server = await listener.AcceptTcpClientAsync ()) {
					using (var secureStream = new SslStream (server.GetStream ())) {
						var done = secureStream.AuthenticateAsServerAsync (cert);

						using (var writer = new StreamWriter (secureStream)) {
							await done;
							Console.WriteLine ("Console: Connected!");
							done = writer.WriteLineAsync ("Connected!");
							numClients--;
							await done;
						}
					}
					server.Close ();
				}
			}

			Console.WriteLine ("async listener stopped");
			listener.Stop ();
		}
	}

	public class TlsCompat
	{

		public static void Usage ()
		{
			Console.WriteLine ("Usage: mono Harness.exe < server | client > < args >");
		}

		public static void Exec (String[] args)
		{
			Console.WriteLine ("Usage: mono Harness.exe < server | client > < args >");
		}

		public static void Main (String[] args)
		{
			if (args.Length < 1) {
				Usage ();
			} else if (args [0] == "server") {
				ServerRunner.Exec (args).Wait ();
			} else if (args [0] == "client") {
				ClientRunner.Exec (args).Wait ();
			} else {
				Usage ();
			}
		}
	}

}
