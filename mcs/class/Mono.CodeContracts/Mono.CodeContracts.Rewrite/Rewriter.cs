//
// Rewriter.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.IO;

namespace Mono.CodeContracts.Rewrite {
	public static class Rewriter {

		public static RewriterResults Rewrite (RewriterOptions options)
		{
			if (!options.Rewrite) {
				return RewriterResults.Warning("Not asked to rewrite");
			}

			if (!options.Assembly.IsSet) {
				return RewriterResults.Error ("No assembly given to rewrite");
			}
			AssemblyDefinition assembly = options.Assembly.IsFilename ?
				AssemblyDefinition.ReadAssembly (options.Assembly.Filename) :
				AssemblyDefinition.ReadAssembly (options.Assembly.Streams.Assembly);
			
			if (options.ForceAssemblyRename != null) {
				assembly.Name = new AssemblyNameDefinition (options.ForceAssemblyRename, new Version (0, 0, 0, 0));
			}

			List<string> errors = new List<string> ();
			List<string> warnings = new List<string> ();

			bool usingMdb = false;
			bool usingPdb = false;
			List<ISymbolReader> symReaders = new List<ISymbolReader> ();
			if (options.Debug) {
				if (options.Assembly.IsStream && options.Assembly.Streams.Symbols == null) {
					warnings.Add ("-debug specified, but no symbol stream provided.");
				} else {
					try {
						ISymbolReaderProvider symProv = new Mono.Cecil.Mdb.MdbReaderProvider ();
						foreach (var module in assembly.Modules) {
							ISymbolReader sym = options.Assembly.IsFilename ?
								symProv.GetSymbolReader (module, options.Assembly.Filename) :
								symProv.GetSymbolReader (module, options.Assembly.Streams.Symbols);
							module.ReadSymbols (sym);
							symReaders.Add(sym);
						}
						usingMdb = true;
					} catch {
						try {
							ISymbolReaderProvider symProv = new Mono.Cecil.Pdb.PdbReaderProvider ();
							foreach (var module in assembly.Modules) {
								ISymbolReader sym = options.Assembly.IsFilename ?
									symProv.GetSymbolReader (module, options.Assembly.Filename) :
									symProv.GetSymbolReader (module, options.Assembly.Streams.Symbols);
								module.ReadSymbols (sym);
								symReaders.Add(sym);
							}
							usingPdb = true;
						} catch {
						}
					}
					if (!usingMdb && !usingPdb) {
						warnings.Add ("-debug specified, but no MDB or PDB symbol file found.");
					}
				}
			}

			var output = options.OutputFile.IsSet ? options.OutputFile : options.Assembly;
			ISymbolWriter symWriter = null;
			if (options.WritePdbFile) {
				if (!options.Debug) {
					return RewriterResults.Error ("Must specify -debug if using -writePDBFile.");
				}
				if (output.IsStream && output.Streams.Symbols==null){
					return RewriterResults.Error ("-writePDFFile specified, but no output symbol stream provided.");
				}
				// TODO: Get symbol writing to work.
				ISymbolWriterProvider symProv = null;
				if (usingMdb) {
					symProv = new Mono.Cecil.Mdb.MdbWriterProvider ();
				} else if (usingPdb) {
					symProv = new Mono.Cecil.Pdb.PdbWriterProvider ();
				} else {
					warnings.Add ("-writePDBFile specified, but no symbol file found, cannot write symbols.");
				}
				if (symProv != null) {
					symWriter = output.IsFilename ?
						symProv.GetSymbolWriter (assembly.MainModule, output.Filename) :
						symProv.GetSymbolWriter (assembly.MainModule, output.Streams.Symbols);
				}
			}
			try {
				PerformRewrite rewriter = new PerformRewrite (symWriter, options);
				rewriter.Rewrite (assembly);

				if (output.IsFilename) {
					assembly.Name.Name = Path.GetFileNameWithoutExtension (output.Filename);
					assembly.Write (output.Filename);
				} else {
					assembly.Write (output.Streams.Assembly);
				}
			} finally {
				if (symWriter != null) {
					symWriter.Dispose ();
				}
				foreach (var symReader in symReaders) {
					try {
						if (symReader != null) {
							symReader.Dispose ();
						}
					} catch {
					}
				}
			}

			return new RewriterResults (warnings, errors);
		}

	}
}
