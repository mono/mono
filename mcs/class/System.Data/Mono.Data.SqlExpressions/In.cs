//
// In.cs
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
	public class In : UnaryExpression {
		IList set;
	
		public In(IExpression e, IList set) : base (e)
		{
			this.set = set;
		}
	
		override public object Eval (DataRow row)
		{
			IComparable val = (IComparable)expr.Eval (row);
			if (val == null)
				return false;

			foreach (IExpression e in set) {
				IComparable setItem = (IComparable)e.Eval (row);
				if (setItem == null)
					continue;
				
				if (Comparison.Compare (val, setItem, row.Table.CaseSensitive) == 0)	
					return true;
			}
			
			return false;
		}
	}
}
