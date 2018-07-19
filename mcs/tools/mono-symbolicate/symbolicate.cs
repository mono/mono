using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Mono.Options;

namespace Mono
{
	public class Symbolicate
	{
		class Command {
			public readonly int MinArgCount;
			public readonly int MaxArgCount;
			public readonly Action<List<string>> Action;

			public Command (Action<List<string>> action, int minArgCount = 0, int maxArgCount = int.MaxValue)
			{
				Action = action;
				MinArgCount = minArgCount;
				MaxArgCount = maxArgCount;
			}
		}

		static Logger logger;

		public static int Main (String[] args)
		{
			var showHelp = false;
			List<string> extra = null;

			Command cmd = null;

			var logLevel = Logger.Level.Warning;

			var options = new OptionSet {
				{ "h|help", "Show this help", v => showHelp = true },
				{ "q", "Quiet, warnings are not displayed", v => logLevel = Logger.Level.Error },
				{ "v", "Verbose, log debug messages", v => logLevel = Logger.Level.Debug },
			};

			try {
				extra = options.Parse (args);
			} catch (OptionException e) {
				Console.WriteLine ("Option error: {0}", e.Message);
				showHelp = true;
			}

			if (extra.Count > 0 && extra[0] == "store-symbols")
				cmd = new Command (StoreSymbolsAction, 2);

			if (cmd != null) {
				extra.RemoveAt (0);
			} else {
				cmd = new Command (SymbolicateAction, 2, 2);
			}

			if (showHelp || extra == null || extra.Count < cmd.MinArgCount || extra.Count > cmd.MaxArgCount) {
				Console.Error.WriteLine ("Usage: symbolicate [options] <msym dir> <input file>");
				Console.Error.WriteLine ("       symbolicate [options] store-symbols <msym dir> [<dir>]+");
				Console.WriteLine ();
				Console.WriteLine ("Available options:");
				options.WriteOptionDescriptions (Console.Out);
				return 1;
			}

			logger = new Logger (logLevel, msg => Console.Error.WriteLine (msg));

			cmd.Action (extra);

			return 0;
		}

		private static void SymbolicateAction (List<string> args)
		{
			var msymDir = args [0];
			var inputFile = args [1];

			var symbolManager = new SymbolManager (msymDir, logger);

			using (StreamReader r = new StreamReader (inputFile)) {
				for (var line = r.ReadLine (); line != null; line = r.ReadLine ()) {
					if (StackFrameData.TryParse (line, out var sfData) && symbolManager.TryResolveLocation (sfData, out var location)) {
						var sign = sfData.Line.Substring (0, sfData.Line.IndexOf (" in <", StringComparison.Ordinal));
						line = $"{sign} in {location.File}:{location.Line}";
					}

					Console.WriteLine (line);
				}
			}
		}

		private static void StoreSymbolsAction (List<string> args)
		{
			var msymDir = args[0];
			var lookupDirs = args.Skip (1).ToArray ();

			var symbolManager = new SymbolManager (msymDir, logger);

			symbolManager.StoreSymbols (lookupDirs);
		}
	}
}
