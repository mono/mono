//
// Block.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript {

	public class Block : AST, ICanModifyContext {

		internal ArrayList elems;

		internal Block (AST parent, Location location)
			: base (parent, location)
		{
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			if (e != null)
				elems.Add (e);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			//
			// Emit variable declarations and function's closure first
			// because of posible free occurrences inside a method. 
			//
			foreach (AST ast in elems)
				if (ast is FunctionDeclaration)
					((FunctionDeclaration) ast).create_closure (ec);
				else if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);
		}

		internal override void Emit (EmitContext ec)
		{
			int n = elems.Count;
			object e;

			//
			// Emit the rest of expressions and statements.
			//
			for (int i = 0; i < n; i++) {
				e = elems [i];
				((AST) e).Emit (ec);
			}
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			AST ast;
			for (int i = 0; i < elems.Count; i++) {
				ast = (AST) elems [i];
				if (ast is FunctionDeclaration) {
					string name = ((FunctionDeclaration) ast).func_obj.name;
					AST binding = (AST) env.Get (ns, Symbol.CreateSymbol (name));

					if (binding == null)
						SemanticAnalyser.Ocurrences.Enter (ns, Symbol.CreateSymbol (name), new DeleteInfo (i, this));
					else {
						DeleteInfo delete_info = (DeleteInfo) SemanticAnalyser.Ocurrences.Get (ns, Symbol.CreateSymbol (name));
						if (delete_info != null) {
							delete_info.Block.elems.RemoveAt (delete_info.Index);
							SemanticAnalyser.Ocurrences.Remove (ns, Symbol.CreateSymbol (name));
							if (delete_info.Block == this)
								if (delete_info.Index < i)
									i--;

							SemanticAnalyser.Ocurrences.Enter (ns, Symbol.CreateSymbol (name), new DeleteInfo (i, this));
						}
					}
				}
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);
			}
		}

		internal override bool Resolve (Environment env)
		{
			AST e;
			bool r = true;
			int i, n = elems.Count;

			for (i = 0; i < n; i++) {
				e = (AST) elems [i];
				if (e is Exp) 
					r &= ((Exp) e).Resolve (env, true);
				else 
					r &= e.Resolve (env);
			}
			return r;
		}
	}

	internal class DeleteInfo {
		private int index;
		private Block block;

		internal DeleteInfo (int index, Block block)
		{
			this.index = index;
			this.block = block;
		}

		internal int Index {
			get { return index; }
		}

		internal Block Block {
			get { return block; }
		}
	}
}
