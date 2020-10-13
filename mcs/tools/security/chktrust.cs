//
// ChkTrust.cs: chktrust clone tool
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using Mono.Security.Authenticode;

[assembly: AssemblyTitle ("Mono CheckTrust")]
[assembly: AssemblyDescription ("Verify if a PE binary has a valid Authenticode(tm) signature")]

namespace Mono.Tools {

	class CheckTrust {

		static private void Header () 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: chktrust [options] filename{0}", Environment.NewLine);
			Console.WriteLine ("\t-q\tquiet mode (no gui)");
			Console.WriteLine ("\t-v\tverbose mode (display status for every steps)");
			Console.WriteLine ("\t-?\thelp (display this help message)");
		}

		// static methods
		static public int Check (string fileName, bool quiet, bool verbose) 
		{
			AuthenticodeDeformatter a = new AuthenticodeDeformatter (fileName);
			
			// debug
/*			FileStream fs = File.Open (fileName + ".sig", FileMode.Create, FileAccess.Write);
			fs.Write (a.Signature, 0, a.Signature.Length);
			fs.Close ();*/

			// get something shorter to display
			fileName = Path.GetFileName (fileName);

			if (verbose) {
				Console.WriteLine ("Verifying file {0} for Authenticode(tm) signatures...{1}", fileName, Environment.NewLine);
			}

			if (a.Timestamp == DateTime.MinValue) {
				// signature only valid if the certificate is valid
				Console.WriteLine ("WARNING! {0} is not timestamped!", fileName);
			}
			else if (verbose) {
				Console.WriteLine ("INFO! {0} was timestamped on {1}", fileName, a.Timestamp);
			}

			if (a.Reason > 0) {
				string msg = null;
				// FAILURES
				switch (a.Reason) {
					case 1:
						msg = "doesn't contain a digital signature";
						break;
					case 2:
						msg = "digital signature is invalid";
						break;
					case 3:
						msg = "countersignature (timestamp) is invalid";
						break;
					case 4:
						msg = "timestamp is outside certificate validity";
						break;
					case 5:
						msg = "use an unsupported hash algorithm. Verification is impossible";
						break;
					case 6:
						msg = "signature can't be traced back to a trusted root";
						break;
					case 7:
						msg = "couldn't find the certificate that signed the file";
						break;
					case 8:
						msg = "certificate is expired and no timestamp is present";
						break;
					default:
						msg = "unknown error";
						break;
				}
	
				Console.WriteLine ("ERROR! {0} {1}!{2}", fileName, msg, Environment.NewLine);
				return 1;
			}

			Console.WriteLine ("SUCCESS: {0} signature is valid{1}and can be traced back to a trusted root!{2}", fileName, Environment.NewLine, Environment.NewLine);
			return 0;
		}

		[STAThread]
		static int Main (string[] args) 
		{
			bool verbose = false;
			bool quiet = true;	// always true as we don't show UI
			bool help = false;
			string fileName = null;

			Header();
			try {
				for (int i=0; i < args.Length; i++) {
					switch (args[i]) {
						case "-q":
						case "-quiet":
							quiet = true;
							break;
						case "-v":
						case "-verbose":
							verbose = true;
							break;
						case "-h":
						case "-help":
						case "-?":
						case "/?":
							help = true;
							break;
						default:
							fileName = args [i];
							break;
					}
				}

				if ((help) || (fileName == null)) 
					Help ();
				else
					return Check (fileName, quiet, verbose);

			}
			catch (CryptographicException ce) {
				Console.WriteLine ("WARNING: " + ce.Message);
				Console.WriteLine ("ERROR: Trust evaluation is incomplete!");
			}
			catch (Exception e) {
				Console.WriteLine ("ERROR: " + e.ToString ());
				Help ();
			}
			Console.WriteLine ();
			return 1;
		}
	}
}
