//
// SN.cs: sn clone tool
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
using System.Text;

using Mono.Security;
using Mono.Security.Cryptography;

[assembly: AssemblyTitle("Mono StrongName")]
[assembly: AssemblyDescription("StrongName utility for signing assemblies")]

namespace Mono.Tools {

	class SN {

		static private void Header () 
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			AssemblyName an = a.GetName ();
		
			object [] att = a.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			string title = ((att.Length > 0) ? ((AssemblyTitleAttribute) att [0]).Title : "Mono StrongName");

			att = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			string copyright = ((att.Length > 0) ? ((AssemblyCopyrightAttribute) att [0]).Copyright : "");

			Console.WriteLine ("{0} {1}", title, an.Version.ToString ());
			Console.WriteLine ("{0}{1}", copyright, Environment.NewLine);
		}

		static string defaultCSP = null;

		// TODO
		static bool LoadConfig () 
		{
			// default CSP
			return false;
		}

		// TODO
		static int SaveConfig () 
		{
			// default CSP
			return 1;
		}

		static byte[] ReadFromFile (string fileName) 
		{
			byte[] data = null;
			FileStream fs = File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			try {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
			}
			finally {
				fs.Close ();
			}
			return data;
		}

		static void WriteToFile (string fileName, byte[] data) 
		{
			FileStream fs = File.Open (fileName, FileMode.Create, FileAccess.Write);
			try {
				fs.Write (data, 0, data.Length);
			}
			finally {
				fs.Close ();
			}
		}

		static void WriteCSVToFile (string fileName, byte[] data, string mask) 
		{
			StreamWriter sw = File.CreateText (fileName);
			try {
				for (int i=0; i < data.Length; i++) {
					if (mask [0] == 'X')
						sw.Write ("0x");
					sw.Write (data [i].ToString (mask));
					sw.Write (", ");
				}
			}
			finally {
				sw.Close ();
			}
		}

		static string ToString (byte[] data) 
		{
			StringBuilder sb = new StringBuilder ();
			for (int i=0; i < data.Length; i++) {
				if ((i % 39 == 0) && (data.Length > 39))
					sb.Append (Environment.NewLine);
				sb.Append (data [i].ToString ("x2"));
				if (i > 1000) {
					sb.Append (" !!! TOO LONG !!!");
					break;
				}
			}
			return sb.ToString ();
		}

		// is assembly signed (or delayed signed) ?
		static bool IsStrongNamed (Assembly assembly) 
		{
			if (assembly == null)
				return false;

			object[] attrs = assembly.GetCustomAttributes (true);
			foreach (object o in attrs) {
				if (o is AssemblyKeyFileAttribute)
					return true;
				else if (o is AssemblyKeyNameAttribute)
					return true;
			}
			return false;
		}

		static void ReSign (string assemblyName, RSA key) 
		{
			// this doesn't load the assembly (well it unloads it ;)
			// http://weblogs.asp.net/nunitaddin/posts/9991.aspx
			AssemblyName an = AssemblyName.GetAssemblyName (assemblyName);
			if (an == null) {
				Console.WriteLine ("Unable to load assembly: {0}", assemblyName);
				return;
			}

			StrongName sign = new StrongName (key);
			// try to compare public key
			bool same = Compare (sign.PublicKey, an.GetPublicKey ());
			if (!same) {
				// second chance, try to compare public key token
				same = Compare (sign.PublicKeyToken, an.GetPublicKeyToken ());
			}

			if ((same) && (an.Flags == AssemblyNameFlags.PublicKey)) {
				if (sign.Sign (assemblyName))
					Console.WriteLine ("Assembly signed");
				else
					Console.WriteLine ("Couldn't sign the assembly");
			}
			else
				Console.WriteLine ("There is no public key present in assembly {0}", assemblyName);
		}

		static void Verify (string assemblyName, StrongName sn) 
		{
			// this doesn't load the assembly (well it unloads it ;)
			// http://weblogs.asp.net/nunitaddin/posts/9991.aspx
			AssemblyName an = AssemblyName.GetAssemblyName (assemblyName);
			if (an == null) {
				Console.WriteLine ("Unable to load assembly: {0}", assemblyName);
				return;
			}

			if (an.Flags != AssemblyNameFlags.PublicKey) {
				Console.WriteLine ("There is no public key present in assembly {0}", assemblyName);
				return;
			}

			if (sn.Verify (assemblyName))
				Console.WriteLine ("Assembly {0} is strongnamed.", assemblyName);
			else
				Console.WriteLine ("Assembly {0} isn't strongnamed", assemblyName);
		}

		static bool Compare (byte[] value1, byte[] value2) 
		{
			if ((value1 == null) || (value2 == null))
				return false;
			bool result = (value1.Length == value2.Length);
			if (result) {
				for (int i=0; i < value1.Length; i++) {
					if (value1 [i] != value2 [i])
						return false;
				}
			}
			return result;
		}

