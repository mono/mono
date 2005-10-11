//
// StrictEquality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc. (http://novell.com)
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

	public class StrictEquality : BinaryOp {

		internal StrictEquality (AST parent, AST left, AST right, JSToken op, Location location)
			: base (parent, left, right, op, location)
		{
		}

		public static bool JScriptStrictEquals (object v1, object v2)
		{
			IConvertible ic1 = v1 as IConvertible;
			IConvertible ic2 = v2 as IConvertible;

			TypeCode tc1 = Convert.GetTypeCode (v1, ic1);
			TypeCode tc2 = Convert.GetTypeCode (v2, ic2);

			bool both_numbers = Convert.IsNumberTypeCode (tc1) && Convert.IsNumberTypeCode (tc2);
			if (tc1 != tc2 && !both_numbers)
				return false;

			switch (tc1) {
			case TypeCode.DBNull:
			case TypeCode.Empty:
				return true;

			case TypeCode.Boolean:
				return ic1.ToBoolean (null) == ic2.ToBoolean (null);

			case TypeCode.String:
				return ic1.ToString (null) == ic2.ToString (null);

			case TypeCode.Object:
				if (v1 is ScriptFunction && v2 is ScriptFunction)
					return v1 == v2 || v1.Equals (v2);
				else
					return v1 == v2;

			default:
				if (both_numbers) {
					double num1;
					if (Convert.IsFloatTypeCode (tc1))
						num1 = ic1.ToDouble (null);
					else
						num1 = (double) ic1.ToInt64 (null);

					double num2;
					if (Convert.IsFloatTypeCode (tc2))
						num2 = ic2.ToDouble (null);
					else
						num2 = (double) ic2.ToInt64 (null);

					return num1 == num2;
				}
				Console.WriteLine ("StrictEquality, tc1 = {0}, tc2 = {1}", tc1, tc2);
				break;
			}
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (env);
			if (right != null)
				r &= right.Resolve (env);
			return r;
		}
		
		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			if (left != null) {
				left.Emit (ec);
				CodeGenerator.EmitBox (ec.ig, left);
			}
			if (right != null) {
				right.Emit (ec);
				CodeGenerator.EmitBox (ec.ig, right);
			}
			ec.ig.Emit (OpCodes.Call, typeof (StrictEquality).GetMethod ("JScriptStrictEquals"));
		}
	}
}
