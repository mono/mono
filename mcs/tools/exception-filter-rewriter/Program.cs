using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ExceptionRewriter {
	class Program {
		public static int Main (string[] _args) 
		{
			try {
				var options = new RewriteOptions ();

				foreach (var arg in _args)
					ParseArgument (arg, options);

				var argv = _args.Where (arg => !arg.StartsWith ("-")).ToArray ();

				var step = options.Overwrite ? 1 : 2;
				if (argv.Length < step) {
					Usage ();
					return 1;
				}

				int exitCode = 0;

				for (int i = 0; i < argv.Length; i += step) {
					var src = argv[i];
					var dst = options.Overwrite ? src : argv[i + 1];

					if (options.Verbose)
						Console.WriteLine ($"{Path.GetFileName (src)} -> {Path.GetFullPath (dst)}...{Environment.NewLine}====");
					else if (argv.Length > step)
						Console.WriteLine ($"{Path.GetFileName (src)} -> {Path.GetFullPath (dst)}");

					var assemblyResolver = new DefaultAssemblyResolver ();
					assemblyResolver.AddSearchDirectory (Path.GetDirectoryName (src));

					using (var def = AssemblyDefinition.ReadAssembly (src, new ReaderParameters {
						ReadWrite = false,
						ReadingMode = ReadingMode.Immediate,
						AssemblyResolver = assemblyResolver,
						ReadSymbols = options.EnableSymbols,
						SymbolReaderProvider = new DefaultSymbolReaderProvider (throwIfNoSymbol: false)
					})) {
						var arw = new AssemblyRewriter (def, options);
						int errorCount = arw.Rewrite ();

						if (errorCount > 0) {
							Console.Error.WriteLine ($"Not saving due to error(s): {dst}");
							exitCode += 1;
						} else {
							var shouldWriteSymbols = options.EnableSymbols && def.MainModule.SymbolReader != null;

							def.Write (dst + ".tmp", new WriterParameters {
								WriteSymbols = shouldWriteSymbols
							});

							File.Copy (dst + ".tmp", dst, true);
							if (File.Exists (dst + ".pdb")) {
								File.Copy (dst + ".pdb", dst.Replace (".exe", ".pdb"), true);
								File.Delete (dst + ".pdb");
							}
							File.Delete (dst + ".tmp");
						}
					}

				}

				return exitCode;
			} finally {
				if (Debugger.IsAttached) {
					Console.WriteLine ("Press enter to exit");
					Console.ReadLine ();
				}
			}
		}

		static void ParseArgument (string arg, RewriteOptions options) 
		{
			if (!arg.StartsWith ("-"))
				return;

			switch (arg) {
				case "--overwrite":
					options.Overwrite = true;
					break;
				case "--abort":
					options.AbortOnError = true;
					break;
				case "--warn":
					options.AbortOnError = false;
					break;
				case "--generics":
					options.EnableGenerics = true;
					break;
				case "--no-generics":
					options.EnableGenerics = false;
					break;
				case "--verbose":
					options.Verbose = true;
					break;
				case "--symbols":
					options.EnableSymbols = true;
					break;
				default:
					throw new Exception ("Unsupported argument: " + arg);
			}
		}

		static void Usage () 
		{
			Console.WriteLine (@"Expected: exception-filter-rewriter [options] input output [input2 output2] ...
or        exception-filter-rewriter [options] --overwrite file1 [file2] ...
--overwrite   Overwrite source file with rewritten output (input is a list of filenames instead of in/out pairs)
--abort       Abort on error (default)
--warn        On error, output warning instead of aborting
--symbols     Enable loading/saving debug information
--generics    Enable rewriting filters for generics (currently broken)
--no-generics Disable rewriting filters for generics (default)
--verbose     Output name of every rewritten method");

		}
	}
}
