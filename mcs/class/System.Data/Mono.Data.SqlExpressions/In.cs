//
// In.cs
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
	internal class In : UnaryExpression {
		IList set;
	
		public In(IExpression e, IList set) : base (e)
		{
			this.set = set;
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj))
				return false;

			if (!(obj is In))
				return false;

			In other = (In) obj;
			if (other.set.Count != set.Count)
				return false;	

			for (int i = 0, count = set.Count; i < count; i++) {
				object o1 = set[i];
				object o2 = other.set[i];

				if (o1 == null && o2 != null)
					return false;

				if (!o1.Equals(o2))
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode ();
			for (int i = 0, count = set.Count; i < count; i++) {
				object o = set[i];
				if (o != null)
					hashCode ^= o.GetHashCode();
			}

			return hashCode;
		}
	
		override public object Eval (DataRow row)
		{
			object o = expr.Eval (row);
			if (o == DBNull.Value)
				return o;
			IComparable val = o as IComparable;
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

		override public bool EvalBoolean (DataRow row)
		{
			return (bool) Eval (row);
		}

	}
}
