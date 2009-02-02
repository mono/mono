//
// MdbWriter.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2007 Novell, Inc.
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

// Inspired by the pdb2mdb tool written by Robert Jordan, thanks Robert!

namespace Mono.Cecil.Mdb {

	using System;
	using System.Collections;

	using Mono.CompilerServices.SymbolWriter;

	using Mono.Cecil;
	using Mono.Cecil.Cil;

	class MdbWriter : ISymbolWriter {

		Guid m_mvid;
		MonoSymbolWriter m_writer;

		Hashtable m_documents;

		public MdbWriter (Guid mvid, string assembly)
		{
			m_mvid = mvid;
			m_writer = new MonoSymbolWriter (assembly);
			m_documents = new Hashtable ();
		}

		static Instruction [] GetInstructions (MethodBody body)
		{
			ArrayList list = new ArrayList ();
			foreach (Instruction instruction in body.Instructions)
				if (instruction.SequencePoint != null)
					list.Add (instruction);

			return list.ToArray (typeof (Instruction)) as Instruction [];
		}

		SourceFile GetSourceFile (Document document)
		{
			string url = document.Url;
			SourceFile file = m_documents [url] as SourceFile;
			if (file != null)
				return file;

			SourceFileEntry entry = m_writer.DefineDocument (url);
			CompileUnitEntry comp_unit = m_writer.DefineCompilationUnit (entry);

			file = new SourceFile (comp_unit, entry);
			m_documents [url] = file;
			return file;
		}

		void Populate (Instruction [] instructions, int [] offsets,
			int [] startRows, int [] startCols, int [] endRows, int [] endCols,
			out SourceFile file)
		{
			SourceFile document = null;

			for (int i = 0; i < instructions.Length; i++) {
				Instruction instr = (Instruction) instructions [i];
				offsets [i] = instr.Offset;

				if (document == null)
					document = GetSourceFile (instr.SequencePoint.Document);

				startRows [i] = instr.SequencePoint.StartLine;
				startCols [i] = instr.SequencePoint.StartColumn;
				endRows [i] = instr.SequencePoint.EndLine;
				endCols [i] = instr.SequencePoint.EndColumn;
			}

			file = document;
		}

		public void Write (MethodBody body)
		{
			SourceMethod meth = new SourceMethod (body.Method);

			SourceFile file;

			Instruction [] instructions = GetInstructions (body);
			int length = instructions.Length;
			if (length == 0)
				return;

			int [] offsets = new int [length];
			int [] startRows = new int [length];
			int [] startCols = new int [length];
			int [] endRows = new int [length];
			int [] endCols = new int [length];

			Populate (instructions, offsets, startRows, startCols, endRows, endCols, out file);

			SourceMethodBuilder builder = m_writer.OpenMethod (file.CompilationUnit, 0, meth);

			for (int i = 0; i < length; i++)
				builder.MarkSequencePoint (offsets [i], file.CompilationUnit.SourceFile,
							   startRows [i], startCols [i], false);

			MarkVariables (body);

			m_writer.CloseMethod ();
		}

		void MarkVariables (MethodBody body)
		{
			for (int i = 0; i < body.Variables.Count; i++) {
				VariableDefinition var = body.Variables [i];
				m_writer.DefineLocalVariable (i, var.Name);
			}
		}

		public byte [] GetDebugHeader ()
		{
			// mdb doesn't need a debug header
			// in the PE file.
			return new byte [0];
		}

		public void Dispose ()
		{
			m_writer.WriteSymbolFile (m_mvid);
		}

		class SourceFile : ISourceFile {
			CompileUnitEntry comp_unit;
			SourceFileEntry entry;

			public SourceFileEntry Entry {
				get { return entry; }
			}

			public CompileUnitEntry CompilationUnit {
				get { return comp_unit; }
			}

			public SourceFile (CompileUnitEntry comp_unit, SourceFileEntry entry)
			{
				this.comp_unit = comp_unit;
				this.entry = entry;
			}
		}

		class SourceMethod : IMethodDef {

			MethodDefinition m_method;

			public string Name {
				get { return m_method.Name; }
			}

			public int Token {
				get { return (int) m_method.MetadataToken.ToUInt (); }
			}

			public SourceMethod (MethodDefinition method)
			{
				m_method = method;
			}
		}
	}
}
