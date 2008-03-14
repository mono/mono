//
// Mono.CSharp.Debugger/MonoSymbolTable.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Text;
using System.IO;

//
// Parts which are actually written into the symbol file are marked with
//
//         #region This is actually written to the symbol file
//         #endregion
//
// Please do not modify these regions without previously talking to me.
//
// All changes to the file format must be synchronized in several places:
//
// a) The fields in these regions (and their order) must match the actual
//    contents of the symbol file.
//
//    This helps people to understand the symbol file format without reading
//    too much source code, ie. you look at the appropriate region and then
//    you know what's actually in the file.
//
//    It is also required to help me enforce b).
//
// b) The regions must be kept in sync with the unmanaged code in
//    mono/metadata/debug-mono-symfile.h
//
// When making changes to the file format, you must also increase two version
// numbers:
//
// i)  OffsetTable.Version in this file.
// ii) MONO_SYMBOL_FILE_VERSION in mono/metadata/debug-mono-symfile.h
//
// After doing so, recompile everything, including the debugger.  Symbol files
// with different versions are incompatible to each other and the debugger and
// the runtime enfore this, so you need to recompile all your assemblies after
// changing the file format.
//

namespace Mono.CompilerServices.SymbolWriter
{
	public struct OffsetTable
	{
		public const int  Version = 41;
		public const int  CompatibilityVersion = 39;
		public const long Magic   = 0x45e82623fd7fa614;

		#region This is actually written to the symbol file
		public int TotalFileSize;
		public int DataSectionOffset;
		public int DataSectionSize;
		public int SourceCount;
		public int SourceTableOffset;
		public int SourceTableSize;
		public int MethodCount;
		public int MethodTableOffset;
		public int MethodTableSize;
		public int TypeCount;
		public int AnonymousScopeCount;
		public int AnonymousScopeTableOffset;
		public int AnonymousScopeTableSize;
		#endregion

		internal OffsetTable (BinaryReader reader, int version)
		{
			TotalFileSize = reader.ReadInt32 ();
			DataSectionOffset = reader.ReadInt32 ();
			DataSectionSize = reader.ReadInt32 ();
			SourceCount = reader.ReadInt32 ();
			SourceTableOffset = reader.ReadInt32 ();
			SourceTableSize = reader.ReadInt32 ();
			MethodCount = reader.ReadInt32 ();
			MethodTableOffset = reader.ReadInt32 ();
			MethodTableSize = reader.ReadInt32 ();
			TypeCount = reader.ReadInt32 ();

			if (version == CompatibilityVersion) {
				AnonymousScopeCount = 0;
				AnonymousScopeTableOffset = 0;
				AnonymousScopeTableSize = 0;
			} else {
				AnonymousScopeCount = reader.ReadInt32 ();
				AnonymousScopeTableOffset = reader.ReadInt32 ();
				AnonymousScopeTableSize = reader.ReadInt32 ();
			}
		}

		internal void Write (BinaryWriter bw, int version)
		{
			bw.Write (TotalFileSize);
			bw.Write (DataSectionOffset);
			bw.Write (DataSectionSize);
			bw.Write (SourceCount);
			bw.Write (SourceTableOffset);
			bw.Write (SourceTableSize);
			bw.Write (MethodCount);
			bw.Write (MethodTableOffset);
			bw.Write (MethodTableSize);
			bw.Write (TypeCount);

			if (version == CompatibilityVersion)
				return;

			bw.Write (AnonymousScopeCount);
			bw.Write (AnonymousScopeTableOffset);
			bw.Write (AnonymousScopeTableSize);
		}

		public override string ToString ()
		{
			return String.Format (
				"OffsetTable [{0} - {1}:{2} - {3}:{4}:{5} - {6}:{7}:{8} - {9}]",
				TotalFileSize, DataSectionOffset, DataSectionSize, SourceCount,
				SourceTableOffset, SourceTableSize, MethodCount, MethodTableOffset,
				MethodTableSize, TypeCount);
		}
	}

