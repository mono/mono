//
// BooleanExpressions.cs
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
	internal class Negation : UnaryExpression {
		public Negation (IExpression e) : base (e) {}
	
		override public object Eval (DataRow row)
		{
			object o = expr.Eval (row);
			if (o == DBNull.Value)
				return o;
			return !((bool)o);
		}

		override public bool EvalBoolean (DataRow row)
		{
			return !expr.EvalBoolean (row);
		}
	}
	
	internal class BoolOperation : BinaryOpExpression {
		public BoolOperation (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			return EvalBoolean (row);
		}

		override public bool EvalBoolean (DataRow row)
		{
			if (op == Operation.OR)
				return (expr1.EvalBoolean (row)) || (expr2.EvalBoolean (row));
			if (op == Operation.AND)
				return (expr1.EvalBoolean (row)) && (expr2.EvalBoolean (row));
			return false;
		}
	}
}
