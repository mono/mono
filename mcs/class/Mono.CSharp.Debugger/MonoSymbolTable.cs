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
		public uint source_table_offset;
		public uint source_table_size;
		public uint method_table_offset;
		public uint method_table_size;
		public uint line_number_table_offset;
		public uint line_number_table_size;

		public void Write (BinaryWriter bw)
		{
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

		public void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
			bw.Write (Address);
		}
	}

	public struct MethodEntry
	{
		public uint Token;
		public uint SourceFileOffset;
		public uint LineNumberTableOffset;
		public uint StartRow;
		public long Address;

		public MethodEntry (uint token, uint sf_offset, uint lnt_offset, uint row)
		{
			this.Token = token;
			this.SourceFileOffset = sf_offset;
			this.LineNumberTableOffset = lnt_offset;
			this.StartRow = row;
			this.Address = 0;
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (Token);
			bw.Write (SourceFileOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (StartRow);
			bw.Write (Address);
		}
	}
}