	public struct LineNumberEntry
	{
		#region This is actually written to the symbol file
		public readonly int Row;
		public readonly int Offset;
		#endregion

		public LineNumberEntry (int row, int offset)
		{
			this.Row = row;
			this.Offset = offset;
		}

		public static LineNumberEntry Null = new LineNumberEntry (0, 0);

		internal LineNumberEntry (BinaryReader reader)
		{
			Row = reader.ReadInt32 ();
			Offset = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
		}

		private class OffsetComparerClass : IComparer
		{
			public int Compare (object a, object b)
			{
				LineNumberEntry l1 = (LineNumberEntry) a;
				LineNumberEntry l2 = (LineNumberEntry) b;

				if (l1.Offset < l2.Offset)
					return -1;
				else if (l1.Offset > l2.Offset)
					return 1;
				else
					return 0;
			}
		}

		private class RowComparerClass : IComparer
		{
			public int Compare (object a, object b)
			{
				LineNumberEntry l1 = (LineNumberEntry) a;
				LineNumberEntry l2 = (LineNumberEntry) b;

				if (l1.Row < l2.Row)
					return -1;
				else if (l1.Row > l2.Row)
					return 1;
				else
					return 0;
			}
		}

		public static readonly IComparer OffsetComparer = new OffsetComparerClass ();
		public static readonly IComparer RowComparer = new RowComparerClass ();

		public override string ToString ()
		{
			return String.Format ("[Line {0}:{1}]", Row, Offset);
		}
	}

	[Obsolete("LexicalBlockEntry has been replaced by CodeBlockEntry.")]
	public class LexicalBlockEntry
	{
		public int Index;
		#region This is actually written to the symbol file
		public int StartOffset;
		public int EndOffset;
		#endregion

		public LexicalBlockEntry (int index, int start_offset)
		{
			this.Index = index;
			this.StartOffset = start_offset;
		}

		internal LexicalBlockEntry (int index, MyBinaryReader reader)
		{
			this.Index = index;
			this.StartOffset = reader.ReadInt32 ();
			this.EndOffset = reader.ReadInt32 ();
		}

		public void Close (int end_offset)
		{
			this.EndOffset = end_offset;
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.Write (StartOffset);
			bw.Write (EndOffset);
		}

		public override string ToString ()
		{
			return String.Format ("[LexicalBlock {0}:{1}]", StartOffset, EndOffset);
		}
	}

	public class CodeBlockEntry
	{
		public int Index;
		#region This is actually written to the symbol file
		public int Parent;
		public Type BlockType;
		public int StartOffset;
		public int EndOffset;
		#endregion

		public enum Type {
			Lexical			= 1,
			CompilerGenerated	= 2,
			IteratorBody		= 3,
			IteratorDispatcher	= 4
		}

		public CodeBlockEntry (int index, int parent, Type type, int start_offset)
		{
			this.Index = index;
			this.Parent = parent;
			this.BlockType = type;
			this.StartOffset = start_offset;
		}

		internal CodeBlockEntry (int index, MyBinaryReader reader)
		{
			this.Index = index;
			int type_flag = reader.ReadLeb128 ();
			BlockType = (Type) (type_flag & 0x3f);
			this.Parent = reader.ReadLeb128 ();
			this.StartOffset = reader.ReadLeb128 ();
			this.EndOffset = reader.ReadLeb128 ();

			/* Reserved for future extensions. */
			if ((type_flag & 0x40) != 0) {
				int data_size = reader.ReadInt16 ();
				reader.BaseStream.Position += data_size;
			}				
		}

		public void Close (int end_offset)
		{
			this.EndOffset = end_offset;
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.WriteLeb128 ((int) BlockType);
			bw.WriteLeb128 (Parent);
			bw.WriteLeb128 (StartOffset);
			bw.WriteLeb128 (EndOffset);
		}

