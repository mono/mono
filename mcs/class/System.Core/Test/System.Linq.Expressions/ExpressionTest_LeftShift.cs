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
//		Jb Evain <jbevain@novell.com>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_LeftShift
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.LeftShift (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.LeftShift (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Arg2WrongType ()
		{
			Expression.LeftShift (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.LeftShift (Expression.Constant (new NoOpClass ()), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.LeftShift (Expression.Constant (true), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.LeftShift (Expression.Constant (2.0), Expression.Constant (1));
		}

		[Test]
		public void Integer ()
		{
			BinaryExpression expr = Expression.LeftShift (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.LeftShift, expr.NodeType, "LeftShift#01");
			Assert.AreEqual (typeof (int), expr.Type, "LeftShift#02");
			Assert.IsNull (expr.Method, "LeftShift#03");
			Assert.AreEqual ("(2 << 1)", expr.ToString(), "LeftShift#04");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.LeftShift (Expression.Constant (a), Expression.Constant (b));
			Assert.AreEqual (ExpressionType.LeftShift, expr.NodeType, "LeftShift#05");
			Assert.AreEqual (typeof (int), expr.Type, "LeftShift#06");
			Assert.IsNull (expr.Method, "LeftShift#07");
			Assert.AreEqual ("(1 << 2)", expr.ToString(), "LeftShift#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_LeftShift");

			BinaryExpression expr = Expression.LeftShift (Expression.Constant (new OpClass ()), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.LeftShift, expr.NodeType, "LeftShift#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "LeftShift#10");
			Assert.AreEqual (mi, expr.Method, "LeftShift#11");
			Assert.AreEqual ("op_LeftShift", expr.Method.Name, "LeftShift#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) << 1)",
				expr.ToString(), "LeftShift#13");
		}

		[Test]
		public void CompileLeftShift ()
		{
			ParameterExpression l = Expression.Parameter (typeof (int), "l"), r = Expression.Parameter (typeof (int), "r");

			var ls = Expression.Lambda<Func<int, int, int>> (
				Expression.LeftShift (l, r), l, r).Compile ();

			Assert.AreEqual (12, ls (6, 1));
			Assert.AreEqual (96, ls (12, 3));
		}

		[Test]
		public void LeftShiftNullableLongAndInt ()
		{
			var l = Expression.Parameter (typeof (long?), "l");
			var r = Expression.Parameter (typeof (int), "r");

			var node = Expression.LeftShift (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (long?), node.Type);

			var ls = Expression.Lambda<Func<long?, int, long?>> (node, l, r).Compile ();

			Assert.AreEqual (null, ls (null, 2));
			Assert.AreEqual (2048, ls (1024, 1));
		}
	}
}
