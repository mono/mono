//
// ArithmeticExpressions.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	public class Negative : UnaryExpression {
		public Negative (IExpression e) : base (e) {}
	
		override public object Eval (DataRow row)
		{
			return Numeric.Negative ((IConvertible)expr.Eval (row));
		}
	}
	
	public class ArithmeticOperation : BinaryOpExpression {
		public ArithmeticOperation (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			object obj1 = expr1.Eval (row);
			object obj2 = expr2.Eval (row);
		
			if (op == Operation.ADD && (obj1 is string || obj2 is string))
				return obj1.ToString() + obj2.ToString();
		
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
