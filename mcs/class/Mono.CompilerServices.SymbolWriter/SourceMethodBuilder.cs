//
// SourceMethodBuilder.cs
//
// Authors:
//   Martin Baulig (martin@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections.Generic;

namespace Mono.CompilerServices.SymbolWriter
{
	public class SourceMethodBuilder
	{
		List<LocalVariableEntry> _locals;
		List<CodeBlockEntry> _blocks;
		List<ScopeVariable> _scope_vars;
		Stack<CodeBlockEntry> _block_stack;
		readonly List<LineNumberEntry> method_lines;

		readonly ICompileUnit _comp_unit;
		readonly int ns_id;
		readonly IMethodDef method;

		public SourceMethodBuilder (ICompileUnit comp_unit)
		{
			this._comp_unit = comp_unit;
			method_lines = new List<LineNumberEntry> ();
		}

		public SourceMethodBuilder (ICompileUnit comp_unit, int ns_id, IMethodDef method)
			: this (comp_unit)
		{
			this.ns_id = ns_id;
			this.method = method;
		}

		public void MarkSequencePoint (int offset, SourceFileEntry file, int line, int column, bool is_hidden)
		{
			MarkSequencePoint (offset, file, line, column, -1, -1, is_hidden);
		}

		public void MarkSequencePoint (int offset, SourceFileEntry file, int line, int column, int end_line, int end_column, bool is_hidden)
		{
			int file_idx = file != null ? file.Index : 0;
			var lne = new LineNumberEntry (file_idx, line, column, end_line, end_column, offset, is_hidden);

			if (method_lines.Count > 0) {
				var prev = method_lines[method_lines.Count - 1];

				//
				// Same offset cannot be used for multiple lines
				// 
				if (prev.Offset == offset) {
					//
					// Use the new location because debugger will adjust
					// the breakpoint to next line with sequence point
					//
					if (LineNumberEntry.LocationComparer.Default.Compare (lne, prev) > 0)
						method_lines[method_lines.Count - 1] = lne;

					return;
				}
			}

			method_lines.Add (lne);
		}

		public void StartBlock (CodeBlockEntry.Type type, int start_offset)
		{
			StartBlock (type, start_offset, _blocks == null ? 1 : _blocks.Count + 1);
		}

		public void StartBlock (CodeBlockEntry.Type type, int start_offset, int scopeIndex)
		{
			if (_block_stack == null) {
				_block_stack = new Stack<CodeBlockEntry> ();
			}
			
			if (_blocks == null)
				_blocks = new List<CodeBlockEntry> ();

			int parent = CurrentBlock != null ? CurrentBlock.Index : -1;

			CodeBlockEntry block = new CodeBlockEntry (
				scopeIndex, parent, type, start_offset);

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

		public LocalVariableEntry[] Locals {
			get {
				if (_locals == null)
					return new LocalVariableEntry [0];
				else {
					return _locals.ToArray ();
				}
			}
		}

		public ICompileUnit SourceFile {
			get {
				return _comp_unit;
			}
		}

		public void AddLocal (int index, string name)
		{
			if (_locals == null)
				_locals = new List<LocalVariableEntry> ();
			int block_idx = CurrentBlock != null ? CurrentBlock.Index : 0;
			_locals.Add (new LocalVariableEntry (index, name, block_idx));
		}

		public ScopeVariable[] ScopeVariables {
			get {
				if (_scope_vars == null)
					return new ScopeVariable [0];

				return _scope_vars.ToArray ();
			}
		}

		public void AddScopeVariable (int scope, int index)
		{
			if (_scope_vars == null)
				_scope_vars = new List<ScopeVariable> ();
			_scope_vars.Add (
				new ScopeVariable (scope, index));
		}

		public void DefineMethod (MonoSymbolFile file)
		{
			DefineMethod (file, method.Token);
		}

		public void DefineMethod (MonoSymbolFile file, int token)
		{
			var blocks = Blocks;
			if (blocks.Length > 0) {
				//
				// When index is provided by user it can be inserted in
				// any order but mdb format does not store its value. It
				// uses stored order as the index instead.
				//
				var sorted = new List<CodeBlockEntry> (blocks.Length);
				int max_index = 0;
				for (int i = 0; i < blocks.Length; ++i) {
					max_index = System.Math.Max (max_index, blocks [i].Index);
				}

				for (int i = 0; i < max_index; ++i) {
					var scope_index = i + 1;

					//
					// Common fast path
					//
					if (i < blocks.Length && blocks [i].Index == scope_index) {
						sorted.Add (blocks [i]);
						continue;
					}

					bool found = false;
					for (int ii = 0; ii < blocks.Length; ++ii) {
						if (blocks [ii].Index == scope_index) {
							sorted.Add (blocks [ii]);
							found = true;
							break;
						}
					}

					if (found)
						continue;

					//
					// Ideally this should never happen but with current design we can
					// generate scope index for unreachable code before reachable code
					//
					sorted.Add (new CodeBlockEntry (scope_index, -1, CodeBlockEntry.Type.CompilerGenerated, 0));
				}

				blocks = sorted.ToArray ();
				//for (int i = 0; i < blocks.Length; ++i) {
				//	if (blocks [i].Index - 1 != i)
				//			throw new ArgumentException ("CodeBlocks cannot be converted to mdb format");
				//}
			}

			var entry = new MethodEntry (
				file, _comp_unit.Entry, token, ScopeVariables,
				Locals, method_lines.ToArray (), blocks, null, MethodEntry.Flags.ColumnsInfoIncluded, ns_id);

			file.AddMethod (entry);
		}
	}
}
