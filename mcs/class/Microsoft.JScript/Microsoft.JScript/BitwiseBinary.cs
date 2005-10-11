//
// BitwiseBinary.cs:
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

	public sealed class BitwiseBinary : BinaryOp {

		public BitwiseBinary (int operatorTok)
			: base (null, null, null, (JSToken) operatorTok, null)
		{
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluateBitwiseBinary (object v1, object v2)
		{
			int num1 = Convert.ToInt32 (v1);
			int num2 = Convert.ToInt32 (v2);

			switch (operatorTok) {
			case JSToken.BitwiseAnd:
				return num1 & num2;
			case JSToken.BitwiseXor:
				return num1 ^ num2;
			case JSToken.BitwiseOr:
				return num1 | num2;
			case JSToken.LeftShift:
				return num1 << num2;
			case JSToken.RightShift:
				return num1 >> num2;
			case JSToken.UnsignedRightShift:
				return UnsignedRightShift (num1, num2);
			}

			Console.WriteLine ("EvaluateBitwiseBinary: operatorTok = {0}", operatorTok);
			throw new NotImplementedException ();
		}

		internal static uint UnsignedRightShift (int num1, int num2)
		{
			return (uint) num1 >> num2;
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
