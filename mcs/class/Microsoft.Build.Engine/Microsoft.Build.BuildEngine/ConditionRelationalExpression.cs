//
// ConditionRelationalExpression.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal sealed class ConditionRelationalExpression : ConditionExpression {
	
		readonly ConditionExpression left;
		readonly ConditionExpression right;
		readonly RelationOperator op;
		
		public ConditionRelationalExpression (ConditionExpression left,
						      ConditionExpression right,
						      RelationOperator op)
		{
			this.left = left;
			this.right = right;
			this.op = op;
		}
		
		public override  bool BoolEvaluate (Project context)
		{
			if (left.CanEvaluateToNumber (context) && right.CanEvaluateToNumber (context)) {
				float l,r;
				
				l = left.NumberEvaluate (context);
				r = right.NumberEvaluate (context);
				
				return NumberCompare (l, r, op);
			} else if (left.CanEvaluateToBool (context) && right.CanEvaluateToBool (context)) {
				bool l,r;
				
				l = left.BoolEvaluate (context);
				r = right.BoolEvaluate (context);
				
				return BoolCompare (l, r, op);
			} else {
				string l,r;
				
				l = left.StringEvaluate (context);
				r = right.StringEvaluate (context);
				
				return StringCompare (l, r, op);
			}
		}
		
		public override float NumberEvaluate (Project context)
		{
			throw new NotSupportedException ();
		}
		
		public override string StringEvaluate (Project context)
		{
			throw new NotSupportedException ();
		}
		
		// FIXME: check if we really can do it
		public override bool CanEvaluateToBool (Project context)
		{
			return true;
		}
		
		public override bool CanEvaluateToNumber (Project context)
		{
			return false;
		}
		
		public override bool CanEvaluateToString (Project context)
		{
			return false;
		}
		
		static bool NumberCompare (float l,
					   float r,
					   RelationOperator op)
		{
			IComparer comparer = CaseInsensitiveComparer.DefaultInvariant;
			
			switch (op) {
			case RelationOperator.Equal:
				return comparer.Compare (l, r) == 0;
			case RelationOperator.NotEqual:
				return comparer.Compare (l, r) != 0;
			case RelationOperator.Greater:
				return comparer.Compare (l, r) > 0;
			case RelationOperator.GreaterOrEqual:
				return comparer.Compare (l, r) >= 0;
			case RelationOperator.Less:
				return comparer.Compare (l, r) < 0;
			case RelationOperator.LessOrEqual:
				return comparer.Compare (l, r) <= 0;
			default:
				throw new NotSupportedException (String.Format ("Relational operator {0} is not supported.", op));
			}
		}

		static bool BoolCompare (bool l,
					 bool r,
					 RelationOperator op)
		{
			IComparer comparer = CaseInsensitiveComparer.DefaultInvariant;
			
			switch (op) {
			case RelationOperator.Equal:
				return comparer.Compare (l, r) == 0;
			case RelationOperator.NotEqual:
				return comparer.Compare (l, r) != 0;
			default:
				throw new NotSupportedException (String.Format ("Relational operator {0} is not supported.", op));
			}
		}

		static bool StringCompare (string l,
					   string r,
					   RelationOperator op)
		{
			IComparer comparer = CaseInsensitiveComparer.DefaultInvariant;
			
			switch (op) {
			case RelationOperator.Equal:
				return comparer.Compare (l, r) == 0;
			case RelationOperator.NotEqual:
				return comparer.Compare (l, r) != 0;
			default:
				throw new NotSupportedException (String.Format ("Relational operator {0} is not supported.", op));
			}
		}
	}
	
	internal enum RelationOperator {
		Equal,
		NotEqual,
		Less,
		Greater,
		LessOrEqual,
		GreaterOrEqual
	}
}
