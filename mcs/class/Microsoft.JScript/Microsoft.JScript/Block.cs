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

		Block ()
		{
			elems = new ArrayList ();
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

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (AST a in elems)
				if (a != null)
					sb.Append (a.ToString () + " ");

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			int i, n = elems.Count;
			object e;
			
			for (i = 0; i < n; i++) {
				e = elems [i];
				if (e is FunctionDeclaration)
					((FunctionDeclaration) e).Emit (ec);
			}
			for (i = 0; i < n; i++) {
				e = elems [i];
				if (!(e is FunctionDeclaration))
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
	