		static void Help (string details) 
		{
			Console.WriteLine ("Usage: sn [-q | -quiet] options [parameters]{0}", Environment.NewLine);
			Console.WriteLine (" -q | -quiet    \tQuiet mode (minimal display){0}", Environment.NewLine);
			switch (details) {
				case "config":
					Console.WriteLine ("Configuration options <1>");
					Console.WriteLine (" -c provider{0}\tChange the default CSP provider", Environment.NewLine);
					Console.WriteLine (" -m [y|n]{0}\tUse a machine [y] key container or user key container [n]", Environment.NewLine);
					Console.WriteLine (" -Vl{0}\tList the verification options", Environment.NewLine);
					Console.WriteLine (" -Vr assembly [userlist]{0}\tExempt the specified assembly from verification for the user list", Environment.NewLine);
					Console.WriteLine (" -Vu assembly{0}\tRemove exemption entry for the specified assembly", Environment.NewLine);
					Console.WriteLine (" -Vx{0}\tRemove all exemptions entries", Environment.NewLine);
					break;
				case "csp":
					Console.WriteLine ("CSP related options <2>");
					Console.WriteLine (" -d container{0}\tDelete the specified key container", Environment.NewLine);
					Console.WriteLine (" -i keypair.snk container{0}\tImport the keypair from a SNK file into a CSP container", Environment.NewLine);
					Console.WriteLine (" -pc container public.key{0}\tExport the public key from a CSP container to the specified file", Environment.NewLine);
					break;
				case "convert":
					Console.WriteLine ("Convertion options");
					Console.WriteLine (" -e assembly output.pub{0}\tExport the assembly public key to the specified file", Environment.NewLine);
					Console.WriteLine (" -p keypair.snk output.pub{0}\tExport the public key from a SNK file to the specified file", Environment.NewLine);
					Console.WriteLine (" -o input output.txt{0}\tConvert the input file to a CSV file (using decimal).", Environment.NewLine);
					Console.WriteLine (" -oh input output.txt{0}\tConvert the input file to a CSV file (using hexadecimal).", Environment.NewLine);
					break;
				case "sn":
					Console.WriteLine ("StrongName signing options");
					Console.WriteLine (" -D assembly1 assembly2{0}\tCompare assembly1 and assembly2 (without signatures)", Environment.NewLine);
					Console.WriteLine (" -k keypair.snk{0}\tCreate a new keypair in the specified file", Environment.NewLine);
					Console.WriteLine (" -R assembly keypair.snk{0}\tResign the assembly with the specified StrongName key file", Environment.NewLine);
					Console.WriteLine (" -Rc assembly container{0}\tResign the assembly with the specified CSP container", Environment.NewLine);
					Console.WriteLine (" -t file{0}\tShow the public key from the specified file <1>", Environment.NewLine);
					Console.WriteLine (" -tp file{0}\tShow the public key and pk token from the specified file <1>", Environment.NewLine);
					Console.WriteLine (" -T assembly{0}\tShow the public key from the specified assembly", Environment.NewLine);
					Console.WriteLine (" -Tp assembly{0}\tShow the public key and pk token from the specified assembly", Environment.NewLine);
					Console.WriteLine (" -V assembly{0}\tVerify the specified assembly signature", Environment.NewLine);
					Console.WriteLine (" -Vf assembly{0}\tVerify the specified assembly signature (even if disabled).", Environment.NewLine);
					break;
				default:
					Console.WriteLine ("Help options");
					Console.WriteLine (" -? | -h        \tShow this help screen about the tool");
					Console.WriteLine (" -? | -h config \tConfiguration options (see strongname.xml)");
					Console.WriteLine (" -? | -h csp    \tCrypto Service Provider (CSP) related options");
					Console.WriteLine (" -? | -h convert\tFormat convertion options");
					Console.WriteLine (" -? | -h sn     \tStrongName signing options");
					break;
			}
			Console.WriteLine ("{0}<1> Currently not implemented in the tool", Environment.NewLine);
			Console.WriteLine ("<2> Implemented in the tool but not in Mono{0}", Environment.NewLine);
		}

