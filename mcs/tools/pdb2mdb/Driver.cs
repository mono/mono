//
// Driver.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2009 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Cci;
using Microsoft.Cci.Pdb;

using Mono.Cecil;

using Mono.CompilerServices.SymbolWriter;

namespace Pdb2Mdb {

	public class Converter {

		MonoSymbolWriter mdb;
		Dictionary<string, SourceFile> files = new Dictionary<string, SourceFile> ();

		public static void Convert (string filename)
		{
			var asm = AssemblyDefinition.ReadAssembly (filename);

			var pdb = asm.Name.Name + ".pdb";
			pdb = Path.Combine (Path.GetDirectoryName (filename), pdb);

			using (var stream = File.OpenRead (pdb)) {
				var funcs = PdbFile.LoadFunctions (stream, true);
				Converter.Convert (asm, funcs, new MonoSymbolWriter (filename));
			}
		}

		internal Converter (MonoSymbolWriter mdb)
		{
			this.mdb = mdb;
		}

		internal static void Convert (AssemblyDefinition assembly, IEnumerable<PdbFunction> functions, MonoSymbolWriter mdb)
		{
			var converter = new Converter (mdb);

			foreach (var function in functions)
				converter.ConvertFunction (function);

			mdb.WriteSymbolFile (assembly.MainModule.Mvid);
		}

		void ConvertFunction (PdbFunction function)
		{
			if (function.lines == null)
				return;

			var method = new SourceMethod { Name = function.name, Token = (int) function.token };

			var file = GetSourceFile (mdb, function);

			var builder = mdb.OpenMethod (file.CompilationUnit, 0, method);

			ConvertSequencePoints (function, file, builder);

			ConvertVariables (function);

			mdb.CloseMethod ();
		}

		void ConvertSequencePoints (PdbFunction function, SourceFile file, SourceMethodBuilder builder)
		{
			foreach (var line in function.lines.SelectMany (lines => lines.lines))
				builder.MarkSequencePoint (
					(int) line.offset,
					file.CompilationUnit.SourceFile,
					(int) line.lineBegin,
					(int) line.colBegin, line.lineBegin == 0xfeefee);
		}

		void ConvertVariables (PdbFunction function)
		{
			foreach (var scope in function.scopes)
				ConvertScope (scope);
		}

		void ConvertScope (PdbScope scope)
		{
			ConvertSlots (scope.slots);

			foreach (var s in scope.scopes)
				ConvertScope (s);
		}

		void ConvertSlots (IEnumerable<PdbSlot> slots)
		{
			foreach (var slot in slots)
				mdb.DefineLocalVariable ((int) slot.slot, slot.name);
		}

		SourceFile GetSourceFile (MonoSymbolWriter mdb, PdbFunction function)
		{
			var name = (from l in function.lines where l.file != null select l.file.name).First ();

			SourceFile file;
			if (files.TryGetValue (name, out file))
				return file;

			var entry = mdb.DefineDocument (name);
			var unit = mdb.DefineCompilationUnit (entry);

			file = new SourceFile (unit, entry);
			files.Add (name, file);
			return file;
		}

		class SourceFile : ISourceFile {
			CompileUnitEntry comp_unit;
			SourceFileEntry entry;

			public SourceFileEntry Entry
			{
				get { return entry; }
			}

			public CompileUnitEntry CompilationUnit
			{
				get { return comp_unit; }
			}

			public SourceFile (CompileUnitEntry comp_unit, SourceFileEntry entry)
			{
				this.comp_unit = comp_unit;
				this.entry = entry;
			}
		}

		class SourceMethod : IMethodDef {

			public string Name { get; set; }

			public int Token { get; set; }
		}
	}

	class Driver {

		static void Main (string [] args)
		{
			if (args.Length != 1)
				Usage ();

			var asm = args [0];

			if (!File.Exists (asm))
				Usage ();

			var assembly = AssemblyDefinition.ReadAssembly (asm);

			var pdb = assembly.Name.Name + ".pdb";
			pdb = Path.Combine (Path.GetDirectoryName (asm), pdb);

			if (!File.Exists (pdb))
				Usage ();

			using (var stream = File.OpenRead (pdb)) {
				Convert (assembly, stream, new MonoSymbolWriter (asm));
			}
		}

		static void Convert (AssemblyDefinition assembly, Stream pdb, MonoSymbolWriter mdb)
		{
			try {
				Converter.Convert (assembly, PdbFile.LoadFunctions (pdb, true), mdb);
			} catch (Exception e) {
				Error (e);
			}
		}

		static void Usage ()
		{
			Console.WriteLine ("Mono pdb to mdb debug symbol store converter");
			Console.WriteLine ("Usage: pdb2mdb assembly");

			Environment.Exit (1);
		}

		static void Error (Exception e)
		{
			Console.WriteLine ("Fatal error:");
			Console.WriteLine (e);
		}
	}
}
