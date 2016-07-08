//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mono.MonoConfig 
{
	delegate int HandleCommand (MConfigOptions options, Configuration config);
	
	struct CommandHandler {
		public readonly HandleCommand Handler;
		public readonly string[] Names;
			
		public CommandHandler (string[] names, HandleCommand handler)
		{
			this.Names = names;
			this.Handler = handler;
		}
	};
	
	class MConfigOptions
	{
		string[] usage = {
			"Usage: mconfig [options] command [command_parameters]",
			"Options:",
			"",
			"  -?,-h,--help                      Display this usage information",
			"  -v,--version                      Display version information",
			"  -c,--config=<filepath>            Read the specified config file in addition to",
			"                                    the standard ones. Settings in this file override ones",
			"                                    in the other files.",
			"  -t,--target={any,web,application} Use this target when executing 'command'",
			"",
			"To see the list of commands, features and default config file templates, run mconfig",
			"without any parameters"
		};

		string[] usageCommands = {
			"Available commands (see 'man mconfig' for details):",
			"  {addfeature,af} <feature_name> [config_file_path]",
			"     Add the named feature to the specified config file",
			"",
			"  {defconfig,dc} [template_name] [target_directory]",
			"     Write a config file based on the named template.",
			""
		};		

		List <string> plain_arguments;
		Dictionary <string, string> unknown_arguments;

		public string ConfigFile;
		public FeatureTarget Target = FeatureTarget.Any;

		public Dictionary <string, string> UnknownArguments {
			get {
				if (unknown_arguments == null || unknown_arguments.Count == 0)
					return null;

				return unknown_arguments;
			}
		}
		
		public string[] PlainArguments {
			get {
				if (plain_arguments == null || plain_arguments.Count == 0)
					return null;
				
				return plain_arguments.ToArray ();
			}
		}
		
		public MConfigOptions ()
		{
			unknown_arguments = new Dictionary <string, string> ();
			plain_arguments = new List <string> ();
		}

		public void Parse (string[] args)
		{
			if (args == null || args.Length == 0)
				return;

			int len = args.Length;
			string arg;
			
			for (int i = 0; i < len; i++) {
				arg = args [i];

				switch (arg [0]) {
					case '-':
					case '/':
						i += ProcessArgument (i, arg, args, len);
						break;

					default:
						plain_arguments.Add (arg);
						break;
				}
			}
		}

		static char[] paramStartChars = {':', '='};
		
		int ProcessArgument (int idx, string argument, string[] args, int argsLen)
		{
			int argnameIdx = 1;
			bool haveMoreDashes = false, badArg = false;
			int argumentLen = argument.Length;

			if (argumentLen < 2)
				badArg = true;
			
			haveMoreDashes = !badArg && (argument [1] == '-');
			
			if (argumentLen == 2 && haveMoreDashes)
				badArg = true;
			
			if (badArg) {
				Console.Error.WriteLine ("Invalid argument: {0}", argument);
				Environment.Exit (1);
			}

			if (haveMoreDashes)
				argnameIdx++;

			int paramPos = argument.IndexOfAny (paramStartChars, argnameIdx);
			bool haveParam = true;
			
			if (paramPos == -1) {
				haveParam = false;
				paramPos = argumentLen;
			}
			
			string argName = argument.Substring (argnameIdx, paramPos - argnameIdx);
			string argParam = haveParam ? argument.Substring (paramPos + 1) : null;

			int ret = 0;
			
			if (!haveParam && haveMoreDashes) {
				idx++;
				if (idx < argsLen) {
					argParam = args [idx];
					ret++;
					haveParam = true;
				}
			}
			
			switch (argName) {
				case "?":
				case "h":
				case "help":
					Usage ();
					break;

				case "v":
				case "version":
					ShowVersion ();
					break;

				case "t":
				case "target":
					if (!haveParam)
						RequiredParameterMissing (argName);
					
					try {
						Target = Helpers.ConvertEnum <FeatureTarget> (argParam, "target");
					} catch (Exception ex) {
						OptionParameterError (argName, ex.Message);
					}
					break;

				default:
					unknown_arguments.Add (argName, argParam);
					break;
			}
			
			return ret;
		}

		void RequiredParameterMissing (string argName)
		{
			Console.Error.WriteLine ("Argument '{0}' requires a parameter", argName);
			Environment.Exit (1);
		}

		void OptionParameterError (string argName, string message)
		{
			Console.Error.WriteLine ("Parameter value is invalid for argument '{0}'.",
						 argName);
			Console.Error.WriteLine (message);
			Environment.Exit (1);
		}

		void ShowUsage (string[] msg, bool exit)
		{
			foreach (string line in msg)
				Console.WriteLine (line);
			if (exit)
				Environment.Exit (1);
		}
		
		public void Usage ()
		{
			ShowUsage (usage, true);
		}

		public void UsageCommands ()
		{
			ShowUsage (usageCommands, false);
		}
		
		void ShowVersion ()
		{
			Assembly asm = Assembly.GetExecutingAssembly () ?? Assembly.GetCallingAssembly ();
			object[] attrs = asm != null ? asm.GetCustomAttributes (false) : null;
			string product = "mconfig", version = "0.0.0.0", copyright = "", description = "";

			if (asm != null) {
				Version v = asm.GetName ().Version;
				if (v != null)
					version = v.ToString ();
			}
			
			if (attrs != null) {				
				foreach (object o in attrs) {
					if (o is AssemblyProductAttribute)
						product = ((AssemblyProductAttribute)o).Product;
					else if (o is AssemblyCopyrightAttribute)
						copyright = ((AssemblyCopyrightAttribute)o).Copyright;
					else if (o is AssemblyDescriptionAttribute)
						description = ((AssemblyDescriptionAttribute)o).Description;
				}
			} else
				Console.WriteLine ("Missing version information");

			Console.WriteLine ("{0} - {1} {2}", product, description, version);
			Console.WriteLine (copyright);
			
			Environment.Exit (1);
		}
	}
	
	class MConfig
	{
		static string[] configPaths = {
			Path.GetFullPath (Path.Combine (Environment.CommandLine, "..", "..", "..","..", "etc", "mono", "mconfig", "config.xml")),
			Path.Combine (ConfigPath, "config.xml"),
			Path.Combine (".", "mconfig.xml"),
			null
		};

		static CommandHandler[] commands = {
			new CommandHandler (new string[] {"addfeature", "af"}, HandleAddFeature),
			new CommandHandler (new string[] {"defconfig", "dc"}, HandleDefaultConfig)
		};
		
		static string ConfigPath {
			get {
				string configPath = Environment.GetEnvironmentVariable ("XDG_CONFIG_HOME");
				if (String.IsNullOrEmpty (configPath))
					configPath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".config");
				return Path.Combine (configPath, "mconfig");
			}
		}

		static HandleCommand FindCommandHandler (string command)
		{
			foreach (CommandHandler ch in commands) {
				foreach (string name in ch.Names)
					if (name == command)
						return ch.Handler;
			}

			return null;
		}

		static void DisplayList (string banner, string[] list)
		{
			Console.WriteLine (banner);
			if (list == null || list.Length == 0) {
				Console.WriteLine ("No data found");
				return;
			}

			foreach (string item in list)
				Console.WriteLine ("  {0}", item);
		}

		static void PrintException (Exception ex, string format, params object[] parms)
		{
			if (ex == null)
				return;
			Console.Error.WriteLine (format, parms);
			Console.Error.WriteLine ("  {0}", ex.Message);
			if (ex.InnerException != null)
				Console.Error.WriteLine ("    {0}", ex.InnerException.Message);
		}
		
		static int Main (string[] args)
		{
			MConfigOptions options = new MConfigOptions ();
			options.Parse (args);
			
			if (!String.IsNullOrEmpty (options.ConfigFile))
				configPaths [3] = options.ConfigFile;
			
			Configuration config = new Configuration ();
			try {
				config.Load (configPaths);
			} catch (Exception ex) {
				PrintException (ex, "Failed to load configuration files:");
				return 1;
			}
			
			string[] commandArguments = options.PlainArguments;
			if (commandArguments == null || commandArguments.Length == 0) {
				options.UsageCommands ();
				DisplayList ("Default config files:", config.DefaultConfigFiles);
				Console.WriteLine ();
				DisplayList ("Available features:", config.Features);
				return 1;
			}
			
			HandleCommand commandHandler = FindCommandHandler (commandArguments [0]);
			if (commandHandler == null) {
				Console.Error.WriteLine ("Unknown command '{0}'", commandArguments [0]);
				return 1;
			}

			IDefaultConfigFileContainer[] containers = config.GetHandlersForInterface <IDefaultConfigFileContainer> ();
			if (containers != null && containers.Length > 0)
				foreach (IDefaultConfigFileContainer c in containers)
					c.OverwriteFile += new OverwriteFileEventHandler (OnOverwriteFile);
			
			return commandHandler (options, config);
		}

		static void OnOverwriteFile (object sender, OverwriteFileEventArgs e)
		{
			Console.Write ("Do you want to overwrite existing file '{0}'? [{1}] ",
				       e.Name, e.Overwrite ? "Y/n" : "y/N");
			ConsoleKeyInfo cki = Console.ReadKey (false);
			switch (cki.Key) {
				case ConsoleKey.N:
					e.Overwrite = false;
					break;

				case ConsoleKey.Y:
					e.Overwrite = true;
					break;
			}
			Console.WriteLine ();
		}

		static int HandleAddFeature (MConfigOptions options, Configuration config)
		{
			string[] commandArguments = options.PlainArguments;
			if (commandArguments.Length < 2) {
				Console.Error.WriteLine ("Command requires at least one argument.");
				return 1;
			}
			
			FeatureTarget target = options.Target;
			string featureName = commandArguments [1], configPath;
			if (commandArguments.Length > 2)
				configPath = commandArguments [2];
			else {
				switch (target) {
					case FeatureTarget.Any:
						Console.Error.WriteLine ("No default config file for target 'Any'");
						return 1;
						
					case FeatureTarget.Web:
						configPath = "Web.config";
						break;
						
					case FeatureTarget.Application:
						configPath = "application.exe.config";
						break;

					default:
						Console.Error.WriteLine ("Unknown target '{0}'", target);
						return 1;
				}
			}
			
			try {
				config.AddFeature (configPath, target, featureName);
			} catch (Exception ex) {
				PrintException (ex, "Failed to add feature '{0}' to config file '{1}'.",
						featureName, configPath);
				return 1;
			}
			
			return 0;
		}

		static int HandleDefaultConfig (MConfigOptions options, Configuration config)
		{
			FeatureTarget target = options.Target;
			string[] commandArguments = options.PlainArguments;
			string configName, targetPath;

			if (commandArguments.Length < 2) {
				switch (target) {
					case FeatureTarget.Any:
						Console.Error.WriteLine ("No default config file for target 'Any'");
						return 1;
						
					case FeatureTarget.Web:
						configName = "Web.config";
						break;
						
					case FeatureTarget.Application:
						configName = "application.exe.config";
						break;

					default:
						Console.Error.WriteLine ("Unknown target '{0}'", target);
						return 1;
				}
			} else
				configName = commandArguments [1];

			if (commandArguments.Length < 3)
				targetPath = ".";
			else
				targetPath = commandArguments [2];

			try {
				config.WriteDefaultConfigFile (configName, targetPath, target);
			} catch (Exception ex) {
				PrintException (ex, "Failed to write default config file '{0}':",
						configName);
				return 1;
			}			

			return 0;
		}
	}
}
