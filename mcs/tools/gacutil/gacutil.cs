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
			}
		}

		public void UninstallAssemblies (string[] args)
		{
			if(args.Length == 0) {
				Console.WriteLine ("ERROR: need an argument to uninstall");
				return;
			}

			if(!Directory.Exists (gac_path + Path.DirectorySeparatorChar  + args[0])) {
				Console.WriteLine ("ERROR: assembly is not in the gac");
				return;
			}

			Directory.Delete (gac_path + Path.DirectorySeparatorChar + args[0], true);
			Console.WriteLine ("Assembly '" + args[0] + "' removed from the gac.");
		}

		public void ListAssemblies (string[] args)
		{
			Console.WriteLine ("The following assemblies are installed into the GAC:");
			DirectoryInfo d = new DirectoryInfo (gac_path);
			foreach (DirectoryInfo namedDir in d.GetDirectories ()) {
				foreach (DirectoryInfo assemblyDir in namedDir.GetDirectories ()) {
					Hashtable assemblyInfo = GetAssemblyInfo (assemblyDir.FullName + Path.DirectorySeparatorChar + "__AssemblyInfo__");
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
				Console.WriteLine ("ERROR: assembly is already in gac");
				return;
			}

			if(!EnsureDirectories (an.Name, version_token)) {
				Console.WriteLine ("ERROR: gac directories could not be created, possibly permission issues");
				return;
			}

			File.Copy (args[0], fullPath + an.Name + ".dll", force);

			StreamWriter sr = File.CreateText (fullPath + "__AssemblyInfo__");
			sr.WriteLine ("DisplayName=" + an.FullName);
			sr.Close ();

			Console.WriteLine ("Assembly installed into the gac");
		}

		private bool EnsureDirectories (string name, string tok)
		{
			//FIXME: Workaround for broken DirectoryInfo.CreateSubdirectory
			try {
				DirectoryInfo d = new DirectoryInfo (gac_path);

				d.CreateSubdirectory (name);
				d = new DirectoryInfo (gac_path + Path.DirectorySeparatorChar + name);
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
