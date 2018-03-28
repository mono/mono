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
			using (var asm = AssemblyDefinition.ReadAssembly (filename)) {

				var pdb = asm.Name.Name + ".pdb";
				pdb = Path.Combine (Path.GetDirectoryName (filename), pdb);

				if (!File.Exists (pdb))
					throw new FileNotFoundException ("PDB file doesn't exist: " + pdb);

				using (var stream = File.OpenRead (pdb)) {
					if (IsPortablePdb (stream))
						throw new PortablePdbNotSupportedException ();

					var funcs = PdbFile.LoadFunctions (stream, true);
					Converter.Convert (asm, funcs, new MonoSymbolWriter (filename));
				}
			}
		}

		static bool IsPortablePdb (FileStream stream)
		{
			const uint ppdb_signature = 0x424a5342;

			var position = stream.Position;
			try {
				var reader = new BinaryReader (stream);
				return reader.ReadUInt32 () == ppdb_signature;
			} finally {
				stream.Position = position;
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
			int last_line = 0;
			foreach (var line in function.lines.SelectMany (lines => lines.lines)) {
				// 0xfeefee is an MS convention, we can't pass it into mdb files, so we use the last non-hidden line
				bool is_hidden = line.lineBegin == 0xfeefee;
				builder.MarkSequencePoint (
					(int) line.offset,
					file.CompilationUnit.SourceFile,
					is_hidden ? last_line : (int) line.lineBegin,
					(int) line.colBegin, is_hidden ? -1 : (int)line.lineEnd, is_hidden ? -1 : (int)line.colEnd,
					is_hidden);
				if (!is_hidden)
					last_line = (int) line.lineBegin;
			}
		}

		void ConvertVariables (PdbFunction function)
		{
			foreach (var scope in function.scopes)
				ConvertScope (scope);
		}

		void ConvertScope (PdbScope scope)
		{
			ConvertSlots (scope, scope.slots);

			foreach (var s in scope.scopes)
				ConvertScope (s);
		}

		void ConvertSlots (PdbScope scope, IEnumerable<PdbSlot> slots)
		{
			int scope_idx = mdb.OpenScope ((int)scope.address);
			foreach (var slot in slots) {
				mdb.DefineLocalVariable ((int) slot.slot, slot.name);
				mdb.DefineScopeVariable (scope_idx, (int)slot.slot);
			}
			mdb.CloseScope ((int)(scope.address + scope.length));
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

	public class PortablePdbNotSupportedException : Exception {
	}

	class Driver {

		static void Main (string [] args)
		{
			if (args.Length != 1)
				Usage ();

			var asm = args [0];

			if (!File.Exists (asm))
				Usage ();

			try {
				Converter.Convert (asm);
			} catch (FileNotFoundException ex) {
				Usage ();
			} catch (PortablePdbNotSupportedException) {
				Console.WriteLine ("Error: A portable PDB can't be converted to mdb.");
				Environment.Exit (2);
			}
			catch (Exception ex) {
				Error (ex);
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

			Environment.Exit (1);
		}
	}
}
