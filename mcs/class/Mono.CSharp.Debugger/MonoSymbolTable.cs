//
// System.Diagnostics.SymbolStore/MonoSymbolTable.cs
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
	public struct OffsetTable
	{
		public const uint Version = 18;
		public const long Magic   = 0x45e82623fd7fa614;

		public uint total_file_size;
		public uint source_table_offset;
		public uint source_table_size;
		public uint method_count;
		public uint method_table_offset;
		public uint method_table_size;
		public uint line_number_table_offset;
		public uint line_number_table_size;
		public uint address_table_size;

		public OffsetTable (BinaryReader reader)
		{
			total_file_size = reader.ReadUInt32 ();
			source_table_offset = reader.ReadUInt32 ();
			source_table_size = reader.ReadUInt32 ();
			method_count = reader.ReadUInt32 ();
			method_table_offset = reader.ReadUInt32 ();
			method_table_size = reader.ReadUInt32 ();
			line_number_table_offset = reader.ReadUInt32 ();
			line_number_table_size = reader.ReadUInt32 ();
			address_table_size = reader.ReadUInt32 ();
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (total_file_size);
			bw.Write (source_table_offset);
			bw.Write (source_table_size);
			bw.Write (method_count);
			bw.Write (method_table_offset);
			bw.Write (method_table_size);
			bw.Write (line_number_table_offset);
			bw.Write (line_number_table_size);
			bw.Write (address_table_size);
		}
	}

	public struct LineNumberEntry
	{
		public readonly uint Row;
		public readonly uint Offset;

		public LineNumberEntry (uint row, uint offset)
		{
			this.Row = row;
			this.Offset = offset;
		}

		internal LineNumberEntry (ISourceLine line)
			: this ((uint) line.Row, (uint) line.Offset)
		{ }

		public LineNumberEntry (BinaryReader reader)
		{
			Row = reader.ReadUInt32 ();
			Offset = reader.ReadUInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
		}

		public override string ToString ()
		{
			return String.Format ("[Line {0}:{1}]", Row, Offset);
		}
	}

	public class MethodAddress
	{
		public readonly ulong StartAddress;
		public readonly ulong EndAddress;
		public readonly uint[] LineAddresses;

		public static int Size {
			get {
				return 3 * 8;
			}
		}

		public MethodAddress (MethodEntry entry, BinaryReader reader)
		{
			StartAddress = reader.ReadUInt64 ();
			EndAddress = reader.ReadUInt64 ();
			LineAddresses = new uint [entry.NumLineNumbers];
			for (int i = 0; i < entry.NumLineNumbers; i++)
				LineAddresses [i] = reader.ReadUInt32 ();
		}

		public override string ToString ()
		{
			return String.Format ("[Address {0:x}:{1:x}]",
					      StartAddress, EndAddress);
		}
	}

	public class MethodEntry
	{
		public readonly uint Token;
		public readonly uint StartRow;
		public readonly uint EndRow;
		public readonly uint NumLineNumbers;

		public readonly uint SourceFileOffset;
		public readonly uint LineNumberTableOffset;
		public readonly uint AddressTableOffset;
		public readonly uint AddressTableSize;

		public readonly string SourceFile = null;
		public readonly LineNumberEntry[] LineNumbers = null;
		public readonly MethodAddress Address = null;

		public MethodEntry (BinaryReader reader, BinaryReader address_reader)
		{
			Token = reader.ReadUInt32 ();
			StartRow = reader.ReadUInt32 ();
			EndRow = reader.ReadUInt32 ();
			NumLineNumbers = reader.ReadUInt32 ();

			SourceFileOffset = reader.ReadUInt32 ();
			LineNumberTableOffset = reader.ReadUInt32 ();
			AddressTableOffset = reader.ReadUInt32 ();
			AddressTableSize = reader.ReadUInt32 ();

			if (SourceFileOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = SourceFileOffset;
				SourceFile = reader.ReadString ();
				reader.BaseStream.Position = old_pos;
			}

			// Console.WriteLine ("METHOD ENTRY: " + this);

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++) {
					LineNumbers [i] = new LineNumberEntry (reader);
					// Console.WriteLine ("LINE: " + LineNumbers [i]);
				}

				reader.BaseStream.Position = old_pos;
			}

			if (AddressTableSize != 0) {
				long old_pos = address_reader.BaseStream.Position;
				address_reader.BaseStream.Position = AddressTableOffset;
				uint is_valid = address_reader.ReadUInt32 ();
				if (is_valid != 0) {
					Address = new MethodAddress (this, address_reader);
					// Console.WriteLine ("ADDRESS: " + Address);
				}
				address_reader.BaseStream.Position = old_pos;
			}
		}

		internal MethodEntry (uint token, uint sf_offset, string source_file,
				      LineNumberEntry[] lines, uint lnt_offset,
				      uint addrtab_offset, uint addrtab_size,
				      uint start_row, uint end_row)
		{
			this.Token = token;
			this.StartRow = start_row;
			this.EndRow = end_row;
			this.NumLineNumbers = (uint) lines.Length;
			this.SourceFileOffset = sf_offset;
			this.LineNumberTableOffset = lnt_offset;
			this.AddressTableOffset = addrtab_offset;
			this.AddressTableSize = addrtab_size;
			this.SourceFile = source_file;
			this.LineNumbers = lines;
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (NumLineNumbers);
			bw.Write (SourceFileOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (AddressTableOffset);
			bw.Write (AddressTableSize);
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4} - {5}:{6}:{7}:{8}]",
					      Token, SourceFile, StartRow, EndRow, NumLineNumbers,
					      SourceFileOffset, LineNumberTableOffset, AddressTableOffset,
					      AddressTableSize);
		}
	}
}
