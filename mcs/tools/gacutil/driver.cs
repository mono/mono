//
// Mono.Tools.GacUtil
//
// Author(s):
//  Todd Berman <tberman@sevenl.net>
//  Jackson Harper <jackson@ximian.com>
//
// Copyright 2003, 2004 Todd Berman 
// Copyright 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

using Mono.Security;

namespace Mono.Tools {

	public class Driver {

		private enum Command {
			Unknown,
			Install,
			InstallFromList,
			Uninstall,
			UninstallFromList,
			UninstallSpecific,
			List,
			Help
		}

		private static bool silent;

		public static int Main (string [] args)
		{
			if (args.Length == 0)
				Usage ();

			Command command = Command.Unknown;
			string command_str = null;

			string libdir;
			string name, package, gacdir, root;
			name = package = root = gacdir = null;
			bool check_refs = false;

			// Check for silent arg first so we can supress
			// warnings during command line parsing
			if (Array.IndexOf (args, "/silent") > -1 || Array.IndexOf (args, "-silent") > -1)
				silent = true;

			for (int i=0; i<args.Length; i++) {
				if (IsSwitch (args [i])) {

					// for cmd line compatibility with other gacutils
					// we always force it though
					if (args [i] == "-f" || args [i] == "/f")
						continue;

					// Ignore this option for now, although we might implement it someday
					if (args [i] == "/r") {
						WriteLine ("WARNING: gacutil does not support traced references." +
							"This option is being ignored.");
						i += 3;
						continue;
					}

					// This is already handled we just dont want to choke on it
					if (args [i] == "-silent" || args [i] == "/silent")
						continue; 

					if (args [i] == "-check_refs" || args [i] == "/check_refs") {
						check_refs = true;
						continue;
					}

					if (command == Command.Unknown) {
						command = GetCommand (args [i]);
						if (command != Command.Unknown) {
							command_str = args [i];
							continue;
						}
					}

					if (i + 1 >= args.Length) {
						Console.WriteLine ("Option " + args [i] + " takes 1 argument");
						return 1;
					}

					switch (args [i]) {
					case "-package":
					case "/package":
						package = args [++i];
						break;
					case "-root":
					case "/root":
						root = args [++i];
						break;
					case "-gacdir":
					case "/gacdir":
						gacdir = args [++i];
						break;
					}
					continue;
				}
				if (name == null)
					name = args [i];
				else
					name += args [i];
			}

			if (command == Command.Unknown && IsSwitch (args [0])) {
				Console.WriteLine ("Unknown command: " + args [0]);
				return 1;
			} else if (command == Command.Unknown) {
				Usage ();
			} else if (command == Command.Help) {
				ShowHelp (true);
				return 1;
			}

			if (gacdir == null) {
				gacdir = GetGacDir ();
				libdir = GetLibDir ();
			} else {
				gacdir = EnsureLib (gacdir);
				libdir = Path.Combine (gacdir, "mono");
				gacdir = Path.Combine (libdir, "gac");
			}

			string link_gacdir = gacdir;
			string link_libdir = libdir;
			if (root != null) {
				libdir = Path.Combine (root, "mono");
				gacdir = Path.Combine (libdir, "gac");
			}

			switch (command) {
			case Command.Install:
				if (name == null) {
					WriteLine ("Option " + command_str + " takes 1 argument");
					return 1;
				}
				if (!Install (check_refs, name, package, gacdir, link_gacdir, libdir, link_libdir))
					return 1;
				break;
			case Command.InstallFromList:
				if (name == null) {
					WriteLine ("Option " + command_str + " takes 1 argument");
					return 1;
				}
				if (!InstallFromList (check_refs, name, package, gacdir, link_gacdir, libdir, link_libdir))
					return 1;
				break;
			case Command.Uninstall:
				if (name == null) {
					WriteLine ("Option " + command_str + " takes 1 argument");
					return 1;
				}
				if (!Uninstall (name, package, gacdir, libdir))
					Environment.Exit (1);
				break;
			case Command.UninstallFromList:
				if (name == null) {
					WriteLine ("Option " + command_str + " takes 1 argument");
					return 1;
				}
				if (!UninstallFromList (name, package, gacdir, libdir))
					return 1;
				break;
			case Command.UninstallSpecific:
				if (name == null) {
					WriteLine ("Opetion " + command_str + " takes 1 argument");
					return 1;
				}
				if (!UninstallSpecific (name, package, gacdir, libdir))
					return 1;
				break;
			case Command.List:
				List (name, gacdir);
				break;
			}

			return 0;
		}

