//
// System.Diagnostics.SymbolStore/MonoSymbolTableWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	internal class MonoSymbolTableWriter
	{
		Hashtable type_hash;
		int last_type_index;

		public MonoSymbolTableWriter ()
		{
			type_hash = new Hashtable ();
		}

		int GetTypeIndex (Type type)
		{
			if (type_hash.Contains (type))
				return (int) type_hash [type];

			int index = ++last_type_index;
			type_hash.Add (type, index);
			return index;
		}

		public byte[] CreateSymbolTable (MonoSymbolWriter symwriter)
		{
			MonoSymbolFile file = new MonoSymbolFile ();

			foreach (SourceMethod method in symwriter.Methods) {
				if (!method.HasSource) {
					Console.WriteLine ("INGORING METHOD: {0}", method);
					continue;
				}

				SourceFileEntry source = file.DefineSource (method.SourceFile.FileName);

				source.DefineMethod (method.MethodBase, method.Token, method.Locals,
						     method.Lines, method.Start.Row, method.End.Row);
			}

			return file.CreateSymbolFile ();
		}
	}
}