		public override string ToString ()
		{
			return String.Format ("[CodeBlock {0}:{1}:{2}:{3}:{4}]",
					      Index, Parent, BlockType, StartOffset, EndOffset);
		}
	}

	public struct LocalVariableEntry
	{
		#region This is actually written to the symbol file
		public readonly int Index;
		public readonly string Name;
		public readonly int BlockIndex;
		#endregion

		public LocalVariableEntry (int index, string name, int block)
		{
			this.Index = index;
			this.Name = name;
			this.BlockIndex = block;
		}

		internal LocalVariableEntry (MonoSymbolFile file, MyBinaryReader reader)
		{
			Index = reader.ReadLeb128 ();
			Name = reader.ReadString ();
			if (file.CompatibilityMode) {
				int sig_length = reader.ReadLeb128 ();
				reader.BaseStream.Position += sig_length;
			}
			BlockIndex = reader.ReadLeb128 ();
		}

		internal void Write (MonoSymbolFile file, MyBinaryWriter bw)
		{
			bw.WriteLeb128 (Index);
			bw.Write (Name);
			if (file.CompatibilityMode)
				bw.WriteLeb128 (0);
			bw.WriteLeb128 (BlockIndex);
		}

		public override string ToString ()
		{
			return String.Format ("[LocalVariable {0}:{1}:{2}]",
					      Name, Index, BlockIndex);
		}
	}

	public struct CapturedVariable
	{
		#region This is actually written to the symbol file
		public readonly string Name;
		public readonly string CapturedName;
		public readonly CapturedKind Kind;
		#endregion

		public enum CapturedKind : byte
		{
			Local,
			Parameter,
			This
		}

		public CapturedVariable (string name, string captured_name,
					 CapturedKind kind)
		{
			this.Name = name;
			this.CapturedName = captured_name;
			this.Kind = kind;
		}

		internal CapturedVariable (MyBinaryReader reader)
		{
			Name = reader.ReadString ();
			CapturedName = reader.ReadString ();
			Kind = (CapturedKind) reader.ReadByte ();
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.Write (Name);
			bw.Write (CapturedName);
			bw.Write ((byte) Kind);
		}

		public override string ToString ()
		{
			return String.Format ("[CapturedVariable {0}:{1}:{2}]",
					      Name, CapturedName, Kind);
		}
	}

	public struct CapturedScope
	{
		#region This is actually written to the symbol file
		public readonly int Scope;
		public readonly string CapturedName;
		#endregion

		public CapturedScope (int scope, string captured_name)
		{
			this.Scope = scope;
			this.CapturedName = captured_name;
		}

		internal CapturedScope (MyBinaryReader reader)
		{
			Scope = reader.ReadLeb128 ();
			CapturedName = reader.ReadString ();
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.WriteLeb128 (Scope);
			bw.Write (CapturedName);
		}

		public override string ToString ()
		{
			return String.Format ("[CapturedScope {0}:{1}]",
					      Scope, CapturedName);
		}
	}

	public struct ScopeVariable
	{
		#region This is actually written to the symbol file
		public readonly int Scope;
		public readonly int Index;
		#endregion

		public ScopeVariable (int scope, int index)
		{
			this.Scope = scope;
			this.Index = index;
		}

		internal ScopeVariable (MyBinaryReader reader)
		{
			Scope = reader.ReadLeb128 ();
			Index = reader.ReadLeb128 ();
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.WriteLeb128 (Scope);
			bw.WriteLeb128 (Index);
		}

		public override string ToString ()
		{
			return String.Format ("[ScopeVariable {0}:{1}]", Scope, Index);
		}
	}

	public class AnonymousScopeEntry
	{
		#region This is actually written to the symbol file
		public readonly int ID;
		#endregion

		ArrayList captured_vars = new ArrayList ();
		ArrayList captured_scopes = new ArrayList ();

