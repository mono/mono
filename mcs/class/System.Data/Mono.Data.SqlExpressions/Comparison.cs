//
// Comparison.cs
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
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal class Comparison : BinaryOpExpression {
		public Comparison (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			return EvalBoolean (row);
		}

		public override bool EvalBoolean (DataRow row)
		{
			IComparable o1 = expr1.Eval (row) as IComparable;
			IComparable o2 = expr2.Eval (row) as IComparable;

			if (o1 == null || o2 == null) {
				if (o1 == null && o2 == null)
					return (op == Operation.EQ);
				else
					return (op == Operation.NE);
			}

			switch(Compare (o1, o2, row.Table.CaseSensitive)) {
			case -1:
				return (op == Operation.NE || op == Operation.LE || op == Operation.LT);
			case 0:
			default:
				return (op == Operation.EQ || op == Operation.LE || op == Operation.GE);
			case 1:
				return (op == Operation.NE || op == Operation.GE || op == Operation.GT);
			}
		}
			
		internal static int Compare (IComparable o1, IComparable o2, bool caseSensitive)
		{
			//TODO: turn this "conversion pipeline" into something nicer

			try {
				if (o1 is string && Numeric.IsNumeric (o2))
						o1 = (IComparable)Convert.ChangeType (o1, o2.GetType ());
				if (o2 is string && Numeric.IsNumeric (o1))
						o2 = (IComparable)Convert.ChangeType (o2, o1.GetType ());
			} catch (FormatException) {
				throw new EvaluateException (String.Format ("Cannot perform compare operation on {0} and {1}.", o1.GetType(), o2.GetType()));
			}

			if (o1 is string && o2 is string && !caseSensitive) {
				o1 = ((string)o1).ToLower();
				o2 = ((string)o2).ToLower();
			}
			
			if (o1.GetType () != o2.GetType ())
				o2 = (IComparable)Convert.ChangeType (o2, o1.GetType ());

			return o1.CompareTo (o2);
		}
	}
}
