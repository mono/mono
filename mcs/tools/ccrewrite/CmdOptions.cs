using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace ccrewrite {
	static class CmdOptions {

		public static bool Initialise (string[] args)
		{
			// Default values
			Help = false;
			Debug = true;
			Level = 4;
			WritePdbFile = true;
			Rewrite = true;
			Assembly = null;
			OutputFile = null;
			BreakIntoDebugger = false;
			ThrowOnFailure = false;

			options = new OptionSet {
				{ "help", "Show this help.", v => CmdOptions.Help = true },
				{ "debug", "Use MDB or PDB debug information (default=true).", v => CmdOptions.Debug = v != null },
				{ "level=", "Instrumentation level, 0 - 4 (default=4).", (int var) => CmdOptions.Level = var},
				{ "writePDBFile", "Write MDB or PDB file (default=true).", v => CmdOptions.WritePdbFile = v != null },
				{ "rewrite", "Rewrite the assembly (default=true).", v => CmdOptions.Rewrite = v != null },
				{ "assembly=", "Assembly to rewrite.", v => CmdOptions.Assembly = v },
				{ "breakIntoDebugger|break", "Break into debugger on contract failure.", v => CmdOptions.BreakIntoDebugger = v != null },
				{ "throwOnFailure|throw", "Throw ContractException on contract failure.", v => CmdOptions.ThrowOnFailure = v != null },
				{ "output|out=", "Output filename of rewritten file.", v => CmdOptions.OutputFile = v },
			};

			try {
				options.Parse (args);
			} catch (OptionException e) {
				ShowUsage (e.Message);
				return false;
			}

			if (Help) {
				ShowUsage ();
				return false;
			}

			return true;
		}

		public static bool Help { get; private set; }
		public static bool Debug { get; private set; }
		public static int Level { get; private set; }
		public static bool WritePdbFile { get; private set; }
		public static bool Rewrite { get; private set; }
		public static string Assembly { get; private set; }
		public static bool BreakIntoDebugger { get; private set; }
		public static bool ThrowOnFailure { get; private set; }
		public static string OutputFile { get; private set; }

		private static OptionSet options;

		public static void ShowUsage (string msg = null)
		{
			Console.WriteLine ("ccrewrite");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			options.WriteOptionDescriptions (Console.Out);
			Console.WriteLine ();
			if (msg != null) {
				Console.WriteLine (msg);
				Console.WriteLine ();
			}
		}

	}
}
