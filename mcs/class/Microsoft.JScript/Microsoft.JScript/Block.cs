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

	public class Block : AST {

		internal ArrayList elems;
		private Hashtable ocurrences;

		Block ()
		{
			elems = new ArrayList ();
			ocurrences = new Hashtable ();
		}

		internal Block (int line_number)
			: this ()
		{
			this.line_number = line_number;
		}

		internal Block (AST parent)
			: this ()
		{
			this.parent = parent;
		}

		internal void Add (AST e)
		{
			if (e != null)
				elems.Add (e);
		}

		internal override void Emit (EmitContext ec)
		{
			int i, n = elems.Count;
			object e;

			//
			// Emit variable declarations first
			// because of posible free occurrences inside
			// a method. 
			//
			for (i = 0; i < n; i++) {
				e = elems [i];
				if (e is VariableStatement)
					((VariableStatement) e).EmitVariableDecls (ec);
			}

			//
			// Emit the function closure before any
			// expression because the ScriptFunction and
			// field created must be set properly before
			// any use. The body gets emitted later.
			//			
			for (i = 0; i < n; i++) {
				e = elems [i];
				if (e is FunctionDeclaration)
					((FunctionDeclaration) e).create_closure (ec);
			}
			
			//
			// Emit the rest of expressions and statements.
			//
			for (i = 0; i < n; i++) {
				e = elems [i];
				((AST) e).Emit (ec);
			}
		}

		internal override bool Resolve (IdentificationTable context)
		{
			AST e;
			bool no_effect;
			bool r = true;
			int i, n = elems.Count;

			if (parent == null || parent is FunctionDeclaration)
				no_effect = true;
			else
				no_effect = false;
			
			for (i = 0; i < elems.Count; i++) {
				e = (AST) elems [i];
				//
				// Add the variables to the symbol
				// tables. If a variable declaration
				// has an initializer we postpone the
				// resolve process of the initializer
				// until we have collected all the
				// variable declarations. 
				//
				if (e is VariableStatement)
					(e as VariableStatement).PopulateContext (context);
				else if (e is FunctionDeclaration) {
					//
					// In the case of function
					// declarations we add
					// function's name to the
					// table but we resolve its
					// body until later, as free
					// variables can be referenced
					// in function's body.
					//
					string name = ((FunctionDeclaration) e).func_obj.name;
					AST binding = (AST) context.Get (Symbol.CreateSymbol (name));

					if (binding == null) {
						ocurrences.Add (name, i);
						context.Enter (Symbol.CreateSymbol (((FunctionDeclaration) e).func_obj.name), new FunctionDeclaration ());
					} else {
						Console.WriteLine ("warning: JS1111: '{0}' has already been defined.", name);
						if (!(binding is FunctionDeclaration))
							throw new Exception ("error JS5040: '" + ((VariableDeclaration) binding).id + "' it's read only.");
						int k = (int) ocurrences [name];
						elems.RemoveAt (k);
						if (k < i)
							i -= 1;
						ocurrences [name] = i;
					}
				}
			}			
			n = elems.Count;
			
			for (i = 0; i < n; i++) {
				e = (AST) elems [i];
				if (e is Exp) 
					r &= ((Exp) e).Resolve (context, no_effect);
				else
					r &= e.Resolve (context);
			}
			return r;			
		}
	}
}
