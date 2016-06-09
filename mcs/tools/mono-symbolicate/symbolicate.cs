using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Mono
{
	public class Symbolicate
	{
		public static int Main (String[] args)
		{
			if (args.Length < 2) {
				Console.Error.WriteLine ("Usage: symbolicate <msym dir> <input file>");
				return 1;
			}

			var msymDir = args [0];
			var inputFile = args [1];

			var symbolManager = new SymbolManager (msymDir);

			using (StreamReader r = new StreamReader (inputFile)) {
				var sb = Process (r, symbolManager);
				Console.WriteLine (sb.ToString ());
			}

			return 0;
		}

		public static StringBuilder Process (StreamReader reader, SymbolManager symbolManager)
		{
			List<StackFrameData> stackFrames = new List<StackFrameData>();
			List<StackTraceMetadata> metadata = new List<StackTraceMetadata>();
			StringBuilder sb = new StringBuilder ();
			bool linesEnded = false;

			for (var line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
				 {
					StackFrameData sfData;
					if (!linesEnded && StackFrameData.TryParse (line, out sfData)) {
						stackFrames.Add (sfData);
						continue;
					}
					linesEnded = true;
				} 

				if (stackFrames.Count > 0) {
					{
						StackTraceMetadata stMetadata;
						if (StackTraceMetadata.TryParse (line, out stMetadata)) {
							metadata.Add (stMetadata);
							continue;
						}
					}

					string aotid = null;
					var aotidMetadata = metadata.FirstOrDefault ( m => m.Id == "AOTID" );
					if (aotidMetadata != null)
						aotid = aotidMetadata.Value;

					var linesMvid = ProcessLinesMVID (metadata);
					var lineNumber = 0;
					foreach (var sfData in stackFrames) {
						string mvid = null;
						if (linesMvid.ContainsKey (lineNumber))
							mvid = linesMvid [lineNumber++];

						symbolManager.TryResolveLocation (sfData, mvid, aotid);

						sb.AppendLine (sfData.ToString ());
					}

					foreach (var m in metadata)
						sb.AppendLine (m.Line);
					
					// Clear lists for next stack trace
					stackFrames.Clear ();
					metadata.Clear ();
				}

				linesEnded = false;

				// Append last line
				sb.AppendLine (line);
			}

			return sb;
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
