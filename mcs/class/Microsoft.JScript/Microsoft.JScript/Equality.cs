//
// Equality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
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
using System.Text;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Equality : BinaryOp {

		internal Equality (AST parent, AST left, AST right, JSToken op)
		{
			this.left = left;
			this.right = right;
			this.op = op;
		}

		public Equality (Context context, AST oper1, AST oper2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		public Equality (int i)
		{
			op = (JSToken) i;
		}

		public bool EvaluateEquality (object v1, object v2)
		{
			return false;
		}


		public static bool JScriptEquals (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (left.ToString ());

			if (op != JSToken.None)
				sb.Append (op + " ");

			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null)
				r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return  Resolve (context);
		}	       

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder local_builder;

			if (op != JSToken.None) {
				Type t = typeof (Equality);				
				local_builder = ig.DeclareLocal (t);
				if (op == JSToken.Equal)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 53);
				else if (op == JSToken.NotEqual)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 54);
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {typeof (int)}));
				ig.Emit (OpCodes.Stloc, local_builder);
				ig.Emit (OpCodes.Ldloc, local_builder);
			}

			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);			       
			
			if (op == JSToken.Equal || op == JSToken.NotEqual) {
				ig.Emit (OpCodes.Call, typeof (Equality).GetMethod ("EvaluateEquality"));

				if (no_effect) {
					Label t_lbl = ig.DefineLabel ();
					Label f_lbl = ig.DefineLabel ();

					if (op == JSToken.Equal)
						ig.Emit (OpCodes.Brtrue_S, t_lbl);
					else if (op == JSToken.NotEqual)
						ig.Emit (OpCodes.Brfalse_S, t_lbl);
					
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Br_S, f_lbl);
					ig.MarkLabel (t_lbl);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.MarkLabel (f_lbl);
					ig.Emit (OpCodes.Pop);
				}				
			}
		}
	}
}
