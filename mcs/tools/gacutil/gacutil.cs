// GacUtil
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

using Mono.Security;

namespace Mono.Tools
{

	public class Driver
	{

		private string gac_path = GetGacPath ();
		
		public static void Main (string[] args)
		{
			Driver d = new Driver ();
			d.Run (args);
		}

		public void Run (string[] args)
		{
			if (args.Length == 0) {
				ShowHelp (false);
				return;
			}
	
			if (args[0] == "/user" || args[0] == "--user") {
				//FIXME: Need to check machine.config to make sure this is legal (potential security hole)
				gac_path = Path.Combine (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono"), "gac");
				gac_path += Path.DirectorySeparatorChar;

				string[] stripped = new string[args.Length - 1];
				Array.Copy (args, 1, stripped, 0, args.Length - 1);
				args = stripped;
			}
	
			string[] remainder_args = new string[args.Length - 1];
			
			if (args.Length >= 2) {
				Array.Copy (args, 1, remainder_args, 0, args.Length - 1);
			}
			
			switch (args[0]) {
				case "/?":
				case "--help":
					ShowHelp (true);
					break;
				case "/i":
				case "--install":
					InstallAssembly (remainder_args);
					break;
				case "/l":
				case "--ls":
					ListAssemblies (remainder_args);
					break;
				case "/u":
				case "--uninstall":
					UninstallAssemblies (remainder_args);
					break;
				case "/il":
				case "--install-from-list":
					InstallAssembliesFromList (remainder_args);
					break;
				case "/ul":
				case "--uninstall-from-list":
					UninstallAssembliesFromList (remainder_args);
					break;
				default:
					ShowHelp (false);
					break;
			}
		}

		public void InstallAssembliesFromList (string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("ERROR: need a file passed");
				return;
			}

			if (!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: file '" + args[0] + "' does not exist");
				return;
			}

			string[] perFile = args;

			using (StreamReader s = File.OpenText (args[0])) {
				string line;

				while((line = s.ReadLine()) != null) {
					perFile[0] = line;
					InstallAssembly (perFile);
				}
			}
		}

		public void UninstallAssembliesFromList (string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("ERROR: file must be passed.");
				return;
			}

			if (!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: file '" + args[0] + "' does not exist");
				return;
			}

			using (StreamReader s = File.OpenText (args[0])) {
				string line;

				while ((line = s.ReadLine ()) != null) {
					UninstallAssemblies (new string[] { line } );
				}
			}
		}

		public void UninstallAssemblies (string[] args)
		{
			if(args.Length == 0) {
				Console.WriteLine ("ERROR: need an argument to uninstall");
				return;
			}

			string joinedArgs = String.Join ("", args);

			string[] assemblyPieces = joinedArgs.Split(new char[] { ',' });

			Hashtable paramInfo = new Hashtable ();

			foreach (string item in assemblyPieces) {
				string[] pieces = item.Trim ().Split (new char[] { '=' }, 2);
				if(pieces.Length == 1)
					paramInfo["assembly"] = pieces[0];
				else
					paramInfo[pieces[0].ToLower ()] = pieces[1];
			}

			if (!Directory.Exists (Path.Combine (gac_path, (string) paramInfo["assembly"]))) {
				Console.WriteLine ("ERROR: Assembly not in gac.");
				return;
			}

			string searchString = (string) paramInfo["assembly"] + Path.DirectorySeparatorChar;

			if (paramInfo.Keys.Count != 1) {
				if (paramInfo["version"] != null) {
					searchString += (string) paramInfo["version"] + "*";
				}
			} else {
				searchString += "*";
			}

			string[] directories = Directory.GetDirectories (gac_path, searchString);

			foreach (string dir in directories) {
				Hashtable info = GetAssemblyInfo (Path.Combine (dir, "__AssemblyInfo__"));
				if(Convert.ToInt32 (info["RefCount"]) == 1) {
					Directory.Delete (dir, true);
					Console.WriteLine ("Assembly removed from the gac.");
				} else {
					info["RefCount"] = ((int) Convert.ToInt32 (info["RefCount"]) - 1).ToString ();
					WriteAssemblyInfo (Path.Combine (dir, "__AssemblyInfo__"), info);
					Console.WriteLine ("Assembly was not deleted because its still needed by other applications");
				}
			}
			if(Directory.GetDirectories (Path.Combine (gac_path, (string) paramInfo["assembly"])).Length == 0) {
				Console.WriteLine ("Cleaning assembly dir, its empty");
				Directory.Delete (Path.Combine (gac_path, (string) paramInfo["assembly"]));
			}
			
		}