		private static bool Install (bool check_refs, string name, string package,
				string gacdir, string link_gacdir, string libdir, string link_libdir)
		{
			string failure_msg = "Failure adding assembly to the cache: ";

			if (!File.Exists (name)) {
				WriteLine (failure_msg + "The system cannot find the file specified.");
				Environment.Exit (1);
			}

			AssemblyName an = null;
			byte [] pub_tok;

			try {
				an = AssemblyName.GetAssemblyName (name);
			} catch {
				WriteLine (failure_msg + "The file specified is not a valid assembly.");
				return false;
			}

			pub_tok = an.GetPublicKeyToken ();
			if (pub_tok == null || pub_tok.Length == 0) {
				WriteLine (failure_msg + "Attempt to install an assembly without a strong name.");
				return false;
			}

			if (check_refs && !CheckReferencedAssemblies (an)) {
				WriteLine (failure_msg + "Attempt to install an assembly that references non " +
						"strong named assemblies with -check_refs enabled.");
				return false;
			}

			string [] siblings = { ".config", ".mdb" };
			string version_token = an.Version + "_" +
					       an.CultureInfo.Name.ToLower (CultureInfo.InvariantCulture) + "_" +
					       GetStringToken (pub_tok);
			string full_path = Path.Combine (Path.Combine (gacdir, an.Name), version_token);
			string asmb_file = Path.GetFileName (name);
			string asmb_path = Path.Combine (full_path, asmb_file);

			try {
				if (Directory.Exists (full_path)) {
					// Wipe out the directory. This way we ensure old assemblies	
					// config files, and AOTd files are removed.
					Directory.Delete (full_path, true);
				}
				Directory.CreateDirectory (full_path);
			} catch {
				WriteLine (failure_msg + "gac directories could not be created, " +
						"possibly permission issues.");
				return false;
			}

			File.Copy (name, asmb_path, true);

			foreach (string ext in siblings) {
				string sibling = String.Concat (name, ext);
				if (File.Exists (sibling))
					File.Copy (sibling, String.Concat (asmb_path, ext), true);
			}

			if (package != null) {
				string link_path = Path.Combine (Path.Combine (link_gacdir, an.Name), version_token);
				string ref_dir = Path.Combine (libdir, package);
				string ref_path = Path.Combine (ref_dir, asmb_file);

				if (File.Exists (ref_path))
					File.Delete (ref_path);
				try {
					Directory.CreateDirectory (ref_dir);
				} catch {
					WriteLine ("ERROR: Could not create package dir file.");
					Environment.Exit (1);
				}
				Symlink (name, Path.Combine (link_path, asmb_file), ref_path);

				WriteLine ("Package exported to: " + ref_path + " -> " +
						Path.Combine (link_path, asmb_file));

			}

			WriteLine ("{0} installed into the gac ({1})", an.Name, gacdir);
			return true;
		}

		private static bool Uninstall (string name, string package, string gacdir, string libdir)
		{
			string [] assembly_pieces = name.Split (new char[] { ',' });
			Hashtable asm_info = new Hashtable ();

			foreach (string item in assembly_pieces) {
				string[] pieces = item.Trim ().Split (new char[] { '=' }, 2);
				if(pieces.Length == 1)
					asm_info ["assembly"] = pieces [0];
				else
					asm_info [pieces[0].Trim ().ToLower (CultureInfo.InvariantCulture)] = pieces [1];
			}

			string asmdir = Path.Combine (gacdir, (string) asm_info ["assembly"]);
			if (!Directory.Exists (asmdir)) {
				WriteLine ("No assemblies found that match: " + name);
				return false;
			}

			string searchString = GetSearchString (asm_info);
			string [] directories = Directory.GetDirectories (asmdir, searchString);

			foreach (string dir in directories) {
				Directory.Delete (dir, true);
				if (package != null) {
					string link_dir = Path.Combine (libdir, package);
					string link = Path.Combine (link_dir, (string) asm_info ["assembly"] + ".dll");
					File.Delete (link);
					if (Directory.GetFiles (link_dir).Length == 0) {
						WriteLine ("Cleaning package directory, it is empty.");
						Directory.Delete (link_dir);
					}
				}
				WriteLine ("Assembly removed from the gac.");
			}

			if(Directory.GetDirectories (asmdir).Length == 0) {
				WriteLine ("Cleaning assembly dir, its empty");
				Directory.Delete (asmdir);
			}

			return true;
		}

