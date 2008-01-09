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
	public class ExpressionTest_Multiply
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Multiply (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Multiply (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.Multiply (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.Multiply (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.Multiply (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.Multiply (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.Multiply, expr.NodeType, "Multiply#01");
			Assert.AreEqual (typeof (int), expr.Type, "Multiply#02");
			Assert.IsNull (expr.Method, "Multiply#03");
			Assert.AreEqual ("(1 * 2)", expr.ToString(), "Multiply#04");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.Multiply (Expression.Constant (a), Expression.Constant (b));
			Assert.AreEqual (ExpressionType.Multiply, expr.NodeType, "Multiply#05");
			Assert.AreEqual (typeof (int), expr.Type, "Multiply#06");
			Assert.IsNull (expr.Method, "Multiply#07");
			Assert.AreEqual ("(1 * 2)", expr.ToString(), "Multiply#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Multiply");

			BinaryExpression expr = Expression.Multiply (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Multiply, expr.NodeType, "Multiply#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "Multiply#10");
			Assert.AreEqual (mi, expr.Method, "Multiply#11");
			Assert.AreEqual ("op_Multiply", expr.Method.Name, "Multiply#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) * value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "Multiply#13");
		}
	}
}
