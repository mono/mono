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
	public class ExpressionTest_ExclusiveOr
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.ExclusiveOr (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.ExclusiveOr (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.ExclusiveOr (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.ExclusiveOr (Expression.Constant (1), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.ExclusiveOr (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		public void Integer ()
		{
			BinaryExpression expr = Expression.ExclusiveOr (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.ExclusiveOr, expr.NodeType, "ExclusiveOr#01");
			Assert.AreEqual (typeof (int), expr.Type, "ExclusiveOr#02");
			Assert.IsNull (expr.Method, "ExclusiveOr#03");
			Assert.AreEqual ("(1 ^ 2)", expr.ToString(), "ExclusiveOr#04");
		}

		[Test]
		public void Boolean ()
		{
			BinaryExpression expr = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));
			Assert.AreEqual (ExpressionType.ExclusiveOr, expr.NodeType, "ExclusiveOr#05");
			Assert.AreEqual (typeof (bool), expr.Type, "ExclusiveOr#06");
			Assert.IsNull (expr.Method, "ExclusiveOr#07");
			Assert.AreEqual ("(True ^ False)", expr.ToString(), "ExclusiveOr#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_ExclusiveOr");

			BinaryExpression expr = Expression.ExclusiveOr (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.ExclusiveOr, expr.NodeType, "ExclusiveOr#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "ExclusiveOr#10");
			Assert.AreEqual (mi, expr.Method, "ExclusiveOr#11");
			Assert.AreEqual ("op_ExclusiveOr", expr.Method.Name, "ExclusiveOr#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) ^ value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "ExclusiveOr#13");
		}
	}
}
