//
// Mono.CSharp.Debugger/MonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// This is the default implementation of the System.Diagnostics.SymbolStore.ISymbolWriter
// interface.
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
using System.Runtime.CompilerServices;
using System.Collections;
using System.IO;
	
namespace Mono.CompilerServices.SymbolWriter
{
	public interface ISourceFile
	{
		SourceFileEntry Entry {
			get;
		}
	}

	public interface ICompileUnit
	{
		CompileUnitEntry Entry {
			get;
		}
	}

	public interface ISourceMethod
	{
		string Name {
			get;
		}

		int NamespaceID {
			get;
		}

		int Token {
			get;
		}
	}

	public class MonoSymbolWriter
	{
		ArrayList methods = null;
		ArrayList sources = null;
		ArrayList comp_units = null;
		protected readonly MonoSymbolFile file;
		string filename = null;
		
		LineNumberEntry [] current_method_lines;
		int current_method_lines_pos = 0;

		private SourceMethod current_method = null;

		public MonoSymbolWriter (string filename)
		{
			this.methods = new ArrayList ();
			this.sources = new ArrayList ();
			this.comp_units = new ArrayList ();
			this.file = new MonoSymbolFile ();

			this.filename = filename + ".mdb";
			
			this.current_method_lines = new LineNumberEntry [50];
		}

		public MonoSymbolFile SymbolFile {
			get { return file; }
		}

		public void CloseNamespace ()
		{ }

		public void DefineLocalVariable (int index, string name)
		{
			if (current_method == null)
				return;

			current_method.AddLocal (index, name);
		}

		public void DefineCapturedLocal (int scope_id, string name, string captured_name)
		{
			file.DefineCapturedVariable (scope_id, name, captured_name,
						     CapturedVariable.CapturedKind.Local);
		}

		public void DefineCapturedParameter (int scope_id, string name, string captured_name)
		{
			file.DefineCapturedVariable (scope_id, name, captured_name,
						     CapturedVariable.CapturedKind.Parameter);
		}

		public void DefineCapturedThis (int scope_id, string captured_name)
		{
			file.DefineCapturedVariable (scope_id, "this", captured_name,
						     CapturedVariable.CapturedKind.This);
		}

		public void DefineCapturedScope (int scope_id, int id, string captured_name)
		{
			file.DefineCapturedScope (scope_id, id, captured_name);
		}

		public void DefineScopeVariable (int scope, int index)
		{
			if (current_method == null)
				return;

			current_method.AddScopeVariable (scope, index);
		}

		[Obsolete]
		public void MarkSequencePoint (int offset, int file, int line, int column)
		{
			if (current_method == null)
				throw new ArgumentNullException ();

			if (current_method_lines_pos == current_method_lines.Length) {
				LineNumberEntry [] tmp = current_method_lines;
				current_method_lines = new LineNumberEntry [current_method_lines.Length * 2];
				Array.Copy (tmp, current_method_lines, current_method_lines_pos);
			}

			current_method_lines [current_method_lines_pos++] = new LineNumberEntry (file, line, offset);
		}

		[Obsolete]
		public void MarkSequencePoint (int offset, int file, int line, int column, bool is_hidden)
		{
			if (current_method == null)
				return;

			if (current_method_lines_pos == current_method_lines.Length) {
				LineNumberEntry [] tmp = current_method_lines;
				current_method_lines = new LineNumberEntry [current_method_lines.Length * 2];
				Array.Copy (tmp, current_method_lines, current_method_lines_pos);
			}

			current_method_lines [current_method_lines_pos++] = new LineNumberEntry (
				file, line, offset, is_hidden);
		}

		public void MarkSequencePoint (int offset, SourceFileEntry file, int line, int column,
					       bool is_hidden)
		{
			if (current_method == null)
				return;

			if (current_method_lines_pos == current_method_lines.Length) {
				LineNumberEntry [] tmp = current_method_lines;
				current_method_lines = new LineNumberEntry [current_method_lines.Length * 2];
				Array.Copy (tmp, current_method_lines, current_method_lines_pos);
			}

			int file_idx = file != null ? file.Index : 0;
			current_method_lines [current_method_lines_pos++] = new LineNumberEntry (
				file_idx, line, offset, is_hidden);
		}

		public void OpenMethod (ICompileUnit file, ISourceMethod method)
		{
			SourceMethod source = new SourceMethod (file, method);

			current_method = source;
			methods.Add (current_method);
		}

		public void SetRealMethodName (string name)
		{
			current_method.RealMethodName = name;
		}

		public void SetCompilerGenerated ()
		{
			current_method.SetCompilerGenerated ();
		}

		public void CloseMethod ()
		{
			if (current_method == null)
				return;
						
			current_method.SetLineNumbers (
				current_method_lines, current_method_lines_pos);
			current_method_lines_pos = 0;
			
			current_method = null;
		}

		public SourceFileEntry DefineDocument (string url)
		{
			SourceFileEntry entry = new SourceFileEntry (file, url);
			sources.Add (entry);
			return entry;
		}

		public SourceFileEntry DefineDocument (string url, byte[] guid, byte[] checksum)
		{
			SourceFileEntry entry = new SourceFileEntry (file, url, guid, checksum);
			sources.Add (entry);
			return entry;
		}

		public CompileUnitEntry DefineCompilationUnit (SourceFileEntry source)
		{
			CompileUnitEntry entry = new CompileUnitEntry (file, source);
			comp_units.Add (entry);
			return entry;
		}

		public int DefineNamespace (string name, CompileUnitEntry unit,
					    string[] using_clauses, int parent)
		{
			if ((unit == null) || (using_clauses == null))
				throw new NullReferenceException ();

			return unit.DefineNamespace (name, using_clauses, parent);
		}

