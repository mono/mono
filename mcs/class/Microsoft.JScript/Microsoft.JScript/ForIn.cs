//
// ForIn.cs:
//
// Author: 
//	Cesar Octavio Lopez Nataren
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

using System;
using System.Collections;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class ForIn : AST {

		AST lhs, obj, body;
		
		internal ForIn (AST parent, int line_number, AST lhs, AST obj, AST body)
		{
			this.parent = parent;
			this.line_number = line_number;
			this.lhs = lhs;
			this.obj = obj;
			this.body = body;
		}

		public static IEnumerator JScriptGetEnumerator (object coll)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (lhs != null)
				if (lhs is VariableStatement)
					((VariableStatement) lhs).PopulateContext (context);
				else
					r &= lhs.Resolve (context);
			if (obj != null)
				r &= obj.Resolve (context);

			if (body != null)
				r &= body.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig;
			
			if (lhs is VariableStatement) {
				VariableStatement stm = (VariableStatement) lhs;
				stm.EmitVariableDecls (ec);
				ig = ec.ig;
				ig.Emit (OpCodes.Ldnull);
				object var = TypeManager.Get (((VariableDeclaration) stm.var_decls [0]).id);
				set_builder (ig, var);

				if (obj != null)
					obj.Emit (ec);

				CodeGenerator.load_engine (InFunction, ig);

				Type convert = typeof (Convert);
				ig.Emit (OpCodes.Call, convert.GetMethod ("ToForInObject"));
				ig.Emit (OpCodes.Call, typeof (ForIn).GetMethod ("JScriptGetEnumerator"));
				Type ienumerator = typeof (IEnumerator);
				LocalBuilder iter = ig.DeclareLocal (ienumerator);
				LocalBuilder current = ig.DeclareLocal (typeof (object));

				ig.Emit (OpCodes.Stloc, iter);
				
				Label init_loop = ig.DefineLabel ();
				Label move_next = ig.DefineLabel ();
				Label exit = ig.DefineLabel ();
								
				ig.Emit (OpCodes.Br, move_next);
				ig.MarkLabel (init_loop);

				if (body != null)
					body.Emit (ec);

				ig.MarkLabel (move_next);

				ig.Emit (OpCodes.Ldloc, iter);
				ig.Emit (OpCodes.Callvirt, ienumerator.GetMethod ("MoveNext"));
				
				ig.Emit (OpCodes.Brfalse, exit);

				ig.Emit (OpCodes.Ldloc, iter);
				ig.Emit (OpCodes.Callvirt, ienumerator.GetProperty ("Current").GetGetMethod ());
				ig.Emit (OpCodes.Stloc, current);
				ig.Emit (OpCodes.Ldloc, current);

				set_builder (ig, var);

				ig.Emit (OpCodes.Br, init_loop);
				ig.MarkLabel (exit);
			} else
				throw new NotImplementedException ();
		}

		void set_builder (ILGenerator ig, object builder)
		{
			if (builder is FieldBuilder)
				ig.Emit (OpCodes.Stsfld, (FieldBuilder) builder);
			else if (builder is LocalBuilder)
				ig.Emit (OpCodes.Stloc, (LocalBuilder) builder);
		}
	}
}
