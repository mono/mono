//
// Like.cs
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
	public class Like : UnaryExpression {
		string pattern;
		bool openStart, openEnd;
		
		public Like (IExpression e, string pattern) : base (e)
		{
			string original = pattern;
			int len = pattern.Length;
			openStart = (pattern[0] == '*' || pattern[0] == '%');
			openEnd = (pattern[len - 1] == '*' || pattern[len - 1] == '%');
			
			pattern = pattern.Trim ('*', '%');
			pattern = pattern.Replace ("[*]", "[[0]]");
			pattern = pattern.Replace ("[%]", "[[1]]");
			if (pattern.IndexOf('*') != -1 || pattern.IndexOf('%') != -1)
				throw new EvaluateException (String.Format ("Pattern '{0}' is invalid.", original));
			pattern = pattern.Replace ("[[0]]", "*");
			pattern = pattern.Replace ("[[1]]", "%");
			pattern = pattern.Replace ("[[]", "[");
			pattern = pattern.Replace ("[]]", "]");
			this.pattern = pattern;
		}

		override public object Eval (DataRow row)
		{
			string str = (string)expr.Eval (row);
			string pattern = this.pattern;
			if (!row.Table.CaseSensitive) {
				str = str.ToLower();
				pattern = pattern.ToLower();
			}
			
			int idx = str.IndexOf (pattern);
			if (idx == -1)
				return false;
				
			return (idx == 0 || openStart) && (idx + pattern.Length == str.Length || openEnd);
		}
	}
}
