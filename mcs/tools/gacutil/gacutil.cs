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
using System.Globalization;
using System.Runtime.InteropServices;

using Mono.Security;

namespace Mono.Tools
{

	public class Driver
	{

		private string libdir = InternalLibdir () + Path.DirectorySeparatorChar;
		private string gac_path = GetGacPath ();
		private string package_name = String.Empty;
		string installed_gac;
		
		public static int Main (string[] args)
		{
			Driver d = new Driver ();
			return d.Run (args);
		}

		public int Run (string[] args)
		{
			if (args.Length == 0) {
				ShowHelp (false);
				return 1;
			}
	
			if (args[0] == "/user" || args[0] == "--user") {
				//FIXME: Need to check machine.config to make sure this is legal (potential security hole)
				gac_path = Path.Combine (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono"), "gac");
				gac_path += Path.DirectorySeparatorChar;
				libdir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono");
				libdir += Path.DirectorySeparatorChar;

				string[] stripped = new string[args.Length - 1];
				Array.Copy (args, 1, stripped, 0, args.Length - 1);
				args = stripped;
			}

			installed_gac = gac_path;
			if (args.Length >= 2 && (args[args.Length - 2] == "/root" || args[args.Length - 2] == "-root" || args[args.Length - 2] == "--root")) {
				gac_path = Path.Combine (Path.Combine (args[args.Length - 1], "mono"), "gac");
				gac_path += Path.DirectorySeparatorChar;
				libdir = Path.Combine (args[args.Length - 1], "mono");
				libdir += Path.DirectorySeparatorChar;

				string[] stripped = new string[args.Length - 2];				Array.Copy (args, 0, stripped, 0, args.Length - 2);
				args = stripped;
			}

			if (args.Length >= 2 && (args[args.Length - 2] == "/package" || args[args.Length - 2] == "--package" || args[args.Length - 2] == "-package")) {
				package_name = args[args.Length - 1];
				string[] stripped = new string[args.Length - 2];
				Array.Copy (args, 0, stripped, 0, args.Length - 2);
				args = stripped;
			}
	

			string[] remainder_args = new string[args.Length - 1];
			
			if (args.Length >= 2) {
				Array.Copy (args, 1, remainder_args, 0, args.Length - 1);
			}
	
			switch (args[0]) {
			case "/?":
			case "-?":
			case "--help":
				ShowHelp (true);
				return 0;
			case "-i":
			case "/i":
			case "--install":
				return InstallAssembly (remainder_args);
			case "/l":
			case "-l":
			case "--ls":
				return ListAssemblies (remainder_args);
			case "/u":
			case "-u":
			case "--uninstall":
				return UninstallAssemblies (remainder_args);
			case "/il":
			case "-il":
			case "--install-from-list":
				return InstallAssembliesFromList (remainder_args);
			case "/ul":
			case "-ul":
			case "--uninstall-from-list":
				return UninstallAssembliesFromList (remainder_args);
			default:
				ShowHelp (false);
				break;
			}

			return 1;
		}

		public int InstallAssembliesFromList (string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("ERROR: need a file passed");
				return 1;
			}

			if (!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: file '" + args[0] + "' does not exist");
				return 1;
			}

			string[] perFile = args;

			int result = 0;
			using (StreamReader s = File.OpenText (args[0])) {
				string line;

				while((line = s.ReadLine()) != null) {
					perFile[0] = line;
					try {
						if (InstallAssembly (perFile) != 0)
							result = 1;
					} catch (Exception e) {
						Console.WriteLine ("Failed for {0}. Reason: {1}", line, e.Message);
						result = 1;
					}
				}
			}

			return result;
		}

		public int UninstallAssembliesFromList (string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("ERROR: file must be passed.");
				return 1;
			}

			if (!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: file '" + args[0] + "' does not exist");
				return 1;
			}

			int result = 0;
			using (StreamReader s = File.OpenText (args[0])) {
				string line;

				while ((line = s.ReadLine ()) != null) {
					if (UninstallAssemblies (new string[] { line } ) != 0)
						result = 1;
				}
			}

			return result;
		}

