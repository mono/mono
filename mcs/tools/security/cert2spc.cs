//
// Cert2Spc.cs: cert2spc clone tool
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Reflection;
using Mono.Security.ASN1;

[assembly: AssemblyTitle("Mono Cert2Spc")]
[assembly: AssemblyDescription("Transform a chain of certificate into an Authenticode(TM) \"Software Publisher Certificate\"")]
[assembly: AssemblyCompany("Sébastien Pouliot, Motus Technologies")]
[assembly: AssemblyProduct("Open Source Tools for .NET")]
[assembly: AssemblyCopyright("Copyright 2002 Motus Technologies. Released under BSD license.")]
[assembly: AssemblyVersion("0.17.99.0")]

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

	static void Process (string[] args) 
	{
		if (args.Length < 2) {
			error = "At least one input and output files must be specified";
			return;
		}

		string outFile = args [args.Length - 1];
		// build certificate/crl list
		ASN1 listOfCerts = new ASN1 (0xA0, null);
		for (int i=0; i < args.Length - 1; i++) {
			FileStream fs = new FileStream (args[i], FileMode.Open, FileAccess.Read);
			byte[] cert = new byte [fs.Length];
			fs.Read (cert, 0, cert.Length);
			listOfCerts.Add (new ASN1(cert));
		}

		// compose header
		ASN1 integer = new ASN1 (0x02, null);
		integer.Value = new byte[1];
		integer.Value[0] = 1;

		ASN1 seqOID = new ASN1 (0x30, null);
		seqOID.Add (new OID ("1.2.840.113549.1.7.1"));

		ASN1 sequence = new ASN1 (0x30, null);
		sequence.Add (integer);
		sequence.Add (new ASN1 (0x31, null)); // empty set
		sequence.Add (seqOID);
		sequence.Add (listOfCerts);
		sequence.Add (new ASN1 (0x31, null)); // empty set

		ASN1 a0 = new ASN1 (0xA0, null);
		a0.Add (sequence);

		ASN1 spc = new ASN1 (0x30, null);
		spc.Add (new OID ("1.2.840.113549.1.7.2"));
		spc.Add (a0);

		// write output file
		FileStream spcFile = new FileStream (outFile, FileMode.Create, FileAccess.Write);
		byte[] rawSpc = spc.GetBytes ();
		spcFile.Write (rawSpc, 0, rawSpc.Length);
		spcFile.Close ();
	}

	[STAThread]
	static void Main (string[] args) 
	{
		try {
			Header();
			Process (args);

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
	}
}

}
