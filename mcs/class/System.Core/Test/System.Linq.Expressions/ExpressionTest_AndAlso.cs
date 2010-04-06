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
	public class ExpressionTest_AndAlso
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.AndAlso (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.AndAlso (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.AndAlso (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.AndAlso (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Integer ()
		{
			Expression.AndAlso (Expression.Constant (1), Expression.Constant (2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.AndAlso (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		public void Boolean ()
		{
			BinaryExpression expr = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));
			Assert.AreEqual (ExpressionType.AndAlso, expr.NodeType, "AndAlso#01");
			Assert.AreEqual (typeof (bool), expr.Type, "AndAlso#02");
			Assert.IsNull (expr.Method, "AndAlso#03");
#if !NET_4_0
			Assert.AreEqual ("(True && False)", expr.ToString(), "AndAlso#04");
#endif
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseAnd");

			BinaryExpression expr = Expression.AndAlso (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.AndAlso, expr.NodeType, "AndAlso#05");
			Assert.AreEqual (typeof (OpClass), expr.Type, "AndAlso#06");
			Assert.AreEqual (mi, expr.Method, "AndAlso#07");
			Assert.AreEqual ("op_BitwiseAnd", expr.Method.Name, "AndAlso#08");
#if !NET_4_0
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) && value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "AndAlso#09");
#endif
		}

		[Test]
		public void AndAlsoTest ()
		{
			var a = Expression.Parameter (typeof (bool), "a");
			var b = Expression.Parameter (typeof (bool), "b");
			var l = Expression.Lambda<Func<bool, bool, bool>> (
				Expression.AndAlso (a, b), a, b);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (bool), be.Type);
			Assert.IsFalse (be.IsLifted);
			Assert.IsFalse (be.IsLiftedToNull);

			var c = l.Compile ();

			Assert.AreEqual (true,  c (true, true), "a1");
			Assert.AreEqual (false, c (true, false), "a2");
			Assert.AreEqual (false, c (false, true), "a3");
			Assert.AreEqual (false, c (false, false), "a4");
		}

		[Test]
		public void AndAlsoLifted ()
		{
			var b = Expression.AndAlso (
				Expression.Constant (null, typeof (bool?)),
				Expression.Constant (null, typeof (bool?)));

			Assert.AreEqual (typeof (bool?), b.Type);
			Assert.IsTrue (b.IsLifted);
			Assert.IsTrue (b.IsLiftedToNull);
		}

		[Test]
		public void AndAlsoNotLifted ()
		{
			var b = Expression.AndAlso (
				Expression.Constant (true, typeof (bool)),
				Expression.Constant (true, typeof (bool)));

			Assert.AreEqual (typeof (bool), b.Type);
			Assert.IsFalse (b.IsLifted);
			Assert.IsFalse (b.IsLiftedToNull);
		}

		[Test]
		public void AndAlsoTestNullable ()
		{
			var a = Expression.Parameter (typeof (bool?), "a");
			var b = Expression.Parameter (typeof (bool?), "b");
			var l = Expression.Lambda<Func<bool?, bool?, bool?>> (
				Expression.AndAlso (a, b), a, b);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (bool?), be.Type);
			Assert.IsTrue (be.IsLifted);
			Assert.IsTrue (be.IsLiftedToNull);

			var c = l.Compile ();

			Assert.AreEqual (true,  c (true, true), "a1");
			Assert.AreEqual (false, c (true, false), "a2");
			Assert.AreEqual (false, c (false, true), "a3");
			Assert.AreEqual (false, c (false, false), "a4");

			Assert.AreEqual (null,  c (true, null), "a5");
			Assert.AreEqual (false, c (false, null), "a6");
			Assert.AreEqual (false, c (null, false), "a7");
			Assert.AreEqual (null,  c (true, null), "a8");
			Assert.AreEqual (null,  c (null, null), "a9");
		}

		[Test]
		public void AndAlsoBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool>), "i");
			var and = Expression.Lambda<Func<Item<bool>, bool>> (
				Expression.AndAlso (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool> (false, true);
			Assert.AreEqual (false, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		[Test]
		public void AndAlsoNullableBoolItem ()
		{
			var i = Expression.Parameter (typeof (Item<bool?>), "i");
			var and = Expression.Lambda<Func<Item<bool?>, bool?>> (
				Expression.AndAlso (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<bool?> (false, true);
			Assert.AreEqual ((bool?) false, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		struct Slot {

			public int Value;

			public Slot (int val)
			{
				this.Value = val;
			}

			public static Slot operator & (Slot a, Slot b)
			{
				return new Slot (a.Value & b.Value);
			}

			public static bool operator true (Slot a)
			{
				return a.Value != 0;
			}

			public static bool operator false (Slot a)
			{
				return a.Value == 0;
			}

			public override string ToString ()
			{
				return Value.ToString ();
			}
		}

		[Test]
		public void UserDefinedAndAlso ()
		{
			var l = Expression.Parameter (typeof (Slot), "l");
			var r = Expression.Parameter (typeof (Slot), "r");

			var method = typeof (Slot).GetMethod ("op_BitwiseAnd");

			var node = Expression.AndAlso (l, r, method);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (method, node.Method);

			var andalso = Expression.Lambda<Func<Slot, Slot, Slot>> (node, l, r).Compile ();

			Assert.AreEqual (new Slot (64), andalso (new Slot (64), new Slot (64)));
			Assert.AreEqual (new Slot (0), andalso (new Slot (32), new Slot (64)));
			Assert.AreEqual (new Slot (0), andalso (new Slot (64), new Slot (32)));
		}

		[Test]
		public void UserDefinedAndAlsoShortCircuit ()
		{
			var i = Expression.Parameter (typeof (Item<Slot>), "i");
			var and = Expression.Lambda<Func<Item<Slot>, Slot>> (
				Expression.AndAlso (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<Slot> (new Slot (0), new Slot (1));
			Assert.AreEqual (new Slot (0), and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=350228
		public void UserDefinedLiftedAndAlsoShortCircuit ()
		{
			var i = Expression.Parameter (typeof (Item<Slot?>), "i");
			var and = Expression.Lambda<Func<Item<Slot?>, Slot?>> (
				Expression.AndAlso (
					Expression.Property (i, "Left"),
					Expression.Property (i, "Right")), i).Compile ();

			var item = new Item<Slot?> (null, new Slot (1));
			Assert.AreEqual ((Slot?) null, and (item));
			Assert.IsTrue (item.LeftCalled);
			Assert.IsFalse (item.RightCalled);
		}

		[Test]
		public void UserDefinedAndAlsoLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var method = typeof (Slot).GetMethod ("op_BitwiseAnd");

			var node = Expression.AndAlso (l, r, method);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (method, node.Method);

			var andalso = Expression.Lambda<Func<Slot?, Slot?, Slot?>> (node, l, r).Compile ();

			Assert.AreEqual (new Slot (64), andalso (new Slot (64), new Slot (64)));
			Assert.AreEqual (new Slot (0), andalso (new Slot (32), new Slot (64)));
			Assert.AreEqual (new Slot (0), andalso (new Slot (64), new Slot (32)));
			Assert.AreEqual (null, andalso (null, new Slot (32)));
			Assert.AreEqual (null, andalso (new Slot (64), null));
			Assert.AreEqual (null, andalso (null, null));
		}

		struct Incomplete {
			public int Value;

			public Incomplete (int val)
			{
				Value = val;
			}

			public static Incomplete operator & (Incomplete a, Incomplete b)
			{
				return new Incomplete (a.Value & b.Value);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IncompleteUserDefinedAndAlso ()
		{
			var l = Expression.Parameter (typeof (Incomplete), "l");
			var r = Expression.Parameter (typeof (Incomplete), "r");

			var method = typeof (Incomplete).GetMethod ("op_BitwiseAnd");

			Expression.AndAlso (l, r, method);
		}

		class A {
			public static bool operator true (A x)
			{
				return true;
			}

			public static bool operator false (A x)
			{
				return false;
			}
		}

		class B : A {
			public static B operator & (B x, B y)
			{
				return new B ();
			}

			public static bool op_True<T> (B x)
			{
				return true;
			}

			public static bool op_False (B x)
			{
				return false;
			}
		}

		[Test] // from https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=350487
		public void Connect350487 ()
		{
			var p = Expression.Parameter (typeof (B), "b");
			var l = Expression.Lambda<Func<B, A>> (
				Expression.AndAlso (p, p), p).Compile ();

			Assert.IsNotNull (l (null));
		}
	}
}
