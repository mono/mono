//
// System.Diagnostics.SymbolStore/MonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is the default implementation of the System.Diagnostics.SymbolStore.ISymbolWriter
// interface.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{

	public class MonoSymbolWriter : IMonoSymbolWriter
	{
		protected string output_filename = null;
		protected Hashtable methods = null;
		protected Hashtable sources = null;

		protected class SourceFile : ISourceFile
		{
			private ArrayList _methods = new ArrayList ();
			private string _file_name;

			public SourceFile (string filename)
			{
				this._file_name = filename;
			}

			public override string ToString ()
			{
				return _file_name;
			}

			// interface ISourceFile

			public string FileName {
				get {
					return _file_name;
				}
			}

			public ISourceMethod[] Methods {
				get {
					ISourceMethod[] retval = new ISourceMethod [_methods.Count];
					_methods.CopyTo (retval);
					return retval;
				}
			}

			public void AddMethod (ISourceMethod method)
			{
				_methods.Add (method);
			}
		}

		protected class SourceBlock : ISourceBlock
		{
			static private int next_index;
			private readonly int _index;

			public SourceBlock (ISourceMethod method, ISourceLine start, ISourceLine end)
			{
				this._method = method;
				this._start = start;
				this._end = end;
				this._index = ++next_index;
			}

			internal SourceBlock (ISourceMethod method, int startOffset)
			{
				this._method = method;
				this._start = new SourceLine (startOffset);
				this._index = ++next_index;
			}

			public override string ToString ()
			{
				return "SourceBlock #" + ID + " (" + Start + " - " + End + ")";
			}

			private readonly ISourceMethod _method;
			internal ISourceLine _start;
			internal ISourceLine _end;

			private ArrayList _locals = new ArrayList ();

			public ISourceMethod SourceMethod {
				get {
					return _method;
				}
			}

			public ISourceLine Start {
				get {
					return _start;
				}
			}

			public ISourceLine End {
				get {
					return _end;
				}
			}

			public int ID {
				get {
					return _index;
				}
			}

			public ILocalVariable[] Locals {
				get {
					ILocalVariable[] retval = new ILocalVariable [_locals.Count];
					_locals.CopyTo (retval);
					return retval;
				}
			}

			public void AddLocal (ILocalVariable local)
			{
				_locals.Add (local);
			}
		}

		protected class SourceLine : ISourceLine
		{
			public SourceLine (int row, int column)
				: this (0, row, column)
			{
				this._type = SourceOffsetType.OFFSET_NONE;
			}

			public SourceLine (int offset, int row, int column)
			{
				this._offset = offset;
				this._row = row;
				this._column = column;
				this._type = SourceOffsetType.OFFSET_IL;
			}

			internal SourceLine (int offset)
				: this (offset, 0, 0)
			{ }

			public override string ToString ()
			{
				return "SourceLine (" + _offset + "," + _row + ":" + _column + ")";
			}

			internal SourceOffsetType _type;
			internal int _offset;
			internal int _row;
			internal int _column;

			// interface ISourceLine

			public SourceOffsetType OffsetType {
				get {
					return _type;
				}
			}

			public int Offset {
				get {
					return _offset;
				}
			}

			public int Row {
				get {
					return _row;
				}
			}

			public int Column {
				get {
					return _column;
				}
			}
		}

		protected class LocalVariable : ILocalVariable
		{
			public LocalVariable (string name, Type type, int token, int index)
				: this (name, type, token, index, null)
			{ }

			public LocalVariable (string name, Type type, int token, int index,
					      ISourceLine line)
			{
				this._name = name;
				this._type = type;
				this._token = token;
				this._index = index;
				this._line = line;
			}

			private readonly string _name;
			private readonly Type _type;
			private readonly int _token;
			private readonly int _index;
			private readonly ISourceLine _line;

			public override string ToString ()
			{
				return "LocalVariable (" + _index + "," + _name + ")";
			}

			// interface ILocalVariable

			public string Name {
				get {
					return _name;
				}
			}

			public Type Type {
				get {
					return _type;
				}
			}

			public int Token {
				get {
					return _token;
				}
			}

			public int Index {
				get {
					return _index;
				}
			}

			public ISourceLine Line {
				get {
					return _line;
				}
			}
		}

		protected class SourceMethod : ISourceMethod
		{
			private ArrayList _lines = new ArrayList ();
			private ArrayList _blocks = new ArrayList ();
			private Hashtable _block_hash = new Hashtable ();
			private Stack _block_stack = new Stack ();

			private readonly MethodInfo _method_info;
			private readonly SourceFile _source_file;
			private readonly int _token;

			private SourceBlock _implicit_block;

			public SourceMethod (int token, MethodInfo method_info, SourceFile source_file) {
				this._method_info = method_info;
				this._source_file = source_file;
				this._token = token;

				this._implicit_block = new SourceBlock (this, 0);
			}

			public void SetSourceRange (int startLine, int startColumn,
						    int endLine, int endColumn)
			{
				Console.WriteLine ("SOURCE RANGE: " + MethodInfo.Name + " " +
						   startLine + ":" + startColumn + " " +
						   endLine + ":" + endColumn);

				_implicit_block._start = new SourceLine (startLine, startColumn);
				_implicit_block._end = new SourceLine (endLine, endColumn);
			}


			public void StartBlock (ISourceBlock block)
			{
				_block_stack.Push (block);
			}

			public void EndBlock (int endOffset) {
				SourceBlock block = (SourceBlock) _block_stack.Pop ();

				block._end = new SourceLine (endOffset);
				_blocks.Add (block);
				_block_hash.Add (block.ID, block);
			}

			public void SetBlockRange (int BlockID, int startOffset, int endOffset)
			{
				SourceBlock block = (SourceBlock) _block_hash [BlockID];
				((SourceLine) block.Start)._offset = startOffset;
				((SourceLine) block.End)._offset = endOffset;
			}

			public ISourceBlock CurrentBlock {
				get {
					if (_block_stack.Count > 0)
						return (ISourceBlock) _block_stack.Peek ();
					else
						return _implicit_block;
				}
			}

			// interface ISourceMethod

			public ISourceLine[] Lines {
				get {
					ISourceLine[] retval = new ISourceLine [_lines.Count];
					_lines.CopyTo (retval);
					return retval;
				}
			}

			public void AddLine (ISourceLine line)
			{
				_lines.Add (line);
			}

			public ISourceBlock[] Blocks {
				get {
					ISourceBlock[] retval = new ISourceBlock [_blocks.Count];
					_blocks.CopyTo (retval);
					return retval;
				}
			}

			public ILocalVariable[] Locals {
				get {
					return _implicit_block.Locals;
				}
			}

			public void AddLocal (ILocalVariable local)
			{
				_implicit_block.AddLocal (local);
			}

			public MethodInfo MethodInfo {
				get {
					return _method_info;
				}
			}

			public ISourceFile SourceFile {
				get {
					return _source_file;
				}
			}

			public int Token {
				get {
					return _token;
				}
			}

			public ISourceLine Start {
				get {
					return _implicit_block.Start;
				}
			}

			public ISourceLine End {
				get {
					return _implicit_block.End;
				}
			}
		}

		protected SourceMethod current_method = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter ()
		{
			methods = new Hashtable ();
			sources = new Hashtable ();
		}

		public void Close () {
			CreateDwarfFile (output_filename);
		}

		public void CloseNamespace () {
		}

		// Create and return a new IMonoSymbolDocumentWriter.
		public ISymbolDocumentWriter DefineDocument (string url,
							     Guid language,
							     Guid languageVendor,
							     Guid documentType)
		{
			return new MonoSymbolDocumentWriter (url);
		}

		public void DefineField (
			SymbolToken parent,
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineGlobalVariable (
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineLocalVariable (string name,
						 FieldAttributes attributes,
						 byte[] signature,
						 SymAddressKind addrKind,
						 int addr1,
						 int addr2,
						 int addr3,
						 int startOffset,
						 int endOffset)
		{
			if (current_method == null)
				return;

			int token = current_method.Token;

			LocalVariable local_info = new LocalVariable (name, typeof (int), token, addr1);

			current_method.CurrentBlock.AddLocal (local_info);
		}

		public void DefineParameter (
			string name,
			ParameterAttributes attributes,
			int sequence,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineSequencePoints (ISymbolDocumentWriter document,
						  int[] offsets,
						  int[] lines,
						  int[] columns,
						  int[] endLines,
						  int[] endColumns)
		{
			SourceLine source_line = new SourceLine (offsets [0], lines [0], columns [0]);

			if (current_method != null)
				current_method.AddLine (source_line);
		}

		public void Initialize (IntPtr emitter, string filename, bool fFullBuild)
		{
			throw new NotSupportedException ("Please use the 'Initialize (string filename)' "
							 + "constructor and read the documentation in "
							 + "Mono.CSharp.Debugger/IMonoSymbolWriter.cs");
		}

		// This is documented in IMonoSymbolWriter.cs
		public void Initialize (string filename)
		{
			this.output_filename = filename;
		}

		public void OpenMethod (SymbolToken method)
		{
			// do nothing
		}

		// This is documented in IMonoSymbolWriter.cs
		public void OpenMethod (SymbolToken symbol_token, MethodInfo method_info,
					string source_file)
		{
			int token = symbol_token.GetToken ();
			SourceFile source_info;

			if (methods.ContainsKey (token))
				methods.Remove (token);

			if (sources.ContainsKey (source_file))
				source_info = (SourceFile) sources [source_file];
			else {
				source_info = new SourceFile (source_file);
				sources.Add (source_file, source_info);
			}

			current_method = new SourceMethod (token, method_info, source_info);

			source_info.AddMethod (current_method);

			methods.Add (token, current_method);

			OpenMethod (symbol_token);
		}

		public void SetMethodSourceRange (ISymbolDocumentWriter startDoc,
						  int startLine, int startColumn,
						  ISymbolDocumentWriter endDoc,
						  int endLine, int endColumn)
		{
			if ((startDoc == null) || (endDoc == null))
				throw new NullReferenceException ();

			if (!(startDoc is MonoSymbolDocumentWriter) || !(endDoc is MonoSymbolDocumentWriter))
				throw new NotSupportedException ("both startDoc and endDoc must be of type "
								 + "MonoSymbolDocumentWriter");

			if (!startDoc.Equals (endDoc))
				throw new NotSupportedException ("startDoc and endDoc must be the same");

			if (current_method != null)
				current_method.SetSourceRange (startLine, startColumn,
							       endLine, endColumn);
		}

		public void CloseMethod () {
			current_method = null;
		}

		public void OpenNamespace (string name)
		{
		}

		public int OpenScope (int startOffset)
		{
			if (current_method == null)
				return 0;

			ISourceBlock block = new SourceBlock (current_method, startOffset);
			current_method.StartBlock (block);

			return block.ID;
		}

		public void CloseScope (int endOffset) {
			if (current_method == null)
				return;

			current_method.EndBlock (endOffset);
		}

		public void SetScopeRange (int scopeID, int startOffset, int endOffset)
		{
			if (current_method == null)
				return;

			current_method.SetBlockRange (scopeID, startOffset, endOffset);
		}

		public void SetSymAttribute (SymbolToken parent, string name, byte[] data)
		{
		}

		public void SetUnderlyingWriter (IntPtr underlyingWriter)
		{
		}

		public void SetUserEntryPoint (SymbolToken entryMethod)
		{
		}

		public void UsingNamespace (string fullName)
		{
		}

		//
		// MonoSymbolWriter implementation
		//
		protected void WriteLocal (DwarfFileWriter.Die parent_die, ILocalVariable local)
		{
			DwarfFileWriter.DieMethodVariable die;

			Console.WriteLine ("WRITE LOCAL: " + (LocalVariable) local);

			die = new DwarfFileWriter.DieMethodVariable (parent_die, local);
		}

		protected void WriteBlock (DwarfFileWriter.Die parent_die, ISourceBlock block)
		{
			DwarfFileWriter.DieLexicalBlock die;

			Console.WriteLine ("WRITE BLOCK: " + (SourceBlock) block);

			die = new DwarfFileWriter.DieLexicalBlock (parent_die, block);

			foreach (ILocalVariable local in block.Locals)
				WriteLocal (die, local);
		}

		protected void WriteMethod (DwarfFileWriter.DieCompileUnit parent_die, ISourceMethod method)
		{
			DwarfFileWriter.DieSubProgram die;

			die = new DwarfFileWriter.DieSubProgram (parent_die, method);

			Console.WriteLine ("WRITE METHOD: " + method.MethodInfo.Name);

			foreach (ILocalVariable local in method.Locals)
				WriteLocal (die, local);

			foreach (ISourceBlock block in method.Blocks)
				WriteBlock (die, block);
		}

		protected void WriteSource (DwarfFileWriter writer, ISourceFile source)
		{
			DwarfFileWriter.CompileUnit compile_unit = new DwarfFileWriter.CompileUnit (
				writer, source.FileName);

			DwarfFileWriter.DieCompileUnit die = new DwarfFileWriter.DieCompileUnit (compile_unit);

			foreach (ISourceMethod method in source.Methods)
				WriteMethod (die, method);
		}

		protected void CreateDwarfFile (string filename)
		{
			DwarfFileWriter writer = new DwarfFileWriter (filename);

			foreach (ISourceFile source in sources.Values)
				WriteSource (writer, source);

			writer.Close ();
		}
	}
}
