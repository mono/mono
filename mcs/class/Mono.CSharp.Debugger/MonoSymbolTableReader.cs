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

		public MonoSymbolTableReader (BinaryReader reader, BinaryReader address_reader)
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

			Methods = new MethodEntry [offset_table.method_count];

			for (int i = 0; i < offset_table.method_count; i++) {
				try {
					Methods [i] = new MethodEntry (reader, address_reader);
				} catch {
					throw new SymbolTableException ("Can't read method table");
				}
			}
		}
	}
}
