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

	internal class MyMemoryStream : Stream
	{
		int length;
		int real_length;
		int position;

		int chunk_size = 4096;
		ArrayList chunks = new ArrayList ();

		private struct Chunk {
			public readonly int Offset;
			public readonly int Length;
			public byte[] Buffer;

			public Chunk (int offset, int length)
			{
				this.Offset = offset;
				this.Length = length;
				this.Buffer = new Byte [length];
			}
		}

		public override long Position {
			get { return position; }

			set {
				if (value > length)
					throw new ArgumentOutOfRangeException ();

				position = (int) value;
			}
		}

		public override long Length {
			get { return length; }
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override void SetLength (long new_length)
		{
			if (new_length < length)
				throw new ArgumentException ();

			while (new_length >= real_length) {
				Chunk new_chunk = new Chunk (real_length, chunk_size);
				chunks.Add (new_chunk);
				real_length += chunk_size;
			}

			length = (int) new_length;
		}

		public override void Flush ()
		{ }

		public override long Seek (long offset, SeekOrigin origin)
		{
			int ref_point;

                        switch (origin) {
			case SeekOrigin.Begin:
				ref_point = 0;
				break;
			case SeekOrigin.Current:
				ref_point = position;
				break;
			case SeekOrigin.End:
				ref_point = length;
				break;
			default:
				throw new ArgumentException ("Invalid SeekOrigin");
                        }

                        if ((ref_point + offset < 0) || (offset > real_length))
                                throw new ArgumentOutOfRangeException ();

                        position = ref_point + (int) offset;

			return position;
		}

		Chunk FindChunk (int offset)
		{
			return (Chunk) chunks [offset / chunk_size];
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int old_count = count;

			while (count > 0) {
				Chunk chunk = FindChunk (position);
				int coffset = position - chunk.Offset;
				int rest = chunk.Length - coffset;
				int size = System.Math.Min (count, rest);

				Array.Copy (chunk.Buffer, coffset, buffer, offset, size);
				position += size;
				offset += size;
				count -= size;
			}

			return old_count;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (position + count > length)
				SetLength (position + count);

			while (count > 0) {
				Chunk chunk = FindChunk (position);
				int coffset = position - chunk.Offset;
				int rest = chunk.Length - coffset;
				int size = System.Math.Min (count, rest);

				Array.Copy (buffer, offset, chunk.Buffer, coffset, size);
				position += size;
				offset += size;
				count -= size;
			}
		}

		public byte[] GetContents ()
		{
			byte[] retval = new byte [length];
			position = 0;
			Read (retval, 0, length);
			return retval;
		}
	}

	public class MonoSymbolFile : IDisposable
	{
		ArrayList methods = new ArrayList ();
		ArrayList sources = new ArrayList ();
		Hashtable method_source_hash = new Hashtable ();
		Hashtable type_hash = new Hashtable ();

		OffsetTable ot;
		int last_type_index;
		int last_method_index;
		int last_source_index;
		int last_namespace_index;

		public MonoSymbolFile ()
		{ }

		internal int AddSource (SourceFileEntry source)
		{
			sources.Add (source);
			return ++last_source_index;
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

		internal int GetNextNamespaceIndex ()
		{
			return ++last_namespace_index;
		}

		internal void WriteString (BinaryWriter bw, string text)
		{
			byte[] data = Encoding.UTF8.GetBytes (text);
			bw.Write ((int) data.Length);
			bw.Write (data);
			StringSize += data.Length;
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

		public byte[] CreateSymbolFile ()
		{
			if (reader != null)
				throw new InvalidOperationException ();

			using (MyMemoryStream stream = new MyMemoryStream ()) {
				Write (new BinaryWriter (stream));
				Console.WriteLine ("WROTE SYMFILE: {0} sources, {1} methods, {2} types, " +
						   "{3} line numbers, {4} locals, {5} namespaces, " +
						   "{6} bytes of string data",
						   SourceCount, MethodCount, TypeCount, LineNumberCount,
						   LocalCount, NamespaceCount, StringSize);
				Console.WriteLine (ot);
				return stream.GetContents ();
			}
		}

		Assembly assembly;
		BinaryReader reader;
		Hashtable method_hash;
		Hashtable source_file_hash;

		Hashtable method_token_hash;
		Hashtable method_name_hash;
		Hashtable source_name_hash;

		protected MonoSymbolFile (Assembly assembly, Stream stream)
		{
			this.assembly = assembly;

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

		public static MonoSymbolFile ReadSymbolFile (Assembly assembly)
		{
			Stream stream = assembly.GetManifestResourceStream ("MonoSymbolFile");
			if (stream == null)
				return null;

			return new MonoSymbolFile (assembly, stream);
		}

		public Assembly Assembly {
			get { return assembly; }
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

		public int NamespaceCount {
			get { return last_namespace_index; }
		}

		internal int LineNumberCount = 0;
		internal int LocalCount = 0;
		internal int StringSize = 0;

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

		public SourceFileEntry[] Sources {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				SourceFileEntry[] retval = new SourceFileEntry [SourceCount];
				for (int i = 0; i < SourceCount; i++)
					retval [i] = GetSourceFile (i + 1);
				return retval;
			}
		}

		public MethodIndexEntry GetMethodIndexEntry (int index)
		{
			int old_pos = (int) reader.BaseStream.Position;
			reader.BaseStream.Position = ot.MethodTableOffset +
				MethodIndexEntry.Size * (index - 1);
			MethodIndexEntry ie = new MethodIndexEntry (reader);
			reader.BaseStream.Position = old_pos;
			return ie;
		}

		public MethodEntry GetMethodByToken (int token)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (method_token_hash == null) {
				method_token_hash = new Hashtable ();

				for (int i = 0; i < MethodCount; i++) {
					MethodIndexEntry ie = GetMethodIndexEntry (i);

					method_token_hash.Add (ie.Token, i);
				}
			}

			object value = method_token_hash [token];
			if (value == null)
				return null;

			return GetMethod ((int) value);
		}

		public MethodEntry GetMethod (MethodBase method)
		{
			if (reader == null)
				throw new InvalidOperationException ();
			int token = assembly.MonoDebugger_GetMethodToken (method);
			return GetMethodByToken (token);
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

			MethodIndexEntry ie = GetMethodIndexEntry (index);
			reader.BaseStream.Position = ie.FileOffset;

			entry = new MethodEntry (this, reader, index);
			method_hash.Add (index, entry);
			return entry;
		}

		public MethodEntry[] Methods {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				MethodEntry[] retval = new MethodEntry [MethodCount];
				for (int i = 0; i < MethodCount; i++)
					retval [i] = GetMethod (i + 1);
				return retval;
			}
		}

		public MethodSourceEntry GetMethodSource (int index)
		{
			if ((index < 1) || (index > ot.MethodCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			object entry = method_source_hash [index];
			if (entry != null)
				return (MethodSourceEntry) entry;

			MethodEntry method = GetMethod (index);
			foreach (MethodSourceEntry source in method.SourceFile.Methods) {
				if (source.Index == index) {
					method_source_hash.Add (index, source);
					return source;
				}
			}

			throw new MonoSymbolFileException ("Internal error.");
		}

		public int FindMethod (string full_name)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (method_name_hash == null) {
				method_name_hash = new Hashtable ();

				for (int i = 0; i < ot.MethodCount; i++) {
					MethodIndexEntry ie = GetMethodIndexEntry (i);
					string name = ReadString (ie.FullNameOffset);

					method_name_hash.Add (name, i + 1);
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
				if (reader != null) {
					reader.Close ();
					reader = null;
				}
			}
		}
	}
}