		private static bool UninstallSpecific (string name, string package,
				string gacdir, string libdir)
		{
			string failure_msg = "Failure to remove assembly from the cache: ";

			if (!File.Exists (name)) {
				WriteLine (failure_msg + "The system cannot find the file specified.");
				return false;
			}

			AssemblyName an = null;

			try {
				an = AssemblyName.GetAssemblyName (name);
			} catch {
				WriteLine (failure_msg + "The file specified is not a valid assembly.");
				return false;
			}

			return Uninstall (an.FullName.Replace (" ", String.Empty),
					package, gacdir, libdir);
		}

		private static void List (string name, string gacdir)
		{
			WriteLine ("The following assemblies are installed into the GAC:");

			if (name != null) {
				FilteredList (name, gacdir);
				return;
			}

			int count = 0;
			DirectoryInfo gacinfo = new DirectoryInfo (gacdir);
			foreach (DirectoryInfo parent in gacinfo.GetDirectories ()) {
				foreach (DirectoryInfo dir in parent.GetDirectories ()) {
					WriteLine (AsmbNameFromVersionString (parent.Name, dir.Name));
					count++;
				}
			}
			WriteLine ("Number of items = " + count);
		}

		private static void FilteredList (string name, string gacdir)
		{
			string [] assembly_pieces = name.Split (new char[] { ',' });
			Hashtable asm_info = new Hashtable ();

			foreach (string item in assembly_pieces) {
				string[] pieces = item.Trim ().Split (new char[] { '=' }, 2);
				if(pieces.Length == 1)
					asm_info ["assembly"] = pieces [0];
				else
					asm_info [pieces[0].Trim ().ToLower (CultureInfo.InvariantCulture)] = pieces [1];
			}
			
			string asmdir = Path.Combine (gacdir, (string) asm_info ["assembly"]);
			if (!Directory.Exists (asmdir)) {
				WriteLine ("Number of items = 0");
				return;
			}
			string search = GetSearchString (asm_info);
			string [] dir_list = Directory.GetDirectories (asmdir, search);

			int count = 0;
			foreach (string dir in dir_list) {
				WriteLine (AsmbNameFromVersionString ((string) asm_info ["assembly"],
						new DirectoryInfo (dir).Name));
				count++;
			}
			WriteLine ("Number of items = " + count);
		}

		private static bool InstallFromList (bool check_refs, string list_file, string package,
				string gacdir, string link_gacdir, string libdir, string link_libdir)
		{
			StreamReader s = null;
			int processed, failed;

			processed = failed = 0;

			try {
				s = new StreamReader (list_file);

				string line;
				while ((line = s.ReadLine ()) != null) {
					if (!Install (check_refs, line, package, gacdir, link_gacdir,
							    libdir, link_libdir))
						failed++;
					processed++;
							
				}
			} catch (IOException ioe) {
				WriteLine ("Failed to open assemblies list file " + list_file + ".");
				return false;
			} finally {
				if (s != null)
					s.Close ();
			}

			return true;
		}

		private static bool UninstallFromList (string list_file, string package,
				string gacdir, string libdir)
		{
			StreamReader s = null;
			int processed, failed;

			processed = failed = 0;

			try {
				s = new StreamReader (list_file);

				string line;
				while ((line = s.ReadLine ()) != null) {
					if (!Uninstall (line, package, gacdir, libdir))
						failed++;
					processed++;
				}
			} catch (IOException ioe) {
				WriteLine ("Failed to open assemblies list file " + list_file + ".");
				return false;
			} finally {
				if (s != null)
					s.Close ();
			}

			return true;
		}

		private static bool CheckReferencedAssemblies (AssemblyName an)
		{
			AppDomain d = null;
			try {
				Assembly a = Assembly.LoadFrom (an.CodeBase);
				AssemblyName corlib = typeof (object).Assembly.GetName ();

				foreach (AssemblyName ref_an in a.GetReferencedAssemblies ()) {
					if (ref_an.Name == corlib.Name) // Just do a string compare so we can install on diff versions
						continue;
					byte [] pt = ref_an.GetPublicKeyToken ();
					if (pt == null || pt.Length == 0) {
						WriteLine ("Assembly " + ref_an.Name + " is not strong named.");
						return false;
					}
				}
			} catch	 (Exception e) {
				WriteLine (e.ToString ()); // This should be removed pre beta3
				return false;
			} finally {
				if (d != null) {
					try {
						AppDomain.Unload (d);
					} catch { }
				}
			}

			return true;
		}

