#define CONTRACTS_FULL

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.IO;
using Mono.CodeContracts.Rewrite;
using Mono.Options;

namespace ccrewrite {

	class Program {

		static void Main (string [] args)
		{
			RewriterOptions options = new RewriterOptions ();

			bool showOptions = false;
			string showMsg = null;

			var optionSet = new OptionSet {
				{ "help", "Show this help.", v => showOptions = v != null },
				{ "debug", "Use MDB or PDB debug information (default=true).", v => options.Debug = v != null },
				{ "level=", "Instrumentation level, 0 - 4 (default=4).", (int var) => options.Level = var},
				{ "writePDBFile", "Write MDB or PDB file (default=true).", v => options.WritePdbFile = v != null },
				{ "rewrite", "Rewrite the assembly (default=true).", v => options.Rewrite = v != null },
				{ "assembly=", "Assembly to rewrite.", v => options.Assembly = v },
				{ "breakIntoDebugger|break", "Break into debugger on contract failure.", v => options.BreakIntoDebugger = v != null },
				{ "throwOnFailure|throw", "Throw ContractException on contract failure.", v => options.ThrowOnFailure = v != null },
				{ "output|out=", "Output filename of rewritten file.", v => options.OutputFile = v },
			};

			try {
				optionSet.Parse (args);
			} catch (OptionException e) {
				showOptions = true;
				showMsg = e.Message;
			}

			if (showOptions) {
				Console.WriteLine ("ccrewrite");
				Console.WriteLine ();
				Console.WriteLine ("Options:");
				optionSet.WriteOptionDescriptions (Console.Out);
				Console.WriteLine ();
				if (showMsg != null) {
					Console.WriteLine (showMsg);
					Console.WriteLine ();
				}
				return;
			}

			var results = Rewriter.Rewrite (options);

			if (results.AnyErrors) {
				foreach (var error in results.Errors) {
					Console.WriteLine ("Error: " + error);
				}
			}
			if (results.AnyWarnings) {
				foreach (var warning in results.Warnings) {
					Console.WriteLine ("Warning: " + warning);
				}
			}

			Console.WriteLine ();
			Console.WriteLine ("*** done ***");
			//Console.ReadKey ();
		}

	}
}
