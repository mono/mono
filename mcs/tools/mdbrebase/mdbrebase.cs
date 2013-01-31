using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Mdb;
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
		return InputPattern != null && OutputPattern != null && !OutputPattern.StartsWith (InputPattern);
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

	public void RewriteMdbFile (string inputAssembly)
	{
		var assemblyName = new FileInfo (inputAssembly).Name;
	
		var readParams = new ReaderParameters () {
			SymbolReaderProvider = new MdbReaderProvider (),
			ReadSymbols = true,
		};

		var assembly = AssemblyDefinition.ReadAssembly (inputAssembly, readParams);

		foreach (var m in assembly.Modules) {
			foreach (var t in m.Types) {
				ProcessType (t);
				foreach (var inner in t.NestedTypes)
					ProcessType (inner);
			}
		}

		var writeParms = new WriterParameters () {
			SymbolWriterProvider = new MdbWriterProvider (),
			WriteSymbols = true,
		};
	
		var tmpdir = Path.GetTempPath ();
		var tmpAsm = tmpdir + assemblyName;
		string finalMdb = inputAssembly + ".mdb";
		if (settings.OutputDirectory != null)
			finalMdb = Path.Combine (settings.OutputDirectory, assemblyName) + ".mdb";

		assembly.Write (tmpAsm, writeParms);	
		Console.WriteLine (tmpAsm);
		File.Delete (tmpAsm);
		File.Delete (finalMdb);
		Console.WriteLine ("Moving {0} to {1}", tmpAsm + ".mdb", finalMdb);
		new FileInfo (tmpAsm + ".mdb").MoveTo (finalMdb);
	}

	void ProcessMethod (MethodDefinition m)
	{
		foreach (var i in m.Body.Instructions) {
			if (i.SequencePoint != null)
				i.SequencePoint.Document.Url = settings.Replace (i.SequencePoint.Document.Url);
		}
	}

	void ProcessType (TypeDefinition t)
	{
		foreach (var m in t.Methods)
			ProcessMethod (m);
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