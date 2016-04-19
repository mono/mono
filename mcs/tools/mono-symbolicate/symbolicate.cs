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

			string typeFullName, methodSignature;
			var methodStr = match.Groups ["Method"].Value.Trim ();
			if (!ExtractSignatures (methodStr, out typeFullName, out methodSignature))
				return line;

			var isOffsetIL = !string.IsNullOrEmpty (match.Groups ["IL"].Value);
			var offsetVarName = (isOffsetIL)? "IL" : "NativeOffset";
			var offset = int.Parse (match.Groups [offsetVarName].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			uint methodIndex = 0xffffff;
			if (!string.IsNullOrEmpty (match.Groups ["MethodIndex"].Value))
				methodIndex = uint.Parse (match.Groups ["MethodIndex"].Value, CultureInfo.InvariantCulture);

			var loc = locProvider.TryGetLocation (typeFullName, methodSignature, offset, isOffsetIL, methodIndex);
			if (loc == null)
				return line;

			return line.Replace ("<filename unknown>:0", string.Format ("{0}:{1}", loc.Document.Url, loc.StartLine));
		}

		static bool ExtractSignatures (string str, out string typeFullName, out string methodSignature)
		{
			var methodNameEnd = str.IndexOf ('(');
			if (methodNameEnd == -1) {
				typeFullName = methodSignature = null;
				return false;
			}

			var typeNameEnd = str.LastIndexOf ('.', methodNameEnd);
			if (typeNameEnd == -1) {
				typeFullName = methodSignature = null;
				return false;
			}

			// Adjustment for Type..ctor ()
			if (typeNameEnd > 0 && str [typeNameEnd - 1] == '.') {
				--typeNameEnd;
			}

			typeFullName = str.Substring (0, typeNameEnd);
			// Remove generic parameters
			typeFullName = Regex.Replace (typeFullName, @"\[[^\[\]]*\]", "");

			methodSignature = str.Substring (typeNameEnd + 1);
			return true;
		}
	}
}