		public int UninstallAssemblies (string[] args)
		{
			if(args.Length == 0) {
				Console.WriteLine ("ERROR: need an argument to uninstall");
				return 1;
			}

			string joinedArgs = String.Join ("", args);

			string[] assemblyPieces = joinedArgs.Split(new char[] { ',' });

			Hashtable paramInfo = new Hashtable ();

			foreach (string item in assemblyPieces) {
				string[] pieces = item.Trim ().Split (new char[] { '=' }, 2);
				if(pieces.Length == 1)
					paramInfo["assembly"] = pieces[0];
				else
					paramInfo[pieces[0].Trim ().ToLower (CultureInfo.InvariantCulture)] = pieces[1];
			}

			if (!Directory.Exists (Path.Combine (gac_path, (string) paramInfo["assembly"]))) {
				Console.WriteLine ("ERROR: Assembly not in gac.");
				return 1;
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
					if (package_name != String.Empty) {
						File.Delete (libdir + package_name + Path.DirectorySeparatorChar + (string)paramInfo["assembly"] + ".dll");
					}
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

			return 0;
		}

		public int ListAssemblies (string[] args)
		{
			Console.WriteLine ("The following assemblies are installed into the GAC:");
			DirectoryInfo d = new DirectoryInfo (gac_path);
			foreach (DirectoryInfo namedDir in d.GetDirectories ()) {
				foreach (DirectoryInfo assemblyDir in namedDir.GetDirectories ()) {
					Hashtable assemblyInfo = GetAssemblyInfo (Path.Combine (assemblyDir.FullName, "__AssemblyInfo__"));
					if (assemblyInfo != null){
						Console.WriteLine ("\t" + assemblyInfo["DisplayName"]);
					}
				}
			}

			return 0;
		}

		private Hashtable GetAssemblyInfo (string filename)
		{
			try {
				Hashtable infoHash = new Hashtable ();
				using (StreamReader s = new StreamReader (filename)) {
					string line;
					
					while ((line = s.ReadLine ()) != null) {
						string[] splitStr = line.Split (new char[] { '=' }, 2);
						infoHash[splitStr[0]] = splitStr[1];
					}
				}
				return infoHash;
			} catch {
				return null;
			}
		}

		private void WriteAssemblyInfo (string filename, Hashtable info)
		{
			using (StreamWriter s = File.CreateText (filename)) {
				foreach (string key in info.Keys) {
					s.WriteLine (key + "=" + (string) info[key]);
				}
			}
		}

		public int InstallAssembly (string[] args)
		{
			if(args.Length == 0) {
				Console.WriteLine ("ERROR: You must specify a valid assembly name after the install switch");
				return 1;
			}

			if(!File.Exists (args[0])) {
				Console.WriteLine ("ERROR: The assembly: '" + args[0] + "' does not exist");
				return 1;
			}

			AssemblyName an = AssemblyName.GetAssemblyName (args[0]);
			string config_path = null;
			byte[] pub_tok = an.GetPublicKeyToken ();

			if (pub_tok == null || pub_tok.Length == 0) {
				Console.WriteLine ("ERROR: assembly has no valid public key token");
				return 1;
			}

			config_path = args [0] + ".config";
			// strong name verification temp. disabled
			/*
			byte[] akey = an.GetPublicKey ();
			if (akey == null || akey.Length < 12) {
				Console.WriteLine ("ERROR: assembly has no valid public key token");
				return;
			}
			StrongName sn = new StrongName (akey);
			if (!sn.Verify (args[0])) {
				Console.WriteLine ("ERROR: invalid strongname signature in assembly");
				return;
			}
			*/

			//FIXME: force=true per mig's request.
			bool force = true;

			if (Array.IndexOf (args, "/f") != -1 || Array.IndexOf (args, "-f") != -1 ||
			    Array.IndexOf (args, "--force") != -1) {
				force = true;
			}
			
			string version_token = an.Version + "_" +
				an.CultureInfo.Name.ToLower (CultureInfo.InvariantCulture) +
				"_" + GetStringToken (an.GetPublicKeyToken ());

			string fullPath = String.Format ("{0}{3}{1}{3}{2}{3}", gac_path, an.Name, version_token, Path.DirectorySeparatorChar);
			string linkPath = String.Format ("{0}{3}{1}{3}{2}{3}", installed_gac, an.Name, version_token, Path.DirectorySeparatorChar);

			if (File.Exists (fullPath + an.Name + ".dll") && force == false) {
				Hashtable assemInfo = GetAssemblyInfo (fullPath + "__AssemblyInfo__");

				if (assemInfo != null){
					assemInfo["RefCount"] = ((int) Convert.ToInt32 (assemInfo["RefCount"]) + 1).ToString ();
					WriteAssemblyInfo (fullPath + "__AssemblyInfo__", assemInfo);
					Console.WriteLine ("RefCount of assembly '" + an.Name + "' increased by one.");
					if (File.Exists (config_path))
						File.Copy (config_path, fullPath + an.Name + ".dll" + ".config", force);
					InstallPackage (libdir, linkPath, an, args [0], Path.GetFileName (args [0]));
				}
				return 0;
			}

			if(!EnsureDirectories (an.Name, version_token)) {
				Console.WriteLine ("ERROR: gac directories could not be created, possibly permission issues");
				return 1;
			}

			File.Copy (args[0], fullPath + an.Name + ".dll", force);
			InstallPackage (libdir, linkPath, an, args [0], Path.GetFileName (args [0]));
			if (File.Exists (config_path)){
				File.Copy (config_path, fullPath + an.Name + ".dll" + ".config", force);
			}

			Hashtable info = new Hashtable ();

			info["DisplayName"] = an.FullName;
			info["RefCount"] = 1.ToString ();

			WriteAssemblyInfo (fullPath + "__AssemblyInfo__", info);

			Console.WriteLine ("{0} installed into the gac ({1})", an.Name, gac_path);
			return 0;
		}

		private void InstallPackage (string libdir, string linkPath,
                                AssemblyName an, string path, string filename)
		{
			if (package_name != String.Empty) {
				string ref_file = libdir + package_name +
						Path.DirectorySeparatorChar + filename;
				if (File.Exists (ref_file)) {
					File.Delete (ref_file);
				}
				if (Path.DirectorySeparatorChar == '/') {
					try {
						Directory.CreateDirectory (libdir + package_name);
					} catch {}
					
					symlink (linkPath + an.Name + ".dll", ref_file);
				} else {
					
					File.Copy (path, ref_file);
				}
				Console.WriteLine ("Package exported to: " + libdir + package_name);
			}
		}

		private bool EnsureDirectories (string name, string tok)
		{
			//FIXME: Workaround for broken DirectoryInfo.CreateSubdirectory
			try {
				DirectoryInfo d = new DirectoryInfo (gac_path);

				d.CreateSubdirectory (name);
				d = new DirectoryInfo (Path.Combine (gac_path, name));
				d.CreateSubdirectory (tok);
				if (package_name != String.Empty) {
					d = new DirectoryInfo (libdir);
					d.CreateSubdirectory (package_name);
				}
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
			sb.Append ("  -i <assembly_path> [ -f ] [-package NAME] [-root ROOTDIR]\n");
			if (detailed == false) {
				sb.Append ("    Installs an assembly into the global assembly cache\n");
			} else {
				sb.Append ("    Installs an assembly to the global assembly cache. <assembly_path> is the\n     name of the file that contains the assembly manifest.                    \n     Example: -i myDll.dll\n");
			}

			sb.Append ("\n");

			sb.Append ("  -il <assembly_path_list_file> [ -f ]\n");
			if (detailed == false) {
				sb.Append ("    Installs one or more assemblies into the global assembly cache\n");
			} else {
				sb.Append ("    Installs on or more assemblies to the global assembly cache.              \n    <assembly_list_file is the path to a text file that contains a list of    \n    assembly manifest file paths. Individual paths in the text file must be   \n    separated by a newline.\n");
				sb.Append ("    Example: -il MyAssemblyList\n");
				sb.Append ("      MyAssemblyList content:\n");
				sb.Append ("      Mydll.dll\n");
				sb.Append ("      Mydll2.dll\n");
				sb.Append ("      path/to/myDll3.dll\n");
			}

			sb.Append ("\n");

			sb.Append ("  -u <assembly_display_name> [-package NAME] [-root ROOTDIR]\n");
			if (detailed == false) {
				sb.Append ("    Uninstalls an assembly from the global assembly cache\n");
			} else {
				sb.Append ("    Uninstalls an assembly. <assembly_display_name> is the name of the assembly\n");
				sb.Append ("    (partial or fully qualified) to remove from the global assembly cache.    \n    If a partial name is specified all matching assemblies will be uninstalled.\n");
				sb.Append ("    Example: /u myDll,Version=1.2.1.0\n");
			}

			sb.Append ("\n");

			sb.Append ("  -ul <assembly_display_name_list_file>\n");
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

			sb.Append ("  -l\n");
			if (detailed == false) {
				sb.Append ("    List the global assembly cache\n");
			} else {
				sb.Append ("    Lists the contents of the global assembly cache.\n");
			}

			sb.Append ("\n");

			sb.Append ("  -?\n");
			if (detailed == false) {
				sb.Append ("    Displays a detailed help screen\n");
			} else {
				sb.Append ("    Displays a detailed help screen\n");
			}

			sb.Append ("\n");

			if (detailed == true) {
				sb.Append ("Options:\n");
				sb.Append ("  -f\n");
				sb.Append ("    Forces reinstall of assembly, resets reference count\n");
				sb.Append ("\n");
				sb.Append ("\n");
				sb.Append ("Note, mono's gacutil also supports these unix like aliases for its commands:\n");
				sb.Append ("  -i  -> --install\n");
				sb.Append ("  -il -> --install-from-list\n");
				sb.Append ("  -u  -> --uninstall\n");
				sb.Append ("  -ul -> --uninstall-from-list\n");
				sb.Append ("  -l  -> --ls\n");
				sb.Append ("  -f  -> --force\n");
				sb.Append ("\n");
				sb.Append ("Mono also allows a User Assembly Cache, this cache can be accessed by passing\n/user as the first argument to gacutil.exe\n");
			}

			Console.WriteLine (sb.ToString ());
		}

		private static string GetGacPath () {
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", BindingFlags.Static|BindingFlags.NonPublic);
			if (gac == null) {
				Console.WriteLine ("ERROR: MS.Net runtime detected, please use the mono runtime for gacutil.exe");
				Environment.Exit (1);
			}
			MethodInfo getGac = gac.GetGetMethod (true);
			return Path.Combine ((string) getGac.Invoke (null, null), "");
		}

		private static string InternalLibdir () {
			MethodInfo libdir = typeof (System.Environment).GetMethod ("internalGetGacPath", BindingFlags.Static|BindingFlags.NonPublic);
			if (libdir == null) {
				Console.WriteLine ("ERROR: MS.Net runtime detected, please use the mono runtime for gacutil.exe");
				Environment.Exit (1);
			}
			return Path.Combine (Path.Combine ((string)libdir.Invoke (null, null), "mono"), "");
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int symlink (string oldpath, string newpath);
	}
}
