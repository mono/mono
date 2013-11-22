//
// Like.cs
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
	internal class Like : UnaryExpression {
		readonly IExpression _pattern;
		
		public Like (IExpression e, IExpression pattern) : base (e)
		{
			_pattern = pattern;
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj))
				return false;

			if (!(obj is Like))
				return false;

			Like other = (Like) obj;
			return _pattern.Equals (other._pattern);
		}

		public override int GetHashCode()
		{
			return _pattern.GetHashCode () ^ base.GetHashCode ();
		}

		override public object Eval (DataRow row)
		{
			string str = expr.Eval (row) as string;
			string pattern = _pattern.Eval (row) as string;

			if (str == null || pattern == null)
				return false;

			bool openStart, openEnd;
			string original = pattern;
			int len = pattern.Length;
			openStart = (pattern [0] == '*' || pattern [0] == '%');
			openEnd = (pattern [len - 1] == '*' || pattern [len - 1] == '%');

			pattern = pattern.Trim ('*', '%');
			pattern = pattern.Replace ("[*]", "[[0]]");
			pattern = pattern.Replace ("[%]", "[[1]]");
			if (pattern.IndexOf ('*') != -1 || pattern.IndexOf ('%') != -1)
				throw new EvaluateException (String.Format ("Pattern '{0}' is invalid.", original));
			pattern = pattern.Replace ("[[0]]", "*");
			pattern = pattern.Replace ("[[1]]", "%");
			pattern = pattern.Replace ("[[]", "[");
			pattern = pattern.Replace ("[]]", "]");

			if (!row.Table.CaseSensitive) {
				str = str.ToLower();
				pattern = pattern.ToLower();
			}
			
			int idx = str.IndexOf (pattern);
			if (idx == -1)
				return false;
				
			return (idx == 0 || openStart) && (idx + pattern.Length == str.Length || openEnd);
		}

		override public bool EvalBoolean (DataRow row)
		{
			return (bool)Eval (row);
		}
	}
}
