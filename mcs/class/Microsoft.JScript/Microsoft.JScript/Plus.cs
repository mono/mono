//
// Plus.cs:
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
using System.Globalization;

namespace Microsoft.JScript {

	public sealed class Plus : BinaryOp {

		public Plus ()
			: base (null, null, null, JSToken.Plus, null)
		{
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluatePlus (object v1, object v2)
		{
			object val1 = Convert.ToPrimitive (v1, null);
			object val2 = Convert.ToPrimitive (v2, null);
			if (Convert.IsString (val1) || Convert.IsString (val2))
				return Convert.ToString (val1) + Convert.ToString (val2);
			else
				return Convert.ToNumber (val1) + Convert.ToNumber (val2);
		}

		public static object DoOp (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
