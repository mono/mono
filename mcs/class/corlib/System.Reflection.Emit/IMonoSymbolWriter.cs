
//
// System.Reflection.Emit/IMonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;

namespace Mono.CSharp.Debugger {
	public interface IMonoSymbolWriter : ISymbolWriter {
		byte[] CreateSymbolFile (AssemblyBuilder assembly_builder);

		void MarkSequencePoint (int offset, int line, int column);

		int DefineNamespace (string name, ISymbolDocumentWriter document,
				     string[] using_clauses, int parent);

		void OpenMethod (ISymbolDocumentWriter document, int startLine, int startColumn,
				 int endLine, int endColumn, MethodBase method, int namespace_id);
	}
}
