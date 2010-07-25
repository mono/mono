//
// SignCode.cs: secutil clone tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Mono.Security.Authenticode;
using Mono.Security.X509;

[assembly: AssemblyTitle("Mono SignCode")]
[assembly: AssemblyDescription("Sign assemblies and PE files using Authenticode(tm).")]

namespace Mono.Tools {

	class SignCode {

		static private void Header () 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: signcode [options] filename{0}", Environment.NewLine);
			Console.WriteLine ("\t-spc spc\tSoftware Publisher Certificate file");
			Console.WriteLine ("\t-v pvk\t\tPrivate Key file");
			Console.WriteLine ("\t-a md5 | sha1\tHash Algorithm (default: MD5)");
			Console.WriteLine ("\t-$ indivisual | commercial\tSignature type");
			Console.WriteLine ("\t-n description\tDescription for the signed file");
			Console.WriteLine ("\t-i url\tURL for the signed file");
			Console.WriteLine ("Timestamp options");
			Console.WriteLine ("\t-t url\tTimestamp service http URL");
			Console.WriteLine ("\t-tr #\tNumber of retries for timestamp");
			Console.WriteLine ("\t-tw #\tDelay between retries");
			Console.WriteLine ("\t-x\tOnly timestamp (no signature)");
			Console.WriteLine ("CSP options");
			Console.WriteLine ("\t-k name\tKey Container Name");
			Console.WriteLine ("\t-p name\tProvider Name");
			Console.WriteLine ("\t-y #\tProvider Type");
			Console.WriteLine ("\t-ky [signature|exchange|#]\tKey Type");
			Console.WriteLine ("\t-r [localMachine|currentUser]\tKey Location");
		}

		static private RSA GetPrivateKey (string keyfile, CspParameters csp)
		{
			RSA rsa = null;

			if (keyfile != null) {
				if (!File.Exists (keyfile)) {
					Console.WriteLine ("Couldn't find '{0}' file.", keyfile);
					return null;
				}

				try {
					PrivateKey pvk = PrivateKey.CreateFromFile (keyfile);
					rsa = pvk.RSA;
				}
				catch (CryptographicException) {
					Console.WriteLine ("Enter password for {0}: ", keyfile);
					string password = Console.ReadLine ();
					try {
						PrivateKey pvk = PrivateKey.CreateFromFile (keyfile, password);
						rsa = pvk.RSA;
					}
					catch (CryptographicException) {
						Console.WriteLine ("Invalid password!");
					}
				}
			}
			else {
				rsa = new RSACryptoServiceProvider (csp);
			}

			return rsa;
		}

		static private X509CertificateCollection GetCertificates (string spcfile)
		{
			if (spcfile == null) {
				Console.WriteLine ("Missing SPC (certificate) file.");
				return null;
			}
			if (!File.Exists (spcfile)) {
				Console.WriteLine ("Couldn't find '{0}' file.", spcfile);
				return null;
			}

			SoftwarePublisherCertificate spc = SoftwarePublisherCertificate.CreateFromFile (spcfile);
			return spc.Certificates;
		}

		[STAThread]
		static int Main(string[] args)
		{
			Header ();
			if (args.Length < 1) {
				Help ();
				return 1;
			}

			CspParameters csp = new CspParameters ();
			string pvkFilename = null;
			string spcFilename = null;
			int timestampRetry = 1;
			int timestampDelay = 0;
			bool sign = true;

			// to be signed
			string tbsFilename = args [args.Length - 1];

			AuthenticodeFormatter af = new AuthenticodeFormatter ();

			int i = 0;
			while (i < args.Length - 1) {
				switch (args[i++]) {
					case "-spc":
						spcFilename = args [i++];
						break;
					case "-v":
						pvkFilename = args [i++];
						break;
					case "-a":
						af.Hash = args [i++];
						break;
					case "-$":
						string auth = args [i++].ToLower ();
						switch (auth) {
							case "individual":
								af.Authority = Authority.Individual;
								break;
							case "commercial":
								af.Authority = Authority.Commercial;
								break;
							default:
								Console.WriteLine ("Unknown authority {0}", auth);
								return 1;
						}
						break;
					case "-n":
						af.Description = args [i++];
						break;
					case "-i":
						af.Url = new Uri (args [i++]);
						break;
					// timestamp options
					case "-t":
						af.TimestampUrl = new Uri (args [i++]);
						break;
					case "-tr":
						timestampRetry = Convert.ToInt32 (args [i++]);
						break;
					case "-tw":
						timestampDelay = Convert.ToInt32 (args [i++]) * 1000;
						break;
					case "-x":
						// only timestamp
						sign = false;  
						break;
					// CSP provider options
					case "-k":
						csp.KeyContainerName = args [i++];
						break;
					case "-p":
						csp.ProviderName = args [i++];
						break;
					case "-y":
						csp.ProviderType = Convert.ToInt32 (args [i++]);
						break;
					case "-ky":
						string key = args [i++];
						switch (key) {
							case "signature":
								csp.KeyNumber = 0;
								break;
							case "exchange":
								csp.KeyNumber = 0;
								break;
							default:
								csp.KeyNumber = Convert.ToInt32 (key);
								break;
						}
						break;
					case "-r":
						string location = args [i++];
						switch (location) {
							case "localMachine":
								csp.Flags = CspProviderFlags.UseMachineKeyStore;
								break;
							case "currentUser":
								csp.Flags = CspProviderFlags.UseDefaultKeyContainer;
								break;
							default:
								Console.WriteLine ("Unknown location {0}", location);
								return 1;
						}
						break;
					// unsupported options
					case "-j":
					case "-jp":
						Console.WriteLine ("Unsupported option {0}", args[i-1]);
						return 1;
					// other options
					case "-?":
						Help ();
						return 0;
				}
			}

			// no need to continue if we can't find the assembly
			// to be signed (and/or timestamped)
			if (!File.Exists (tbsFilename)) {
				Console.WriteLine ("Couldn't find {0}.", tbsFilename);
				return 1;
			}

			if (sign) {
				RSA rsa = GetPrivateKey (pvkFilename, csp);
				if (rsa == null) {
					Console.WriteLine ("No private key available to sign the assembly.");
					return 1;
				}
				af.RSA = rsa;

				X509CertificateCollection certs = GetCertificates (spcFilename);
				if ((certs == null) || (certs.Count == 0)) {
					Console.WriteLine ("No certificates available to sign the assembly.");
					return 1;
				}
				af.Certificates.AddRange (certs);

				if (!af.Sign (tbsFilename)) {
					Console.WriteLine ("Couldn't sign file '{0}'.", tbsFilename);
					return 1;
				}
			} else if (af.TimestampUrl != null) {
				bool ts = false;
				// only timestamp an already signed file
				for (int j = 0; j < timestampRetry && !ts; j++) {
					ts = af.Timestamp (tbsFilename);
					// wait (unless it's the last try) and retry
					if (!ts && (j < timestampRetry - 1)) {
						Console.WriteLine ("Couldn't timestamp file '{0}', will retry in {1} ms", tbsFilename, timestampDelay);
						Thread.Sleep (timestampDelay);
					}
				}
				if (!ts) {
					Console.WriteLine ("Couldn't timestamp file '{0}' after {1} retries.", tbsFilename, timestampRetry);
					return 1;
				}
			} else {
				Help ();
				return 1;
			}

			Console.WriteLine ("Success");
			return 0;
		}
	}
}
