//
// SN.cs: sn clone tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006,2008 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using Mono.Security.Cryptography;
using Mono.Security.X509;

[assembly: AssemblyTitle("Mono StrongName")]
[assembly: AssemblyDescription("StrongName utility for signing assemblies")]

namespace Mono.Tools {

	class SN {

		static private void Header () 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static string defaultCSP;

		static bool LoadConfig (bool quiet) 
		{
			MethodInfo config = typeof (System.Environment).GetMethod ("GetMachineConfigPath",
				BindingFlags.Static|BindingFlags.NonPublic);

			if (config != null) {
				string path = (string) config.Invoke (null, null);

				bool exist = File.Exists (path);
				if (!quiet && !exist)
					Console.WriteLine ("Couldn't find machine.config");

				StrongNameManager.LoadConfig (path);
				return exist;
			}
			else if (!quiet)
				Console.WriteLine ("Couldn't resolve machine.config location (corlib issue)");
			
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
				if (i > 2080) {
					// ensure we can display up to 16384 bits keypair
					sb.Append (" !!! TOO LONG !!!");
					break;
				}
			}
			return sb.ToString ();
		}

		static RSA GetKeyFromFile (string filename)
		{
			byte[] data = ReadFromFile (filename);
			try {
				// for SNK files (including the ECMA pseudo-key)
				return new StrongName (data).RSA;
			}
			catch {
				if (data.Length == 0 || data [0] != 0x30)
					throw;
				// this could be a PFX file
				Console.Write ("Enter password for private key (will be visible when typed): ");
				PKCS12 pfx = new PKCS12 (data, Console.ReadLine ());
				// works only if a single key is present
				if (pfx.Keys.Count != 1)
					throw;
				RSA rsa = (pfx.Keys [0] as RSA);
				if (rsa == null)
					throw;
				return rsa;
			}
		}
#if false
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
#endif
		static bool ReSign (string assemblyName, RSA key, bool quiet) 
		{
			// this doesn't load the assembly (well it unloads it ;)
			// http://weblogs.asp.net/nunitaddin/posts/9991.aspx
			AssemblyName an = null;
			try {
				an = AssemblyName.GetAssemblyName (assemblyName);
			}
			catch {
			}
			if (an == null) {
				Console.WriteLine ("Unable to load assembly: {0}", assemblyName);
				return false;
			}

			StrongName sign = new StrongName (key);
			byte[] token = an.GetPublicKeyToken ();

			// first, try to compare using a mapped public key (e.g. ECMA)
			bool same = Compare (sign.PublicKey, StrongNameManager.GetMappedPublicKey (token));
			if (!same) {
				// second, try to compare using the assembly public key
				same = Compare (sign.PublicKey, an.GetPublicKey ());
				if (!same) {
					// third (and last) chance, try to compare public key token
					same = Compare (sign.PublicKeyToken, token);
				}
			}

			if (same) {
				bool signed = sign.Sign (assemblyName);
				if (!quiet || !signed) {
					Console.WriteLine (signed ? "Assembly {0} signed." : "Couldn't sign the assembly {0}.", 
							   assemblyName);
				}
				return signed;
			}
			
			Console.WriteLine ("Couldn't sign the assembly {0} with this key pair. Public key of assembly did not match signing public key.", assemblyName);
			return false;
		}