		public int OpenScope (int start_offset)
		{
			if (current_method == null)
				return 0;

			current_method.StartBlock (CodeBlockEntry.Type.Lexical, start_offset);
			return 0;
		}

		public void CloseScope (int end_offset)
		{
			if (current_method == null)
				return;

			current_method.EndBlock (end_offset);
		}

		public void OpenCompilerGeneratedBlock (int start_offset)
		{
			if (current_method == null)
				return;

			current_method.StartBlock (CodeBlockEntry.Type.CompilerGenerated,
						   start_offset);
		}

		public void CloseCompilerGeneratedBlock (int end_offset)
		{
			if (current_method == null)
				return;

			current_method.EndBlock (end_offset);
		}

		public void StartIteratorBody (int start_offset)
		{
			current_method.StartBlock (CodeBlockEntry.Type.IteratorBody,
						   start_offset);
		}

		public void EndIteratorBody (int end_offset)
		{
			current_method.EndBlock (end_offset);
		}

		public void StartIteratorDispatcher (int start_offset)
		{
			current_method.StartBlock (CodeBlockEntry.Type.IteratorDispatcher,
						   start_offset);
		}

		public void EndIteratorDispatcher (int end_offset)
		{
			current_method.EndBlock (end_offset);
		}

		public void DefineAnonymousScope (int id)
		{
			file.DefineAnonymousScope (id);
		}

		public void WriteSymbolFile (Guid guid)
		{
			foreach (SourceMethod method in methods)
				method.DefineMethod (file);

			try {
				// We mmap the file, so unlink the previous version since it may be in use
				File.Delete (filename);
			} catch {
				// We can safely ignore
			}
			using (FileStream fs = new FileStream (filename, FileMode.Create, FileAccess.Write)) {
				file.CreateSymbolFile (guid, fs);
			}
		}

		protected class SourceMethod
		{
			LineNumberEntry [] lines;
			private ArrayList _locals;
			private ArrayList _blocks;
			private ArrayList _scope_vars;
			private Stack _block_stack;
			private string _real_name;
			private ISourceMethod _method;
			private ICompileUnit _comp_unit;
			private MethodEntry.Flags _method_flags;

			public SourceMethod (ICompileUnit comp_unit, ISourceMethod method)
			{
				this._comp_unit = comp_unit;
				this._method = method;
			}

			public void StartBlock (CodeBlockEntry.Type type, int start_offset)
			{
				if (_block_stack == null)
					_block_stack = new Stack ();
				if (_blocks == null)
					_blocks = new ArrayList ();

				int parent = CurrentBlock != null ? CurrentBlock.Index : -1;

				CodeBlockEntry block = new CodeBlockEntry (
					_blocks.Count + 1, parent, type, start_offset);

				_block_stack.Push (block);
				_blocks.Add (block);
			}

			public void EndBlock (int end_offset)
			{
				CodeBlockEntry block = (CodeBlockEntry) _block_stack.Pop ();
				block.Close (end_offset);
			}

			public CodeBlockEntry[] Blocks {
				get {
					if (_blocks == null)
						return new CodeBlockEntry [0];

					CodeBlockEntry[] retval = new CodeBlockEntry [_blocks.Count];
					_blocks.CopyTo (retval, 0);
					return retval;
				}
			}

			public CodeBlockEntry CurrentBlock {
				get {
					if ((_block_stack != null) && (_block_stack.Count > 0))
						return (CodeBlockEntry) _block_stack.Peek ();
					else
						return null;
				}
			}

			public LineNumberEntry[] Lines {
				get {
					return lines;
				}
			}

			public LocalVariableEntry[] Locals {
				get {
					if (_locals == null)
						return new LocalVariableEntry [0];
					else {
						LocalVariableEntry[] retval =
							new LocalVariableEntry [_locals.Count];
						_locals.CopyTo (retval, 0);
						return retval;
					}
				}
			}

			public void AddLocal (int index, string name)
			{
				if (_locals == null)
					_locals = new ArrayList ();
				int block_idx = CurrentBlock != null ? CurrentBlock.Index : 0;
				_locals.Add (new LocalVariableEntry (index, name, block_idx));
			}

			public ScopeVariable[] ScopeVariables {
				get {
					if (_scope_vars == null)
						return new ScopeVariable [0];

					ScopeVariable[] retval = new ScopeVariable [_scope_vars.Count];
					_scope_vars.CopyTo (retval);
					return retval;
				}
			}

			public void AddScopeVariable (int scope, int index)
			{
				if (_scope_vars == null)
					_scope_vars = new ArrayList ();
				_scope_vars.Add (
					new ScopeVariable (scope, index));
			}

			public string RealMethodName {
				get { return _real_name; }
				set { _real_name = value; }
			}

			public void SetCompilerGenerated ()
			{
				_method_flags |= MethodEntry.Flags.IsCompilerGenerated;
			}

			public ICompileUnit SourceFile {
				get { return _comp_unit; }
			}

			public ISourceMethod Method {
				get { return _method; }
			}

			internal void SetLineNumbers (LineNumberEntry [] lns, int count)
			{
				lines = new LineNumberEntry [count];
				Array.Copy (lns, lines, count);
			}

			public void DefineMethod (MonoSymbolFile file)
			{
				MethodEntry entry = new MethodEntry (
					file, _comp_unit.Entry, _method.Token, ScopeVariables,
					Locals, Lines, Blocks, RealMethodName, _method_flags,
					_method.NamespaceID);

				file.AddMethod (entry);
			}
		}
	}
}
