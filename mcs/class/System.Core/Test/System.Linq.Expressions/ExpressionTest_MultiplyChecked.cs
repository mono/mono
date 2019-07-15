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
	public class ExpressionTest_MultiplyChecked
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.MultiplyChecked (null, Expression.Constant(1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.MultiplyChecked (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.MultiplyChecked (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.MultiplyChecked (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.MultiplyChecked (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.MultiplyChecked (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.MultiplyChecked, expr.NodeType, "MultiplyChecked#01");
			Assert.AreEqual (typeof (int), expr.Type, "MultiplyChecked#02");
			Assert.IsNull (expr.Method, "MultiplyChecked#03");
			Assert.AreEqual ("(1 * 2)", expr.ToString(), "MultiplyChecked#15");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.MultiplyChecked (Expression.Constant (a), Expression.Constant (b));
			Assert.AreEqual (ExpressionType.MultiplyChecked, expr.NodeType, "MultiplyChecked#04");
			Assert.AreEqual (typeof (int), expr.Type, "MultiplyChecked#05");
			Assert.IsNull (expr.Method, null, "MultiplyChecked#06");
			Assert.AreEqual ("(1 * 2)", expr.ToString(), "MultiplyChecked#16");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Multiply");

			BinaryExpression expr = Expression.MultiplyChecked (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.MultiplyChecked, expr.NodeType, "MultiplyChecked#07");
			Assert.AreEqual (typeof (OpClass), expr.Type, "MultiplyChecked#08");
			Assert.AreEqual (mi, expr.Method, "MultiplyChecked#09");
			Assert.AreEqual ("op_Multiply", expr.Method.Name, "MultiplyChecked#10");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) * value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "MultiplyChecked#17");
		}

		[Test]
		public void UserDefinedStruct ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpStruct).GetMethod ("op_Multiply");

			BinaryExpression expr = Expression.MultiplyChecked (Expression.Constant (new OpStruct ()), Expression.Constant (new OpStruct ()));
			Assert.AreEqual (ExpressionType.MultiplyChecked, expr.NodeType, "MultiplyChecked#11");
			Assert.AreEqual (typeof (OpStruct), expr.Type, "MultiplyChecked#12");
			Assert.AreEqual (mi, expr.Method, "MultiplyChecked#13");
			Assert.AreEqual ("op_Multiply", expr.Method.Name, "MultiplyChecked#14");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpStruct) * value(MonoTests.System.Linq.Expressions.OpStruct))",
				expr.ToString(), "MultiplyChecked#18");
		}

	}
}
