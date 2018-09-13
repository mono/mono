//
// pinvoke-checker.cs
//
// Authors:
//	Martin Baulig <mabaul@microsoft.com>
//
// Copyright (C) 2018 Xamarin Inc (http://www.xamarin.com)
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Program
{
	internal class DynamicLibrary
	{
		public string Name { get; private set; }
		public string Path { get; private set; }

		public DynamicLibrary (string name)
		{
			var pos = name.IndexOf (':');
			if (pos < 0) {
				Name = Path = name;
			} else {
				Name = name.Substring (0, pos);
				Path = name.Substring (pos + 1);
			}
		}

		public DynamicLibrary (string name, string path)
		{
			Name = name;
			Path = path;
		}
	}

	internal class CmdOptions
	{
		public bool ShowHelp { get; set; }
		public bool Verbose { get; set; }

		public List<DynamicLibrary> DynamicLibraries { get; }
		public List<string> IgnoreList { get; }

		public string MonoRuntime { get; set; }

		public CmdOptions ()
		{
			DynamicLibraries = new List<DynamicLibrary> ();
			IgnoreList = new List<string> ();
		}
	}

	internal CmdOptions Options {
		get;
	}

	internal string FileName {
		get;
	}

	internal Dictionary<string, List<string>> DynamicSymbols {
		get;
	}

	Program (string[] args)
	{
		Options = new CmdOptions ();
		DynamicSymbols = new Dictionary<string, List<string>> ();

		var p = new OptionSet () {
			{ "d|dylib=", "Dynamic library to check for symbols",
				v => Options.DynamicLibraries.Add (new DynamicLibrary (v)) },
			{ "i|ignore=", "Ignore this DLL",
				v => Options.IgnoreList.Add (v) },
			{ "r|runtime=", "Add __Internal library",
				v => Options.MonoRuntime = v },
			{ "h|help",  "Display available options",
				v => Options.ShowHelp = v != null },
			{ "v|verbose",  "Use verbose output",
				v => Options.Verbose = v != null },
		};

		List<string> extra;
		try {
			extra = p.Parse (args);
		}
		catch (OptionException e) {
			Console.WriteLine (e.Message);
			Console.WriteLine ("Try 'pinvoke-checker -help' for more information.");
			Environment.Exit (1);
			return;
		}

		if (Options.ShowHelp) {
			ShowHelp (p);
			Environment.Exit (0);
		}

		if (extra.Count != 1) {
			ShowHelp (p);
			Environment.Exit (2);
		}

		FileName = extra[0];
	}

	public static int Main (string[] args)
	{
		var program = new Program (args);
		return program.Run ();
	}

	int Run ()
	{
		if (!LoadDynamicLibraries ())
			return 3;

		CheckAssembly ();

		return 0;
	}

	static void ShowHelp (OptionSet p)
	{
		Console.WriteLine ("Usage: pinvoke-checker [options] assembly");
		Console.WriteLine ("Checks whether we don't have any missing P/Invokes.");
		Console.WriteLine ();
		Console.WriteLine ("Options:");
		p.WriteOptionDescriptions (Console.Out);
	}

	bool LoadDynamicLibraries ()
	{
		foreach (var dynamicLibrary in Options.DynamicLibraries) {
			if (!File.Exists (dynamicLibrary.Path)) {
				Console.Error.WriteLine ($"Error reading shared library '{dynamicLibrary.Path}'");
				return false;
			}

			if (!LoadDynamicLibrary (dynamicLibrary))
				return false;
		}

		return true;
	}

	bool LoadDynamicLibrary (DynamicLibrary library)
	{
		if (Platform.IsMacOS)
			return LoadDynamicLibraryMac (library);
		else
			return LoadDynamicLibraryLinux (library);
	}

	bool LoadDynamicLibraryMac (DynamicLibrary library)
	{
		var process = new Process ();
		process.StartInfo.FileName = "/usr/bin/nm";
		process.StartInfo.Arguments = $"-g -j -U {library.Path}";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;

		string output;
		try {
			process.Start ();

			output = process.StandardOutput.ReadToEnd ();
			process.WaitForExit ();
		} catch (Exception ex) {
			Console.Error.WriteLine ($"Command failed: {ex.Message}");
			return false;
		}

		var symbols = output.Split ();
		DynamicSymbols.Add (library.Name, new List<string> (symbols));

		Console.WriteLine ($"Added {symbols.Length} symbols from {library.Path}.");

		return true;
	}

	bool LoadDynamicLibraryLinux (DynamicLibrary library)
	{
		var process = new Process ();
		process.StartInfo.FileName = "/usr/bin/nm";
		process.StartInfo.Arguments = $"-g --defined-only {library.Path}";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;

		var symbols = new List<string> ();
		try {
			process.Start ();

			var regex = new Regex (@"^([0-9a-fA-F]+)\s(\w)\s(\w+)");

			string line;
			while ((line = process.StandardOutput.ReadLine ()) != null) {
				var match = regex.Match (line);
				if (!match.Success)
					continue;
				symbols.Add (match.Groups[3].Value);
			}

			process.WaitForExit ();
		} catch (Exception ex) {
			Console.Error.WriteLine ($"Command failed: {ex.Message}");
			return false;
		}

		DynamicSymbols.Add (library.Name, symbols);

		Console.WriteLine ($"Added {symbols.Count} symbols from {library.Path}.");

		return true;
	}

	void CheckAssembly ()
	{
		var readerParameters = new ReaderParameters {
			ReadSymbols = true,
			ReadWrite = true,
			SymbolReaderProvider = new DefaultSymbolReaderProvider (false)
		};

		using (var assembly = AssemblyDefinition.ReadAssembly (FileName, readerParameters)) {
			foreach (var module in assembly.Modules) {
				foreach (var type in module.GetTypes ()) {
					foreach (var method in type.Methods) {
						CheckMethod (method);
					}
				}
			}

		}
	}

	void CheckMethod (MethodDefinition method)
	{
		if (!method.HasPInvokeInfo)
			return;

		var dll = method.PInvokeInfo.Module.Name;
		if (Options.IgnoreList.Contains (dll))
			return;
		
		switch (dll) {
		case "libc":
			return;
		case "__Internal":
			if (Options.MonoRuntime == null)
				return;
			dll = Options.MonoRuntime;
			break;
		}

		if (!DynamicSymbols.TryGetValue (dll, out var symbols)) {
			if (File.Exists (dll) && LoadDynamicLibrary (new DynamicLibrary (dll)))
				symbols = DynamicSymbols[dll];
			else if (File.Exists (dll + ".dylib") && LoadDynamicLibrary (new DynamicLibrary (dll, dll + ".dylib")))
				symbols = DynamicSymbols[dll];
			else if (File.Exists (dll + ".so") && LoadDynamicLibrary (new DynamicLibrary (dll, dll + ".so")))
				symbols = DynamicSymbols[dll];
			else {
				Console.Error.WriteLine ($"Cannot find assembly: '{dll}'.");
				DynamicSymbols.Add (dll, null);
				return;
			}
		}

		if (symbols == null)
			return;

		var function = method.PInvokeInfo.EntryPoint;
		switch (function) {
			case "dlopen":
			case "dlsym":
			case "dlclose":
				return;
		}

		if (Platform.IsMacOS) {
			if (symbols.Contains ("_" + function))
				return;
		} else {
			if (symbols.Contains (function))
				return;
		}

		Console.Error.WriteLine ($"Library '{dll}' does not contain '{function}'.");

	}
}