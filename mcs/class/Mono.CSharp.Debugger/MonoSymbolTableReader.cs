using System;
using System.IO;
using System.Collections;

namespace Mono.CSharp.Debugger
{
	public class SymbolTableException : Exception
	{
		public SymbolTableException ()
			: base ("Invalid mono symbol table")
		{ }

		public SymbolTableException (string message)
			: base (message)
		{ }
	}

	public class MonoSymbolTableReader
	{
		public readonly MethodEntry[] Methods;

		public MonoSymbolTableReader (BinaryReader reader)
		{
			//
			// Read the offset table.
			//
			OffsetTable offset_table;
			try {
				long magic = reader.ReadInt64 ();
				uint version = reader.ReadUInt32 ();
				if ((magic != OffsetTable.Magic) || (version != OffsetTable.Version))
					throw new SymbolTableException ();
				offset_table = new OffsetTable (reader);
			} catch {
				throw new SymbolTableException ();
			}

			//
			// Read the method table.
			//
			reader.BaseStream.Position = offset_table.method_table_offset;
			long end = reader.BaseStream.Position + offset_table.method_table_size;

			ArrayList methods = new ArrayList ();

			while (reader.BaseStream.Position < end) {
				try {
					methods.Add (new MethodEntry (reader));
				} catch {
					throw new SymbolTableException ("Can't read method table");
				}
			}

			Methods = new MethodEntry [methods.Count];
			methods.CopyTo (Methods);
		}
	}
}