		public void ListAssemblies (string[] args)
		{
			Console.WriteLine ("The following assemblies are installed into the GAC:");
			DirectoryInfo d = new DirectoryInfo (gac_path);
			foreach (DirectoryInfo namedDir in d.GetDirectories ()) {
				foreach (DirectoryInfo assemblyDir in namedDir.GetDirectories ()) {
					Hashtable assemblyInfo = GetAssemblyInfo (Path.Combine (assemblyDir.FullName, "__AssemblyInfo__"));
					Console.WriteLine ("\t" + assemblyInfo["DisplayName"]);
				}
			}
		}

		private Hashtable GetAssemblyInfo (string filename)
		{
			Hashtable infoHash = new Hashtable ();
			using (StreamReader s = new StreamReader (filename)) {
				string line;

				while ((line = s.ReadLine ()) != null) {
					string[] splitStr = line.Split (new char[] { '=' }, 2);
					infoHash[splitStr[0]] = splitStr[1];
				}
			}
			return infoHash;
		}

		private void WriteAssemblyInfo (string filename, Hashtable info)
		{
			using (StreamWriter s = File.CreateText (filename)) {
				foreach (string key in info.Keys) {
					s.WriteLine (key + "=" + (string) info[key]);
				}
			}
		}

		public void InstallAssembly (string[] args)
		{
			if(args.Length == 0) {
				Console.WriteLine ("ERROR: You must specify a valid assembly name after the install switch");
				return;
			}

			if(!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: The assembly: '" + args[0] + "' does not exist");
				return;
			}

			AssemblyName an = AssemblyName.GetAssemblyName (args[0]);

			byte[] pub_tok = an.GetPublicKeyToken ();

			if (pub_tok == null || pub_tok.Length == 0) {
				Console.WriteLine ("ERROR: assembly has no valid public key token");
				return;
			}

			byte[] akey = an.GetPublicKey ();
			if (akey == null || akey.Length < 12) {
				Console.WriteLine ("ERROR: assembly has no valid public key token");
				return;
			}
			byte[] pkey = new byte [akey.Length - 12];
			Buffer.BlockCopy (akey, 12, pkey, 0, pkey.Length);
			StrongName sn = new StrongName (pkey);
			if (!sn.Verify (args[0])) {
				Console.WriteLine ("ERROR: invalid strongname signature in assembly");
				return;
			}

			bool force = false;

			if(args.Length == 2 && (args[1] == "/f" || args[1] == "--force"))
				force = true;

		
			string version_token = an.Version + "__" + GetStringToken (an.GetPublicKeyToken ());

			string fullPath = String.Format ("{0}{3}{1}{3}{2}{3}", gac_path, an.Name, version_token, Path.DirectorySeparatorChar);

			if (File.Exists (fullPath + an.Name + ".dll") && force == false) {
				Hashtable assemInfo = GetAssemblyInfo (fullPath + "__AssemblyInfo__");
				assemInfo["RefCount"] = ((int) Convert.ToInt32 (assemInfo["RefCount"]) + 1).ToString ();
				WriteAssemblyInfo (fullPath + "__AssemblyInfo__", assemInfo);
				Console.WriteLine ("RefCount of assembly '" + an.Name + "' increased by one.");
				return;
			}

			if(!EnsureDirectories (an.Name, version_token)) {
				Console.WriteLine ("ERROR: gac directories could not be created, possibly permission issues");
				return;
			}

			File.Copy (args[0], fullPath + an.Name + ".dll", force);

			Hashtable info = new Hashtable ();

			info["DisplayName"] = an.FullName;
			info["RefCount"] = 1.ToString ();

			WriteAssemblyInfo (fullPath + "__AssemblyInfo__", info);

			Console.WriteLine ("Assembly installed into the gac");
		}

		private bool EnsureDirectories (string name, string tok)
		{
			//FIXME: Workaround for broken DirectoryInfo.CreateSubdirectory
			try {
				DirectoryInfo d = new DirectoryInfo (gac_path);

				d.CreateSubdirectory (name);
				d = new DirectoryInfo (Path.Combine (gac_path, name));
				d.CreateSubdirectory (tok);
			} catch {
				return false;
			}
			return true;
		}

