//
// PostOrPrefixOperator.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class PostOrPrefixOperator : UnaryOp {

		bool prefix;

		public PostOrPrefixOperator (int operatorTok)
		{
			oper = (JSToken) operatorTok;
		}

		internal PostOrPrefixOperator (AST parent, AST operand, JSToken oper, bool prefix)
		{
			this.parent = parent;
			this.operand = operand;
			this.oper = oper;
			this.prefix = prefix;
		}

		public object EvaluatePostOrPrefix (ref object v)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return operand.Resolve (context);
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			if (operand is Exp)
				return ((Exp) operand).Resolve (context, no_effect);
			else
				return operand.Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			if (oper == JSToken.None)
				operand.Emit (ec);
			else {
				ILGenerator ig = ec.ig;
				Type post_prefix = typeof (PostOrPrefixOperator);
				LocalBuilder post_prefix_local = ig.DeclareLocal (post_prefix);
				LocalBuilder tmp_obj = ig.DeclareLocal (typeof (object));

				switch (this.oper) {
				case JSToken.Increment:
					if (prefix)
						ig.Emit (OpCodes.Ldc_I4_3);
					else 
						ig.Emit (OpCodes.Ldc_I4_1);
					break;
				case JSToken.Decrement:
					if (prefix)
						ig.Emit (OpCodes.Ldc_I4_2);
					else
						ig.Emit (OpCodes.Ldc_I4_0);
					break;
				}

				ig.Emit (OpCodes.Newobj, post_prefix.GetConstructor (new Type [] { typeof (int) }));
				ig.Emit (OpCodes.Stloc, post_prefix_local);

				if (operand is Identifier)
					((Identifier) operand).EmitLoad (ec);
				else throw new NotImplementedException ();

				ig.Emit (OpCodes.Stloc, tmp_obj);
				ig.Emit (OpCodes.Ldloc, post_prefix_local);
				ig.Emit (OpCodes.Ldloca_S, tmp_obj);
				ig.Emit (OpCodes.Call, post_prefix.GetMethod ("EvaluatePostOrPrefix"));

				//
				// if does not appear as a global expression
				//
				if (prefix && !(parent is ScriptBlock)) {
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Stloc, tmp_obj);
				}

				if (operand is Identifier)
					((Identifier) operand).EmitStore (ec);
				else throw new NotImplementedException ();

				//
				// If value will be used, load the
				// temp var that holded the value
				// before inc/dec was evaluated
				//
				if (!(parent is ScriptBlock || parent is FunctionDeclaration ||
				      parent is FunctionExpression || parent is Block))
					ig.Emit (OpCodes.Ldloc, tmp_obj);
			}
		}
	}
}
