//
// Mono.Tools.GacUtil
//
// Author(s):
//  Tood Berman <tberman@gentoo.org>
//  Jackson Harper <jackson@ximian.com>
//
// Copyright 2003 Todd Berman
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
			Uninstall,
			UninstallSpecific,
			List,
			Help
		}

		public static int Main (string [] args)
		{
			if (args.Length == 0)
				Usage ();

			Command command = GetCommand (args [0]);

			if (command == Command.Unknown && IsSwitch (args [0])) {
				Console.WriteLine ("Unknown command: " + args [0]);
				Environment.Exit (1);
			} else if (command == Command.Unknown) {
				Usage ();
			} else if (command == Command.Help) {
				ShowHelp (true);
				Environment.Exit (1);
			}

			string libdir = GetLibDir ();
			string name, package, gacdir, root;
			name = package = root = gacdir = null;

			for (int i=1; i<args.Length; i++) {
				if (IsSwitch (args [i])) {

					// for cmd line compatibility with other gacutils
					if (args [i] == "-f" || args [i] == "/f")
						continue;

					if (i + 1 >= args.Length) {
						Console.WriteLine ("Option " + args [i] + " takes 1 argument");
						Environment.Exit (1);
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

			if (gacdir == null)
				gacdir = GetGacDir ();
			else
				gacdir = Path.Combine (Path.Combine (gacdir, "mono"), "gac");

			string link_gacdir = gacdir;
			if (root != null) {
				libdir = Path.Combine (root, "mono");
				gacdir = Path.Combine (libdir, "gac");
			}

			switch (command) {
			case Command.Install:
				if (name == null) {
					Console.WriteLine ("Option " + args [0] + " takes 1 argument");
					Environment.Exit (1);
				}
				Install (name, package, gacdir, link_gacdir, libdir);
				break;
			case Command.Uninstall:
				if (name == null) {
					Console.WriteLine ("Option " + args [0] + " takes 1 argument");
					Environment.Exit (1);
				}
				Uninstall (name, package, gacdir, libdir);
				break;
			case Command.UninstallSpecific:
				if (name == null) {
					Console.WriteLine ("Opetion " + args [0] + " takes 1 argument");
					Environment.Exit (1);
				}
				UninstallSpecific (name, package, gacdir, libdir);
				break;
			case Command.List:
				List (name, gacdir);
				break;
			}

			return 0;
		}

		private static void Install (string name, string package,
				string gacdir, string link_gacdir, string libdir)
		{
			string failure_msg = "Failure adding assembly to the cache: ";

			if (!File.Exists (name)) {
				Console.WriteLine (failure_msg + "The system cannot find the file specified.");
				Environment.Exit (1);
			}

			AssemblyName an = null;
			byte [] pub_tok;

			try {
				an = AssemblyName.GetAssemblyName (name);
			} catch {
				Console.WriteLine (failure_msg + "The file specified is not a valid assembly.");
				Environment.Exit (1);
			}

			pub_tok = an.GetPublicKeyToken ();
			if (pub_tok == null || pub_tok.Length == 0) {
				Console.WriteLine (failure_msg + "Attempt to install an assembly without a strong name.");
				Environment.Exit (1);
			}

			string conf_name = name + ".config";
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
				Console.WriteLine (failure_msg + "gac directories could not be created, " +
						"possibly permission issues.");
				// Environment.Exit (1);
				throw;
			}

			File.Copy (name, asmb_path, true);
			if (File.Exists (conf_name))
				File.Copy (conf_name, asmb_path + ".config", true);

			if (package != null) {
				string link_path = Path.Combine (Path.Combine (link_gacdir,an.Name), version_token);
				string ref_dir = Path.Combine (libdir, package);
				string ref_path = Path.Combine (ref_dir, asmb_file);
				if (File.Exists (ref_path))
					File.Delete (ref_path);
				try {
					Directory.CreateDirectory (ref_dir);
				} catch {
					Console.WriteLine ("ERROR: Could not create package dir file.");
					Environment.Exit (1);
				}
				Symlink (Path.Combine (link_path, asmb_file), ref_path);
				Console.WriteLine ("Package exported to: " + Path.Combine (libdir, package));
			}

			Console.WriteLine ("{0} installed into the gac ({1})", an.Name, gacdir); 
		}

		private static void Uninstall (string name, string package, string gacdir, string libdir)
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
				Console.WriteLine ("No assemblies found that match: " + name);
				Environment.Exit (1);
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
						Console.WriteLine ("Cleaning package directory, it is empty.");
						Directory.Delete (link_dir);
					}
				}
				Console.WriteLine ("Assembly removed from the gac.");
			}

			if(Directory.GetDirectories (asmdir).Length == 0) {
				Console.WriteLine ("Cleaning assembly dir, its empty");
				Directory.Delete (asmdir);
			}
		}

		private static void UninstallSpecific (string name, string package,
				string gacdir, string libdir)
		{
			string failure_msg = "Failure to remove assembly from the cache: ";

			if (!File.Exists (name)) {
				Console.WriteLine (failure_msg + "The system cannot find the file specified.");
				Environment.Exit (1);
			}

			AssemblyName an = null;

			try {
				an = AssemblyName.GetAssemblyName (name);
			} catch {
				Console.WriteLine (failure_msg + "The file specified is not a valid assembly.");
				Environment.Exit (1);
			}

			Uninstall (an.FullName.Replace (" ", String.Empty),
					package, gacdir, libdir);
		}

		private static void List (string name, string gacdir)
		{
			Console.WriteLine ("The following assemblies are installed into the GAC:");

			if (name != null) {
				FilteredList (name, gacdir);
				return;
			}

			int count = 0;
			DirectoryInfo gacinfo = new DirectoryInfo (gacdir);
			foreach (DirectoryInfo parent in gacinfo.GetDirectories ()) {
				foreach (DirectoryInfo dir in parent.GetDirectories ()) {
					Console.WriteLine (AsmbNameFromVersionString (parent.Name, dir.Name));
					count++;
				}
			}
			Console.WriteLine ("Number of items = " + count);
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
				Console.WriteLine ("Number of items = 0");
				return;
			}
			string search = GetSearchString (asm_info);
			string [] dir_list = Directory.GetDirectories (asmdir, search);

			int count = 0;
			foreach (string dir in dir_list) {
				Console.WriteLine (AsmbNameFromVersionString ((string) asm_info ["assembly"],
						new DirectoryInfo (dir).Name));
				count++;
			}
			Console.WriteLine ("Number of items = " + count);
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
			return (arg [0] == '-' || (arg [0] == '/' && !File.Exists (arg)));
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
			case "-u":
			case "/u":
			case "/uf":
			case "--uninstall":
				c = Command.Uninstall;
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

		private static void Symlink (string oldpath, string newpath) {
			if (Path.DirectorySeparatorChar == '/') {
				symlink (oldpath, newpath);
			} else {
				File.Copy (oldpath, newpath);
			}
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int symlink (string oldpath, string newpath);

		private static string GetGacDir () {
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath",
					BindingFlags.Static|BindingFlags.NonPublic);
			if (gac == null) {
				Console.WriteLine ("ERROR: Mono runtime not detected, please use " +
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
				Console.WriteLine ("ERROR: Mono runtime not detected, please use " +
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

		private static void Usage ()
		{
			ShowHelp (false);
			Environment.Exit (1);
		}

		private static void ShowHelp (bool detailed)
		{
			Console.WriteLine ("Usage: gacutil.exe <commands> [ <options> ]");
			Console.WriteLine ("Commands:");

			Console.WriteLine ("-i <assembly_path> [-package NAME] [-root ROOTDIR]");
			Console.WriteLine ("\tInstalls an assembly into the global assembly cache.");
			if (detailed) {
				Console.WriteLine ("\t<assembly_path> is the name of the file that " +
						"contains the assembly manifest\n" +
						"\tExample: -i myDll.dll");
			}
			Console.WriteLine ();

			Console.WriteLine ("-u <assembly_display_name> [-package NAME] [-root ROOTDIR]");
			Console.WriteLine ("\tUninstalls an assembly from the global assembly cache.");
			if (detailed) {
				Console.WriteLine ("\t<assembly_display_name> is the name of the assembly\n" +
						"\t(partial or fully qualified) to remove " +
						"from the global assembly cache.\n" +
						"\tIf a partial name is specified all matching " +
						"assemblies will be uninstalled.\n" +
						"\tExample: /u myDll,Version=1.2.1.0\n");
			}
			Console.WriteLine ();

			Console.WriteLine ("-l [assembly_name] [-root ROOTDIR]");
			Console.WriteLine ("\tLists the contents of the global assembly cache.");
			if (detailed) {
				Console.WriteLine ("\tWhen the <assembly_name> parameter is specified only " +
						"matching assemblies are listed.");
			}
			Console.WriteLine ();

			Console.WriteLine ("-?");
			Console.WriteLine ("\tDisplays a detailed help screen\n");
			Console.WriteLine ();
		}
	}
}

