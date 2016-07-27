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
				var sb = Process (r, symbolManager);
				Console.Write (sb.ToString ());
			}
		}

		private static void StoreSymbolsAction (List<string> args)
		{
			var msymDir = args[0];
			var lookupDirs = args.Skip (1).ToArray ();

			var symbolManager = new SymbolManager (msymDir, logger);

			symbolManager.StoreSymbols (lookupDirs);
		}

		public static StringBuilder Process (StreamReader reader, SymbolManager symbolManager)
		{
			List<StackFrameData> stackFrames = new List<StackFrameData>();
			List<StackTraceMetadata> metadata = new List<StackTraceMetadata>();
			StringBuilder sb = new StringBuilder ();
			bool linesEnded = false;

			for (var line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
				StackFrameData sfData;
				if (!linesEnded && StackFrameData.TryParse (line, out sfData)) {
					stackFrames.Add (sfData);
					continue;
				}

				if (stackFrames.Count > 0) {
					linesEnded = true;

					StackTraceMetadata stMetadata;
					if (StackTraceMetadata.TryParse (line, out stMetadata)) {
						metadata.Add (stMetadata);
						continue;
					}

					DumpStackTrace (symbolManager, sb, stackFrames, metadata);
		
					// Clear lists for next stack trace
					stackFrames.Clear ();
					metadata.Clear ();
				}

				linesEnded = false;

				// Append last line
				sb.AppendLine (line);
			}

			if (stackFrames.Count > 0)
				DumpStackTrace (symbolManager, sb, stackFrames, metadata);

			return sb;
		}

		private static void DumpStackTrace (SymbolManager symbolManager, StringBuilder sb, List<StackFrameData> stackFrames, List<StackTraceMetadata> metadata)
		{
			string aotid = null;
			var aotidMetadata = metadata.FirstOrDefault ( m => m.Id == "AOTID" );
			if (aotidMetadata != null)
				aotid = aotidMetadata.Value;

			var linesMvid = ProcessLinesMVID (metadata);
			var lineNumber = -1;
			foreach (var sfData in stackFrames) {
				string mvid = null;
				lineNumber++;
				if (!sfData.IsValid)
					continue;
				if (linesMvid.ContainsKey (lineNumber))
					mvid = linesMvid [lineNumber];

				symbolManager.TryResolveLocation (sfData, mvid, aotid);

				sb.AppendLine (sfData.ToString ());
			}

			foreach (var m in metadata)
				sb.AppendLine (m.Line);
		}

		private static Dictionary<int, string> ProcessLinesMVID (List<StackTraceMetadata> metadata)
		{
			var linesMvid = new Dictionary<int, string> ();
			var mvidData = metadata.Where ( m => m.Id == "MVID" ).Select ( m => m.Value );
			foreach (var m in mvidData) {
				var s1 = m.Split (new char[] {' '}, 2);
				var mvid = s1 [0];
				var lines = s1 [1].Split (',');
				foreach (var line in lines)
					linesMvid.Add (int.Parse (line), mvid);
			}

			return linesMvid;
		}
	}
}
