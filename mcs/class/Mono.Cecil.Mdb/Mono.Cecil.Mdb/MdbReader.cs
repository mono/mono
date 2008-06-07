//
// MdbReader.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
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

namespace Mono.Cecil.Mdb {

	using System.Collections;

	using Mono.Cecil.Cil;

	using Mono.CompilerServices.SymbolWriter;

	class MdbReader : ISymbolReader {

		MonoSymbolFile m_symFile;
		Hashtable m_documents;
		Hashtable m_scopes;

		public MdbReader (MonoSymbolFile symFile)
		{
			m_symFile = symFile;
			m_documents = new Hashtable ();
			m_scopes = new Hashtable ();
		}

		Instruction GetInstruction (MethodBody body, IDictionary instructions, int offset)
		{
			Instruction instr = (Instruction) instructions [offset];
			if (instr != null)
				return instr;

			return body.Instructions.Outside;
		}

		public void Read (MethodBody body, IDictionary instructions)
		{
			MethodEntry entry = m_symFile.GetMethodByToken ((int) body.Method.MetadataToken.ToUInt ());
			if (entry == null)
				return;

			ReadScopes (entry, body, instructions);
			ReadLineNumbers (entry, instructions);
			ReadLocalVariables (entry, body);
		}

		void ReadLocalVariables (MethodEntry entry, MethodBody body)
		{
			LocalVariableEntry[] locals = entry.GetLocals ();
			foreach (LocalVariableEntry loc in locals) {
				VariableDefinition var = body.Variables [loc.Index];
				var.Name = loc.Name;

				Scope scope = m_scopes [loc.BlockIndex] as Scope;
				if (scope == null)
					continue;
				scope.Variables.Add (var);
			}
		}

		void ReadLineNumbers (MethodEntry entry, IDictionary instructions)
		{
			LineNumberTable lnt = entry.GetLineNumberTable ();
			foreach (LineNumberEntry line in lnt.LineNumbers) {
				Instruction instr = instructions [line.Offset] as Instruction;
				if (instr == null)
					continue;

				Document doc = GetDocument (entry.CompileUnit.SourceFile);
				instr.SequencePoint = new SequencePoint (doc);
				instr.SequencePoint.StartLine = line.Row;
				instr.SequencePoint.EndLine = line.Row;
			}
		}

		Document GetDocument (SourceFileEntry file)
		{
			Document doc = m_documents [file.FileName] as Document;
			if (doc != null)
				return doc;

			doc = new Document (file.FileName);

			m_documents [file.FileName] = doc;
			return doc;
		}

		void ReadScopes (MethodEntry entry, MethodBody body, IDictionary instructions)
		{
			CodeBlockEntry[] blocks = entry.GetCodeBlocks ();
			foreach (CodeBlockEntry cbe in blocks) {
				if (cbe.BlockType != CodeBlockEntry.Type.Lexical)
					continue;

				Scope s = new Scope ();
				s.Start = GetInstruction (body, instructions, cbe.StartOffset);
				s.End = GetInstruction(body, instructions, cbe.EndOffset);
				m_scopes [entry.Index] = s;

				if (!AddScope (body, s))
					body.Scopes.Add (s);
			}
		}

		bool AddScope (IScopeProvider provider, Scope s)
		{
			foreach (Scope scope in provider.Scopes) {
				if (AddScope (scope, s))
					return true;

				if (s.Start.Offset >= scope.Start.Offset && s.End.Offset <= scope.End.Offset) {
					scope.Scopes.Add (s);
					return true;
				}
			}

			return false;
		}

		public void Dispose ()
		{
			m_symFile.Dispose ();
		}
	}
}
