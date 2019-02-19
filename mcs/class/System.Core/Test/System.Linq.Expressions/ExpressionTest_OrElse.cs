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
	public class ExpressionTest_OrElse
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.OrElse (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.OrElse (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.OrElse (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.OrElse (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Integer ()
		{
			Expression.OrElse (Expression.Constant (1), Expression.Constant (2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.OrElse (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		public void Boolean ()
		{
			BinaryExpression expr = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));
			Assert.AreEqual (ExpressionType.OrElse, expr.NodeType, "OrElse#01");
			Assert.AreEqual (typeof (bool), expr.Type, "OrElse#02");
			Assert.IsNull (expr.Method, "OrElse#03");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseOr");

			BinaryExpression expr = Expression.OrElse (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.OrElse, expr.NodeType, "OrElse#05");
			Assert.AreEqual (typeof (OpClass), expr.Type, "OrElse#06");
			Assert.AreEqual (mi, expr.Method, "OrElse#07");
			Assert.AreEqual ("op_BitwiseOr", expr.Method.Name, "OrElse#08");
		}

		public class BrokenMethod {
			public static int operator | (BrokenMethod a, BrokenMethod b)
			{
				return 1;
			}
		}

		public class BrokenMethod2 {
			public static BrokenMethod2 operator | (BrokenMethod2 a, int b)
			{
				return null;
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MethodInfoReturnType ()
		{
			Expression.OrElse (Expression.Constant (new BrokenMethod ()),
					   Expression.Constant (new BrokenMethod ()));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MethodInfoReturnType2 ()
		{
			Expression.OrElse (Expression.Constant (new BrokenMethod2 ()),
					   Expression.Constant (1));
		}

		[Test]
		public void OrElseNotLifted ()
		{
			var b = Expression.OrElse (
				Expression.Constant (true, typeof (bool)),
				Expression.Constant (true, typeof (bool)));

			Assert.AreEqual (typeof (bool), b.Type);
			Assert.IsFalse (b.IsLifted);
			Assert.IsFalse (b.IsLiftedToNull);
		}

		[Test]
		public void OrElseTest ()
		{
			var a = Expression.Parameter (typeof (bool), "a");
			var b = Expression.Parameter (typeof (bool), "b");
			var l = Expression.Lambda<Func<bool, bool, bool>> (
				Expression.OrElse (a, b), a, b);

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
		public void OrElseLifted ()
		{
			var b = Expression.OrElse (
				Expression.Constant (null, typeof (bool?)),
				Expression.Constant (null, typeof (bool?)));

			Assert.AreEqual (typeof (bool?), b.Type);
			Assert.IsTrue (b.IsLifted);
			Assert.IsTrue (b.IsLiftedToNull);
		}

		[Test]
		public void OrElseTestNullable ()
		{
			var a = Expression.Parameter (typeof (bool?), "a");
			var b = Expression.Parameter (typeof (bool?), "b");
			var l = Expression.Lambda<Func<bool?, bool?, bool?>> (
				Expression.OrElse (a, b), a, b);

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
		public void OrElseBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool>), "i");
			var and = Expression.Lambda<Func<Item<bool>, bool>> (
				Expression.OrElse (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool> (true, false);
			Assert.AreEqual (true, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		[Test]
		public void OrElseNullableBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool?>), "i");
			var and = Expression.Lambda<Func<Item<bool?>, bool?>> (
				Expression.OrElse (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool?> (true, false);
			Assert.AreEqual ((bool?) true, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		struct Slot {

			public int Value;

			public Slot (int val)
			{
				this.Value = val;
			}

			public static Slot operator | (Slot a, Slot b)
			{
				return new Slot (a.Value | b.Value);
			}

			public static bool operator true (Slot a)
			{
				return a.Value != 0;
			}

			public static bool operator false (Slot a)
			{
				return a.Value == 0;
			}
		}

		[Test]
		public void UserDefinedOrElse ()
		{
			var l = Expression.Parameter (typeof (Slot), "l");
			var r = Expression.Parameter (typeof (Slot), "r");

			var method = typeof (Slot).GetMethod ("op_BitwiseOr");

			var node = Expression.OrElse (l, r, method);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (method, node.Method);

			var orelse = Expression.Lambda<Func<Slot, Slot, Slot>> (node, l, r).Compile ();

			Assert.AreEqual (new Slot (64), orelse (new Slot (64), new Slot (64)));
			Assert.AreEqual (new Slot (32), orelse (new Slot (32), new Slot (64)));
		}

		[Test]
		public void UserDefinedOrElseShortCircuit ()
		{
			var i = Expression.Parameter (typeof (Item<Slot>), "i");
			var orelse = Expression.Lambda<Func<Item<Slot>, Slot>> (
				Expression.OrElse (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<Slot> (new Slot (1), new Slot (0));
			Assert.AreEqual (new Slot (1), orelse (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=350228
		public void UserDefinedLiftedOrElseShortCircuit ()
		{
			var i = Expression.Parameter (typeof (Item<Slot?>), "i");
			var orelse = Expression.Lambda<Func<Item<Slot?>, Slot?>> (
				Expression.OrElse (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<Slot?> (new Slot (1), null);
			Assert.AreEqual ((Slot?) new Slot (1), orelse (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		struct Incomplete {
			public int Value;

			public Incomplete (int val)
			{
				Value = val;
			}

			public static Incomplete operator | (Incomplete a, Incomplete b)
			{
				return new Incomplete (a.Value | b.Value);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IncompleteUserDefinedOrElse ()
		{
			var l = Expression.Parameter (typeof (Incomplete), "l");
			var r = Expression.Parameter (typeof (Incomplete), "r");

			var method = typeof (Incomplete).GetMethod ("op_BitwiseOr");

			Expression.OrElse (l, r, method);
		}
	}
}
