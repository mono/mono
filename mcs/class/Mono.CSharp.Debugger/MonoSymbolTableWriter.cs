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
		public const long magic_id = 0x45e82623fd7fa614;
		public const int  symbol_table_version = 16;

		BinaryWriter bw;

		public MonoSymbolTableWriter (string output_filename)
		{
			FileStream stream = new FileStream (output_filename, FileMode.Create);
			bw = new BinaryWriter (stream);
		}

		public void WriteSymbolTable (IMonoSymbolWriter symwriter)
		{
			// Magic number and file version.
			bw.Write (magic_id);
			bw.Write (symbol_table_version);

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

			ot.line_number_table_offset = (uint) bw.BaseStream.Position;
			foreach (ISourceMethod method in symwriter.Methods) {
				if (method.Start == null || method.Start.Row == 0)
					continue;

				methods.Add (method, (uint) bw.BaseStream.Position);

				foreach (ISourceLine line in method.Lines) {
					LineNumberEntry lne = new LineNumberEntry (line);
					lne.Write (bw);
				}

				LineNumberEntry.Null.Write (bw);
			}
			ot.line_number_table_size = (uint) bw.BaseStream.Position -
				ot.line_number_table_offset;

			//
			// Write method table
			//
			ot.method_table_offset = (uint) bw.BaseStream.Position;
			foreach (ISourceMethod method in methods.Keys) {
				MethodEntry entry = new MethodEntry (
					(uint) method.Token, (uint) sources [method.SourceFile],
					(uint) methods [method], (uint) method.Start.Row);
				entry.Write (bw);
			}
			ot.method_table_size = (uint) bw.BaseStream.Position -  ot.method_table_offset;

			//
			// Write offset table
			//
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