		private string GetStringToken (byte[] tok)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < tok.Length ; i++) {
				sb.Append (tok[i].ToString ("x2"));
			}
			return sb.ToString ();
		}

		public void ShowHelp (bool detailed)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("Usage: gacutil.exe <commands> [ <options> ]\n");
			sb.Append ("Commands:\n");
			sb.Append ("  /i <assembly_path> [ /f ]\n");
			if (detailed == false) {
				sb.Append ("    Installs an assembly into the global assembly cache\n");
			} else {
				sb.Append ("    Installs an assembly to the global assembly cache. <assembly_path> is the\n     name of the file that contains the assembly manifest.                    \n     Example: /i myDll.dll\n");
			}

			sb.Append ("\n");

			sb.Append ("  /il <assembly_path_list_file> [ /f ]\n");
			if (detailed == false) {
				sb.Append ("    Installs one or more assemblies into the global assembly cache\n");
			} else {
				sb.Append ("    Installs on or more assemblies to the global assembly cache.              \n    <assembly_list_file is the path to a text file that contains a list of    \n    assembly manifest file paths. Individual paths in the text file must be   \n    separated by a newline.\n");
				sb.Append ("    Example: /il MyAssemblyList\n");
				sb.Append ("      MyAssemblyList content:\n");
				sb.Append ("      Mydll.dll\n");
				sb.Append ("      Mydll2.dll\n");
				sb.Append ("      path/to/myDll3.dll\n");
			}

			sb.Append ("\n");

			sb.Append ("  /u <assembly_display_name>\n");
			if (detailed == false) {
				sb.Append ("    Uninstalls an assembly from the global assembly cache\n");
			} else {
				sb.Append ("    Uninstalls an assembly. <assembly_display_name> is the name of the assembly\n");
				sb.Append ("    (partial or fully qualified) to remove from the global assembly cache.    \n    If a partial name is specified all matching assemblies will be uninstalled.\n");
				sb.Append ("    Example: /u myDll,Version=1.2.1.0\n");
			}

			sb.Append ("\n");

			sb.Append ("  /ul <assembly_display_name_list_file>\n");
			if (detailed == false) {
				sb.Append ("    Uninstalls one or more assemblies from the global assembly cache\n");
			} else {
				sb.Append ("    Uninstalls one or more assemblies from the global assembly cache.         \n    <assembly_display_name_list_file> is the path to a text file that contains\n    a list of assembly names. Individual names in the text file must be       \n    separated by a newline.\n");
				sb.Append ("    Example: /ul MyAssemblyList\n");
				sb.Append ("      MyAssemblyList content:\n");
				sb.Append ("      MyDll1.dll,Version=1.0.0.0\n");
				sb.Append ("      MyDll2.dll,Version=1.2.0.0\n");
			}

			sb.Append ("\n");

			sb.Append ("  /l\n");
			if (detailed == false) {
				sb.Append ("    List the global assembly cache\n");
			} else {
				sb.Append ("    Lists the contents of the global assembly cache.\n");
			}

			sb.Append ("\n");

			sb.Append ("  /?\n");
			if (detailed == false) {
				sb.Append ("    Displays a detailed help screen\n");
			} else {
				sb.Append ("    Displays a detailed help screen\n");
			}

			sb.Append ("\n");

			if (detailed == true) {
				sb.Append ("Options:\n");
				sb.Append ("  /f\n");
				sb.Append ("    Forces reinstall of assembly, resets reference count\n");
				sb.Append ("\n");
				sb.Append ("\n");
				sb.Append ("Note, mono's gacutil also supports these unix like aliases for its commands:\n");
				sb.Append ("  /i  -> --install\n");
				sb.Append ("  /il -> --install-from-list\n");
				sb.Append ("  /u  -> --uninstall\n");
				sb.Append ("  /ul -> --uninstall-from-list\n");
				sb.Append ("  /l  -> --ls\n");
				sb.Append ("  /f  -> --force\n");
				sb.Append ("\n");
				sb.Append ("Mono also allows a User Assembly Cache, this cache can be accessed by passing\n/user as the first argument to gacutil.exe\n");
			}

			Console.WriteLine (sb.ToString ());
		}

		private static string GetGacPath () {
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", BindingFlags.Static|BindingFlags.NonPublic);
			MethodInfo getGac = gac.GetGetMethod (true);
			return Path.Combine ((string) getGac.Invoke (null, null), "");
		}
	}
}