		public AnonymousScopeEntry (int id)
		{
			this.ID = id;
		}

		internal AnonymousScopeEntry (MyBinaryReader reader)
		{
			ID = reader.ReadLeb128 ();

			int num_captured_vars = reader.ReadLeb128 ();
			for (int i = 0; i < num_captured_vars; i++)
				captured_vars.Add (new CapturedVariable (reader));

			int num_captured_scopes = reader.ReadLeb128 ();
			for (int i = 0; i < num_captured_scopes; i++)
				captured_scopes.Add (new CapturedScope (reader));
		}

		internal void AddCapturedVariable (string name, string captured_name,
						   CapturedVariable.CapturedKind kind)
		{
			captured_vars.Add (new CapturedVariable (name, captured_name, kind));
		}

		public CapturedVariable[] CapturedVariables {
			get {
				CapturedVariable[] retval = new CapturedVariable [captured_vars.Count];
				captured_vars.CopyTo (retval, 0);
				return retval;
			}
		}

		internal void AddCapturedScope (int scope, string captured_name)
		{
			captured_scopes.Add (new CapturedScope (scope, captured_name));
		}

		public CapturedScope[] CapturedScopes {
			get {
				CapturedScope[] retval = new CapturedScope [captured_scopes.Count];
				captured_scopes.CopyTo (retval, 0);
				return retval;
			}
		}

		internal void Write (MyBinaryWriter bw)
		{
			bw.WriteLeb128 (ID);

			bw.WriteLeb128 (captured_vars.Count);
			foreach (CapturedVariable cv in captured_vars)
				cv.Write (bw);

			bw.WriteLeb128 (captured_scopes.Count);
			foreach (CapturedScope cs in captured_scopes)
				cs.Write (bw);
		}

		public override string ToString ()
		{
			return String.Format ("[AnonymousScope {0}]", ID);
		}
	}

	public class SourceFileEntry
	{
		#region This is actually written to the symbol file
		public readonly int Index;
		int Count;
		int NamespaceCount;
		int NameOffset;
		int MethodOffset;
		int NamespaceTableOffset;
		#endregion

		MonoSymbolFile file;
		string file_name;
		ArrayList methods;
		ArrayList namespaces;
		bool creating;

		public static int Size {
			get { return 24; }
		}

		public SourceFileEntry (MonoSymbolFile file, string file_name)
		{
			this.file = file;
			this.file_name = file_name;
			this.Index = file.AddSource (this);

			creating = true;
			methods = new ArrayList ();
			namespaces = new ArrayList ();
		}

		[Obsolete]
		public void DefineMethod (string name, int token, LocalVariableEntry[] locals,
					  LineNumberEntry[] lines, LexicalBlockEntry[] blocks,
					  int start, int end, int namespace_id)
		{
			DefineMethod (token, null, locals, lines, null, null,
				      start, end, namespace_id);
		}

		public void DefineMethod (int token, ScopeVariable[] scope_vars,
					  LocalVariableEntry[] locals, LineNumberEntry[] lines,
					  CodeBlockEntry[] blocks, string real_name,
					  int start, int end, int namespace_id)
		{
			if (!creating)
				throw new InvalidOperationException ();

			MethodEntry entry = new MethodEntry (
				file, this, (int) token, scope_vars, locals, lines,
				blocks, real_name, start, end, namespace_id);

			methods.Add (entry);
			file.AddMethod (entry);
		}

		public int DefineNamespace (string name, string[] using_clauses, int parent)
		{
			if (!creating)
				throw new InvalidOperationException ();

			int index = file.GetNextNamespaceIndex ();
			NamespaceEntry ns = new NamespaceEntry (name, index, using_clauses, parent);
			namespaces.Add (ns);
			return index;
		}

