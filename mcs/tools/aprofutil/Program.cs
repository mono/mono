using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Mono.Options;
using System.Linq;
using Mono.Profiler.Aot;

using static System.Console;

namespace aotprofiletool {
	class MainClass {
		static readonly string Name = "aotprofile-tool";

		static bool AdbForward;
		static bool Methods;
		static bool Modules;
		static bool Summary;
		static bool Types;
		static bool Verbose;

		static Regex FilterMethod;
		static Regex SkipMethod;
		static Regex FilterModule;
		static Regex SkipModule;
		static Regex FilterType;
		static Regex SkipType;

		static int SkipCount = 0;
		static int TakeCount = int.MaxValue;

		static string Output;

		static int Port = -1;

		static string ProcessArguments (string [] args)
		{
			var help = false;
			var options = new OptionSet {
				$"Usage: {Name}.exe OPTIONS* <aotprofile-file>",
				"",
				"Processes AOTPROFILE files created by Mono's AOT Profiler",
				"",
				"Copyright 2019 Microsoft Corporation",
				"",
				"Options:",
				{ "h|help|?",
					"Show this message and exit",
				  v => help = v != null },
				{ "a|all",
					"Show modules, types and methods in the profile",
				  v => Modules = Types = Methods = true },
				{ "d|modules",
					"Show modules in the profile",
				  v => Modules = true },
				{ "f|adb-forward",
					"Set adb socket forwarding for Android",
				  v => AdbForward = true },
				{ "filter-method=",
					"Include by method with regex {VALUE}",
				  v => FilterMethod = new Regex (v) },
				{ "skip-method=",
					"Exclude by method with regex {VALUE}",
				  v => SkipMethod = new Regex (v) },
				{ "filter-module=",
					"Include by module with regex {VALUE}",
				  v => FilterModule = new Regex (v) },
				{ "skip-module=",
					"Exclude by module with regex {VALUE}",
				  v => SkipModule = new Regex (v) },
				{ "filter-type=",
					"Include by type with regex {VALUE}",
				  v => FilterType = new Regex (v) },
				{ "skip-type=",
					"Exclude by type with regex {VALUE}",
				  v => SkipType = new Regex (v) },
				{ "take-count=",
					"Take {VALUE} methods that match",
				  v => TakeCount = int.Parse (v) },
				{ "skip-count=",
					"Skip the first {VALUE} matching methods",
				  v => SkipCount = int.Parse (v) },
				{ "m|methods",
					"Show methods in the profile",
				  v => Methods = true },
				{ "o|output=",
					"Write profile to {OUTPUT} file",
				  v => Output = v },
				{ "p|port=",
					"Read profile from aot profiler using local connection on {PORT}",
				  v => int.TryParse (v, out Port) },
				{ "s|summary",
					"Show summary of the profile",
				  v => Summary = true },
				{ "t|types",
					"Show types in the profile",
				  v => Types = true },
				{ "v|verbose",
					"Output information about progress during the run of the tool",
				  v => Verbose = true },
				"",
				"If no other option than -v is used then --all is used by default"
			};

			var remaining = options.Parse (args);

			if (help || args.Length < 1) {
				options.WriteOptionDescriptions (Out);

				Environment.Exit (0);
			}

			if (remaining.Count != 1 && Port < 0) {
				Error ("Please specify one <aotprofile-file> to process or network PORT with -p.");
				Environment.Exit (2);
			}

			return remaining.Count > 0 ? remaining [0] : null;
		}

		static ProfileData ReadProfileFromPort (ProfileReader reader)
		{
			ProfileData pd;

			if (AdbForward) {
				var cmdArgs = $"forward tcp:{Port} tcp:{Port}";
				if (Verbose)
					ColorWriteLine ($"Calling 'adb {cmdArgs}'...", ConsoleColor.Yellow);

				System.Diagnostics.Process.Start ("adb", cmdArgs);
			}

			using (var client = new TcpClient ("127.0.0.1", Port)) {
				using (var stream = client.GetStream ()) {
					var msgData = System.Text.Encoding.ASCII.GetBytes ("save\n");

					stream.Write (msgData, 0, msgData.Length);

					if (Verbose)
						ColorWriteLine ($"Reading from '127.0.0.1:{Port}'...", ConsoleColor.Yellow);

					using (var memoryStream = new MemoryStream (128 * 1024)) {
						var data = new byte [4 * 1024];
						int len;

						while ((len = stream.Read (data, 0, data.Length)) > 0) {
							memoryStream.Write (data, 0, len);

							if (Verbose)
								ColorWrite ($"Read {len} bytes...\r", ConsoleColor.Yellow);
						}

						if (Verbose)
							ColorWriteLine ($"Read total {memoryStream.Length} bytes...", ConsoleColor.Yellow);

						memoryStream.Seek (0, SeekOrigin.Begin);

						pd = reader.ReadAllData (memoryStream);
					}
				}
			}

			return pd;
		}

