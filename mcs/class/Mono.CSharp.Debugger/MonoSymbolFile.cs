//
// System.Diagnostics.SymbolStore/MonoSymbolFile.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public class MonoSymbolFileException : Exception
	{
		public MonoSymbolFileException ()
			: base ()
		{ }

		public MonoSymbolFileException (string message, params object[] args)
			: base (String.Format (message, args))
		{ }
	}

	public class MonoSymbolFile : IDisposable
	{
		ArrayList methods = new ArrayList ();
		ArrayList sources = new ArrayList ();
		Hashtable source_hash = new Hashtable ();
		Hashtable type_hash = new Hashtable ();

		OffsetTable ot;
		int last_type_index;
		int last_method_index;

		public MonoSymbolFile ()
		{ }

		public SourceFileEntry DefineSource (string source_file)
		{
			if (reader != null)
				throw new InvalidOperationException ();

			SourceFileEntry source = (SourceFileEntry) source_hash [source_file];
			if (source == null) {
				source = new SourceFileEntry (this, source_file, sources.Count + 1);
				source_hash.Add (source_file, source);
				sources.Add (source);
			}
			return source;
		}

		internal int DefineType (Type type)
		{
			if (type_hash.Contains (type))
				return (int) type_hash [type];

			int index = ++last_type_index;
			type_hash.Add (type, index);
			return index;
		}

		internal void AddMethod (MethodEntry entry)
		{
			methods.Add (entry);
		}

		internal int GetNextTypeIndex ()
		{
			return ++last_type_index;
		}

		internal int GetNextMethodIndex ()
		{
			return ++last_method_index;
		}

		internal void WriteString (BinaryWriter bw, string text)
		{
			byte[] data = Encoding.UTF8.GetBytes (text);
			bw.Write ((int) data.Length);
			bw.Write (data);
		}

		internal string ReadString (int offset)
		{
			int old_pos = (int) reader.BaseStream.Position;
			reader.BaseStream.Position = offset;
			int length = reader.ReadInt32 ();

			byte[] data = reader.ReadBytes (length);
			string text = Encoding.UTF8.GetString (data);
			reader.BaseStream.Position = old_pos;
			return text;
		}

		void Write (BinaryWriter bw)
		{
			// Magic number and file version.
			bw.Write (OffsetTable.Magic);
			bw.Write (OffsetTable.Version);

			//
			// Offsets of file sections; we must write this after we're done
			// writing the whole file, so we just reserve the space for it here.
			//
			long offset_table_offset = bw.BaseStream.Position;
			ot.Write (bw);

			//
			// Write data sections.
			//
			ot.DataSectionOffset = (int) bw.BaseStream.Position;
			foreach (SourceFileEntry source in sources)
				source.WriteData (bw);
			ot.DataSectionSize = (int) bw.BaseStream.Position - ot.DataSectionOffset;

			//
			// Write method table.
			//
			ot.MethodTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < methods.Count; i++) {
				MethodEntry entry = (MethodEntry) methods [i];
				entry.WriteIndex (bw);
			}
			ot.MethodTableSize = (int) bw.BaseStream.Position - ot.MethodTableOffset;

			//
			// Write source table.
			//
			ot.SourceTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < sources.Count; i++) {
				SourceFileEntry source = (SourceFileEntry) sources [i];
				source.Write (bw);
			}
			ot.SourceTableSize = (int) bw.BaseStream.Position - ot.SourceTableOffset;

			//
			// Fixup offset table.
			//
			ot.TypeCount = last_type_index;
			ot.MethodCount = methods.Count;
			ot.SourceCount = sources.Count;

			//
			// Write offset table.
			//
			ot.TotalFileSize = (int) bw.BaseStream.Position;
			bw.Seek ((int) offset_table_offset, SeekOrigin.Begin);
			ot.Write (bw);
			bw.Seek (0, SeekOrigin.End);
		}

		public void WriteSymbolFile (string output_filename)
		{
			if (reader != null)
				throw new InvalidOperationException ();

			using (FileStream stream = new FileStream (output_filename, FileMode.Create))
				Write (new BinaryWriter (stream));
		}

		FileStream stream;
		BinaryReader reader;
		Hashtable method_hash;
		Hashtable source_file_hash;

		Hashtable method_name_hash;
		Hashtable source_name_hash;

		public MonoSymbolFile (string file_name)
		{
			stream = File.OpenRead (file_name);
			reader = new BinaryReader (stream);

			try {
				long magic = reader.ReadInt64 ();
				long version = reader.ReadInt32 ();
				if ((magic != OffsetTable.Magic) || (version != OffsetTable.Version))
					throw new MonoSymbolFileException ();
				ot = new OffsetTable (reader);
			} catch {
				throw new MonoSymbolFileException ();
			}

			method_hash = new Hashtable ();
			source_file_hash = new Hashtable ();
		}

		public int SourceCount {
			get { return ot.SourceCount; }
		}

		public int MethodCount {
			get { return ot.MethodCount; }
		}

		public int TypeCount {
			get { return ot.TypeCount; }
		}

		public SourceFileEntry GetSourceFile (int index)
		{
			if ((index < 1) || (index > ot.SourceCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			SourceFileEntry source = (SourceFileEntry) source_file_hash [index];
			if (source != null)
				return source;

			reader.BaseStream.Position = ot.SourceTableOffset +
				SourceFileEntry.Size * (index - 1);
			source = new SourceFileEntry (this, reader);
			source_file_hash.Add (index, source);
			return source;
		}

		public MethodEntry GetMethod (int index)
		{
			if ((index < 1) || (index > ot.MethodCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			MethodEntry entry = (MethodEntry) method_hash [index];
			if (entry != null)
				return entry;

			reader.BaseStream.Position = ot.MethodTableOffset + 8 * (index - 1);
			reader.BaseStream.Position = reader.ReadInt32 ();

			entry = new MethodEntry (this, reader);
			method_hash.Add (index, entry);
			return entry;
		}

		public int FindMethod (string full_name)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (method_name_hash == null) {
				method_name_hash = new Hashtable ();

				for (int i = 0; i < ot.MethodCount; i++) {
					reader.BaseStream.Position = ot.MethodTableOffset + 8 * i;

					int offset = reader.ReadInt32 ();
					int name_offset = reader.ReadInt32 ();
					string name = ReadString (name_offset);

					method_name_hash.Add (name, i);
				}
			}

			object value = method_name_hash [full_name];
			if (value == null)
				return -1;
			return (int) value;
		}

		public int FindSource (string file_name)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (source_name_hash == null) {
				source_name_hash = new Hashtable ();

				for (int i = 0; i < ot.SourceCount; i++) {
					SourceFileEntry source = GetSourceFile (i + 1);

					source_name_hash.Add (source.FileName, i);
				}
			}

			object value = source_name_hash [file_name];
			if (value == null)
				return -1;
			return (int) value;
		}

		internal BinaryReader BinaryReader {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				return reader;
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (stream != null) {
					stream.Close ();
					stream = null;
				}
				if (reader != null) {
					reader.Close ();
					reader = null;
				}
			}
		}
	}
}