		internal void WriteData (MyBinaryWriter bw)
		{
			NameOffset = (int) bw.BaseStream.Position;
			bw.Write (file_name);

			ArrayList list = new ArrayList ();
			foreach (MethodEntry entry in methods)
				list.Add (entry.Write (file, bw));
			list.Sort ();
			Count = list.Count;

			MethodOffset = (int) bw.BaseStream.Position;
			foreach (MethodSourceEntry method in list)
				method.Write (bw);

			NamespaceCount = namespaces.Count;
			NamespaceTableOffset = (int) bw.BaseStream.Position;
			foreach (NamespaceEntry ns in namespaces)
				ns.Write (file, bw);
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Index);
			bw.Write (Count);
			bw.Write (NamespaceCount);
			bw.Write (NameOffset);
			bw.Write (MethodOffset);
			bw.Write (NamespaceTableOffset);
		}

		internal SourceFileEntry (MonoSymbolFile file, BinaryReader reader)
		{
			this.file = file;

			Index = reader.ReadInt32 ();
			Count = reader.ReadInt32 ();
			NamespaceCount = reader.ReadInt32 ();
			NameOffset = reader.ReadInt32 ();
			MethodOffset = reader.ReadInt32 ();
			NamespaceTableOffset = reader.ReadInt32 ();

			file_name = file.ReadString (NameOffset);
		}

		public string FileName {
			get { return file_name; }
		}

		public MethodSourceEntry[] Methods {
			get {
				if (creating)
					throw new InvalidOperationException ();

				BinaryReader reader = file.BinaryReader;
				int old_pos = (int) reader.BaseStream.Position;

				reader.BaseStream.Position = MethodOffset;
				ArrayList list = new ArrayList ();
				for (int i = 0; i < Count; i ++)
					list.Add (new MethodSourceEntry (reader));
				reader.BaseStream.Position = old_pos;

				MethodSourceEntry[] retval = new MethodSourceEntry [Count];
				list.CopyTo (retval, 0);
				return retval;
			}
		}

		public NamespaceEntry[] Namespaces {
			get {
				if (creating)
					throw new InvalidOperationException ();

				MyBinaryReader reader = file.BinaryReader;
				int old_pos = (int) reader.BaseStream.Position;

				reader.BaseStream.Position = NamespaceTableOffset;
				ArrayList list = new ArrayList ();
				for (int i = 0; i < NamespaceCount; i ++)
					list.Add (new NamespaceEntry (file, reader));
				reader.BaseStream.Position = old_pos;

				NamespaceEntry[] retval = new NamespaceEntry [list.Count];
				list.CopyTo (retval, 0);
				return retval;
			}
		}

