//
// cert-sync.cs: Import the root certificates from Linux SSL store into Mono
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Jo Shields <jo.shields@xamarin.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2014 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.X509;

[assembly: AssemblyTitle ("Linux Cert Store Sync")]
[assembly: AssemblyDescription ("Synchronize local certs with certs from local Linux trust store.")]

namespace Mono.Tools
{

	class CertSync
	{
	
		static string inputFile;
		static bool quiet;

		static X509Certificate DecodeCertificate (string s)
		{
			byte[] rawdata = Convert.FromBase64String (s);
			return new X509Certificate (rawdata);
		}

		static Stream GetFile ()
		{
			try {
				if (inputFile != null) {
					return File.OpenRead (inputFile);
				} else {
					return null;
				}
			} catch {
				return null;
			}
		}

		static X509CertificateCollection DecodeCollection ()
		{
			X509CertificateCollection roots = new X509CertificateCollection ();
			StringBuilder sb = new StringBuilder ();
			bool processing = false;

			using (Stream s = GetFile ()) {
				if (s == null) {
					WriteLine ("Couldn't retrieve the file using the supplied information.");
					return null;
				}

				StreamReader sr = new StreamReader (s);
				while (true) {
					string line = sr.ReadLine ();
					if (line == null)
						break;

					if (processing) {
						if (line.StartsWith ("-----END CERTIFICATE-----")) {
							processing = false;
							X509Certificate root = DecodeCertificate (sb.ToString ());
							roots.Add (root);

							sb = new StringBuilder ();
							continue;
						}
						sb.Append (line);
					} else {
						processing = line.StartsWith ("-----BEGIN CERTIFICATE-----");
					}
				}
				return roots;
			}
		}

		static int Process ()
		{
			X509CertificateCollection roots = DecodeCollection ();
			if (roots == null) {
				return 1;
			} else if (roots.Count == 0) {
				WriteLine ("No certificates were found.");
				return 0;
			}
				
			X509Stores stores = (X509StoreManager.LocalMachine);
			X509CertificateCollection trusted = stores.TrustedRoot.Certificates;
			int additions = 0;
			WriteLine ("I already trust {0}, your new list has {1}", trusted.Count, roots.Count);
			foreach (X509Certificate root in roots) {
				if (!trusted.Contains (root)) {
					try {
						stores.TrustedRoot.Import (root);
						WriteLine ("Certificate added: {0}", root.SubjectName);
					} catch {
						WriteLine ("Warning: Could not import {0}");
					}
					additions++;
				}
			}
			if (additions > 0)
				WriteLine ("{0} new root certificates were added to your trust store.", additions);

			X509CertificateCollection removed = new X509CertificateCollection ();
			foreach (X509Certificate trust in trusted) {
				if (!roots.Contains (trust)) {
					removed.Add (trust);
				}
			}
			if (removed.Count > 0) {
				WriteLine ("{0} previously trusted certificates were removed.", removed.Count);

				foreach (X509Certificate old in removed) {
					stores.TrustedRoot.Remove (old);
					WriteLine ("Certificate removed: {0}", old.SubjectName);
				}
			}
			WriteLine ("Import process completed.");
			return 0;
		}

		static string Thumbprint (string algorithm, X509Certificate certificate)
		{
			HashAlgorithm hash = HashAlgorithm.Create (algorithm);
			byte[] digest = hash.ComputeHash (certificate.RawData);
			return BitConverter.ToString (digest);
		}

		static bool ParseOptions (string[] args)
		{
			if (args.Length < 1)
				return false;

			for (int i = 0; i < args.Length - 1; i++) {
				switch (args [i]) {
				case "--quiet":
					quiet = true;
					break;
				default:
					WriteLine ("Unknown option '{0}'.");
					return false;
				}
			}
			inputFile = args [args.Length - 1];
			if (!File.Exists (inputFile)) {
				WriteLine ("Unknown option or file not found '{0}'.");
				return false;
			}
			return true;
		}

		static void Header ()
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static void Help ()
		{
			Console.WriteLine ("Usage: cert-sync [--quiet] system-ca-bundle.crt");
			Console.WriteLine ("Where system-ca-bundle.crt is in PEM format");
		}

		static void WriteLine (string str)
		{
			if (!quiet)
				Console.WriteLine (str);
		}

		static void WriteLine (string format, params object[] args)
		{
			if (!quiet)
				Console.WriteLine (format, args);
		}

		static int Main (string[] args)
		{
			try {
				if (!ParseOptions (args)) {
					Header ();
					Help ();
					return 1;
				}
				if (!quiet) {
					Header ();
				}
				return Process ();
			} catch (Exception e) {
				// ignore quiet on exception
				Console.WriteLine ("Error: {0}", e);
				return 1;
			}
		}
	}
}