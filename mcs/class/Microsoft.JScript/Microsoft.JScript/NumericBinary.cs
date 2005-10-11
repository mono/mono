//
// NumericBinary.cs:
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

namespace Microsoft.JScript {

	public sealed class NumericBinary : BinaryOp {

		public NumericBinary (int operatorTok)
			: base (null, null, null, (JSToken) operatorTok, null)
		{			
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluateNumericBinary (object v1, object v2)
		{
			switch (op) {
			case JSToken.Minus:
				return Convert.ToNumber (v1) - Convert.ToNumber (v2);

			case JSToken.Multiply:
				return Convert.ToNumber (v1) * Convert.ToNumber (v2);

			case JSToken.Divide:
				return Convert.ToNumber (v1) / Convert.ToNumber (v2);

			case JSToken.Modulo:
				return Convert.ToNumber (v1) % Convert.ToNumber (v2);
			}

			Console.WriteLine ("v1 = {0}, v2 = {1}, op = {2}", v1, v2, op);
			throw new NotImplementedException ();
		}

		public static object DoOp (object v1, object v2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
