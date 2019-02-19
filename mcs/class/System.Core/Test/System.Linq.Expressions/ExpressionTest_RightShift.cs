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
	[Category("SRE")]
	public class ExpressionTest_RightShift
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.RightShift (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.RightShift (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Arg2WrongType ()
		{
			Expression.RightShift (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.RightShift (Expression.Constant (new NoOpClass ()), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.RightShift (Expression.Constant (true), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.RightShift (Expression.Constant (2.0), Expression.Constant (1));
		}

		[Test]
		public void Integer ()
		{
			BinaryExpression expr = Expression.RightShift (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.RightShift, expr.NodeType, "RightShift#01");
			Assert.AreEqual (typeof (int), expr.Type, "RightShift#02");
			Assert.IsNull (expr.Method, "RightShift#03");
			Assert.AreEqual ("(2 >> 1)", expr.ToString(), "RightShift#04");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.RightShift (Expression.Constant (a), Expression.Constant (b));
			Assert.AreEqual (ExpressionType.RightShift, expr.NodeType, "RightShift#05");
			Assert.AreEqual (typeof (int), expr.Type, "RightShift#06");
			Assert.IsNull (expr.Method, "RightShift#07");
			Assert.AreEqual ("(1 >> 2)", expr.ToString(), "RightShift#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_RightShift");

			BinaryExpression expr = Expression.RightShift (Expression.Constant (new OpClass ()), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.RightShift, expr.NodeType, "RightShift#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "RightShift#10");
			Assert.AreEqual (mi, expr.Method, "RightShift#11");
			Assert.AreEqual ("op_RightShift", expr.Method.Name, "RightShift#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) >> 1)",
				expr.ToString(), "RightShift#13");
		}

		[Test]
		public void CompileRightShift ()
		{
			var l = Expression.Parameter (typeof (int), "l");
			var r = Expression.Parameter (typeof (int), "r");

			var rs = Expression.Lambda<Func<int, int, int>> (
				Expression.RightShift (l, r), l, r).Compile ();

			Assert.AreEqual (3, rs (6, 1));
			Assert.AreEqual (1, rs (12, 3));
		}

		[Test]
		public void RightShiftNullableLongAndInt ()
		{
			var l = Expression.Parameter (typeof (long?), "l");
			var r = Expression.Parameter (typeof (int), "r");

			var node = Expression.RightShift (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (long?), node.Type);

			var rs = Expression.Lambda<Func<long?, int, long?>> (node, l, r).Compile ();

			Assert.AreEqual (null, rs (null, 2));
			Assert.AreEqual (512, rs (1024, 1));
		}
	}
}
