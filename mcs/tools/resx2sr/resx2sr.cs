//
// resx2sr.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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
using System.IO;
using System.Collections.Generic;
using System.Resources;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Mono.Options;

public class Program
{
	class CmdOptions
	{
		public bool ShowHelp { get; set; }
		public string OutputFile { get; set; }
		public bool ExistingOnly { get; set; }
	}

	internal static List<string> InputFiles;
	internal static Dictionary<string, object> ExistingKeys;

	public static int Main (string[] args)
	{
		var options = new CmdOptions ();

		InputFiles = new List<string> ();
		ExistingKeys = new Dictionary<string, object> ();

		string className = "SR";

		var p = new OptionSet () {
			{ "o|out=", "Specifies output file name",
				v => options.OutputFile = v },
			{ "i|in=", "Specifies input file name",
				v => InputFiles.Add (v) },
			{ "n|name=", "Name for generated class, default is 'SR'",
				v => className = v },
			{ "h|help",  "Display available options",
				v => options.ShowHelp = v != null },
			{ "e|existing", "Only update existing values, do not add keys",
				v => options.ExistingOnly = true
			}
		};

		List<string> extra;
		try {
			extra = p.Parse (args);
		} catch (OptionException e) {
			Console.WriteLine (e.Message);
			Console.WriteLine ("Try 'resx2sr -help' for more information.");
			return 1;
		}

		if (options.ShowHelp) {
			ShowHelp (p);
			return 0;
		}

		if (extra.Count < 1) {
			ShowHelp (p);
			return 2;
		}

		if (!LoadInputFiles ())
			return 4;

		var resxStrings = new List<Tuple<string, string, string>> ();
		if (!LoadStrings (resxStrings, extra))
			return 3;

		GenerateFile (className, resxStrings, options);

		return 0;
	}

	static void ShowHelp (OptionSet p)
	{
		Console.WriteLine ("Usage: resx2sr [options] input-files");
		Console.WriteLine ("Generates C# file with string constants from resource file");
		Console.WriteLine ();
		Console.WriteLine ("Options:");
		p.WriteOptionDescriptions (Console.Out);
	}

	static void GenerateFile (string className, List<Tuple<string, string, string>> txtStrings, CmdOptions options)
	{
//		var outputFile = options.OutputFile ?? "SR.cs";

		using (var str = options.OutputFile == null ? Console.Out : new StreamWriter (options.OutputFile)) {
			str.WriteLine ("//");
			str.WriteLine ("// This file was generated by resx2sr tool");
			str.WriteLine ("//");
			str.WriteLine ();

			int nsIdx = className.LastIndexOf ('.');
			if (nsIdx > 0) {
				str.WriteLine ($"namespace {className.Substring(0,nsIdx)}");
				str.WriteLine ("{");
				className = className.Substring (nsIdx+1);
			}

			str.WriteLine ($"partial class {className}");
			str.WriteLine ("{");

			var dict = new Dictionary<string, string> ();

			foreach (var entry in txtStrings) {

				if (options.ExistingOnly && !ExistingKeys.ContainsKey(entry.Item1)) {
					continue;
				}

				var value = ToCSharpString (entry.Item2);
				string found;
				if (dict.TryGetValue (entry.Item1, out found)) {
					if (found == value)
						continue;

					str.WriteLine ($"\t// Constant value mismatch");
				} else {
					dict.Add (entry.Item1, value);
				}

				str.Write ($"\tpublic const string {entry.Item1} = \"{value}\";");

				if (!string.IsNullOrEmpty (entry.Item3))
					str.Write (" // {entry.Item3}");

				str.WriteLine ();
			}

			foreach (var v in ExistingKeys.Keys) {
				if (!dict.ContainsKey (v)) {
					str.Write ($"\tpublic const string {v} = \"{v}\";");
				}
			}

			str.WriteLine ("}");

			if (nsIdx > 0) {
				str.WriteLine ("}");
			}
		}
	}

	static string ToCSharpString (string str)
	{
		str = str.Replace ("\n", "\\n");

		return str.Replace ("\\", "\\\\").Replace ("\"", "\\\"");
	}

	static bool LoadStrings (List<Tuple<string, string, string>> resourcesStrings, List<string> files)
	{
		var keys = new Dictionary<string, string> ();
		foreach (var fileName in files) {
			if (!File.Exists (fileName)) {
				Console.Error.WriteLine ($"Error reading resource file '{fileName}'");
				return false;
			}

			if (string.Equals (Path.GetExtension (fileName), ".txt", StringComparison.OrdinalIgnoreCase)) {
				resourcesStrings.AddRange (ReadTextResources (fileName));
			} else {
				resourcesStrings.AddRange (ReadResxFile (fileName));
			}
		}

		return true;
	}

	static IEnumerable<Tuple<string,string,string>> ReadResxFile (string fileName)
	{
		var rr = new ResXResourceReader (fileName);
		rr.UseResXDataNodes = true;
		var dict = rr.GetEnumerator ();
		while (dict.MoveNext ()) {
			var node = (ResXDataNode)dict.Value;
			yield return Tuple.Create (node.Name, (string) node.GetValue ((ITypeResolutionService)null), node.Comment);
		}
	}

	static IEnumerable<Tuple<string,string,string>> ReadTextResources (string fileName)
	{
		foreach (var line in File.ReadAllLines (fileName)) {
			if (line.Length == 0 || line[0] == ';') {
				continue;
			}
			var idx = line.IndexOf ('=');
			if (idx < 1) {
				Console.Error.WriteLine ($"Error reading resource file '{fileName}'");
				continue;
			}
			yield return Tuple.Create (line.Substring(0, idx), line.Substring(idx+1), (string)null);
		}
	}

	static bool LoadInputFiles ()
	{
		var reg = new Regex (@"\s*public const string (\w+)\s+=\s+");
		var keys = new Dictionary<string, string> ();
		foreach (var fileName in InputFiles) {
			if (!File.Exists (fileName)) {
				Console.Error.WriteLine ($"Error reading input file '{fileName}'");
				return false;
			}

			using (var reader = new StreamReader (fileName)) {
				string line;
				while ((line = reader.ReadLine ()) != null) {
					var match = reg.Match (line);
					if (!match.Success)
						continue;

					var key = match.Groups[1].Value;
					ExistingKeys[key] = null;
				}
			}
		}

		return true;
	}
}
