//
// Literal.cs
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
	internal class Literal : BaseExpression {
		object val;
	
		public Literal (object val)
		{
			this.val = val;
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj))
				return false;

			if (!(obj is Literal))
				return false;

			Literal other = (Literal) obj;
			if (other.val != null) {
				if (!other.val.Equals (val))
					return false;
			}
			else if (val != null)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return val.GetHashCode () ^ base.GetHashCode ();
		}
	
		public override object Eval (DataRow row)
		{
			return val;
		}

		override public bool DependsOn(DataColumn other)
		{
			return false;
		}
	}
}
