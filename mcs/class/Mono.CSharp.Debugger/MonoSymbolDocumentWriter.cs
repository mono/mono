//
// System.Diagnostics.SymbolStore/MonoSymbolDocumentWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is the default implementation of the
// System.Diagnostics.SymbolStore.ISymbolDocumentWriter interface.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.IO;
	
namespace Mono.CSharp.Debugger
{

	public class MonoSymbolDocumentWriter : ISymbolDocumentWriter
	{
		protected string url;

		//
		// Constructor
		//
		public MonoSymbolDocumentWriter (string url)
		{
			this.url = url;
		}

		public string FileName {
			get {
				return url;
			}
		}

		//
		// Interface ISymbolDocumentWriter
		//

		public void SetCheckSum (Guid algorithmId, byte[] checkSum)
		{
			throw new NotSupportedException ();
		}

		public void SetSource (byte[] source)
		{
			throw new NotSupportedException ();
		}
	}
}
