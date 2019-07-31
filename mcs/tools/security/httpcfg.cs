//
// httpcfg.cs: manages certificates used by HttpListener
//
// Authors:
//	Gonzalo Paniagua Javier <gonzalo@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Authenticode;

[assembly: AssemblyTitle ("Mono Certificate Management for HttpListener use")]
[assembly: AssemblyDescription ("Manage X.509 certificates to be used in HttpListener.")]

namespace Mono.Tools {
	class HttpCfg {
		enum Action {
			None,
			Add,
			Delete,
			List
		}

		static Action action;
		static string pvkfile;
		static string certfile;
		static string p12file;
		static string passwd;
		static ushort port;

		static void Help (bool exit)
		{
			Console.WriteLine ("Usage is:\n" + 
					   "\thttpcfg -add -port NN [-cert CERT -pvk PVK] [-p12 P12 -pwd PASSWORD]\n" +
					   "\thttpcfg -del -port NN\n" +
					   "\thttpcfg -list");
			if (exit)
				Environment.Exit (1);
		}

		static void ProcessArguments (string [] args)
		{
			for (int i = 0; i < args.Length; i++){
				string arg = args [i];
				switch (arg){
				case "-add":
					if (action != Action.None) {
						Console.Error.WriteLine ("error: conflicting options.");
						Help (true);
					}
					action = Action.Add;
					break;
				case "-del":
				case "-delete":
					if (action != Action.None) {
						Console.Error.WriteLine ("error: conflicting options.");
						Help (true);
					}
					action = Action.Delete;
					break;
				case "-list":
					if (action != Action.None) {
						Console.Error.WriteLine ("error: conflicting options.");
						Help (true);
					}
					action = Action.List;
					break;
				case "-port":
					if (port != 0) {
						Console.Error.WriteLine ("error: more than one port specified.");
						Help (true);
					}

					try {
						port = Convert.ToUInt16 (args [++i]);
					} catch (IndexOutOfRangeException) {
						Console.Error.WriteLine ("Error: no port specified.");
						Help (true);
					} catch {
						Console.Error.WriteLine ("Error: invalid port.");
						Help (true);
					}
					break;
				
				case "-p12":
					if (p12file != null) {
						Console.Error.WriteLine ("error: more than one p12 file specified.");
						Help (true);
					}

					if (pvkfile != null || certfile != null) {
						Console.Error.WriteLine ("error: use either -p12 or -pvk and -cert.");
						Help (true);
					}
					p12file = args [++i];
					break;
				
				case "-pvk":
					if (pvkfile != null) {
						Console.Error.WriteLine ("error: more than one PVK file specified.");
						Help (true);
					}
					
					if (p12file != null) {
						Console.Error.WriteLine ("error: use either -p12 or -pvk and -cert.");
						Help (true);
					}
					
					pvkfile = args [++i];
					break;
				case "-cert":
					if (certfile != null) {
						Console.Error.WriteLine ("error: more than one CER file specified.");
						Help (true);
					}
					
					if (p12file != null) {
						Console.Error.WriteLine ("error: use either -p12 or -pvk and -cert.");
						Help (true);
					}
					
					certfile = args [++i];
					break;
				
				case "-pwd":
					if (passwd != null) {
						Console.Error.WriteLine ("error: more than one password specified.");
						Help (true);
					}
					passwd = args [++i];
					break;
				
				default:
					Console.Error.WriteLine ("error: Unknown argument: {0}", arg);
					Help (true);
					break;
				}
			}

			if (action == Action.None) {
				Console.Error.WriteLine ("error: no action specified.");
				Help (true);
			}

			if ((pvkfile != null && certfile == null) || (pvkfile == null && certfile != null)) {
				Console.Error.WriteLine ("error: -cert and -pvk must be used.");
				Help (true);
			}

			if (action != Action.List && port == 0) {
				Console.Error.WriteLine ("error: -port is missing or bogus.");
				Help (true);
			}

			if (action == Action.Delete && (pvkfile != null || certfile != null || p12file != null)) {
			//if (action == Action.Delete && (pvkfile != null || certfile != null)) {
				Console.Error.WriteLine ("error: -delete only expects a -port option.");
				Help (true);
			}
		}

		
		static void AddP12 (string path, string filename, string password, ushort port)
		{
			X509Certificate2 x509 = null;
			try {
				x509 = new X509Certificate2 (filename, password);
			} catch (Exception e) {
				Console.Error.WriteLine ("error loading certificate [{0}]", e.Message);
				Help (true);
			}

			string target_cert = Path.Combine (path, String.Format ("{0}.cer", port));
			if (File.Exists (target_cert)) {
				Console.Error.WriteLine ("error: there is already a certificate for that port.");
				Help (true);
			}
			string target_pvk = Path.Combine (path, String.Format ("{0}.pvk", port));
			if (File.Exists (target_pvk)) {
				Console.Error.WriteLine ("error: there is already a certificate for that port.");
				Help (true);
			}

			using (Stream cer = File.OpenWrite (target_cert)) {
				byte [] raw = x509.RawData;
				cer.Write (raw, 0, raw.Length);
			}
			
			PrivateKey pvk = new PrivateKey();
			pvk.RSA = x509.PrivateKey as RSA;
			pvk.Save(target_pvk);			
		}
		

