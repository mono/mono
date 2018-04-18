using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;

public class Driver
{
	public static void Main (String[] args) {
		//
		// The assembly itself needs to be changed to add some data needed to
		// find the matching .pdb file.
		//
		if (args.Length != 2) {
			Console.Error.WriteLine ("Usage: mdb2pdb <input assembly> <output assembly>");
			Environment.Exit (1);
		}

		string infile = args [0];
		string outfile = args [1];

		if (infile == outfile) {
			Console.Error.WriteLine ("Input and output file names needs to be different.");
			Environment.Exit (1);
		}

		var reader_parameters = new ReaderParameters {
			SymbolReaderProvider = new MdbReaderProvider (),
		};
		using (var module = ModuleDefinition.ReadModule (infile, reader_parameters)) {
			var writer_parameters = new WriterParameters {
				SymbolWriterProvider = new PortablePdbWriterProvider (),
			};

			module.Write (outfile, writer_parameters);
		}
	}
}
