//
// MdbWriter.cs
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
	using SDS = System.Diagnostics.SymbolStore;
	using Mono.CompilerServices.SymbolWriter;
	using Mono.Cecil.Cil;

	class MdbWriter : ISymbolWriter {

		SymbolWriterImpl m_writer;
		Hashtable m_documents;

		public MdbWriter (SymbolWriterImpl writer)
		{
			m_writer = writer;
			m_documents = new Hashtable ();
		}

		public void Write (MethodBody body, byte [][] variables)
		{
			Document document = CreateDocuments (body);
			if (document != null) {
				SDS.ISymbolDocumentWriter docWriter = GetDocument (document);
				m_writer.SetMethodSourceRange (docWriter, 1, 1, docWriter, int.MaxValue, int.MaxValue);
			}

			m_writer.OpenMethod (new SDS.SymbolToken ((int) body.Method.MetadataToken.ToUInt ()));
			m_writer.SetSymAttribute (new SDS.SymbolToken (), "__name", System.Text.Encoding.UTF8.GetBytes (body.Method.Name));

			CreateScopes (body, body.Scopes, variables);
			m_writer.CloseMethod ();
		}

		void CreateScopes (MethodBody body, ScopeCollection scopes, byte [][] variables)
		{
			foreach (Scope s in scopes) {
				int startOffset = s.Start.Offset;
				int endOffset = s.End == body.Instructions.Outside ?
					body.Instructions[body.Instructions.Count - 1].Offset + 1 :
					s.End.Offset;

				m_writer.OpenScope (startOffset);
				//m_writer.UsingNamespace (body.Method.DeclaringType.Namespace);
				//m_writer.OpenNamespace (body.Method.DeclaringType.Namespace);

				int start = body.Instructions.IndexOf (s.Start);
				int end = s.End == body.Instructions.Outside ?
					body.Instructions.Count - 1 :
					body.Instructions.IndexOf (s.End);

				ArrayList instructions = new ArrayList();
				for (int i = start; i <= end; i++)
					if (body.Instructions [i].SequencePoint != null)
						instructions.Add (body.Instructions [i]);

				Document doc = null;

				int [] offsets = new int [instructions.Count];
				int [] startRows = new int [instructions.Count];
				int [] startCols = new int [instructions.Count];
				int [] endRows = new int [instructions.Count];
				int [] endCols = new int [instructions.Count];

				for (int i = 0; i < instructions.Count; i++) {
					Instruction instr = (Instruction) instructions [i];
					offsets [i] = instr.Offset;

					if (doc == null)
						doc = instr.SequencePoint.Document;

					startRows [i] = instr.SequencePoint.StartLine;
					startCols [i] = instr.SequencePoint.StartColumn;
					endRows [i] = instr.SequencePoint.EndLine;
					endCols [i] = instr.SequencePoint.EndColumn;
				}

				m_writer.DefineSequencePoints (GetDocument (doc),
					offsets, startRows, startCols, endRows, endCols);

				CreateLocalVariables (s, startOffset, endOffset, variables);

				CreateScopes (body, s.Scopes, variables);
				m_writer.CloseNamespace ();

				m_writer.CloseScope (endOffset);
			}
		}

		void CreateLocalVariables (Scope s, int startOffset, int endOffset, byte [][] variables)
		{
			for (int i = 0; i < s.Variables.Count; i++) {
				VariableDefinition var = s.Variables [i];
				m_writer.DefineLocalVariable (
					var.Name,
					0,
					variables [var.Index],
					0,
					0,
					0,
					0,
					startOffset,
					endOffset);
			}
		}

		Document CreateDocuments (MethodBody body)
		{
			Document doc = null;
			foreach (Instruction instr in body.Instructions) {
				if (instr.SequencePoint == null)
					continue;

				if (doc == null)
					doc = instr.SequencePoint.Document;

				GetDocument (instr.SequencePoint.Document);
			}

			return doc;
		}

		SDS.ISymbolDocumentWriter GetDocument (Document document)
		{
			SDS.ISymbolDocumentWriter docWriter = m_documents [document.Url] as SDS.ISymbolDocumentWriter;
			if (docWriter != null)
				return docWriter;

			docWriter = m_writer.DefineDocument (
				document.Url,
				GuidAttribute.GetGuidFromValue ((int) document.Language, typeof (DocumentLanguage)),
				GuidAttribute.GetGuidFromValue ((int) document.LanguageVendor, typeof (DocumentLanguageVendor)),
				GuidAttribute.GetGuidFromValue ((int) document.Type, typeof (DocumentType)));

			m_documents [document.Url] = docWriter;
			return docWriter;
		}

		public void Dispose ()
		{
			m_writer.Close ();
		}
	}
}