		public static void Main (string [] args)
		{
			var path = ProcessArguments (args);

			if (args.Length == 1) {
				Modules = Types = Methods = true;
			}

			var reader = new ProfileReader ();
			ProfileData pd = null;

			if (path == null) {
				if (Port < 0) {
					Error ($"You should specify path or -p PORT to read the profile.");
					Environment.Exit (4);
				} else {
					try {
						pd = ReadProfileFromPort (reader);
					} catch (Exception e) {
						Error ($"Unable to read profile through local port: {Port}.\n{e}");
						Environment.Exit (5);
					}
				}
			} else if (!File.Exists (path)) {
				Error ($"'{path}' doesn't exist.");
				Environment.Exit (3);
			} else {
				using (var stream = new FileStream (path, FileMode.Open)) {
					if (Verbose)
						ColorWriteLine ($"Reading '{path}'...", ConsoleColor.Yellow);

					pd = reader.ReadAllData (stream);
				}
			}

			List<MethodRecord> methods = new List<MethodRecord> (pd.Methods);
			ICollection<TypeRecord> types = new List<TypeRecord> (pd.Types);
			ICollection<ModuleRecord> modules = new List<ModuleRecord> (pd.Modules);

			if (FilterMethod != null || FilterType != null || FilterModule != null) {
				types = new HashSet<TypeRecord> ();
				modules = new HashSet<ModuleRecord> ();

				methods = pd.Methods.Where (method => {
					var type = method.Type;
					var module = type.Module;

					if (FilterModule != null) {
						var match = FilterModule.Match (module.ToString ());

						if (!match.Success)
							return false;
					}

					if (SkipModule != null) {
						var skip = SkipModule.Match (module.ToString ());

						if (skip.Success)
							return false;
					}

					if (FilterType != null) {
						var match = FilterType.Match (method.Type.ToString ());

						if (!match.Success)
							return false;
					}

					if (SkipType != null) {
						var skip = SkipType.Match (method.Type.ToString ());

						if (skip.Success)
							return false;
					}

					if (FilterMethod != null) {
						var match = FilterMethod.Match (method.ToString ());

						if (!match.Success)
							return false;
					}

					if (SkipMethod != null)	{
						var skip = SkipMethod.Match (method.ToString ());

						if (skip.Success)
							return false;
					}

					return true;
				}).Skip (SkipCount).Take (TakeCount).ToList ();

				foreach (var method in methods) {
					var type = method.Type;
					var module = type.Module;

					types.Add (type);
					modules.Add (module);
				}
			}

			if (FilterMethod == null && FilterType != null) {
				foreach (var type in pd.Types) {
					if (types.Contains (type))
						continue;

					var match = FilterType.Match (type.ToString ());

					if (!match.Success)
						continue;

					types.Add (type);
				}
			}

			if (Modules) {
				ColorWriteLine ($"Modules:", ConsoleColor.Green);

				foreach (var module in modules)
					WriteLine ($"\t{module.Mvid} {module.ToString ()}");
			}

			if (Types) {
				ColorWriteLine ($"Types:", ConsoleColor.Green);

				foreach (var type in types)
					WriteLine ($"\t{type}");
			}

			if (Methods) {
				ColorWriteLine ($"Methods:", ConsoleColor.Green);

				foreach (var method in methods)
					WriteLine ($"\t{method}");
			}

			if (Summary) {
				ColorWriteLine ($"Summary:", ConsoleColor.Green);
				WriteLine ($"\tModules: {modules.Count.ToString ("N0"),10}{(modules.Count != pd.Modules.Length ? $"  (of {pd.Modules.Length})" : "" )}");
				WriteLine ($"\tTypes:   {types.Count.ToString ("N0"),10}{(types.Count != pd.Types.Length ? $"  (of {pd.Types.Length})" : "")}");
				WriteLine ($"\tMethods: {methods.Count.ToString ("N0"),10}{(methods.Count != pd.Methods.Length ? $"  (of {pd.Methods.Length})" : "")}");
			}

			if (!string.IsNullOrEmpty (Output)) {
				if (Verbose)
					ColorWriteLine ($"Going to write the profile to '{Output}'", ConsoleColor.Yellow);
				var modulesArray = new ModuleRecord [modules.Count];
				modules.CopyTo (modulesArray, 0);
				var typesArray = new TypeRecord [types.Count];
				types.CopyTo (typesArray, 0);
				var updatedPD = new ProfileData (modulesArray, typesArray, methods.ToArray ());

				using (var stream = new FileStream (Output, FileMode.Create)) {
					var writer = new ProfileWriter ();
					writer.WriteAllData (stream, updatedPD);
				}
			}
		}

		static void ColorMessage (string message, ConsoleColor color, TextWriter writer, bool writeLine = true)
		{
			ForegroundColor = color;

			if (writeLine)
				writer.WriteLine (message);
			else
				writer.Write (message);

			ResetColor ();
		}

		public static void ColorWriteLine (string message, ConsoleColor color) => ColorMessage (message, color, Out);

		public static void ColorWrite (string message, ConsoleColor color) => ColorMessage (message, color, Out, false);

		public static void Error (string message) => ColorMessage ($"Error: {Name}: {message}", ConsoleColor.Red, Console.Error);

		public static void Warning (string message) => ColorMessage ($"Warning: {Name}: {message}", ConsoleColor.Yellow, Console.Error);
	}
}
