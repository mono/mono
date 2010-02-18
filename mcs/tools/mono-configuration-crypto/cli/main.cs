using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Mono.Options;
using Mono.Configuration.Crypto;

[assembly: AssemblyTitle ("mono-configuration-crypto")]
[assembly: AssemblyDescription ("Mono configuration utility to manage encryption keys and encrypt/decrypt config file sections")]
[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct ("Mono Configuration Cryptography Tools")]
[assembly: AssemblyCopyright ("Copyright (c) 2010 Novell, Inc (http://novell.com, http://mono-project.com/)")]
[assembly: AssemblyVersion (Consts.FxVersion)]

namespace MonoConfigurationCrypto
{
	class MonoConfigurationCrypto
	{
		void Success ()
		{
			Console.WriteLine ("Success.");
		}
		
		void Failure (Exception ex, Config cfg)
		{
			Failure (ex, cfg, null);
		}

		void Failure (Config cfg, string message, params object[] parms)
		{
			Failure (null, cfg, message, parms);
		}
		
		void Failure (Exception ex, Config cfg, string message, params object[] parms)
		{
			if (!String.IsNullOrEmpty (message)) {
				if (parms == null || parms.Length == 0)
					Console.Error.WriteLine (message);
				else
					Console.Error.WriteLine (message, parms);
			}
			
			if (ex != null) {
				if (cfg.Verbose)
					Console.Error.WriteLine (ex.ToString ());
				else
					Console.Error.WriteLine (ex.Message);
			}
			Console.Error.WriteLine ("Failure.");
		}
		
		bool ListContainers (Config cfg)
		{
			try {
				var kc = new KeyContainerCollection (cfg.UseMachinePath);
				int count;
				foreach (KeyContainer c in kc) {
					count = c.Count;
					Console.WriteLine ("{0} container '{1}' ({2} key{3})", c.Local ? "Local" : "Global", c.Name, count,
							   count == 1 ? String.Empty : "s");
				}
			} catch (Exception ex) {
				Failure (ex, cfg);
			}
			
			return true;
		}

		string FindConfigFile (string path, string configFileName)
		{
			if (!Directory.Exists (path))
				return null;
			
			string fileName = null;
			foreach (var s in Directory.GetFiles (path, "*.*", SearchOption.TopDirectoryOnly)) {
				string fn = Path.GetFileName (s);
				if (String.Compare (fn, configFileName, StringComparison.OrdinalIgnoreCase) == 0) {
					fileName = fn;
					break;
				}
			}

			if (fileName == null)
				return null;

			return Path.Combine (path, fileName);
		}
		