		[STAThread]
		static int Main (string[] args)
		{
			if (args.Length < 1) {
				Header ();
				Help (null);
				return 1;
			}

			int i = 0;
			string param = args [i];
			bool quiet = ((param == "-quiet") || (param == "-q"));
			if (quiet)
				i++;
			else
				Header();

			bool config = LoadConfig ();

			StrongName sn = null;
			AssemblyName an = null;
			RSACryptoServiceProvider rsa = null;
			CspParameters csp = new CspParameters ();
			csp.ProviderName = defaultCSP;

			switch (args [i++]) {
				case "-c":
					// Change global CSP provider options
					defaultCSP = args [i];
					return SaveConfig ();
				case "-d":
					// Delete specified key container
					csp.KeyContainerName = args [i];
					rsa = new RSACryptoServiceProvider (csp);
					rsa.PersistKeyInCsp = false;
					if (!quiet)
						Console.WriteLine ("Keypair in container {0} has been deleted", args [i]);
					break;
				case "-D":
					StrongName a1 = new StrongName ();
					byte[] h1 = a1.Hash (args [i++]);
					StrongName a2 = new StrongName ();
					byte[] h2 = a2.Hash (args [i++]);
					if (Compare (h1, h2)) {
						Console.WriteLine ("Both assembly are identical (same digest for metadata)");
						// TODO: if equals then compare signatures
					}
					else
						Console.WriteLine ("Assemblies are not identical (different digest for metadata)");
					break;
				case "-e":
					// Export public key from assembly
					an = AssemblyName.GetAssemblyName (args [i++]);
					WriteToFile (args[i], an.GetPublicKey ());
					if (!quiet)
						Console.WriteLine ("Public Key extracted to file {0}", args [i]);
					break;
				case "-i":
					// import keypair from SNK to container
					sn = new StrongName (ReadFromFile (args [i++]));
					csp.KeyContainerName = args [i];
					rsa = new RSACryptoServiceProvider (csp);
					rsa.ImportParameters (sn.RSA.ExportParameters (true));
					break;
				case "-k":
					// Create a new strong name key pair
					// (a new RSA keypair automagically if none is present)
					sn = new StrongName ();
					WriteToFile (args[i], CryptoConvert.ToCapiKeyBlob (sn.RSA, true));
					if (!quiet)
						Console.WriteLine ("A new strong name keypair has been generated in {0}", args [i]);
					break;
				case "-o":
					byte[] infileD = ReadFromFile (args [i++]);
					WriteCSVToFile (args [i], infileD, "D");
					if (!quiet)
						Console.WriteLine ("Output CVS file is {0} (decimal format)", args [i]);
					break;
				case "-oh":
					byte[] infileX2 = ReadFromFile (args [i++]);
					WriteCSVToFile (args [i], infileX2, "X2");
					if (!quiet)
						Console.WriteLine ("Output CVS file is {0} (hexadecimal format)", args [i]);
					break;
				case "-p":
					// Extract public key from SNK file
					sn = new StrongName (ReadFromFile (args [i++]));
					WriteToFile (args[i], sn.PublicKey);
					if (!quiet)
						Console.WriteLine ("Public Key extracted to file {0}", args [i]);
					break;
				case "-pc":
					// Extract public key from container
					csp.KeyContainerName = args [i++];
					rsa = new RSACryptoServiceProvider (csp);
					sn = new StrongName (rsa);
					WriteToFile (args[i], sn.PublicKey);
					if (!quiet)
						Console.WriteLine ("Public Key extracted to file {0}", args [i]);
					break;
				case "-R":
					string filename = args [i++];
					sn = new StrongName (ReadFromFile (args [i]));
					ReSign (filename, sn.RSA);
					break;
				case "-Rc":
					filename = args [i++];
					csp.KeyContainerName = args [i];
					rsa = new RSACryptoServiceProvider (csp);
					ReSign (filename, rsa);
					break;
				case "-t":
					// Show public key token from file
					sn = new StrongName (ReadFromFile (args [i]));
					// note: ignore quiet
					Console.WriteLine ("Public Key Token: " + ToString (sn.PublicKeyToken), Environment.NewLine);
					break;
				case "-tp":
					// Show public key and public key token from assembly
					sn = new StrongName (ReadFromFile (args [i]));
					// note: ignore quiet
					Console.WriteLine ("Public Key:" + ToString (sn.PublicKey));
					Console.WriteLine ("{0}Public Key Token: " + ToString (sn.PublicKeyToken), Environment.NewLine);
					break;
				case "-T":
					// Show public key token from assembly
					an = AssemblyName.GetAssemblyName (args [i++]);
					// note: ignore quiet
					Console.WriteLine ("Public Key Token: " + ToString (an.GetPublicKeyToken ()));
					break;
				case "-Tp":
					// Show public key and public key token from assembly
					an = AssemblyName.GetAssemblyName (args [i++]);
					// note: ignore quiet
					Console.WriteLine ("Public Key:" + ToString (an.GetPublicKey ()));
					Console.WriteLine ("{0}Public Key Token: " + ToString (an.GetPublicKeyToken ()), Environment.NewLine);
					break;
				case "-v":
					filename = args [i++];
					an = AssemblyName.GetAssemblyName (filename);
					byte[] akey = an.GetPublicKey ();
					byte[] pkey = new byte [akey.Length - 12];
					Buffer.BlockCopy (akey, 12, pkey, 0, pkey.Length);
					sn = new StrongName (pkey);
					Verify (filename, sn);
					break;
				case "-vf":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vl":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vr":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vu":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vx":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-?":
				case "-h":
					Help ((i < args.Length) ? args [i] : null);
					break;
				default:
					if (!quiet)
						Console.WriteLine ("Unknown option {0}", args [i-1]);
					return 1;
			}
			return 0;
		}
	}
}
