//
// Comparison.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Data;

namespace Mono.Data.SqlExpressions {
	public class Comparison : BinaryOpExpression {
		public Comparison (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			IComparable o1 = (IComparable)expr1.Eval (row);
			IComparable o2 = (IComparable)expr2.Eval (row);

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
			} catch (Exception) {
				throw new EvaluateException("Comparison of numeric and non-numeric values is not allowed.");
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
