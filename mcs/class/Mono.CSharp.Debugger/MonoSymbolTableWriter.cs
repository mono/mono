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
	public class MonoSymbolTableWriter : IDisposable
	{
		BinaryWriter bw;

		public MonoSymbolTableWriter (string output_filename)
		{
			FileStream stream = new FileStream (output_filename, FileMode.Create);
			bw = new BinaryWriter (stream);
		}

		public void WriteSymbolTable (IMonoSymbolWriter symwriter)
		{
			// Magic number and file version.
			bw.Write (OffsetTable.Magic);
			bw.Write (OffsetTable.Version);

			OffsetTable ot;

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
			int source_idx = 0;

			ot.source_table_offset = (uint) bw.BaseStream.Position;
			foreach (ISourceFile source in symwriter.Sources) {
				if (sources.ContainsKey (source))
					continue;

				sources.Add (source, (uint) bw.BaseStream.Position);
				bw.Write (source.FileName);
			}
			ot.source_table_size = (uint) bw.BaseStream.Position - ot.source_table_offset;

			//
			// Write line number table
			//
			Hashtable methods = new Hashtable ();

			uint address_table_size = 0;

			ot.line_number_table_offset = (uint) bw.BaseStream.Position;
			foreach (ISourceMethod method in symwriter.Methods) {
				if (method.Start == null || method.Start.Row == 0)
					continue;

				int count = method.Lines.Length;
				LineNumberEntry[] lines = new LineNumberEntry [count];

				uint pos = (uint) bw.BaseStream.Position;

				uint address_table_offset = address_table_size;
				uint my_size = (uint) (MethodAddress.Size + count * 8);
				address_table_size += my_size;

				for (int i = 0; i < count; i++) {
					lines [i] = new LineNumberEntry (method.Lines [i]);
					lines [i].Write (bw);
				}

				MethodEntry entry = new MethodEntry (
					(uint) method.Token, (uint) sources [method.SourceFile],
					method.SourceFile.FileName, lines, pos, address_table_offset,
					my_size, (uint) method.Start.Row, (uint) method.End.Row);

				methods.Add (method, entry);

			}
			ot.line_number_table_size = (uint) bw.BaseStream.Position -
				ot.line_number_table_offset;

			//
			// Write method table
			//
			ot.method_count = (uint) methods.Count;
			ot.method_table_offset = (uint) bw.BaseStream.Position;
			foreach (MethodEntry entry in methods.Values)
				entry.Write (bw);
			ot.method_table_size = (uint) bw.BaseStream.Position -  ot.method_table_offset;

			//
			// Write offset table
			//
			ot.address_table_size = address_table_size;
			ot.total_file_size = (uint) bw.BaseStream.Position;
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
