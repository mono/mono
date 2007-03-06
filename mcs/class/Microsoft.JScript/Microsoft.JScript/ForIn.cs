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

	public class ForIn : AST, ICanModifyContext {

		AST lhs, obj, body;
		
		internal ForIn (AST parent, AST lhs, AST obj, AST body, Location location)
			: base (parent, location)
		{
			this.lhs = lhs;
			this.obj = obj;
			this.body = body;
		}

		public static IEnumerator JScriptGetEnumerator (object coll)
		{
			IEnumerable e = coll as IEnumerable;
			if (e == null)
				throw new JScriptException(JSError.NotCollection);
			
			return e.GetEnumerator();
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (lhs is ICanModifyContext)
				((ICanModifyContext) lhs).PopulateContext (env, ns);

			if (body is ICanModifyContext)
				((ICanModifyContext) body).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (lhs is ICanModifyContext)
				((ICanModifyContext) lhs).EmitDecls (ec);

			if (body is ICanModifyContext)
				((ICanModifyContext) body).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (lhs != null)
				if (lhs is VariableStatement)
					((ICanModifyContext) lhs).PopulateContext (env, String.Empty);
				else
					r &= lhs.Resolve (env);
			if (obj != null)
				r &= obj.Resolve (env);

			if (body != null) {
				if (body is Exp)
					r &= ((Exp) body).Resolve (env, true);
				else
					r &= body.Resolve (env);
			}
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			bool varStm = lhs is VariableStatement;
			object var = null;

			if (varStm) {
				VariableStatement stm = (VariableStatement) lhs;
				ig.Emit (OpCodes.Ldnull);
				var = TypeManager.Get (((VariableDeclaration) stm.var_decls [0]).id);
				set_builder (ig, var);
			}
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

			if (varStm)
				set_builder (ig, var);
			else {
				if (lhs is Expression) {
					AST ast = ((Expression) lhs).Last;
					if (ast is Identifier)
						((Identifier) ast).EmitStore (ec);
					else 
						throw new NotImplementedException ();
				} else
 					throw new NotImplementedException ();
			}

			ig.Emit (OpCodes.Br, init_loop);
			ig.MarkLabel (exit);
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
