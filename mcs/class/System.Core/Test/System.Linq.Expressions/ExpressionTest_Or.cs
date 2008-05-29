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
	public class ExpressionTest_Or
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Or (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Or (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.Or (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.Or (Expression.Constant (1), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.Or (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		public void Integer ()
		{
			BinaryExpression expr = Expression.Or (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.Or, expr.NodeType, "Or#01");
			Assert.AreEqual (typeof (int), expr.Type, "Or#02");
			Assert.IsNull (expr.Method, "Or#03");
			Assert.AreEqual ("(1 | 2)", expr.ToString(), "Or#04");
		}

		[Test]
		public void Boolean ()
		{
			BinaryExpression expr = Expression.Or (Expression.Constant (true), Expression.Constant (false));
			Assert.AreEqual (ExpressionType.Or, expr.NodeType, "Or#05");
			Assert.AreEqual (typeof (bool), expr.Type, "Or#06");
			Assert.IsNull (expr.Method, "Or#07");
			Assert.AreEqual ("(True Or False)", expr.ToString(), "Or#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseOr");

			BinaryExpression expr = Expression.Or (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Or, expr.NodeType, "Or#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "Or#10");
			Assert.AreEqual (mi, expr.Method, "Or#11");
			Assert.AreEqual ("op_BitwiseOr", expr.Method.Name, "Or#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) | value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "Or#13");
		}

		[Test]
		public void OrBoolTest ()
		{
			var a = Expression.Parameter (typeof (bool), "a");
			var b = Expression.Parameter (typeof (bool), "b");
			var l = Expression.Lambda<Func<bool, bool, bool>> (
				Expression.Or (a, b), a, b);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (bool), be.Type);
			Assert.IsFalse (be.IsLifted);
			Assert.IsFalse (be.IsLiftedToNull);

			var c = l.Compile ();

			Assert.AreEqual (true,  c (true, true), "o1");
			Assert.AreEqual (true,  c (true, false), "o2");
			Assert.AreEqual (true,  c (false, true), "o3");
			Assert.AreEqual (false, c (false, false), "o4");
		}

		[Test]
		public void OrBoolNullableTest ()
		{
			var a = Expression.Parameter (typeof (bool?), "a");
			var b = Expression.Parameter (typeof (bool?), "b");
			var l = Expression.Lambda<Func<bool?, bool?, bool?>> (
				Expression.Or (a, b), a, b);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (bool?), be.Type);
			Assert.IsTrue (be.IsLifted);
			Assert.IsTrue (be.IsLiftedToNull);

			var c = l.Compile ();

			Assert.AreEqual (true,  c (true, true),   "o1");
			Assert.AreEqual (true,  c (true, false),  "o2");
			Assert.AreEqual (true,  c (false, true),  "o3");
			Assert.AreEqual (false, c (false, false), "o4");

			Assert.AreEqual (true, c (true, null),  "o5");
			Assert.AreEqual (null, c (false, null), "o6");
			Assert.AreEqual (null, c (null, false), "o7");
			Assert.AreEqual (true, c (true, null),  "o8");
			Assert.AreEqual (null, c (null, null),  "o9");
		}

		[Test]
		public void OrBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool>), "i");
			var and = Expression.Lambda<Func<Item<bool>, bool>> (
				Expression.Or (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool> (true, false);
			Assert.AreEqual (true, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsTrue (item.RightCalled);
		}

		[Test]
		public void OrNullableBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool?>), "i");
			var and = Expression.Lambda<Func<Item<bool?>, bool?>> (
				Expression.Or (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool?> (true, false);
			Assert.AreEqual ((bool?) true, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsTrue (item.RightCalled);
		}

		[Test]
		public void OrIntTest ()
		{
			var a = Expression.Parameter (typeof (int), "a");
			var b = Expression.Parameter (typeof (int), "b");
			var or = Expression.Lambda<Func<int, int, int>> (
				Expression.Or (a, b), a, b).Compile ();

			Assert.AreEqual ((int?) 1, or (1, 1), "o1");
			Assert.AreEqual ((int?) 1, or (1, 0), "o2");
			Assert.AreEqual ((int?) 1, or (0, 1), "o3");
			Assert.AreEqual ((int?) 0, or (0, 0), "o4");
		}

		[Test]
		public void OrIntNullableTest ()
		{
			var a = Expression.Parameter (typeof (int?), "a");
			var b = Expression.Parameter (typeof (int?), "b");
			var c = Expression.Lambda<Func<int?, int?, int?>> (
				Expression.Or (a, b), a, b).Compile ();

			Assert.AreEqual ((int?) 1, c (1, 1), "o1");
			Assert.AreEqual ((int?) 1, c (1, 0), "o2");
			Assert.AreEqual ((int?) 1, c (0, 1), "o3");
			Assert.AreEqual ((int?) 0, c (0, 0), "o4");

			Assert.AreEqual ((int?) null, c (1, null), "o5");
			Assert.AreEqual ((int?) null, c (0, null), "o6");
			Assert.AreEqual ((int?) null, c (null, 0), "o7");
			Assert.AreEqual ((int?) null, c (1, null), "o8");
			Assert.AreEqual ((int?) null, c (null, null), "o9");
		}
	}
}
