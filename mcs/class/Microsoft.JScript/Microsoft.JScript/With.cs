//
// With.cs:
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

using System;
using System.Text;
using Microsoft.JScript.Vsa;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class With : AST {

		AST exp, stm;

		internal With (AST parent, AST exp, AST stm, int line_number)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
			this.line_number = line_number;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (exp.ToString () + " ");
			sb.Append (stm.ToString ());

			return sb.ToString ();
		}

		public static Object JScriptWith (object withObj, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (context);
			if (stm != null)
				r &= stm.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (exp != null)
				exp.Emit (ec);

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (With).GetMethod ("JScriptWith"));
			ig.Emit (OpCodes.Pop);

			ig.BeginExceptionBlock ();

			if (stm != null)
				stm.Emit (ec);

			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("PopScriptObject"));
			ig.Emit (OpCodes.Pop);			

			ig.EndExceptionBlock ();
		}
	}
}
