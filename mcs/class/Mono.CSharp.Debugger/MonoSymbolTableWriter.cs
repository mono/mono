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
	internal class MonoSymbolTableWriter : IDisposable
	{
		BinaryWriter bw;

		public MonoSymbolTableWriter (string output_filename)
		{
			FileStream stream = new FileStream (output_filename, FileMode.Create);
			bw = new BinaryWriter (stream);
		}

		public void WriteSymbolTable (MonoSymbolWriter symwriter)
		{
			// Magic number and file version.
			bw.Write (OffsetTable.Magic);
			bw.Write (OffsetTable.Version);

			OffsetTable ot = new OffsetTable ();

			//
			// Offsets of file sections; we must write this after we're done
			// writing the whole file, so we just reserve the space for it here.
			//
			long offset_table_offset = bw.BaseStream.Position;
			ot.Write (bw);

			//
			// Write source file table.
			//
			Hashtable sources = new Hashtable ();

			ot.source_table_offset = (int) bw.BaseStream.Position;
			foreach (ISourceFile source in symwriter.Sources) {
				if (sources.ContainsKey (source))
					continue;

				sources.Add (source, (int) bw.BaseStream.Position);
				byte[] file_name = Encoding.UTF8.GetBytes (source.FileName);
				bw.Write ((int) file_name.Length);
				bw.Write (file_name);
			}
			ot.source_table_size = (int) bw.BaseStream.Position - ot.source_table_offset;

			//
			// Write line number table
			//
			Hashtable methods = new Hashtable ();

			int address_table_size = 0;

			ot.line_number_table_offset = (int) bw.BaseStream.Position;
			foreach (ISourceMethod method in symwriter.Methods) {
				if (method.Start == null || method.Start.Row == 0)
					continue;

				int count = method.Lines.Length;
				LineNumberEntry[] lines = new LineNumberEntry [count];

				int pos = (int) bw.BaseStream.Position;

				int address_table_offset = address_table_size;
				int my_size = (int) (MethodAddress.Size + count * 8);
				address_table_size += my_size;

				int num_params = method.Parameters.Length;
				int num_locals = method.Locals.Length;

				int variable_table_offset = address_table_size;
				int my_size2 = VariableInfo.Size * (num_params + num_locals);
				if (!method.MethodBase.IsStatic)
					my_size2 += VariableInfo.Size;
				address_table_size += my_size2;

				for (int i = 0; i < count; i++) {
					lines [i] = new LineNumberEntry (method.Lines [i]);
					lines [i].Write (bw);
				}

				MethodEntry entry = new MethodEntry (
					(int) method.Token, (int) sources [method.SourceFile],
					method.SourceFile.FileName, !method.MethodBase.IsStatic,
					num_params, num_locals, lines, pos, address_table_offset,
					variable_table_offset, my_size + my_size2,
					(int) method.Start.Row, (int) method.End.Row);

				methods.Add (method, entry);

			}
			ot.line_number_table_size = (int) bw.BaseStream.Position -
				ot.line_number_table_offset;

			//
			// Write method table
			//
			ot.method_count = methods.Count;
			ot.method_table_offset = (int) bw.BaseStream.Position;
			foreach (MethodEntry entry in methods.Values)
				entry.Write (bw);
			ot.method_table_size = (int) bw.BaseStream.Position -  ot.method_table_offset;

			//
			// Write offset table
			//
			ot.address_table_size = address_table_size;
			ot.total_file_size = (int) bw.BaseStream.Position;
			bw.Seek ((int) offset_table_offset, SeekOrigin.Begin);
			ot.Write (bw);
			bw.Seek (0, SeekOrigin.End);
		}

		void IDisposable.Dispose() {
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
			{
				bw.Close ();
			}
		}
	}
}
