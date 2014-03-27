using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Mono.CompilerServices.SymbolWriter;
using Mono.Options;

namespace Mono.MdbRebase
{

class Settings
{
	public string OutputDirectory { get; set; }
	public string InputPattern { get; set; }
	public string OutputPattern { get; set; }
	public bool InputPatternIsRegex { get; set; }
	public bool FileNamesOnly { get; set; }
	public bool Verbose { get; set; }

	Regex inputPatternRegex;

	public bool Validate ()
	{
		return InputPattern != null && OutputPattern != null;
	}

	public string Replace (string input)
	{
		if (InputPatternIsRegex) {
			if (inputPatternRegex == null)
				inputPatternRegex = new Regex (InputPattern);
			return inputPatternRegex.Replace (input, OutputPattern);
		} else {
			if (input.StartsWith (InputPattern))
				return OutputPattern + input.Substring (InputPattern.Length);
		}

		return input;
	}
}

class MdbRebase
{
	Settings settings;

	public MdbRebase (Settings settings)
	{
		this.settings = settings;
	}

	public void RewriteMdbFile (string inputFile)
	{
		Console.WriteLine ("Processing {0}", inputFile);
		var input = MonoSymbolFile.ReadSymbolFile (inputFile);

		var output = new MonoSymbolFile ();

		foreach (var s in input.Sources) {
			var newFileName = settings.FileNamesOnly
				? Path.Combine (Path.GetDirectoryName (s.FileName), settings.Replace (Path.GetFileName (s.FileName)))
				: settings.Replace (s.FileName);

			if (settings.Verbose)
				Console.WriteLine ("{0} -> {1}", s.FileName, newFileName);

			s.FileName = newFileName;
			output.AddSource (s);
		}

		foreach (var cu in input.CompileUnits) {
			cu.ReadAll ();
			output.AddCompileUnit (cu);
		}
	
		foreach (var m in input.Methods) {
			m.ReadAll ();
			output.AddMethod (m);
		}


		var mdbName = new FileInfo (inputFile).Name;
		var tmpMdb = Path.Combine (Path.GetTempPath (), mdbName);
		var finalMdb = inputFile;
		if (settings.OutputDirectory != null)
			finalMdb = Path.Combine (settings.OutputDirectory, mdbName);

		using (var stream = new FileStream (tmpMdb, FileMode.Create)) {
			output.CreateSymbolFile (input.Guid, stream);
		}
		input.Dispose ();

		File.Delete (finalMdb);
		File.Move (tmpMdb, finalMdb);
	}
}

class Driver {
	static void Usage (OptionSet options)
	{
		Console.WriteLine (@"Usage: mdbrebase [options] <ASSEMBLY_TO_FIX>");
		if (options != null) {
			Console.WriteLine ();
			Console.WriteLine ("Available options:");
			options.WriteOptionDescriptions (Console.Out);
		}
		Console.WriteLine ();
		
		Environment.Exit (-1);
	}

	static int Main (string[] args) {
		var s = new Settings ();
		bool showHelp = false;

		var p = new OptionSet () {
			{ "d=|output=",  "Output directory to the mdb file, replace existing one if ommited", v => s.OutputDirectory = v },
			{ "v|verbose", "Be verbose with output (show individual path rewrites)", v => s.Verbose = true },
			{ "f|filenames", "Only operate on file names, not full absolute paths", v => s.FileNamesOnly = true },
			{ "r|regex", "Input pattern is a regular expression", v => s.InputPatternIsRegex = true },
			{ "i=|input-pattern=", "Input pattern to replace (must not be a prefix to output-pattern)(required)", v => s.InputPattern = v },
			{ "o=|output-pattern=", "Output pattern to replace (required)", v => s.OutputPattern = v },
			{ "h|?|help", v => showHelp = true },
		};

		List <string> extra = p.Parse (args);

		if (showHelp || extra.Count < 1 || !s.Validate ())
			Usage (p);

		var m = new MdbRebase (s);
		foreach (var a in extra)
			m.RewriteMdbFile (a);
		return 0;

	}
}
}