		private static string GetSearchString (Hashtable asm_info)
		{
			if (asm_info.Keys.Count == 1)
				return "*";
			string version, culture, token;

			version = asm_info ["version"] as string;
			version = (version == null ? "*" : version);
			culture = asm_info ["culture"] as string;
			culture = (culture == null ? "*" : culture.ToLower (CultureInfo.InvariantCulture));
			token = asm_info ["publickeytoken"] as string;
			token = (token == null ? "*" : token.ToLower (CultureInfo.InvariantCulture));
			
			return String.Format ("{0}_{1}_{2}", version, culture, token);
		}

		private static string AsmbNameFromVersionString (string name, string str)
		{
			string [] pieces = str.Split ('_');
			return String.Format ("{0}, Version={1}, Culture={2}, PublicKeyToken={3}",
					name, pieces [0], (pieces [1] == String.Empty ? "neutral" : pieces [1]),
					pieces [2]);
		}

		private static bool IsSwitch (string arg)
		{
			return (arg [0] == '-' || arg [0] == '/');
		}

		private static Command GetCommand (string arg)
		{
			Command c = Command.Unknown;

			switch (arg) {
			case "-i":
			case "/i":
			case "--install":
				c = Command.Install;
				break;
			case "-il":
			case "/il":
			case "--install-from-list":
				c = Command.InstallFromList;
				break;
			case "-u":
			case "/u":
			case "/uf":
			case "--uninstall":
				c = Command.Uninstall;
				break;
			case "-ul":
			case "/ul":
			case "--uninstall-from-list":
				c = Command.UninstallFromList;
				break;
			case "-us":
			case "/us":
			case "--uninstall-specific":
				c = Command.UninstallSpecific;
				break;
			case "-l":
			case "/l":
			case "--list":
				c = Command.List;
				break;
			case "-?":
			case "/?":
			case "--help":
				c = Command.Help;
				break;
			}
			return c;	 
		}

		// name = name of dll being installed
		// oldpath = path the symlink points to -- cannot be assumed to be a valid file
		// newpath = name of symlink
		private static void Symlink (string name, string oldpath, string newpath) {
			if (Path.DirectorySeparatorChar == '/') {
				symlink (oldpath, newpath);
			} else {
				File.Copy (name, newpath);
			}
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int symlink (string oldpath, string newpath);

		private static string GetGacDir () {
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath",
					BindingFlags.Static|BindingFlags.NonPublic);
			if (gac == null) {
				WriteLine ("ERROR: Mono runtime not detected, please use " +
						"the mono runtime for gacutil.exe");
				Environment.Exit (1);
			}
			MethodInfo get_gac = gac.GetGetMethod (true);
			return (string) get_gac.Invoke (null, null);
		}

		private static string GetLibDir () {
			MethodInfo libdir = typeof (System.Environment).GetMethod ("internalGetGacPath",
					BindingFlags.Static|BindingFlags.NonPublic);
			if (libdir == null) {
				WriteLine ("ERROR: Mono runtime not detected, please use " +
						"the mono runtime for gacutil.exe");
				Environment.Exit (1);
			}
			return Path.Combine ((string)libdir.Invoke (null, null), "mono");
		}

