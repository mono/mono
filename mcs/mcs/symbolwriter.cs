//
// symbolwriter.cs: The symbol writer
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;

namespace Mono.CSharp {
	public class SymbolWriter {
		ISymbolWriter symwriter;

		protected SymbolWriter (ISymbolWriter symwriter)
		{
			this.symwriter = symwriter;
		}

		bool Initialize ()
		{
			Location.DefineSymbolDocuments (this);

			return true;
		}

		public ISymbolDocumentWriter DefineDocument (string path)
		{
			return symwriter.DefineDocument (
				path, SymLanguageType.CSharp, SymLanguageVendor.Microsoft,
				SymDocumentType.Text);
		}

		public void OpenMethod (MethodToken token, Location start, Location end)
		{
			symwriter.OpenMethod (new SymbolToken (token.Token));
			symwriter.SetMethodSourceRange (
				start.SymbolDocument, start.Row, 0, end.SymbolDocument, end.Row, 0);
		}

		public void CloseMethod ()
		{
			symwriter.CloseMethod ();
		}

		public static SymbolWriter GetSymbolWriter (ModuleBuilder module)
		{
			ISymbolWriter symwriter = module.GetSymWriter ();

			if (symwriter == null)
				return null;

			SymbolWriter writer = new SymbolWriter (symwriter);
			if (!writer.Initialize ())
				return null;

			return writer;
		}
	}
}
