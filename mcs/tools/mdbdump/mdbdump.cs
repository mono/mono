using System;
using System.IO;
using Mono.CompilerServices.SymbolWriter;
using Mono.Cecil;

public class MdbDump
{
	public static int Main (String[] args) {
		if (args.Length < 1) {
			Console.Error.WriteLine ("Usage: mdbdump <assembly>");
			return 1;
		}

		var assembly = AssemblyDefinition.ReadAssembly (args [0]);

		var f = MonoSymbolFile.ReadSymbolFile (args [0] + ".mdb");
		foreach (var m in f.Methods) {
			var lt = m.GetLineNumberTable ();
			Console.WriteLine (assembly.MainModule.LookupToken (m.Token));
			foreach (var lne in lt.LineNumbers)
				Console.WriteLine (lne);
		}
		return 0;
	}
}
