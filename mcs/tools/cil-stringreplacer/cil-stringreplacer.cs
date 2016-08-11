//
// cil-stringreplacer.cs
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Options;
using System.Collections.Generic;

public class Program
{
	class CmdOptions
	{
		public bool ShowHelp { get; set; }
		public bool Verbose { get; set; }
		public List<string> ResourcesStrings { get; }

		public CmdOptions ()
		{
			ResourcesStrings = new List<string> ();
		}
	}

	public static int Main (string[] args)
	{
		var options = new CmdOptions ();

		var p = new OptionSet () {
			{ "r|resourcestrings=", "File with string resource in key=value format",
				v => options.ResourcesStrings.Add (v) },
			{ "h|help",  "Display available options", 
				v => options.ShowHelp = v != null },
			{ "v|verbose",  "Use verbose output", 
				v => options.Verbose = v != null },			
		};

		List<string> extra;
		try {
			extra = p.Parse (args);
		}
		catch (OptionException e) {
			Console.WriteLine (e.Message);
			Console.WriteLine ("Try 'cil-stringreplacer -help' for more information.");
			return 1;
		}

		if (options.ShowHelp) {
			ShowHelp (p);
			return 0;
		}

		if (extra.Count != 1) {
			ShowHelp (p);
			return 2;
		}

		var resourcesStrings = new Dictionary<string, string> ();
		if (!LoadGetResourceStrings (resourcesStrings, options))
			return 3;

		RewriteAssembly (extra [0], resourcesStrings, options);

		return 0;
	}

	static void ShowHelp (OptionSet p)
	{
		Console.WriteLine ("Usage: cil-stringreplacer [options] assembly");
		Console.WriteLine ("Rewrites all occurences of string keys with their values from string resource file");
		Console.WriteLine ();
		Console.WriteLine ("Options:");
		p.WriteOptionDescriptions (Console.Out);
	}

	static void RewriteAssembly (string assemblyLocation, Dictionary<string, string> resourcesStrings, CmdOptions options)
	{
		var readerParameters = new ReaderParameters { ReadSymbols = true, ReadWrite = true };
		using (var assembly = AssemblyDefinition.ReadAssembly (assemblyLocation, readerParameters)) {
			foreach (var module in assembly.Modules) {
				foreach (var type in module.GetTypes ()) {
					foreach (var method in type.Methods) {
						if (!method.HasBody)
							continue;
						
						foreach (var instr in method.Body.Instructions) {
							if (instr.OpCode != OpCodes.Ldstr)
								continue;

							string value;
							if (resourcesStrings.TryGetValue ((string)instr.Operand, out value)) {
								if (options.Verbose) {
									Console.WriteLine ($"Replacing '{instr.Operand}' with '{value}'");
								}

								instr.Operand = value;
							}
						}
					}
				}
			}

			var writerParameters = new WriterParameters { WriteSymbols = true };
			assembly.Write (writerParameters);
		}
	}

	static bool LoadGetResourceStrings (Dictionary<string, string> resourcesStrings, CmdOptions options)
	{
		foreach (var fileName in options.ResourcesStrings) {
			if (!File.Exists (fileName)) {
				Console.Error.WriteLine ($"Error reading resource file '{fileName}'");
				return false;
			}

			foreach (var l in File.ReadLines (fileName)) {
				var line = l.Trim ();
				if (line.Length == 0 || line [0] == '#' || line [0] == ';')
					continue;

				var epos = line.IndexOf ('=');
				if (epos < 0)
					continue;

				var key = line.Substring (0, epos).Trim ();
				var value = line.Substring (epos + 1).Trim ();

				resourcesStrings [key] = value;
			}
		}

		return true;
	}
}