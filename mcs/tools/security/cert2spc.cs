//
// Cert2Spc.cs: cert2spc clone tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;

using Mono.Security.Authenticode;
using Mono.Security.X509;

[assembly: AssemblyTitle("Mono Cert2Spc")]
[assembly: AssemblyDescription("Transform a set of X.509 certificates and CRLs into an Authenticode(TM) \"Software Publisher Certificate\"")]

namespace Mono.Tools {

class Cert2Spc {

	static private string error;

	static private void Header () 
	{
		Console.WriteLine (new AssemblyInfo ().ToString ());
	}

	static private void Help () 
	{
		Console.WriteLine ("Usage: cert2spc certificate|crl [certificate|crl] [...] outputfile.spc{0}", Environment.NewLine);
	}

	// until we have real CRL support
	static byte[] GetFile (string filename) 
	{
		byte[] data = null;
		using (FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
			data = new byte [fs.Length];
			fs.Read (data, 0, data.Length);
			fs.Close ();
		}
		return data;
	}

	static int Process (string[] args) 
	{
		int nargs = args.Length - 1;
		if (nargs < 1) {
			error = "At least one input and output files must be specified";
			return 1;
		}

		string output = args [nargs];
		SoftwarePublisherCertificate spc = new SoftwarePublisherCertificate ();

		for (int i=0; i < args.Length - 1; i++) {
			switch (Path.GetExtension (args[i]).ToLower ()) {
				case ".cer":
				case ".crt":
					spc.Certificates.Add (new X509Certificate (GetFile (args[i])));
					break;
				case ".crl":
					spc.Crls.Add (GetFile (args[i]));
					break;
				default:
					error = "Unknown file extension : " + args[i];
					return 1;
			}
		}

		using (FileStream fs = File.Open (output, FileMode.Create, FileAccess.Write)) {
			byte[] data = spc.GetBytes ();
			fs.Write (data, 0, data.Length);
			fs.Close ();
		}
		return 0;
	}

	[STAThread]
	static int Main (string[] args) 
	{
		int result = 1;
		try {
			Header ();
			result = Process (args);

			if (error == null)
				Console.WriteLine ("Success");
			else {
				Console.WriteLine ("Error: {0}{1}", error, Environment.NewLine);
				Help ();
			}
		}
		catch (Exception e) {
			Console.WriteLine ("Error: " + e.ToString ());
			Help ();
		}
		return result;
	}
}

}
