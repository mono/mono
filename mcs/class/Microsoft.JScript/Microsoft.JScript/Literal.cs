//
// Literal.cs
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
using System.Reflection.Emit;
using System.Collections;

namespace Microsoft.JScript {

	internal class This : AST {

		internal This ()
		{
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class BooleanLiteral : Exp {

		internal bool val;

		internal BooleanLiteral (AST parent, bool val)
		{
			this.parent = parent;
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (val)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Box, typeof (System.Boolean));

			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}

	public class NumericLiteral : Exp {

		double val;

		internal NumericLiteral (AST parent, double val)
		{
			this.parent = parent;
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
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
			if (parent is Unary) {
				Unary tmp = parent as Unary;
				if (tmp.oper == JSToken.Minus)
					ig.Emit (OpCodes.Ldc_R8, (double) (val * -1));
			} else
				ig.Emit (OpCodes.Ldc_R8, (double) val);
			ig.Emit (OpCodes.Box, typeof (System.Double));
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}

	public class ObjectLiteral : Exp {
		
		internal ArrayList elems;
		
		internal ObjectLiteral (ArrayList elems)
		{
			this.elems = elems;
		}

		internal ObjectLiteral (AST parent)
		{
			this.parent = parent;
			elems = new ArrayList ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			foreach (AST ast in elems)
				r &= ast.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("GetOriginalObjectConstructor"));
			ig.Emit (OpCodes.Call, typeof (ObjectConstructor).GetMethod ("ConstructObject"));

			foreach (ObjectLiteralItem item in elems) {
				ig.Emit (OpCodes.Dup);
				item.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (JSObject).GetMethod ("SetMemberValue2"));
			}
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}

		internal void Add (ObjectLiteralItem item)
		{
			elems.Add (item);
		}
	}

	internal class ObjectLiteralItem : AST {
		internal string property_name;
		internal AST exp;

		internal ObjectLiteralItem (object obj)
		{
			if (obj != null)
				property_name = obj.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return exp.Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			ec.ig.Emit (OpCodes.Ldstr, property_name);
			exp.Emit (ec);
		}
	}

	public class PropertyName {
		string name;
		internal string Name {
			get { return name; }
			set { name = value; }
		}
	}

	internal class RegExpLiteral : AST {
		internal string re;
		internal string flags;

		internal RegExpLiteral (string re, string flags)
		{
			this.re = re;
			this.flags = flags;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}		
}