		static void AddCertPvk (string path, string cert, string pvk, ushort port)
		{
			try {
				X509Certificate2 x509 = new X509Certificate2 (cert);
				var privateKey = PrivateKey.CreateFromFile (pvk).RSA;
				x509 = x509.CopyWithPrivateKey ((RSA)privateKey);
			} catch (Exception e) {
				Console.Error.WriteLine ("error loading certificate or private key [{0}]", e.Message);
				Help (true);
			}

			string target_cert = Path.Combine (path, String.Format ("{0}.cer", port));
			if (File.Exists (target_cert)) {
				Console.Error.WriteLine ("error: there is already a certificate for that port.");
				Help (true);
			}
			string target_pvk = Path.Combine (path, String.Format ("{0}.pvk", port));
			if (File.Exists (target_pvk)) {
				Console.Error.WriteLine ("error: there is already a certificate for that port.");
				Help (true);
			}
			File.Copy (cert, target_cert);
			File.Copy (pvk, target_pvk);
		}

		static void Delete (string path, ushort port)
		{
			string pattern = String.Format ("{0}.*", port);
			string [] files = Directory.GetFiles (path, pattern);
			foreach (string f in files) {
				try {
					File.Delete (f);
				} catch (Exception e) {
					Console.Error.WriteLine ("error removing file {0} [{1}].", f, e.Message);
				}
			}
		}

		static void List (string path)
		{
			string [] files = Directory.GetFiles (path, "*");
			foreach (string f in files) {
				if (f.EndsWith (".cer")) {
					X509Certificate2 x509 = new X509Certificate2 (f);
					Console.WriteLine ("Port: {0} Thumbprint: {1}", Path.GetFileNameWithoutExtension (f), x509.Thumbprint);
				}
			}
		}

		static int Main (string[] args)
		{
			try {
				ProcessArguments (args);
			} catch (IndexOutOfRangeException) {
				Console.Error.WriteLine ("error: missing argument.");
				Help (true);
			}

			if (action == Action.None) {
				Help (true);
			}

			string dirname = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string path = Path.Combine (dirname, ".mono");
			path = Path.Combine (path, "httplistener");
			if (false == Directory.Exists (path)) {
				try {
					Directory.CreateDirectory (path);
				} catch (Exception e) {
					Console.Error.WriteLine ("error: creating directory {0} [{1}]", path, e.Message);
					return 1;
				}
			}

			switch (action) {
			case Action.Add:
				if (p12file != null)
					AddP12 (path, p12file, passwd, port);
				else
					AddCertPvk (path, certfile, pvkfile, port);
				break;
			case Action.Delete:
				Delete (path, port);
				break;
			case Action.List:
				List (path);
				break;
			}
			return 0;
		}
	}
}

