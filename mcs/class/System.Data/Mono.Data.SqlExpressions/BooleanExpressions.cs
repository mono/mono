//
// BooleanExpressions.cs
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
	public class Negation : UnaryExpression {
		public Negation (IExpression e) : base (e) {}
	
		override public object Eval (DataRow row)
		{
			return !((bool)expr.Eval (row));
		}
	}
	
	public class BoolOperation : BinaryOpExpression {
		public BoolOperation (Operation op, IExpression e1, IExpression e2) : base (op, e1, e2) {}
	
		override public object Eval (DataRow row)
		{
			if (op == Operation.OR)
				return ((bool)expr1.Eval (row)) || ((bool)expr2.Eval (row));
			if (op == Operation.AND)
				return ((bool)expr1.Eval (row)) && ((bool)expr2.Eval (row));
				
			return false;
		}
	}	
}
