//
// Relational.cs:
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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Relational : BinaryOp {

		internal Relational (AST parent, AST left, AST right, JSToken op, Location location)
			: base (parent, left, right, op, location)
		{
		}
		
		public Relational (int operatorTok)
			: base (null, null, null, (JSToken) operatorTok, null)
		{		
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public double EvaluateRelational (object v1, object v2)
		{
			return JScriptCompare (v1, v2);
		}


		public static double JScriptCompare (object v1, object v2)
		{
			object p1 = Convert.ToPrimitive (v1, null);
			object p2 = Convert.ToPrimitive (v2, null);
			if (Convert.IsString (p1) && Convert.IsString (p2)) {
				string s1 = Convert.ToString (p1);
				string s2 = Convert.ToString (p2);
				return s1.CompareTo (s2);
			} else {
				double n1 = Convert.ToNumber (p1);
				double n2 = Convert.ToNumber (p2);
				return n1 - n2;
			}
		}

		internal override bool Resolve (Environment env)
		{
			if (left != null)
				left.Resolve (env);

			if (right != null)
				right.Resolve (env);

			return true;			
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (op == JSToken.None &&  right == null) {
				left.Emit (ec);
				return;
			} else if (op == JSToken.Instanceof) {
				if (left != null) {
					left.Emit (ec);
					CodeGenerator.EmitBox (ig, left);
				}
				if (right != null) {
					right.Emit (ec);
					CodeGenerator.EmitBox (ig, right);
				}
				ig.Emit (OpCodes.Call, typeof (Instanceof).GetMethod ("JScriptInstanceof"));
				ig.Emit (OpCodes.Box, typeof (Boolean));
				return;
			} else if (op == JSToken.In) {
				if (left != null)
					left.Emit (ec);
				if (right != null)
					right.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (In).GetMethod ("JScriptIn"));
				return;
			} 
			Type t = typeof (Relational);						
			LocalBuilder loc = ig.DeclareLocal (t);			
			ConstructorInfo ctr_info;
			
			switch (op) {
			case JSToken.GreaterThan:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 57);
				break;
			case JSToken.LessThan:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 58);
				break;
			case JSToken.LessThanEqual:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 59);
				break;
			case JSToken.GreaterThanEqual:
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 60);
				break;
			}
			
			ctr_info = typeof (Relational).GetConstructor (new Type [] { typeof (Int32) });
			ig.Emit (OpCodes.Newobj, ctr_info);
			ig.Emit (OpCodes.Stloc, loc);
			ig.Emit (OpCodes.Ldloc, loc);
			
			if (left != null) {
				left.Emit (ec);
				CodeGenerator.EmitBox (ig, left);
			}
			if (right != null) {
				right.Emit (ec);
				CodeGenerator.EmitBox (ig, right);
			}
			
			ig.Emit (OpCodes.Call, t.GetMethod ("EvaluateRelational"));

			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}
}
