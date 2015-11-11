using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Symbolicate
{
	public class Program
	{
		static Regex regex = new Regex (@"\w*at (?<Method>.+) *(\[0x(?<IL>.+)\]|<0x.+ \+ 0x(?<NativeOffset>.+)>( (?<MethodIndex>\d+)|)) in <filename unknown>:0");

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

			string typeFullName;
			var methodStr = match.Groups ["Method"].Value.Trim ();
			if (!TryParseMethodType (methodStr, out typeFullName))
				return line;

			var isOffsetIL = !string.IsNullOrEmpty (match.Groups ["IL"].Value);
			var offsetVarName = (isOffsetIL)? "IL" : "NativeOffset";
			var offset = int.Parse (match.Groups [offsetVarName].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			uint methodIndex = 0xffffff;
			if (!string.IsNullOrEmpty (match.Groups ["MethodIndex"].Value))
				methodIndex = uint.Parse (match.Groups ["MethodIndex"].Value, CultureInfo.InvariantCulture);

			Location location;
			if (!locProvider.TryGetLocation (methodStr, typeFullName, offset, isOffsetIL, methodIndex, out location))
				return line;

			return line.Replace ("<filename unknown>:0", string.Format ("{0}:{1}", location.FileName, location.Line));
		}

		static bool TryParseMethodType (string str, out string typeFullName)
		{
			typeFullName = null;

			var methodNameEnd = str.IndexOf ("(");
			if (methodNameEnd == -1)
				return false;

			// Remove parameters
			str = str.Substring (0, methodNameEnd);

			// Remove generic parameters
			str = Regex.Replace (str, @"\[[^\[\]]*\]", "");

			var typeNameEnd = str.LastIndexOf (".");
			if (methodNameEnd == -1 || typeNameEnd == -1)
				return false;

			// Remove method name
			typeFullName = str.Substring (0, typeNameEnd);

			return true;
		}
	}
}