//
// SecUtil.cs: secutil clone tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Text;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

[assembly: AssemblyTitle("Mono SecUtil")]
[assembly: AssemblyDescription("Extract strongname and X509 certificates from assemblies.")]

namespace Mono.Tools {

class SecUtil {

	static private bool hexDisplay;
	static private bool vbMode;
	static private string error;

	static private void WriteArray (byte[] array) 
	{
		StringBuilder sb = new StringBuilder ();
		if (hexDisplay) {
			sb.Append ("0x");
			for (int i=0; i < array.Length; i++) 
				sb.Append (array [i].ToString ("X2"));
		}
		else {
			sb.Append ((vbMode ? "( " : "{ "));
			for (int i=0; i < array.Length; i++) {
				sb.Append (array [i]);
				if (i != array.Length-1)
					sb.Append (", ");
			}
			sb.Append ((vbMode ? " )" : " }"));
		}
		Console.WriteLine (sb.ToString ());
	}

	static private void StrongName (string fileName) 
	{
		AssemblyName an = AssemblyName.GetAssemblyName (fileName);
		byte[] key = an.GetPublicKey ();

		if (key == null) {
			error = "Error: Assembly has no strong name";
		} else {
			Console.WriteLine ("PublicKey =");
			WriteArray (key);
			Console.WriteLine ("Name =");
			Console.WriteLine (an.Name);
			Console.WriteLine ("Version =");
			Console.WriteLine (an.Version.ToString ());
		}
	}

	static private void Certificate (string fileName) 
	{
		X509Certificate x509 = X509Certificate.CreateFromSignedFile (fileName);
		if (x509 == null)
			error = "Error: Specified file isn't signed";
		else {
			Console.WriteLine ("X.509 Certificate =");
			WriteArray (x509.GetRawCertData ());
		}
	}

	static private void Header () 
	{
		Console.WriteLine (new AssemblyInfo ().ToString ());
	}

	static private void Help () 
	{
		Console.WriteLine ("Usage: secutil [options] [filename]{0}", Environment.NewLine);
		Console.WriteLine ("secutil -s assembly");
		Console.WriteLine ("secutil -strongname assembly");
		Console.WriteLine ("\tShow strongname informations about the assembly{0}", Environment.NewLine);
		Console.WriteLine ("secutil -x assembly");
		Console.WriteLine ("secutil -x509certificate assembly");
		Console.WriteLine ("\tShow the X509 Authenticode certificate for the assembly{0}", Environment.NewLine);
		Console.WriteLine ("secutil -hex");
		Console.WriteLine ("\tShow data in hexadecimal{0}", Environment.NewLine);
		Console.WriteLine ("secutil -a");
		Console.WriteLine ("secutil -array");
		Console.WriteLine ("\tShow data in a decimal array (default){0}", Environment.NewLine);
		Console.WriteLine ("secutil -v");
		Console.WriteLine ("secutil -vbmode");
		Console.WriteLine ("\tShow data in a VisualBasic friendly format{0}", Environment.NewLine);
		Console.WriteLine ("secutil -c");
		Console.WriteLine ("secutil -cmode");
		Console.WriteLine ("\tShow data in a C/C++/C# friendly format (default){0}", Environment.NewLine);
		Console.WriteLine ("secutil -h");
		Console.WriteLine ("secutil -help");
		Console.WriteLine ("secutil -?");
		Console.WriteLine ("secutil /?");
		Console.WriteLine ("\tShow help about this tool{0}", Environment.NewLine);
	}

	[STAThread]
	static void Main (string[] args)
	{
		bool sn = false;
		bool cert = false;
		bool help = false;
		string fileName = null;

		Header();

		try {
			for (int i=0; i < args.Length - 1; i++) {
				switch (args[i]) {
				case "-s":
				case "-strongname":
					sn = true;
					fileName = args[i+1];
					break;
				case "-x":
				case "-x509certificate":
					cert = true;
					fileName = args[i+1];
					break;
				case "-hex":
					hexDisplay = true;
					break;
				case "-a":
				case "-array":
					hexDisplay = false;
					break;
				case "-v":
				case "-vbmode":
					vbMode = true;
					break;
				case "-c":
				case "-cmode":
					vbMode = false;
					break;
				case "-h":
				case "-help":
				case "-?":
				case "/?":
					help = true;
					break;
				}
			}

			if (help)
				Help ();
			if (sn)
				StrongName (fileName);
			else if (cert)
				Certificate (fileName);
			else
				Help ();

			Console.WriteLine ((error == null) ? "Success" : error);
		}
		catch (Exception e) {
			Console.WriteLine ("Error: " + e.ToString ());
		}
	}
}

}
