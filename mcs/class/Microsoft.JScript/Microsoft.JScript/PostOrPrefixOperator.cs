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
using System.Diagnostics;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class PostOrPrefixOperator : UnaryOp {

		bool prefix;

		public PostOrPrefixOperator (int operatorTok)
			: base (null, null)
		{
			oper = (JSToken) operatorTok;
		}

		internal PostOrPrefixOperator (AST parent, AST operand, JSToken oper, bool prefix, Location location)
			: base (parent, location)
		{
			this.operand = operand;
			this.oper = oper;
			this.prefix = prefix;
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluatePostOrPrefix (ref object v)
		{
			double value = Convert.ToNumber (v);
			v = value;
			int oper = (int) this.oper;
			if (oper % 2 == 1) /* prefix? */
				return value + 1;
			else
				return value - 1;
		}

		internal override bool Resolve (Environment env)
		{
			return operand.Resolve (env);
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			if (operand is Exp)
				return ((Exp) operand).Resolve (env, no_effect);
			else
				return operand.Resolve (env);
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

				Binary assign = null;
				if (operand is Identifier)
					((Identifier) operand).EmitLoad (ec);
				else if (operand is Binary) {
					Binary binary = operand as Binary;
					binary.no_effect = false;
					binary.assign = false;
					assign = new Binary (binary.parent, binary.left, binary.right, binary.op, binary.location);
					assign.assign = true;
					assign.late_bind = true;
					assign.no_effect = false;
					if (binary.op == JSToken.LeftBracket || binary.op == JSToken.AccessField) {
						binary.Emit (ec);
						ig.Emit (OpCodes.Box, typeof (object));
					} else
						throw new NotImplementedException (String.Format ("Unhandled binary op {0}", operand));
				} else {
					Console.WriteLine ("PostOrPrefixOperator: prefix = {0}, oper = {1}, operand = {2}",
						prefix, oper, operand.GetType ());
					throw new NotImplementedException ();
				}

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
				else if (operand is Binary)
					assign.Emit (ec);
				else
					throw new NotImplementedException ();

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
