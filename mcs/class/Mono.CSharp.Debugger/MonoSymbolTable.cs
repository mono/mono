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
		public const uint Version = 16;
		public const long Magic   = 0x45e82623fd7fa614;

		public uint total_file_size;
		public uint source_table_offset;
		public uint source_table_size;
		public uint method_table_offset;
		public uint method_table_size;
		public uint line_number_table_offset;
		public uint line_number_table_size;

		public OffsetTable (BinaryReader reader)
		{
			total_file_size = reader.ReadUInt32 ();
			source_table_offset = reader.ReadUInt32 ();
			source_table_size = reader.ReadUInt32 ();
			method_table_offset = reader.ReadUInt32 ();
			method_table_size = reader.ReadUInt32 ();
			line_number_table_offset = reader.ReadUInt32 ();
			line_number_table_size = reader.ReadUInt32 ();
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (total_file_size);
			bw.Write (source_table_offset);
			bw.Write (source_table_size);
			bw.Write (method_table_offset);
			bw.Write (method_table_size);
			bw.Write (line_number_table_offset);
			bw.Write (line_number_table_size);
		}
	}

	public struct LineNumberEntry
	{
		public uint Row;
		public uint Offset;
		public uint Address;

		public static LineNumberEntry Null = new LineNumberEntry (0, 0);

		public LineNumberEntry (uint row, uint offset)
		{
			this.Row = row;
			this.Offset = offset;
			this.Address = 0;
		}

		public LineNumberEntry (ISourceLine line)
			: this ((uint) line.Row, (uint) line.Offset)
		{ }		

		public LineNumberEntry (BinaryReader reader)
		{
			Row = reader.ReadUInt32 ();
			Offset = reader.ReadUInt32 ();
			Address = reader.ReadUInt32 ();
		}

		public bool IsNull {
			get {
				return Row == 0;
			}
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
			bw.Write (Address);
		}

		public override string ToString ()
		{
			return String.Format ("[Line {0}:{1}:{2}]", Row, Offset, Address);
		}
	}

	public struct MethodEntry
	{
		public uint Token;
		public uint SourceFileOffset;
		public uint LineNumberTableOffset;
		public uint StartRow;
		public long Address;

		public readonly string SourceFile;
		public readonly LineNumberEntry[] LineNumbers;

		public MethodEntry (BinaryReader reader)
		{
			Token = reader.ReadUInt32 ();
			SourceFileOffset = reader.ReadUInt32 ();
			LineNumberTableOffset = reader.ReadUInt32 ();
			StartRow = reader.ReadUInt32 ();
			Address = reader.ReadInt64 ();

			long old_pos = reader.BaseStream.Position;
			reader.BaseStream.Position = LineNumberTableOffset;

			ArrayList lines = new ArrayList ();

			while (true) {
				LineNumberEntry lne = new LineNumberEntry (reader);
				if (lne.IsNull)
					break;
				lines.Add (lne);
			}

			reader.BaseStream.Position = SourceFileOffset;
			SourceFile = reader.ReadString ();
			reader.BaseStream.Position = old_pos;

			LineNumbers = new LineNumberEntry [lines.Count];
			lines.CopyTo (LineNumbers);
		}

		public MethodEntry (uint token, uint sf_offset, uint lnt_offset, uint row)
		{
			this.Token = token;
			this.SourceFileOffset = sf_offset;
			this.LineNumberTableOffset = lnt_offset;
			this.StartRow = row;
			this.Address = 0;
			this.SourceFile = null;
			this.LineNumbers = new LineNumberEntry [0];
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (Token);
			bw.Write (SourceFileOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (StartRow);
			bw.Write (Address);
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4}]",
					      Token, SourceFileOffset, LineNumberTableOffset,
					      StartRow, Address);
		}
	}
}
