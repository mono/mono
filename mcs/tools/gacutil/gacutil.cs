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

namespace Mono.Tools
{

	public class Driver
	{

		private string gac_path = "/usr/lib/mono/gac/";
		
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
				case "--un-install":
					UninstallAssemblies (remainder_args);
					break;
				default:
					ShowHelp (false);
					break;
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

			//FIXME: need to ensure strongly named here.

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
			Console.WriteLine ("help placeholder");
		}
	}
}