		static int Verify (string assemblyName, bool forceVerification, bool quiet) 
		{
			// this doesn't load the assembly (well it unloads it ;)
			// http://weblogs.asp.net/nunitaddin/posts/9991.aspx
			AssemblyName an = null;
			try {
				an = AssemblyName.GetAssemblyName (assemblyName);
			}
			catch {
			}
			if (an == null) {
				Console.WriteLine ("Unable to load assembly: {0}", assemblyName);
				return 2;
			}

			byte[] publicKey = StrongNameManager.GetMappedPublicKey (an.GetPublicKeyToken ());
			if ((publicKey == null) || (publicKey.Length < 12)) {
				// no mapping
				publicKey = an.GetPublicKey ();
				if ((publicKey == null) || (publicKey.Length < 12)) {
					Console.WriteLine ("{0} is not a strongly named assembly.", assemblyName);
					return 2;
				}
			}

			// Note: MustVerify is based on the original token (by design). Public key
			// remapping won't affect if the assembly is verified or not.
			if (forceVerification || StrongNameManager.MustVerify (an)) {
				RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (publicKey, 12);
				StrongName sn = new StrongName (rsa);
				if (sn.Verify (assemblyName)) {
					if (!quiet)
						Console.WriteLine ("Assembly {0} is strongnamed.", assemblyName);
					return 0;
				}
				else {
					Console.WriteLine ("Assembly {0} is delay-signed but not strongnamed", assemblyName);
					return 1;
				}
			}
			else {
				Console.WriteLine ("Assembly {0} is strongnamed (verification skipped).", assemblyName);
				return 0;
			}
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
					Console.WriteLine ("{0}<1> Currently not implemented in the tool", Environment.NewLine);
					break;
				case "csp":
					Console.WriteLine ("CSP related options");
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
					Console.WriteLine (" -t file{0}\tShow the public key token from the specified file", Environment.NewLine);
					Console.WriteLine (" -tp file{0}\tShow the public key and pk token from the specified file", Environment.NewLine);
					Console.WriteLine (" -T assembly{0}\tShow the public key token from the specified assembly", Environment.NewLine);
					Console.WriteLine (" -Tp assembly{0}\tShow the public key and pk token from the specified assembly", Environment.NewLine);
					Console.WriteLine (" -v assembly{0}\tVerify the specified assembly signature", Environment.NewLine);
					Console.WriteLine (" -vf assembly{0}\tVerify the specified assembly signature (even if disabled).", Environment.NewLine);
					break;
				default:
					Console.WriteLine ("Help options");
					Console.WriteLine (" -? | -h        \tShow this help screen about the tool");
					Console.WriteLine (" -? | -h config \tConfiguration options");
					Console.WriteLine (" -? | -h csp    \tCrypto Service Provider (CSP) related options");
					Console.WriteLine (" -? | -h convert\tFormat convertion options");
					Console.WriteLine (" -? | -h sn     \tStrongName signing options");
					break;
			}
		}

		static int Process (string[] args)
		{
			int i = 0;
			string param = args [i];
			bool quiet = ((param == "-quiet") || (param == "-q"));
			if (quiet)
				i++;
			else
				Header();

			LoadConfig (quiet);

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
					int size = 1024;
					if (i < args.Length + 2) {
						try {
							size = Int32.Parse (args[i++]);
						}
						catch {
							// oops, that wasn't a valid key size (assume 1024 bits)
							i--;
						}
					}
					sn = new StrongName (size);
					WriteToFile (args[i], CryptoConvert.ToCapiKeyBlob (sn.RSA, true));
					if (!quiet)
						Console.WriteLine ("A new {0} bits strong name keypair has been generated in file '{1}'.", size, args [i]);
					break;
				case "-m":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-o":
					byte[] infileD = ReadFromFile (args [i++]);
					WriteCSVToFile (args [i], infileD, "D");
					if (!quiet)
						Console.WriteLine ("Output CSV file is {0} (decimal format)", args [i]);
					break;
				case "-oh":
					byte[] infileX2 = ReadFromFile (args [i++]);
					WriteCSVToFile (args [i], infileX2, "X2");
					if (!quiet)
						Console.WriteLine ("Output CVS file is {0} (hexadecimal format)", args [i]);
					break;
				case "-p":
					// Extract public key from SNK or PKCS#12/PFX file
					sn = new StrongName (GetKeyFromFile (args [i++]));
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
					if (! ReSign (filename, GetKeyFromFile (args [i]), quiet))
						return 1;
					break;
				case "-Rc":
					filename = args [i++];
					csp.KeyContainerName = args [i];
					rsa = new RSACryptoServiceProvider (csp);
					if (! ReSign (filename, rsa, quiet))
						return 1;
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
					byte [] pkt = an.GetPublicKeyToken ();
					if (pkt == null) {
						Console.WriteLine ("{0} does not represent a strongly named assembly.", args [i - 1]);
					} else {
						Console.WriteLine ("Public Key Token: " + ToString (pkt));
					}
					break;
				case "-Tp":
					// Show public key and public key token from assembly
					an = AssemblyName.GetAssemblyName (args [i++]);
					byte [] token = an.GetPublicKeyToken ();
					if (token == null) {
						Console.WriteLine ("{0} does not represent a strongly named assembly.", args [i - 1]);
					} else {
						Console.WriteLine ("Public Key:" + ToString (an.GetPublicKey ()));
						Console.WriteLine ("{0}Public Key Token: " + ToString (token), Environment.NewLine);
					}
					break;
				case "-v":
					filename = args [i++];
					return Verify (filename, false, quiet);
				case "-vf":
					filename = args [i++];
					return Verify (filename, true, quiet);	// force verification
				case "-Vl":
					Console.WriteLine (new StrongNameManager ().ToString ());
					break;
				case "-Vr":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vu":
					Console.WriteLine ("Unimplemented option");
					break;
				case "-Vx":
					// we must remove <verificationSettings> from each config files
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

		[STAThread]
		static int Main (string[] args)
		{
			try {
				if (args.Length < 1) {
					Header ();
					Help (null);
				} else {
					return Process (args);
				}
			}
			catch (IndexOutOfRangeException) {
				Console.WriteLine ("ERROR: Invalid number of parameters.{0}", Environment.NewLine);
				Help (null);
			}
			catch (CryptographicException ce) {
				Console.WriteLine ("ERROR: {0}", ce.Message);
			}
			catch (Exception e) {
				Console.WriteLine ("ERROR: Unknown error during processing: {0}", e);
			}

			return 1;
		}
	}
}
