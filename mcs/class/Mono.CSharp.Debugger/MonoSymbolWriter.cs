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
		protected ArrayList locals = null;
		protected ArrayList methods = null;
		protected ArrayList sources = null;
		protected readonly MonoSymbolFile file;
		private string filename = null;
		
		LineNumberEntry [] current_method_lines;
		int current_method_lines_pos = 0;

		internal ISourceFile[] Sources {
			get {
				ISourceFile[] retval = new ISourceFile [sources.Count];
				sources.CopyTo (retval, 0);
				return retval;
			}
		}

		private SourceMethod current_method = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter (string filename)
		{
			this.methods = new ArrayList ();
			this.sources = new ArrayList ();
			this.locals = new ArrayList ();
			this.file = new MonoSymbolFile ();

			this.filename = filename + ".mdb";
			
			this.current_method_lines = new LineNumberEntry [50];
		}

		public void CloseNamespace ()
		{ }

		public void DefineLocalVariable (string name, byte[] signature)
		{
			if (current_method == null)
				return;

			current_method.AddLocal (name, signature);
		}

		public void MarkSequencePoint (int offset, int line, int column)
		{
			if (current_method == null)
				return;

			if (current_method_lines_pos == current_method_lines.Length) {
				LineNumberEntry [] tmp = current_method_lines;
				current_method_lines = new LineNumberEntry [current_method_lines.Length * 2];
				Array.Copy (tmp, current_method_lines, current_method_lines_pos);
			}
			
			current_method_lines [current_method_lines_pos++] = new LineNumberEntry (line, offset);
		}

		public void OpenMethod (ISourceFile file, ISourceMethod method,
					int startRow, int startColumn,
					int endRow, int endColumn)
		{
			SourceMethod source = new SourceMethod (
				file, method, startRow, startColumn, endRow, endColumn);

			current_method = source;
			methods.Add (current_method);
		}

		public void CloseMethod ()
		{
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

		public int DefineNamespace (string name, SourceFileEntry source,
					    string[] using_clauses, int parent)
		{
			if ((source == null) || (using_clauses == null))
				throw new NullReferenceException ();

			return source.DefineNamespace (name, using_clauses, parent);
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

		public void WriteSymbolFile (Guid guid)
		{
			foreach (SourceMethod method in methods) {
				method.SourceFile.Entry.DefineMethod (
					method.Method.Name, method.Method.Token,
					method.Locals, method.Lines, method.Blocks,
					method.Start.Row, method.End.Row,
					method.Method.NamespaceID);
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
			private Stack _block_stack;
			private int next_block_id = 0;
			private ISourceMethod _method;
			private ISourceFile _file;
			private LineNumberEntry _start, _end;

			private LexicalBlockEntry _implicit_block;

			public SourceMethod (ISourceFile file, ISourceMethod method,
					     int startLine, int startColumn,
					     int endLine, int endColumn)
			{
				this._file = file;
				this._method = method;

				this._start = new LineNumberEntry (startLine, 0);
				this._end = new LineNumberEntry (endLine, 0);

				this._implicit_block = new LexicalBlockEntry (0, 0);
			}

			public void StartBlock (int startOffset)
			{
				LexicalBlockEntry block = new LexicalBlockEntry (
					++next_block_id, startOffset);
				if (_block_stack == null)
					_block_stack = new Stack ();
				_block_stack.Push (block);
				if (_blocks == null)
					_blocks = new ArrayList ();
				_blocks.Add (block);
			}

			public void EndBlock (int endOffset)
			{
				LexicalBlockEntry block =
					(LexicalBlockEntry) _block_stack.Pop ();

				block.Close (endOffset);
			}

			public LexicalBlockEntry[] Blocks {
				get {
					if (_blocks == null)
						return new LexicalBlockEntry [0];
					else {
						LexicalBlockEntry[] retval =
							new LexicalBlockEntry [_blocks.Count];
						_blocks.CopyTo (retval, 0);
						return retval;
					}
				}
			}

			public LexicalBlockEntry CurrentBlock {
				get {
					if ((_block_stack != null) && (_block_stack.Count > 0))
						return (LexicalBlockEntry) _block_stack.Peek ();
					else
						return _implicit_block;
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

			public void AddLocal (string name, byte[] signature)
			{
				if (_locals == null)
					_locals = new ArrayList ();
				_locals.Add (new LocalVariableEntry (
						     name, signature, CurrentBlock.Index));
			}

			public ISourceFile SourceFile {
				get { return _file; }
			}

			public ISourceMethod Method {
				get { return _method; }
			}

			public LineNumberEntry Start {
				get { return _start; }
			}

			public LineNumberEntry End {
				get { return _end; }
			}

			//
			// Passes on the lines from the MonoSymbolWriter. This method is
			// free to mutate the lns array, and it does.
			//
			internal void SetLineNumbers (LineNumberEntry [] lns, int count)
			{
				int pos = 0;

				int last_offset = -1;
				int last_row = -1;
				for (int i = 0; i < count; i++) {
					LineNumberEntry line = lns [i];

					if (line.Offset > last_offset) {
						if (last_row >= 0)
							lns [pos++] = new LineNumberEntry (
								last_row, last_offset);

						last_row = line.Row;
						last_offset = line.Offset;
					} else if (line.Row > last_row) {
						last_row = line.Row;
					}
				}
			
				lines = new LineNumberEntry [count + ((last_row >= 0) ? 1 : 0)];
				Array.Copy (lns, lines, pos);
				if (last_row >= 0)
					lines [pos] = new LineNumberEntry (
						last_row, last_offset);
			}
		}
	}
}
