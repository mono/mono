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
		MethodInfo define_namespace;
		MethodInfo open_method;

		protected SymbolWriter (ISymbolWriter symwriter)
		{
			this.symwriter = symwriter;
		}

		bool Initialize ()
		{
			Type type = symwriter.GetType ();
			define_namespace = type.GetMethod ("DefineNamespace", new Type[] {
				typeof (string), typeof (ISymbolDocumentWriter),
				typeof (string []), typeof (int) });
			if (define_namespace == null)
				return false;

			open_method = type.GetMethod ("OpenMethod", new Type[] {
				typeof (ISymbolDocumentWriter), typeof (int), typeof (int),
				typeof (int), typeof (int), typeof (MethodBase), typeof (int) });
			if (open_method == null)
				return false;

			Location.DefineSymbolDocuments (this);
			Namespace.DefineNamespaces (this);

			return true;
		}

		public ISymbolDocumentWriter DefineDocument (string path)
		{
			return symwriter.DefineDocument (
				path, SymLanguageType.CSharp, SymLanguageVendor.Microsoft,
				SymDocumentType.Text);
		}

		public int DefineNamespace (string name, SourceFile file, string[] using_list, int parent)
		{
			return (int) define_namespace.Invoke (symwriter, new object[] {
				name, file.SymbolDocument, using_list, parent });
		}

		public void OpenMethod (TypeContainer parent, MethodBase method, Location start, Location end)
		{
			int ns_id = parent.NamespaceEntry.SymbolFileID;
			open_method.Invoke (symwriter, new object[] {
				start.SymbolDocument, start.Row, 0, end.Row, 0, method, ns_id });
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
