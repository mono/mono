//
// Cert2Spc.cs: cert2spc clone tool
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Authenticode;

[assembly: AssemblyTitle("Mono Cert2Spc")]
[assembly: AssemblyDescription("Transform a set of X.509 certificates and CRLs into an Authenticode(TM) \"Software Publisher Certificate\"")]

namespace Mono.Tools {

class Cert2Spc {

	static private string error;

	static private void Header () 
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		AssemblyName an = a.GetName ();
	
		object [] att = a.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
		string title = ((att.Length > 0) ? ((AssemblyTitleAttribute) att [0]).Title : "Mono SecUtil");

		att = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
		string copyright = ((att.Length > 0) ? ((AssemblyCopyrightAttribute) att [0]).Copyright : "");

		Console.WriteLine ("{0} {1}", title, an.Version.ToString ());
		Console.WriteLine ("{0}{1}", copyright, Environment.NewLine);
	}

	static private void Help () 
	{
		Console.WriteLine ("Usage: cert2spc certificate|crl [certificate|crl] [...] outputfile.spc{0}", Environment.NewLine);
	}

	// until we have real CRL support
	static byte[] GetFile (string filename) 
	{
		FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] data = new byte [fs.Length];
		fs.Read (data, 0, data.Length);
		fs.Close ();
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
					spc.Certificates.Add (X509Certificate.CreateFromCertFile (args[i]));
					break;
				case ".crl":
					spc.CRLs.Add (GetFile (args[i]));
					break;
				default:
					error = "Unknown file extension : " + args[i];
					return 1;
			}
		}

		FileStream fs = File.Open (output, FileMode.Create, FileAccess.Write);
		byte[] data = spc.GetBytes ();
		fs.Write (data, 0, data.Length);
		fs.Close ();
		return 0;
	}

	[STAThread]
	static int Main (string[] args) 
	{
		int result = 1;
		try {
			Header();
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
