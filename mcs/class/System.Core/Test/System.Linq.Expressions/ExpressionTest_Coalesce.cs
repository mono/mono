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
//
// Authors:
//		Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Coalesce
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Coalesce (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Coalesce (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NonNullLeftParameter ()
		{
			// This throws because they are both doubles, which are never 
			Expression.Coalesce (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Incompatible_Arguments ()
		{
			// The artuments are not compatible
			Expression.Coalesce (Expression.Parameter (typeof (int?), "a"),
					     Expression.Parameter (typeof (bool), "b"));
		}

		[Test]
		public void Coalesce_1 ()
		{
			// Build the expression by hand for now:
			ParameterExpression pa = Expression.Parameter (typeof (int?), "a");
			ParameterExpression pb = Expression.Parameter (typeof (int?), "b");
			BinaryExpression c = Expression.Coalesce (pa, pb);
			
			Expression<Func<int?, int?, int?>> e = Expression.Lambda<Func<int?,int?,int?>>(
				c, new ParameterExpression [] { pa, pb });

			Func<int?,int?,int?> comp = e.Compile ();

			Assert.AreEqual (10, comp (10, 20));
		}
	}
}
