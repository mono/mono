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
