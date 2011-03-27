//
// crlupdate.cs: CRL downloader / updater
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using System.Net;
using System.Reflection;

using Mono.Security.X509;
using Mono.Security.X509.Extensions;

[assembly: AssemblyTitle ("Mono CRL Updater")]
[assembly: AssemblyDescription ("Download and update X.509 certificate revocation lists from your stores.")]

namespace Mono.Tools {

	class CrlUpdater {

		static private void Header ()
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static private void Help ()
		{
			Console.WriteLine ("Usage: crlupdate [-m] [-v] [-f] [-?]");
			Console.WriteLine ();
			Console.WriteLine ("\t-m\tuse the machine certificate store (default to user)");
			Console.WriteLine ("\t-v\tverbose mode (display status for every steps)");
			Console.WriteLine ("\t-f\tforce download (and replace existing CRL)");
			Console.WriteLine ("\t-?\tDisplay this help message");
			Console.WriteLine ();
		}

		static void Download (string url, X509Store store)
		{
			if (verbose)
				Console.WriteLine ("Downloading: {0}", url);

			WebClient wc = new WebClient ();
			string error = "download";
			try {
				byte [] data = wc.DownloadData (url);
				error = "decode";
				X509Crl crl = new X509Crl (data);
				error = "import";
				store.Import (crl);
			}
			catch (Exception e) {
				Console.WriteLine ("ERROR: could not {0}: {1}", error, url);
				if (verbose) {
					Console.WriteLine (e);
					Console.WriteLine ();
				}
			}
		}

		static byte [] GetAuthorityKeyIdentifier (X509Extension ext)
		{
			if (ext == null)
				return null;

			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension (ext);
			return aki.Identifier;
		}

		static byte [] GetSubjectKeyIdentifier (X509Extension ext)
		{
			if (ext == null)
				return null;

			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
			return ski.Identifier;
		}

		static bool Compare (byte [] a, byte [] b)
		{
			if (a == null)
				return (b == null);
			else if (b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++) {
				if (a [i] != b [i])
					return false;
			}
			return true;
		}

		static X509Crl FindCrl (X509Certificate cert, X509Store store)
		{
			string name = cert.SubjectName;
			byte [] ski = GetSubjectKeyIdentifier (cert.Extensions ["2.5.29.14"]);
			foreach (X509Crl crl in store.Crls) {
				if (crl.IssuerName != cert.SubjectName)
					continue;
				if ((ski == null) || Compare (ski, GetAuthorityKeyIdentifier (crl.Extensions ["2.5.29.35"])))
					return crl;
			}
			return null;
		}

		static void UpdateStore (X509Store store)
		{
			// for each certificate
			foreach (X509Certificate cert in store.Certificates) {

				// do we already have a matching CRL ? (or are we forced to download?)
				X509Crl crl = force ? null : FindCrl (cert, store);
				// without a CRL (or with a CRL in need of updating)
				if ((crl == null) || !crl.IsCurrent) {
					X509Extension ext = cert.Extensions ["2.5.29.31"];
					if (ext == null) {
						if (verbose)
							Console.WriteLine ("WARNING: No cRL distribution point found for '{0}'", cert.SubjectName);
						continue;
					}

					CRLDistributionPointsExtension crlDP = new CRLDistributionPointsExtension (ext);
					foreach (var dp in crlDP.DistributionPoints) {
						string name = dp.Name.Trim ();
						if (name.StartsWith ("URL="))
							Download (name.Substring (4), store);
						else if (verbose)
							Console.WriteLine ("WARNING: Unsupported distribution point: '{0}'", name);
					}
				}
			}
		}

		static bool verbose = false;
		static bool force = false;

		static int Main (string [] args)
		{
			bool machine = false;

			for (int i = 0; i < args.Length; i++) {
				switch (args [i]) {
				case "-m":
				case "--m":
					machine = true;
					break;
				case "-v":
				case "--v":
					verbose = true;
					break;
				case "-f":
				case "--f":
					force = true;
					break;
				case "-help":
				case "--help":
				case "-?":
				case "--?":
					Help ();
					return 0;
				}
			}

			try {
				X509Stores stores = ((machine) ? X509StoreManager.LocalMachine : X509StoreManager.CurrentUser);
				// for all store (expect Untrusted)
				UpdateStore (stores.TrustedRoot);
				UpdateStore (stores.IntermediateCA);
				UpdateStore (stores.Personal);
				UpdateStore (stores.OtherPeople);
				return 0;
			}
			catch (Exception e) {
				Console.WriteLine ("ERROR: Unexpected exception: {0}", e);
				return 1;
			}
		}
	}
}