		private static string GetStringToken (byte[] tok)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < tok.Length ; i++)
				sb.Append (tok[i].ToString ("x2"));
			return sb.ToString ();
		}

		private static string CombinePaths (string a, string b)
		{
			string dsc = Path.DirectorySeparatorChar.ToString ();
			string sep = (a.EndsWith (dsc) ? String.Empty : dsc);
			string end = (b.StartsWith (dsc) ? b.Substring (1) : b);
			return String.Concat (a, sep, end);
		}

		private static string EnsureLib (string dir)
		{
			DirectoryInfo d = new DirectoryInfo (dir);
			if (d.Name == "lib")
				return dir;
			return Path.Combine (dir, "lib");
		}

		private static void WriteLine ()
		{
			if (silent)
				return;
			Console.WriteLine ();
		}

		private static void WriteLine (string line)
		{
			if (silent)
				return;
			Console.WriteLine (line);
		}

		private static void WriteLine (string line, params object [] p)
		{
			if (silent)
				return; 
			Console.WriteLine (line, p);
		}

		private static void Usage ()
		{
			ShowHelp (false);
			Environment.Exit (1);
		}

		private static void ShowHelp (bool detailed)
		{
			WriteLine ("Usage: gacutil.exe <commands> [ <options> ]");
			WriteLine ("Commands:");

			WriteLine ("-i <assembly_path> [-check_refs] [-package NAME] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tInstalls an assembly into the global assembly cache.");
			if (detailed) {
				WriteLine ("\t<assembly_path> is the name of the file that contains the " +
						"\tassembly manifest\n" +
						"\tExample: -i myDll.dll");
			}
			WriteLine ();

			WriteLine ("-il <assembly_list_file> [-check_refs] [-package NAME] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tInstalls one or more assemblies into the global assembly cache.");
			if (detailed) {
				WriteLine ("\t<assembly_list_file> is the path to a test file containing a list of\n" +
						"\tassembly file paths on separate lines.\n" +
						"\tExample -il assembly_list.txt\n" +
						"\t\tassembly_list.txt contents:\n" +
						"\t\tassembly1.dll\n" +
						"\t\tassembly2.dll");
			}
			WriteLine ();
			
			WriteLine ("-u <assembly_display_name> [-package NAME] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tUninstalls an assembly from the global assembly cache.");
			if (detailed) {
				WriteLine ("\t<assembly_display_name> is the name of the assembly (partial or\n" +
						"\tfully qualified) to remove from the global assembly cache. If a \n" +
						"\tpartial name is specified all matching assemblies will be uninstalled.\n" +
						"\tExample: -u myDll,Version=1.2.1.0");
			}
			WriteLine ();

			WriteLine ("-ul <assembly_list_file> [-package NAME] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tUninstalls one or more assemblies from the global assembly cache.");
			if (detailed) {
				WriteLine ("\t<assembly_list_file> is the path to a test file containing a list of\n" +
						"\tassembly names on separate lines.\n" +
						"\tExample -ul assembly_list.txt\n" +
						"\t\tassembly_list.txt contents:\n" +
						"\t\tassembly1,Version=1.0.0.0,Culture=en,PublicKeyToken=0123456789abcdef\n" +
						"\t\tassembly2,Version=2.0.0.0,Culture=en,PublicKeyToken=0123456789abcdef");
			}
			WriteLine ();

			WriteLine ("-us <assembly_path> [-package NAME] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tUninstalls an assembly using the specifed assemblies full name.");
			if (detailed) {
				WriteLine ("\t<assembly path> is the path to an assembly. The full assembly name\n" +
						"\tis retrieved from the specified assembly if there is an assembly in\n" +
						"\tthe GAC with a matching name, it is removed.\n" +
						"\tExample: -us myDll.dll");
			}
			WriteLine ();

			WriteLine ("-l [assembly_name] [-root ROOTDIR] [-gacdir GACDIR]");
			WriteLine ("\tLists the contents of the global assembly cache.");
			if (detailed) {
				WriteLine ("\tWhen the <assembly_name> parameter is specified only matching\n" +
						"\tassemblies are listed.");
			}
			WriteLine ();

			WriteLine ("-?");
			WriteLine ("\tDisplays a detailed help screen");
			WriteLine ();

			if (!detailed)
				return;

			WriteLine ("Options:");
			WriteLine ("-package <NAME>");
			WriteLine ("\tUsed to create a directory in prefix/lib/mono with the name NAME, and a\n" +
					"\tsymlink is created from NAME/assembly_name to the assembly on the GAC.\n" +
					"\tThis is used so developers can reference a set of libraries at once.");
			WriteLine ();

			WriteLine ("-gacdir <GACDIR>");
			WriteLine ("\tUsed to specify the GACs base directory. Once an assembly has been installed\n" +
					"\tto a non standard gacdir the MONO_GAC_PREFIX environment variable must be used\n" +
					"\tto access the assembly.");
			WriteLine ();

			WriteLine ("-root <ROOTDIR>");
			WriteLine ("\tUsed by developers integrating this with automake tools or packaging tools\n" +
					"\tthat require a prefix directory to  be specified. The root represents the\n" +
					"\t\"libdir\" component of a prefix (typically prefix/lib).");
			WriteLine ();

			WriteLine ("-check_refs");
			WriteLine ("\tUsed to ensure that the assembly being installed into the GAC does not\n" +
					"\treference any non strong named assemblies. Assemblies being installed to\n" +
					"\tthe GAC should not reference non strong named assemblies, however the is\n" +
					"\tan optional check.");

			WriteLine ();
			WriteLine ("Ignored Options:");
			WriteLine ("-f");
			WriteLine ("\tThe Mono gacutil ignores the -f option to maintian commandline compatibility with");
			WriteLine ("\tother gacutils. gacutil will always force the installation of a new assembly.");

			WriteLine ();
			WriteLine ("-r <reference_scheme> <reference_id> <description>");
			WriteLine ("\tThe Mono gacutil has not implemented traced references and will emit a warning");
			WriteLine ("\twhen this option is used.");
		}
	}
}