		public override string ToString ()
		{
			return String.Format ("SourceFileEntry ({0}:{1}:{2})",
					      Index, file_name, Count);
		}
	}

	public struct MethodSourceEntry : IComparable
	{
		#region This is actually written to the symbol file
		public readonly int Index;
		public readonly int FileOffset;
		public readonly int StartRow;
		public readonly int EndRow;
		#endregion

		public MethodSourceEntry (int index, int file_offset, int start, int end)
		{
			this.Index = index;
			this.FileOffset = file_offset;
			this.StartRow = start;
			this.EndRow = end;
		}

		internal MethodSourceEntry (BinaryReader reader)
		{
			Index = reader.ReadInt32 ();
			FileOffset = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
		}

		public static int Size {
			get { return 16; }
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Index);
			bw.Write (FileOffset);
			bw.Write (StartRow);
			bw.Write (EndRow);
		}

		public int CompareTo (object obj)
		{
			MethodSourceEntry method = (MethodSourceEntry) obj;

			if (method.StartRow < StartRow)
				return -1;
			else if (method.StartRow > StartRow)
				return 1;
			else
				return 0;
		}

		public override string ToString ()
		{
			return String.Format ("MethodSourceEntry ({0}:{1}:{2}:{3})",
					      Index, FileOffset, StartRow, EndRow);
		}
	}

	public struct MethodIndexEntry
	{
		#region This is actually written to the symbol file
		public readonly int FileOffset;
		public readonly int Token;
		#endregion

		public static int Size {
			get { return 8; }
		}

		public MethodIndexEntry (int offset, int token)
		{
			this.FileOffset = offset;
			this.Token = token;
		}

		internal MethodIndexEntry (BinaryReader reader)
		{
			FileOffset = reader.ReadInt32 ();
			Token = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (FileOffset);
			bw.Write (Token);
		}

		public override string ToString ()
		{
			return String.Format ("MethodIndexEntry ({0}:{1:x})",
					      FileOffset, Token);
		}
	}

	public class MethodEntry : IComparable
	{
		#region This is actually written to the symbol file
		public readonly int SourceFileIndex;
		public readonly int Token;
		public readonly int StartRow;
		public readonly int EndRow;
		public readonly int NumLocals;
		public readonly int NumLineNumbers;
		public readonly int NamespaceID;
		public readonly bool LocalNamesAmbiguous;

		int NameOffset;
		int LocalVariableTableOffset;
		int LineNumberTableOffset;

		public readonly int NumCodeBlocks;
		int CodeBlockTableOffset;

		public readonly int NumScopeVariables;
		int ScopeVariableTableOffset;

		int RealNameOffset;
		#endregion

		int index;
		int file_offset;

		public readonly SourceFileEntry SourceFile;
		public readonly LineNumberEntry[] LineNumbers;
		public readonly int[] LocalTypeIndices;
		public readonly LocalVariableEntry[] Locals;
		public readonly CodeBlockEntry[] CodeBlocks;
		public readonly ScopeVariable[] ScopeVariables;

		[Obsolete]
		public LexicalBlockEntry[] LexicalBlocks {
			get { return new LexicalBlockEntry [0]; }
		}

		public readonly string RealName;

		public readonly MonoSymbolFile SymbolFile;

		public int Index {
			get { return index; }
			set { index = value; }
		}

		internal MethodEntry (MonoSymbolFile file, MyBinaryReader reader, int index)
		{
			this.SymbolFile = file;
			this.index = index;
			SourceFileIndex = reader.ReadInt32 ();
			Token = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
			NumLocals = reader.ReadInt32 ();
			NumLineNumbers = reader.ReadInt32 ();
			NameOffset = reader.ReadInt32 ();
			reader.ReadInt32 (); /* Compatibility with old file format */
			LocalVariableTableOffset = reader.ReadInt32 ();
			LineNumberTableOffset = reader.ReadInt32 ();
			reader.ReadInt32 (); /* Compatibility with old file format */
			reader.ReadInt32 (); /* Likewise. */
			NamespaceID = reader.ReadInt32 ();
			LocalNamesAmbiguous = reader.ReadInt32 () != 0;

			if (!file.CompatibilityMode) {
				NumCodeBlocks = reader.ReadInt32 ();
				CodeBlockTableOffset = reader.ReadInt32 ();

				NumScopeVariables = reader.ReadInt32 ();
				ScopeVariableTableOffset = reader.ReadInt32 ();

				RealNameOffset = reader.ReadInt32 ();
				if (RealNameOffset != 0)
					RealName = file.ReadString (RealNameOffset);
			}

			SourceFile = file.GetSourceFile (SourceFileIndex);

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++)
					LineNumbers [i] = new LineNumberEntry (reader);

				reader.BaseStream.Position = old_pos;
			}

			if (LocalVariableTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LocalVariableTableOffset;

				Locals = new LocalVariableEntry [NumLocals];

				for (int i = 0; i < NumLocals; i++)
					Locals [i] = new LocalVariableEntry (file, reader);

				reader.BaseStream.Position = old_pos;
			}

			if (CodeBlockTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = CodeBlockTableOffset;

				CodeBlocks = new CodeBlockEntry [NumCodeBlocks];
				for (int i = 0; i < NumCodeBlocks; i++)
					CodeBlocks [i] = new CodeBlockEntry (i, reader);

				reader.BaseStream.Position = old_pos;
			}

			if (NumScopeVariables != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = ScopeVariableTableOffset;

				ScopeVariables = new ScopeVariable [NumScopeVariables];
				for (int i = 0; i < NumScopeVariables; i++)
					ScopeVariables [i] = new ScopeVariable (reader);

				reader.BaseStream.Position = old_pos;
			}
		}

		internal MethodEntry (MonoSymbolFile file, SourceFileEntry source,
				      int token, ScopeVariable[] scope_vars,
				      LocalVariableEntry[] locals, LineNumberEntry[] lines,
				      CodeBlockEntry[] blocks, string real_name,
				      int start_row, int end_row, int namespace_id)
		{
			this.SymbolFile = file;

			index = -1;

			Token = token;
			SourceFileIndex = source.Index;
			SourceFile = source;
			StartRow = start_row;
			EndRow = end_row;
			NamespaceID = namespace_id;

			LineNumbers = BuildLineNumberTable (lines);
			NumLineNumbers = LineNumbers.Length;

			file.NumLineNumbers += NumLineNumbers;

			NumLocals = locals != null ? locals.Length : 0;
			Locals = locals;

			if (NumLocals <= 32) {
				// Most of the time, the O(n^2) factor is actually
				// less than the cost of allocating the hash table,
				// 32 is a rough number obtained through some testing.
				
				for (int i = 0; i < NumLocals; i ++) {
					string nm = locals [i].Name;
					
					for (int j = i + 1; j < NumLocals; j ++) {
						if (locals [j].Name == nm) {
							LocalNamesAmbiguous = true;
							goto locals_check_done;
						}
					}
				}
			locals_check_done :
				;
			} else {
				Hashtable local_names = new Hashtable ();
				foreach (LocalVariableEntry local in locals) {
					if (local_names.Contains (local.Name)) {
						LocalNamesAmbiguous = true;
						break;
					}
					local_names.Add (local.Name, local);
				}
			}

			NumCodeBlocks = blocks != null ? blocks.Length : 0;
			CodeBlocks = blocks;

			NumScopeVariables = scope_vars != null ? scope_vars.Length : 0;
			ScopeVariables = scope_vars;

			RealName = real_name;
		}
		
		static LineNumberEntry [] tmp_buff = new LineNumberEntry [20];

		// BuildLineNumberTable() eliminates duplicate line numbers and ensures
		// we aren't going "backwards" since this would counfuse the runtime's
		// debugging code (and the debugger).
		//
		// In the line number table, the "offset" field most be strictly
		// monotonic increasing; that is, the next entry must not have an offset
		// which is equal to or less than the current one.
		//
		// The most common case is that our input (ie. the line number table as
		// we get it from mcs) contains several entries with the same offset
		// (and different line numbers) - but it may also happen that the offset
		// is decreasing (this can be considered as an exception, such lines will
		// simply be discarded).
		LineNumberEntry[] BuildLineNumberTable (LineNumberEntry[] line_numbers)
		{
			int pos = 0;
			int last_offset = -1;
			int last_row = -1;

			if (line_numbers == null)
				return new LineNumberEntry [0];
			
			if (tmp_buff.Length < (line_numbers.Length + 1))
				tmp_buff = new LineNumberEntry [(line_numbers.Length + 1) * 2];

			for (int i = 0; i < line_numbers.Length; i++) {
				LineNumberEntry line = line_numbers [i];

				if (line.Offset > last_offset) {
					if (last_row >= 0)
						tmp_buff [pos ++] = new LineNumberEntry (last_row, last_offset);
					last_row = line.Row;
					last_offset = line.Offset;
				} else if (line.Row > last_row) {
					last_row = line.Row;
				}
			}

			if (last_row >= 0)
				tmp_buff [pos ++] = new LineNumberEntry (last_row, last_offset);

			LineNumberEntry [] retval = new LineNumberEntry [pos];
			Array.Copy (tmp_buff, retval, pos);
			return retval;
		}

		internal MethodSourceEntry Write (MonoSymbolFile file, MyBinaryWriter bw)
		{
			if (index <= 0)
				throw new InvalidOperationException ();

			NameOffset = (int) bw.BaseStream.Position;

			LocalVariableTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumLocals; i++)
				Locals [i].Write (file, bw);
			file.LocalCount += NumLocals;

			LineNumberTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumLineNumbers; i++)
				LineNumbers [i].Write (bw);
			file.LineNumberCount += NumLineNumbers;

			CodeBlockTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumCodeBlocks; i++)
				CodeBlocks [i].Write (bw);

			ScopeVariableTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumScopeVariables; i++)
				ScopeVariables [i].Write (bw);

			if (RealName != null) {
				RealNameOffset = (int) bw.BaseStream.Position;
				bw.Write (RealName);
			}

			file_offset = (int) bw.BaseStream.Position;

			bw.Write (SourceFileIndex);
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (NumLocals);
			bw.Write (NumLineNumbers);
			bw.Write (NameOffset);
			bw.Write (0); /* Compatibility with old file format. */
			bw.Write (LocalVariableTableOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (0); /* Compatibility with old file format. */
			bw.Write (0); /* Likewise. */
			bw.Write (NamespaceID);
			bw.Write (LocalNamesAmbiguous ? 1 : 0);

			if (!file.CompatibilityMode) {
				bw.Write (NumCodeBlocks);
				bw.Write (CodeBlockTableOffset);

				bw.Write (NumScopeVariables);
				bw.Write (ScopeVariableTableOffset);

				bw.Write (RealNameOffset);
			}

			return new MethodSourceEntry (index, file_offset, StartRow, EndRow);
		}

		internal void WriteIndex (BinaryWriter bw)
		{
			new MethodIndexEntry (file_offset, Token).Write (bw);
		}

		public int CompareTo (object obj)
		{
			MethodEntry method = (MethodEntry) obj;

			if (method.Token < Token)
				return 1;
			else if (method.Token > Token)
				return -1;
			else
				return 0;
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4} - {6}:{7} - {5}]",
					      index, Token, SourceFileIndex, StartRow, EndRow,
					      SourceFile, NumLocals, NumLineNumbers);
		}
	}

	public struct NamespaceEntry
	{
		#region This is actually written to the symbol file
		public readonly string Name;
		public readonly int Index;
		public readonly int Parent;
		public readonly string[] UsingClauses;
		#endregion

		public NamespaceEntry (string name, int index, string[] using_clauses, int parent)
		{
			this.Name = name;
			this.Index = index;
			this.Parent = parent;
			this.UsingClauses = using_clauses != null ? using_clauses : new string [0];
		}

		internal NamespaceEntry (MonoSymbolFile file, MyBinaryReader reader)
		{
			Name = reader.ReadString ();
			Index = reader.ReadLeb128 ();
			Parent = reader.ReadLeb128 ();

			int count = reader.ReadLeb128 ();
			UsingClauses = new string [count];
			for (int i = 0; i < count; i++)
				UsingClauses [i] = reader.ReadString ();
		}

		internal void Write (MonoSymbolFile file, MyBinaryWriter bw)
		{
			bw.Write (Name);
			bw.WriteLeb128 (Index);
			bw.WriteLeb128 (Parent);
			bw.WriteLeb128 (UsingClauses.Length);
			foreach (string uc in UsingClauses)
				bw.Write (uc);
		}

		public override string ToString ()
		{
			return String.Format ("[Namespace {0}:{1}:{2}]", Name, Index, Parent);
		}
	}
}
