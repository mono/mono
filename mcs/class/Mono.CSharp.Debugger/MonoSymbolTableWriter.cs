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
		string output_filename;
		Hashtable type_hash;
		int last_type_index;

		public MonoSymbolTableWriter (string output_filename)
		{
			this.output_filename = output_filename;
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

		public void WriteSymbolTable (MonoSymbolWriter symwriter)
		{
			MonoSymbolFile file = new MonoSymbolFile ();

			foreach (SourceMethod method in symwriter.Methods) {
				if ((method.Start == null) || (method.SourceFile == null)) {
					Console.WriteLine ("INGORING METHOD: {0}", method);
					continue;
				}

				SourceFileEntry source = file.DefineSource (method.SourceFile.FileName);

				int count = method.Lines.Length;
				LineNumberEntry[] lines = new LineNumberEntry [count];
				for (int i = 0; i < count; i++)
					lines [i] = new LineNumberEntry (method.Lines [i]);

				source.DefineMethod (method.MethodBase, method.Token, method.Locals,
						     lines, method.Start.Row, method.End.Row);
			}

			file.WriteSymbolFile (output_filename);
		}
	}
}
