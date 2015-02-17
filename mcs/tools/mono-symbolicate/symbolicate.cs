using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Symbolicate
{
	public class Program
	{
		static Regex regex = new Regex (@"\w*at (?<MethodName>.+) \((?<MethodParams>.*)\) *(\[0x(?<IL>.+)\]|<0x.* \+ 0x(?<NativeOffset>.+)>) in <filename unknown>:0");

		public static int Main (String[] args)
		{
			if (args.Length < 2) {
				Console.Error.WriteLine ("Usage: symbolicate <assembly path> <input file> [lookup directories]");
				return 1;
			}

			var assemblyPath = args [0];
			var inputFile = args [1];

			var locProvider = new LocationProvider ();

			for (var i = 2; i < args.Length; i++)
				locProvider.AddDirectory (args [i]);

			locProvider.AddAssembly (assemblyPath);

			using (StreamReader r = new StreamReader (inputFile)) {
			    for (var line = r.ReadLine (); line != null; line = r.ReadLine ()) {
					line = SymbolicateLine (line, locProvider);
					Console.WriteLine (line);
			    }
			}

			return 0;
		}

		static string SymbolicateLine (string line, LocationProvider locProvider)
		{
			var match = regex.Match (line);
			if (!match.Success)
				return line;

			var methodName = match.Groups ["MethodName"].Value;
			var methodParams = ParseParametersTypes (match.Groups ["MethodParams"].Value);

			var isOffsetIL = !string.IsNullOrEmpty (match.Groups ["IL"].Value);
			var offsetVarName = (isOffsetIL)? "IL" : "NativeOffset";
			var offset = int.Parse (match.Groups [offsetVarName].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			Location location;
			if (!locProvider.TryGetLocation (methodName, methodParams, offset, isOffsetIL, out location))
				return line;

			return line.Replace ("<filename unknown>:0", string.Format ("{0}:{1}", location.FileName, location.Line));
		}

		static string[] ParseParametersTypes (string parameters)
		{
			if (string.IsNullOrEmpty (parameters))
				return new string [0];

			var paramsArray = parameters.Split (',');
			var paramsTypes = new string [paramsArray.Length];
			for (var i = 0; i < paramsArray.Length; i++)
				paramsTypes [i] = paramsArray [i].Trim ().Split (new char[]{' '}, 2)[0];

			return paramsTypes;
		}
	}
}