//
// ConditionOrExpression.cs
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
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal sealed class ConditionOrExpression : ConditionExpression {
	
		readonly ConditionExpression left;
		readonly ConditionExpression right;
		
		public ConditionOrExpression (ConditionExpression left, ConditionExpression right)
		{
			this.left = left;
			this.right = right;
		}
		
		public ConditionExpression Left {
			get { return left; }
		}
		
		public ConditionExpression Right {
			get { return right; }
		}
	
		public override  bool BoolEvaluate (Project context)
		{
			if (left.BoolEvaluate (context))
				return true;
			if (right.BoolEvaluate (context))
				return true;
			return false;
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
	}
}
