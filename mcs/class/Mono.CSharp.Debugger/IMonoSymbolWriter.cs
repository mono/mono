//
// System.Diagnostics.SymbolStore/IMonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This interface is derived from System.Diagnostics.SymbolStore.ISymbolWriter.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public interface IMonoSymbolWriter : ISymbolWriter
	{
		// The ISymbolWriter interface has an `IntPtr emitter' argument which
		// seems to be a pointer an unmanaged interface containing the actual
		// symbol writer. I was unable to find any documentation about how
		// exactly this is used - but it seems to be in some proprietary,
		// undocumented DLL.
		//
		// Since I want this interface to be usable on the Windows platform as
		// well, I added this custom constructor. You should use this version
		// of `Initialize' to make sure you're actually using this implementation.
		void Initialize (string filename);

		// This is some kind of a hack - there isn't any way yet to get the
		// method name and source file back from a token.
		void OpenMethod (SymbolToken token, MethodInfo method_info, string source_file);
	}
}
