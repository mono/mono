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
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public interface IMonoSymbolWriter : System.Diagnostics.SymbolStore.ISymbolWriter
	{
		void Initialize (string assembly_filename, string filename, string[] args);
	}

	internal enum SourceOffsetType
	{
		OFFSET_NONE,
		OFFSET_IL,
		OFFSET_LOCAL,
		OFFSET_PARAMETER
	}

	internal interface ITypeHandle
	{
		string Name {
			get;
		}

		Type Type {
			get;
		}

		int Token {
			get;
		}
	}
}
