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
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	internal class SourceFile : SourceFileEntry, ISymbolDocumentWriter
	{
		private ArrayList _methods = new ArrayList ();

		public SourceFile (MonoSymbolFile file, string filename)
			: base (file, filename)
		{ }

		public new SourceMethod[] Methods {
			get {
				SourceMethod[] retval = new SourceMethod [_methods.Count];
				_methods.CopyTo (retval);
				return retval;
			}
		}

		public void AddMethod (SourceMethod method)
		{
			_methods.Add (method);
		}

		void ISymbolDocumentWriter.SetCheckSum (Guid algorithmId, byte[] checkSum)
		{
			throw new NotSupportedException ();
		}

		void ISymbolDocumentWriter.SetSource (byte[] source)
		{
			throw new NotSupportedException ();
		}
	}

	internal class SourceMethod
	{
		private ArrayList _lines = new ArrayList ();
		private ArrayList _locals = new ArrayList ();
		private ArrayList _blocks = new ArrayList ();
		private Stack _block_stack = new Stack ();
		private int next_block_id = 0;

		internal readonly MethodBase _method_base;
		internal SourceFile _source_file;
		internal int _token;
		private int _namespace_id;
		private LineNumberEntry _start, _end;
		private MonoSymbolFile _file;

		private LexicalBlockEntry _implicit_block;

		internal SourceMethod (MonoSymbolFile file, SourceFile source_file,
				       int startLine, int startColumn, int endLine, int endColumn,
				       MethodBase method_base, int namespace_id)
		{
			this._file = file;
			this._method_base = method_base;
			this._source_file = source_file;
			this._namespace_id = namespace_id;

			this._start = new LineNumberEntry (startLine, 0);
			this._end = new LineNumberEntry (endLine, 0);

			this._implicit_block = new LexicalBlockEntry (0, 0);
		}

		public void StartBlock (int startOffset)
		{
			LexicalBlockEntry block = new LexicalBlockEntry (++next_block_id, startOffset);
			_block_stack.Push (block);
			_blocks.Add (block);
		}

		public void EndBlock (int endOffset)
		{
			LexicalBlockEntry block = (LexicalBlockEntry) _block_stack.Pop ();

			block.Close (endOffset);
		}

		public LexicalBlockEntry[] Blocks {
			get {
				LexicalBlockEntry[] retval = new LexicalBlockEntry [_blocks.Count];
				_blocks.CopyTo (retval, 0);
				return retval;
			}
		}

		public LexicalBlockEntry CurrentBlock {
			get {
				if (_block_stack.Count > 0)
					return (LexicalBlockEntry) _block_stack.Peek ();
				else
					return _implicit_block;
			}
		}

		public LineNumberEntry[] Lines {
			get {
				LineNumberEntry[] retval = new LineNumberEntry [_lines.Count];
				_lines.CopyTo (retval);
				return retval;
			}
		}

		public void AddLine (LineNumberEntry line)
		{
			_lines.Add (line);
		}

		public LocalVariableEntry[] Locals {
			get {
				LocalVariableEntry[] retval = new LocalVariableEntry [_locals.Count];
				_locals.CopyTo (retval, 0);
				return retval;
			}
		}

		public void AddLocal (string name, FieldAttributes attributes, byte[] signature)
		{
			_locals.Add (new LocalVariableEntry (name, attributes, signature, CurrentBlock.Index));
		}

		public MethodBase MethodBase {
			get {
				return _method_base;
			}
		}

		public string FullName {
			get {
				return _method_base.DeclaringType.FullName + "." + _method_base.Name;
			}
		}

		public Type ReturnType {
			get {
				if (_method_base is MethodInfo)
					return ((MethodInfo)_method_base).ReturnType;
				else if (_method_base is ConstructorInfo)
					return _method_base.DeclaringType;
				else
					throw new NotSupportedException ();
			}
		}

		public ParameterInfo[] Parameters {
			get {
				if (_method_base == null)
					return new ParameterInfo [0];

				ParameterInfo [] retval = _method_base.GetParameters ();
				if (retval == null)
					return new ParameterInfo [0];
				else
					return retval;
			}
		}

		public SourceFile SourceFile {
			get {
				return _source_file;
			}
		}

		public int Token {
			get {
				if (_token != 0)
					return _token;
				else
					throw new NotSupportedException ();
			}
		}

		public bool HasSource {
			get {
				return _source_file != null;
			}
		}

		public LineNumberEntry Start {
			get {
				return _start;
			}
		}

		public LineNumberEntry End {
			get {
				return _end;
			}
		}

		public int NamespaceID {
			get {
				return _namespace_id;
			}
		}
	}

	public class MonoSymbolWriter : IMonoSymbolWriter
	{
		protected ModuleBuilder module_builder;
		protected ArrayList locals = null;
		protected ArrayList orphant_methods = null;
		protected ArrayList methods = null;
		protected Hashtable sources = null;
		private MonoSymbolFile file = null;

		internal SourceMethod[] Methods {
			get {
				SourceMethod[] retval = new SourceMethod [methods.Count];
				methods.CopyTo (retval);
				return retval;
			}
		}

		internal SourceFile[] Sources {
			get {
				SourceFile[] retval = new SourceFile [sources.Count];
				sources.Values.CopyTo (retval, 0);
				return retval;
			}
		}

		private SourceMethod current_method = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter (ModuleBuilder mb)
		{
			this.module_builder = mb;
			this.methods = new ArrayList ();
			this.sources = new Hashtable ();
			this.orphant_methods = new ArrayList ();
			this.locals = new ArrayList ();
			this.file = new MonoSymbolFile ();
		}

		public void Close ()
		{
			throw new InvalidOperationException ();
		}

		public byte[] CreateSymbolFile (AssemblyBuilder assembly_builder)
		{
			DoFixups (assembly_builder);

			return CreateOutput (assembly_builder);
		}

		public void CloseNamespace () {
		}

		// Create and return a new IMonoSymbolDocumentWriter.
		public ISymbolDocumentWriter DefineDocument (string url,
							     Guid language,
							     Guid languageVendor,
							     Guid documentType)
		{
			SourceFile source_info = new SourceFile (file, url);
			sources.Add (url, source_info);
			return source_info;
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
			throw new NotSupportedException ();
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
			throw new NotSupportedException ();
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

			current_method.AddLocal (name, attributes, signature);
		}

		public void DefineParameter (string name,
					     ParameterAttributes attributes,
					     int sequence,
					     SymAddressKind addrKind,
					     int addr1,
					     int addr2,
					     int addr3)
		{
			throw new NotSupportedException ();
		}

		public void DefineSequencePoints (ISymbolDocumentWriter document,
						  int[] offsets,
						  int[] lines,
						  int[] columns,
						  int[] endLines,
						  int[] endColumns)
		{
			throw new NotSupportedException ();
		}

		public void MarkSequencePoint (int offset, int line, int column)
		{
			if (current_method == null)
				return;

			LineNumberEntry source_line = new LineNumberEntry (line, offset);
			current_method.AddLine (source_line);
		}

		public void Initialize (IntPtr emitter, string filename, bool fFullBuild)
		{
			throw new NotSupportedException ();
		}

		public void OpenMethod (SymbolToken symbol_token)
		{
			throw new NotSupportedException ();
		}

		public void SetMethodSourceRange (ISymbolDocumentWriter startDoc,
						  int startLine, int startColumn,
						  ISymbolDocumentWriter endDoc,
						  int endLine, int endColumn)
		{
			throw new NotSupportedException ();
		}

		public void OpenMethod (ISymbolDocumentWriter document, int startLine, int startColumn,
					int endLine, int endColumn, MethodBase method, int namespace_id)
		{
			SourceFile source_info = document as SourceFile;

			if ((source_info == null) || (method == null))
				throw new NullReferenceException ();

			current_method = new SourceMethod (file, source_info, startLine, startColumn,
							   endLine, endColumn, method, namespace_id);

			methods.Add (current_method);
			source_info.AddMethod (current_method);
		}

		public void CloseMethod ()
		{
			current_method = null;
		}

		public int DefineNamespace (string name, ISymbolDocumentWriter document,
					    string[] using_clauses, int parent)
		{
			if ((document == null) || (using_clauses == null))
				throw new NullReferenceException ();
			if (!(document is SourceFile))
				throw new ArgumentException ();

			SourceFile source_info = (SourceFile) document;

			return source_info.DefineNamespace (name, using_clauses, parent);
		}

		public void OpenNamespace (string name)
		{
			throw new NotSupportedException ();
		}

		public int OpenScope (int startOffset)
		{
			if (current_method == null)
				return 0;

			current_method.StartBlock (startOffset);
			return 0;
		}

		public void CloseScope (int endOffset)
		{
			if (current_method == null)
				return;

			current_method.EndBlock (endOffset);
		}

		public void SetScopeRange (int scopeID, int startOffset, int endOffset)
		{
			throw new NotSupportedException ();
		}

		public void SetSymAttribute (SymbolToken parent, string name, byte[] data)
		{
			throw new NotSupportedException ();
		}

		public void SetUnderlyingWriter (IntPtr underlyingWriter)
		{
			throw new NotSupportedException ();
		}

		public void SetUserEntryPoint (SymbolToken entryMethod)
		{
			throw new NotSupportedException ();
		}

		public void UsingNamespace (string fullName)
		{
			throw new NotSupportedException ();
		}

		//
		// MonoSymbolWriter implementation
		//
		protected void DoFixups (Assembly assembly)
		{
			foreach (SourceMethod method in methods) {
				if (method._method_base is MethodBuilder) {
					MethodBuilder mb = (MethodBuilder) method._method_base;
					method._token = mb.GetToken ().Token;
				} else if (method._method_base is ConstructorBuilder) {
					ConstructorBuilder cb = (ConstructorBuilder) method._method_base;
					method._token = cb.GetToken ().Token;
				} else
					throw new NotSupportedException ();

				if (method.SourceFile == null)
					orphant_methods.Add (method);
			}
		}

		protected byte[] CreateOutput (Assembly assembly)
		{
			foreach (SourceMethod method in Methods) {
				if (!method.HasSource) {
					Console.WriteLine ("INGORING METHOD: {0}", method);
					continue;
				}

				method.SourceFile.DefineMethod (
					method.MethodBase, method.Token, method.Locals,
					method.Lines, method.Blocks, method.Start.Row, method.End.Row,
					method.NamespaceID);
			}

			return file.CreateSymbolFile ();
		}
	}
}

