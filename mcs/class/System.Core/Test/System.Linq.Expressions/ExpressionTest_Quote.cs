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
	[Category("SRE")]
	public class ExpressionTest_Quote
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Quote (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void QuoteConstant ()
		{
			Expression.Quote (Expression.Constant (1));
		}

		[Test]
		public void CompiledQuote ()
		{
			var quote42 = Expression.Lambda<Func<Expression<Func<int>>>> (
				Expression.Quote (
					Expression.Lambda<Func<int>> (
						42.ToConstant ()))).Compile ();

			var get42 = quote42 ().Compile ();

			Assert.AreEqual (42, get42 ());
		}

		[Test]
		public void ParameterInQuotedExpression () // #550722
		{
			// Expression<Func<string, Expression<Func<string>>>> e = (string s) => () => s;

			var s = Expression.Parameter (typeof (string), "s");

			var lambda = Expression.Lambda<Func<string, Expression<Func<string>>>> (
				Expression.Quote (
					Expression.Lambda<Func<string>> (s, new ParameterExpression [0])),
				s);

			var fs = lambda.Compile () ("bingo").Compile ();

			Assert.AreEqual ("bingo", fs ());
		}
	}
}
