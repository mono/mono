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
		public const int Version = 25;
		public const long Magic   = 0x45e82623fd7fa614;

		public int total_file_size;
		public int source_table_offset;
		public int source_table_size;
		public int method_count;
		public int method_table_offset;
		public int method_table_size;
		public int line_number_table_offset;
		public int line_number_table_size;
		public int local_variable_table_offset;
		public int local_variable_table_size;
		public int type_count;
		public int type_index_table_offset;
		public int type_index_table_size;
		public int address_table_size;

		public OffsetTable (IMonoBinaryReader reader)
		{
			total_file_size = reader.ReadInt32 ();
			source_table_offset = reader.ReadInt32 ();
			source_table_size = reader.ReadInt32 ();
			method_count = reader.ReadInt32 ();
			method_table_offset = reader.ReadInt32 ();
			method_table_size = reader.ReadInt32 ();
			line_number_table_offset = reader.ReadInt32 ();
			line_number_table_size = reader.ReadInt32 ();
			local_variable_table_offset = reader.ReadInt32 ();
			local_variable_table_size = reader.ReadInt32 ();
			type_count = reader.ReadInt32 ();
			type_index_table_offset = reader.ReadInt32 ();
			type_index_table_size = reader.ReadInt32 ();
			address_table_size = reader.ReadInt32 ();
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
			bw.Write (local_variable_table_offset);
			bw.Write (local_variable_table_size);
			bw.Write (type_count);
			bw.Write (type_index_table_offset);
			bw.Write (type_index_table_size);
			bw.Write (address_table_size);
		}
	}

	public struct LineNumberEntry
	{
		public readonly int Row;
		public readonly int Offset;

		public LineNumberEntry (int row, int offset)
		{
			this.Row = row;
			this.Offset = offset;
		}

		internal LineNumberEntry (SourceLine line)
			: this (line.Row, line.Offset)
		{ }

		public LineNumberEntry (IMonoBinaryReader reader)
		{
			Row = reader.ReadInt32 ();
			Offset = reader.ReadInt32 ();
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

	public struct LocalVariableEntry
	{
		public readonly string Name;
		public readonly FieldAttributes Attributes;
		public readonly byte[] Signature;

		public LocalVariableEntry (string Name, FieldAttributes Attributes, byte[] Signature)
		{
			this.Name = Name;
			this.Attributes = Attributes;
			this.Signature = Signature;
		}

		public LocalVariableEntry (IMonoBinaryReader reader)
		{
			int name_length = reader.ReadInt32 ();
			byte[] name = reader.ReadBuffer (name_length);
			Name = Encoding.UTF8.GetString (name);
			Attributes = (FieldAttributes) reader.ReadInt32 ();
			int sig_length = reader.ReadInt32 ();
			Signature = reader.ReadBuffer (sig_length);
		}

		internal void Write (BinaryWriter bw)
		{
			byte[] name = Encoding.UTF8.GetBytes (Name);
			bw.Write ((int) name.Length);
			bw.Write (name);
			bw.Write ((int) Attributes);
			bw.Write ((int) Signature.Length);
			bw.Write (Signature);
		}

		public override string ToString ()
		{
			return String.Format ("[LocalVariable {0}:{1}]", Name, Attributes);
		}
	}

	public class VariableInfo
	{
		public readonly int Index;
		public readonly int Offset;
		public readonly int Size;
		public readonly AddressMode Mode;
		public readonly int BeginScope;
		public readonly int EndScope;

		public enum AddressMode : long
		{
			Stack		= 0,
			Register	= 0x10000000,
			TwoRegisters	= 0x20000000
		}

		const long AddressModeFlags = 0xf0000000;

		public static int StructSize {
			get {
				return 20;
			}
		}

		public VariableInfo (IMonoBinaryReader reader)
		{
			Index = reader.ReadInt32 ();
			Offset = reader.ReadInt32 ();
			Size = reader.ReadInt32 ();
			BeginScope = reader.ReadInt32 ();
			EndScope = reader.ReadInt32 ();

			Mode = (AddressMode) (Index & AddressModeFlags);
			Index = (int) ((long) Index & ~AddressModeFlags);

			Console.WriteLine (this);
		}

		public override string ToString ()
		{
			return String.Format ("[VariableInfo {0}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}]",
					      Mode, Index, Offset, Size, BeginScope, EndScope);
		}
	}

	public class MethodAddress
	{
		public readonly long StartAddress;
		public readonly long EndAddress;
		public readonly int[] LineAddresses;

		public static int Size {
			get {
				return 20;
			}
		}

		public MethodAddress (MethodEntry entry, IMonoBinaryReader reader)
		{
			StartAddress = reader.ReadInt64 ();
			EndAddress = reader.ReadInt64 ();
			LineAddresses = new int [entry.NumLineNumbers];
			for (int i = 0; i < entry.NumLineNumbers; i++)
				LineAddresses [i] = reader.ReadInt32 ();
		}

		public override string ToString ()
		{
			return String.Format ("[Address {0:x}:{1:x}]",
					      StartAddress, EndAddress);
		}
	}

	public class MethodEntry
	{
		public readonly int Token;
		public readonly int StartRow;
		public readonly int EndRow;
		public readonly int NumLineNumbers;
		public readonly int ThisTypeIndex;
		public readonly int NumParameters;
		public readonly int NumLocals;

		public int TypeIndexTableOffset;
		public int LocalVariableTableOffset;
		public readonly int SourceFileOffset;
		public readonly int LineNumberTableOffset;
		public readonly int AddressTableOffset;
		public readonly int VariableTableOffset;
		public readonly int AddressTableSize;

		public readonly string SourceFile = null;
		public readonly LineNumberEntry[] LineNumbers = null;
		public readonly MethodAddress Address = null;
		public readonly VariableInfo ThisVariable = null;
		public readonly VariableInfo[] ParameterVarInfo = null;
		public readonly VariableInfo[] LocalVarInfo = null;
		public readonly int[] ParamTypeIndices = null;
		public readonly int[] LocalTypeIndices = null;
		public readonly LocalVariableEntry[] Locals = null;

		public static int Size
		{
			get {
				return 56;
			}
		}

		public MethodEntry (IMonoBinaryReader reader, IMonoBinaryReader address_reader)
		{
			Token = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
			ThisTypeIndex = reader.ReadInt32 ();
			NumParameters = reader.ReadInt32 ();
			NumLocals = reader.ReadInt32 ();

			NumLineNumbers = reader.ReadInt32 ();
			TypeIndexTableOffset = reader.ReadInt32 ();
			LocalVariableTableOffset = reader.ReadInt32 ();
			SourceFileOffset = reader.ReadInt32 ();
			LineNumberTableOffset = reader.ReadInt32 ();
			AddressTableOffset = reader.ReadInt32 ();
			VariableTableOffset = reader.ReadInt32 ();
			AddressTableSize = reader.ReadInt32 ();

			if (SourceFileOffset != 0) {
				long old_pos = reader.Position;
				reader.Position = SourceFileOffset;
				int source_file_length = reader.ReadInt32 ();
				byte[] source_file = reader.ReadBuffer (source_file_length);
				SourceFile = Encoding.UTF8.GetString (source_file);
				reader.Position = old_pos;
			}

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.Position;
				reader.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++)
					LineNumbers [i] = new LineNumberEntry (reader);

				reader.Position = old_pos;
			}

			if (LocalVariableTableOffset != 0) {
				long old_pos = reader.Position;
				reader.Position = LocalVariableTableOffset;

				Locals = new LocalVariableEntry [NumLocals];

				for (int i = 0; i < NumLocals; i++)
					Locals [i] = new LocalVariableEntry (reader);

				reader.Position = old_pos;
			}

			if (AddressTableSize != 0) {
				long old_pos = address_reader.Position;
				address_reader.Position = AddressTableOffset;
				int is_valid = address_reader.ReadInt32 ();
				if (is_valid != 0)
					Address = new MethodAddress (this, address_reader);
				address_reader.Position = VariableTableOffset;
				if (ThisTypeIndex != 0)
					ThisVariable = new VariableInfo (address_reader);
				ParameterVarInfo = new VariableInfo [NumParameters];
				for (int i = 0; i < NumParameters; i++)
					ParameterVarInfo [i] = new VariableInfo (address_reader);
				LocalVarInfo = new VariableInfo [NumLocals];
				for (int i = 0; i < NumLocals; i++)
					LocalVarInfo [i] = new VariableInfo (address_reader);
				address_reader.Position = old_pos;
			}

			if (TypeIndexTableOffset != 0) {
				long old_pos = reader.Position;
				reader.Position = TypeIndexTableOffset;

				ParamTypeIndices = new int [NumParameters];
				LocalTypeIndices = new int [NumLocals];

				for (int i = 0; i < NumParameters; i++)
					ParamTypeIndices [i] = reader.ReadInt32 ();
				for (int i = 0; i < NumLocals; i++)
					LocalTypeIndices [i] = reader.ReadInt32 ();

				reader.Position = old_pos;
			}
		}

		internal MethodEntry (int token, int sf_offset, string source_file,
				      int this_type_index, int[] param_type_indices,
				      int[] local_type_indices, LocalVariableEntry[] locals,
				      LineNumberEntry[] lines, int lnt_offset,
				      int addrtab_offset, int vartab_offset, int addrtab_size,
				      int start_row, int end_row)
		{
			this.Token = token;
			this.StartRow = start_row;
			this.EndRow = end_row;
			this.NumLineNumbers = lines.Length;
			this.ThisTypeIndex = this_type_index;
			this.NumParameters = param_type_indices.Length;
			this.NumLocals = local_type_indices.Length;
			this.ParamTypeIndices = param_type_indices;
			this.LocalTypeIndices = local_type_indices;
			this.Locals = locals;
			this.SourceFileOffset = sf_offset;
			this.LineNumberTableOffset = lnt_offset;
			this.AddressTableOffset = addrtab_offset;
			this.VariableTableOffset = vartab_offset;
			this.AddressTableSize = addrtab_size;
			this.SourceFile = source_file;
			this.LineNumbers = lines;
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (ThisTypeIndex);
			bw.Write (NumParameters);
			bw.Write (NumLocals);
			bw.Write (NumLineNumbers);
			bw.Write (TypeIndexTableOffset);
			bw.Write (LocalVariableTableOffset);
			bw.Write (SourceFileOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (AddressTableOffset);
			bw.Write (VariableTableOffset);
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