		bool EncryptSection (Config cfg)
		{
			string configSection = cfg.ConfigSectionName;
			string containerName = cfg.ContainerName;
			string configFile = FindConfigFile (cfg.ApplicationPhysicalPath, cfg.ConfigFileName);

			Console.WriteLine ("Encrypting section '{0}' in config file '{1}' using key container '{2}'...", configSection, configFile, containerName);
			
			if (String.IsNullOrEmpty (configFile)) {
				Failure (cfg, "No config file found in directory '{0}'", cfg.ApplicationPhysicalPath);
				return true;
			}
			
			if (String.IsNullOrEmpty (configSection)) {
				Failure (cfg, "No config section name specified.");
				return true;
			}
			
			try {
				var cs = new ConfigSection ();
				cs.Encrypt (configFile, configSection, containerName, cfg.UseMachinePath);
				Console.WriteLine ("Success.");
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}

		bool DecryptSection (Config cfg)
		{
			string configSection = cfg.ConfigSectionName;
			string containerName = cfg.ContainerName;
			string configFile = FindConfigFile (cfg.ApplicationPhysicalPath, cfg.ConfigFileName);

			Console.WriteLine ("Decrypting section '{0}' in config file '{1}' using key container '{2}'...", configSection, configFile, containerName);
			
			if (String.IsNullOrEmpty (configFile)) {
				Failure (cfg, "No config file found in directory '{0}'", cfg.ApplicationPhysicalPath);
				return true;
			}
			
			if (String.IsNullOrEmpty (configSection)) {
				Failure (cfg, "No config section name specified.");
				return true;
			}
			
			try {
				var cs = new ConfigSection ();
				cs.Decrypt (configFile, configSection, containerName, cfg.UseMachinePath);
				Console.WriteLine ("Success.");
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}

		bool CreateKey (Config cfg)
		{
			string name = cfg.ContainerName;
			KeyContainerCollection kc;

			Console.WriteLine ("Creating RSA key container '{0}'...", name);
			try {
				kc = new KeyContainerCollection (cfg.UseMachinePath);
				if (kc.Contains (name)) {
					Failure (cfg, "The RSA container already exists.");
					return true;
				}

				var k = new Key (name, cfg.KeySize, cfg.UseMachinePath);
				if (!k.IsValid) {
					Failure (cfg, "Failed to generate RSA key pair.");
					return true;
				}
				k.Save ();
				Success ();
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}

		bool ImportKey (Config cfg)
		{
			string containerName = cfg.ContainerName;
			string fileName = cfg.FileName;

			Console.WriteLine ("Importing an RSA key from file '{0}' into the container '{1}'...", fileName, containerName);
			if (String.IsNullOrEmpty (containerName)) {
				Failure (cfg, "Unspecified container name.");
				return true;
			}

			if (String.IsNullOrEmpty (fileName)) {
				Failure (cfg, "Unspecified file name.");
				return true;
			}

			if (!File.Exists (fileName)) {
				Failure (cfg, "Key file '{0}' does not exist.", fileName);
				return true;
			}

			KeyContainerCollection kcc;
			Key key;
			KeyContainer kc;
			try {
				kcc = new KeyContainerCollection (cfg.UseMachinePath);
				kc = kcc [containerName];
				
				if (kc != null)
					key = kc [0];
				else
					key = null;

				// No validation is performed on the key - this is left for the
				// encryption algorithm implementation to do.
				string keyvalue = File.ReadAllText (fileName);
				if (key == null)
					key = new Key (containerName, keyvalue, cfg.UseMachinePath);
				else {
					key.KeyValue = keyvalue;
					key.ContainerName = containerName;
				}

				key.Save ();
				Console.WriteLine ("Success.");
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}

		bool ExportKey (Config cfg)
		{
			string containerName = cfg.ContainerName;
			string fileName = cfg.FileName;

			Console.WriteLine ("Exporting an RSA key from container '{0}' to file '{1}'...", containerName, fileName);
			if (String.IsNullOrEmpty (containerName)) {
				Failure (cfg, "Unspecified container name.");
				return true;
			}

			if (String.IsNullOrEmpty (fileName)) {
				Failure (cfg, "Unspecified file name.");
				return true;
			}

			KeyContainerCollection kcc;
			Key key;
			KeyContainer kc;
			try {
				kcc = new KeyContainerCollection (cfg.UseMachinePath);
				kc = kcc [containerName];
				
				if (kc != null)
					key = kc [0];
				else {
					Failure (cfg, "Container '{0}' does not exist.", containerName);
					return true;
				}

				if (key == null) {
					Failure (cfg, "Container '{0}' exists but it does not contain any keys.", containerName);
					return true;
				}
				
				File.WriteAllText (fileName, key.KeyValue);
				Console.WriteLine ("Success.");
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}

		bool RemoveContainer (Config cfg)
		{
			string containerName = cfg.ContainerName;
			Console.WriteLine ("Removing container '{0}'...", containerName);
			if (String.IsNullOrEmpty (containerName)) {
				Failure (cfg, "Unspecified container name.");
				return true;
			}

			try {
				KeyContainer.RemoveFromDisk (containerName, cfg.UseMachinePath);
				
				Console.WriteLine ("Success.");
			} catch (Exception ex) {
				Failure (ex, cfg);
				return true;
			}
			
			return false;
		}
		
		void ShowHeader ()
		{
			string title = String.Empty, version = String.Empty, description = String.Empty, copyright = String.Empty;
			Assembly asm = Assembly.GetExecutingAssembly ();
			foreach (object o in asm.GetCustomAttributes (false)) {
				if (o is AssemblyTitleAttribute)
					title = ((AssemblyTitleAttribute)o).Title;
				else if (o is AssemblyCopyrightAttribute)
					copyright = ((AssemblyCopyrightAttribute)o).Copyright;
				else if (o is AssemblyDescriptionAttribute)
					description = ((AssemblyDescriptionAttribute)o).Description;
			}

			version = asm.GetName ().Version.ToString ();
			
			Console.WriteLine ("{1} - version {2}{0}{3}{0}{4}{0}",
					   Environment.NewLine,
					   title,
					   version,
					   description,
					   copyright);
		}
		
		void Run (string[] args)
		{
			var cfg = new Config ();
			var actions = new List <Func <Config, bool>> ();
			var options = new OptionSet () {
				{ "h|?|help", "Show usage information", v => cfg.ShowHelp = true },
				{ "v|verbose", "Show verbose information (including exception stacktraces)", v => cfg.Verbose = true },
				{ "m|machine|global", "Use machine (global) store for all the key actions", v => cfg.UseMachinePath = true },
				{ "u|user|local", "Use local (user) store for all the key actions [*]", v => cfg.UseMachinePath = false },
				{ "l|list", "List all the key container names in the store", v => actions.Add (ListContainers) },
				{ "c|create", "Creates an RSA public/private key pair", v => actions.Add (CreateKey) },
				{ "i|import", "Import key to a container", v => actions.Add (ImportKey) },
				{ "x|export", "Export key from a container", v => actions.Add (ExportKey) },
				{ "r|remove", "Remove a container", v => actions.Add (RemoveContainer) },
				{ "f=|file=", "File name for import or export operations", (string s) => cfg.FileName = s },
				{ "cf=|config-file=", String.Format ("Config file name (not path) [{0}]", Config.DefaultConfigFileName), (string s) => cfg.ConfigFileName = s },
				{ "n=|name=", String.Format ("Container name [{0}]", Config.DefaultContainerName), (string s) => cfg.ContainerName = s },
				{ "s=|size=", String.Format ("Key size [{0}]", Config.DefaultKeySize), (uint s) => cfg.KeySize = s },
				{ "p=|path=", String.Format ("Application physical path [{0}]", Config.DefaultApplicationPhysicalPath), (string s) => cfg.ApplicationPhysicalPath = s },
				{ "d=|dec=|decrypt=", "Decrypt configuration section", (string s) => { cfg.ConfigSectionName = s; actions.Add (DecryptSection);} },
				{ "e=|enc=|encrypt=", "Encrypt configuration section", (string s) => { cfg.ConfigSectionName = s; actions.Add (EncryptSection);} },
			};
			options.Parse (args);

			if (cfg.ShowHelp) {
				ShowHeader ();
				options.WriteOptionDescriptions (Console.Out);
				return;
			}
			
			foreach (var action in actions)
				if (action (cfg))
					Environment.Exit (0);
		}
		
		static void Main (string[] args)
		{
			new MonoConfigurationCrypto ().Run (args);
		}
	}
}
