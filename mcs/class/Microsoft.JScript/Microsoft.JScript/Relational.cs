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
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Relational : BinaryOp {

		internal Relational (AST parent, AST left, AST right, JSToken op)
			: base (left, right, op)
		{
			this.parent = parent;
		}
		
		public Relational (int operatorTok)
			: base (null, null, (JSToken) operatorTok)
		{		
		}

		public double EvaluateRelational (object v1, object v2)
		{
			return -1;
		}


		public static double JScriptCompare (object v1, object v2)
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
			if (left != null)
				left.Resolve (context);

			if (right != null)
				right.Resolve (context);

			return true;			
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (op == JSToken.None &&  right == null) {
				left.Emit (ec);
				return;
			} else if (op == JSToken.InstanceOf) {
				if (left != null)
					left.Emit (ec);
				if (right != null)
					right.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (InstanceOf).GetMethod ("JScriptInstanceof"));
				return;
			} else 	if (op == JSToken.In) {
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
			
			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);

			ig.Emit (OpCodes.Call, t.GetMethod ("EvaluateRelational"));

			if (no_effect) {
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Conv_R8);

				Label a, b;
				a = ig.DefineLabel ();
				b = ig.DefineLabel ();
				
				switch (op) {
				case JSToken.GreaterThan:
					ig.Emit (OpCodes.Bgt_S, a);
					break;
				case JSToken.LessThan:
					ig.Emit (OpCodes.Blt_S, a);
					break;
				case JSToken.LessThanEqual:
					ig.Emit (OpCodes.Ble_S, a);
					break;
				case JSToken.GreaterThanEqual:
					ig.Emit (OpCodes.Bge_S, a);
					break;
				}			

				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Br_S, b);
				ig.MarkLabel (a);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.MarkLabel (b);

				if (no_effect)
					ig.Emit (OpCodes.Pop);
				else
					ig.Emit (OpCodes.Box, typeof (bool));
			} 
		}
	}
}
