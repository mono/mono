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
		public string ILFile { get; set; }
		public bool MonoMscorlib { get; set; }

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
			{ "ilreplace=", "File with IL code to be used instead",
				v => options.ILFile = v },
			{ "mscorlib-debug", "IL customizations for Mono's mscorlib",
				v => options.MonoMscorlib = v != null },
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
		var methods = new Dictionary<string, MethodBody> (StringComparer.Ordinal);
		if (options.ILFile != null) {
			var rp = new ReaderParameters {
				InMemory = true
			};

			using (var module = ModuleDefinition.ReadModule (options.ILFile,rp)) {
				foreach (var type in module.GetTypes ()) {
					foreach (var method in type.Methods) {
						if (!method.HasBody)
							continue;

						methods.Add (method.FullName, method.Body);
					}
				}
			}
		}

		var readerParameters = new ReaderParameters {
			ReadSymbols = true,
			ReadWrite = true,
			SymbolReaderProvider = new DefaultSymbolReaderProvider (false)
		};

		using (var assembly = AssemblyDefinition.ReadAssembly (assemblyLocation, readerParameters)) {
			foreach (var module in assembly.Modules) {
				foreach (var type in module.GetTypes ()) {
					if (options.MonoMscorlib && type.Name == "Debug" && type.Namespace == "System.Diagnostics") {
						type.Name = "DebugPrivate";
					}

					foreach (var method in type.Methods) {
						if (!method.HasBody)
							continue;

						MethodBody newBody;
						if (methods.TryGetValue (method.FullName, out newBody)) {
							var mbody = method.Body;
							mbody.Instructions.Clear ();
							foreach (var instr in newBody.Instructions) {
								switch (instr.OpCode.OperandType) {
								case OperandType.InlineType:
									var tr = (TypeReference)instr.Operand;
									foreach (var t in method.GenericParameters) {
										if (tr.FullName == t.FullName) {
											instr.Operand = t;
											break;
										}
									}

									break;
								}

								mbody.Instructions.Add (instr);
							}

							method.Body.Variables.Clear ();
							foreach (var variable in newBody.Variables) {
								mbody.Variables.Add (variable);
							}
						}

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

			var writerParameters = new WriterParameters () {
				WriteSymbols = assembly.MainModule.HasSymbols
			};

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