//
// Literal.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Data;

namespace Mono.Data.SqlExpressions {
	public class Literal : IExpression {
		object val;
	
		public Literal (object val)
		{
			this.val = val;
		}
	
		public object Eval (DataRow row)
		{
			return val;
		}
	}
}
