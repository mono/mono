//
// System.Diagnostics.SymbolStore/IMonoBinaryReader.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
	
namespace Mono.CSharp.Debugger
{
	public interface IMonoBinaryReader
	{
		long Position {
			get; set;
		}

		int ReadInt32 ();

		long ReadInt64 ();

		byte[] ReadBuffer (int size);
	}
}
