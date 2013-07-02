using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
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

	public bool Validate ()
	{
		return InputPattern != null && OutputPattern != null;
	}

	public string Replace (string input)
	{
		if (input.StartsWith (InputPattern))
			return input.Replace (InputPattern, OutputPattern);
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
			s.FileName = settings.Replace (s.FileName);
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
		new FileInfo (tmpMdb).MoveTo (finalMdb);
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