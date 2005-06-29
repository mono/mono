//
// ArithmeticExpressions.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal class Negative : UnaryExpression {
		public Negative (IExpression e) : base (e) {}
	
		override public object Eval (DataRow row)
		{
			return Numeric.Negative ((IConvertible)expr.Eval (row));
		}
	}
	
	internal class ArithmeticOperation : BinaryOpExpression {
		public ArithmeticOperation (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			object obj1 = expr1.Eval (row);
			if (obj1 == DBNull.Value || obj1 == null)
				return obj1;
			object obj2 = expr2.Eval (row);
			if (obj2 == DBNull.Value || obj2 == null)
				return obj2;
		
			if (op == Operation.ADD && (obj1 is string || obj2 is string))
				return obj1.ToString () + obj2.ToString ();
		
			IConvertible o1 = (IConvertible)obj1;
			IConvertible o2 = (IConvertible)obj2;
			
			switch (op) {
			case Operation.ADD:
				return Numeric.Add (o1, o2);
			case Operation.SUB:
				return Numeric.Subtract (o1, o2);
			case Operation.MUL:
				return Numeric.Multiply (o1, o2);
			case Operation.DIV:
				return Numeric.Divide (o1, o2);
			case Operation.MOD:
				return Numeric.Modulo (o1, o2);
			default:
				return 0;
			}
		}
	}
}
