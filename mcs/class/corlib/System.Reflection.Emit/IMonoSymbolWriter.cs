
//
// System.Reflection.Emit/IMonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;

namespace Mono.CSharp.Debugger {
	public interface IMonoSymbolWriter : ISymbolWriter {
		byte[] CreateSymbolFile (AssemblyBuilder assembly_builder);
	}
}
