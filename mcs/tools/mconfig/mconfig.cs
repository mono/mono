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

using Mono.GetOptions;

[assembly: Mono.About ("Utility to modify .NET config files")]
[assembly: Mono.Author ("Marek Habersack")]
[assembly: Mono.UsageComplement ("command [COMMAND_ARGUMENTS]")]
[assembly: Mono.ReportBugsTo ("mhabersack@novell.com")]

namespace Mono.MonoConfig 
{
	delegate int HandleCommand (MConfigOptions options, Configuration config);
	
	struct CommandHandler {
		public readonly HandleCommand Handler;
		public readonly string[] Names;
		public readonly string Syntax;
		public readonly string Documentation;
			
		public CommandHandler (string[] names, HandleCommand handler, string syntax, string documentation)
		{
			this.Names = names;
			this.Handler = handler;
			this.Syntax = syntax;
			this.Documentation = documentation;
		}
	};
	
	class MConfigOptions : Options
	{
		public delegate void ListDefaultConfigsHandler ();
		public delegate void ListFeaturesHandler ();

		public event ListDefaultConfigsHandler OnListDefaultConfigs;
		public event ListFeaturesHandler OnListFeatures;
		
		[Option ("output version information and exit", 'v', "version")]
		public override WhatToDoNext DoAbout ()
		{
			return base.DoAbout ();
		}

		[Option ("display this help and exit", 'h', "help")]
		public override WhatToDoNext DoHelp ()
		{
			WhatToDoNext ret = base.DoHelp ();
			
			Console.WriteLine ("\nCommands:\n");
			foreach (CommandHandler ch in commands)
				Console.WriteLine ("{{{0}}} {1}\n\t{2}\n",
						   String.Join (" | ", ch.Names),
						   ch.Syntax,
						   ch.Documentation);

			return ret;
		}

		[Option ("read the specified config file in addition to the standard ones.", 'c', "config")]
		public string ConfigFile;

		[Option ("consider only features for the given target (Any, Web, Application). Default: Any", 't', "target")]
		public string Target;

		[Option ("list all default config file names defined in the configuration files", 'C', "list-configs")]
		public bool ListDefaultConfigs;
		
		[Option ("list all features defined in the config files", 'F', "list-features")]
		public bool ListFeatures;
		
		CommandHandler[] commands;
		
		public MConfigOptions (CommandHandler[] commands) : base (null)
		{
			this.commands = commands;
		}

		protected override void InitializeOtherDefaults ()
		{
			ParsingMode = OptionsParsingMode.Both | OptionsParsingMode.GNU_DoubleDash;
			BreakSingleDashManyLettersIntoManyOptions = true;
		}
	}
	
	class MConfig
	{
		static string[] configPaths = {
			Constants.GlobalConfigPath,
			Path.Combine (ConfigPath, "config.xml"),
			Path.Combine (".", "mconfig.xml"),
			null
		};

		static CommandHandler[] commands = {
			new CommandHandler (new string[] {"addfeature", "af"},
					    HandleAddFeature,
					    "FEATURE_NAME [CONFIG_FILE_PATH]",
					    "Adds the named feature to the indicated config file. If CONFIG_FILE_PATH is omitted " +
					    "the name will be chosen based on the selected target (-t). " +
					    "If the file does not exist, it " +
					    "will be created"),
			new CommandHandler (new string[] {"defconfig", "dc"},
					    HandleDefaultConfig,
					    "[[CONFIG_NAME] [TARGET_DIRECTORY]]",
					    "Writes a default config file from definition named by CONFIG_NAME to a target " +
					    "directory given in TARGET_DIRECTORY.")
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
				Console.WriteLine (" {0}", item);
		}
		
		static int Main (string[] args)
		{
			MConfigOptions options = new MConfigOptions (commands);			
			options.ProcessArgs (args);			

			if (!String.IsNullOrEmpty (options.ConfigFile))
				configPaths [3] = options.ConfigFile;
			
			Configuration config = new Configuration ();
			config.Load (configPaths);

			bool doQuit = false;
			if (options.ListDefaultConfigs) {
				DisplayList ("Default config files", config.DefaultConfigFiles);
				doQuit = true;
			}

			if (options.ListFeatures) {
				DisplayList ("Available features", config.Features);
				doQuit = true;
			}

			if (doQuit)
				return 0;
			
			string[] remainingArguments = options.RemainingArguments;
			if (remainingArguments == null || remainingArguments.Length == 0) {
				options.DoHelp ();
				return 1;
			}
			
			HandleCommand commandHandler = FindCommandHandler (remainingArguments [0]);
			if (commandHandler == null) {
				Console.Error.WriteLine ("Unknown command '{0}'", remainingArguments [0]);
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
			string[] remainingArguments = options.RemainingArguments;
			if (remainingArguments.Length < 2) {
				Console.Error.WriteLine ("Command requires at least one argument.");
				return 1;
			}
			
			FeatureTarget target = FeatureTarget.Any;

			if (!String.IsNullOrEmpty (options.Target))
				target = Helpers.ConvertTarget (options.Target);

			string featureName = remainingArguments [1], configPath;
			if (remainingArguments.Length > 2)
				configPath = remainingArguments [2];
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
				Console.Error.WriteLine ("Failed to add feature '{0}' to config file '{1}'.\n{2}",
							 featureName, configPath, ex.Message);
				return 1;
			}
			
			return 0;
		}

		static int HandleDefaultConfig (MConfigOptions options, Configuration config)
		{
			FeatureTarget target = FeatureTarget.Any;

			if (!String.IsNullOrEmpty (options.Target))
				target = Helpers.ConvertTarget (options.Target);

			string[] remainingArguments = options.RemainingArguments;
			string configName, targetPath;

			if (remainingArguments.Length < 2) {
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
				configName = remainingArguments [1];

			if (remainingArguments.Length < 3)
				targetPath = ".";
			else
				targetPath = remainingArguments [2];

			try {
				config.WriteDefaultConfigFile (configName, targetPath, target);
			} catch (Exception ex) {
				Console.Error.WriteLine ("Failed to write default config file '{0}':\n{1}",
							 configName, ex.Message);
				return 1;
			}			

			return 0;
		}
	}
}
