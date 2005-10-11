//
// NumericUnary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// Copyright (C) 2005, Novell Inc (http://novell.com)
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

	public sealed class NumericUnary : UnaryOp {

		public NumericUnary (int operatorTok)
			: base (null, null)
		{
			oper = (JSToken) operatorTok;
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluateUnary (object v)
		{
			switch (oper) {
			case JSToken.Minus:
				return -Convert.ToNumber (v);

			case JSToken.Plus:
				return Convert.ToNumber (v);

			case JSToken.BitwiseNot:
				return ~Convert.ToInt32 (v);

			case JSToken.LogicalNot:
				return !Convert.ToBoolean (v);
			}

			Console.WriteLine ("EvaluateUnary: unknown oper {0}", oper);
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
