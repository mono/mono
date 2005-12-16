//
// BinaryOp.cs:
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
using System.Reflection;

namespace Microsoft.JScript {
	public abstract class BinaryOp : Exp {
		protected AST operand1, operand2;
		protected MethodInfo operatorMeth;
		protected JSToken operatorTok;
		protected Type type1, type2;

		protected MethodInfo GetOperator (IReflect ir1, IReflect ir2)
		{
			throw new NotImplementedException ();
		}

		internal BinaryOp (AST parent, AST left, AST right, JSToken op, Location location)
			: base (parent, location)
		{
			operand1 = left;
			operand2 = right;
			operatorTok = op;
		}

		internal JSToken op {
			get { return operatorTok; }
		}

		internal AST left {
			get { return operand1; }
			set { operand1 = value; }
		}

		internal AST right {
			get { return operand2; }
			set { operand2 = value; }
		}
	}
}
