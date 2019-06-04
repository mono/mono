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
	public class ExpressionTest_Add
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Add (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Add (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.Add (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.Add (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.Add (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.Add (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.Add, expr.NodeType, "Add#01");
			Assert.AreEqual (typeof (int), expr.Type, "Add#02");
			Assert.IsNull (expr.Method, "Add#03");
			Assert.AreEqual ("(1 + 2)", expr.ToString(), "Add#04");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.Add (Expression.Constant (a,typeof(int?)),
								Expression.Constant (b, typeof(int?)));
			Assert.AreEqual (ExpressionType.Add, expr.NodeType, "Add#05");
			Assert.AreEqual (typeof (int?), expr.Type, "Add#06");
			Assert.IsNull (expr.Method, "Add#07");
			Assert.AreEqual ("(1 + 2)", expr.ToString(), "Add#08");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Addition");

			OpClass left = new OpClass ();
			BinaryExpression expr = Expression.Add (Expression.Constant (left), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Add, expr.NodeType, "Add#09");
			Assert.AreEqual (typeof (OpClass), expr.Type, "Add#10");
			Assert.AreEqual (mi, expr.Method, "Add#11");
			Assert.AreEqual ("op_Addition", expr.Method.Name, "Add#12");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) + value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "Add#13");

			Expression.Lambda<Func<OpClass>> (expr);

#if false

	//
	// We do not have support for objects that are not really
	// constants, like this case.   Need to figure out what to do
	// with those
	//

			Func<OpClass> compiled = l.Compile ();
			Assert.AreEqual (left, compiled  ());
#endif
		}

		public class S {
			public static int MyAdder (int a, int b){
				return 1000;
			}
		}

		[Test]
		public void TestMethodAddition ()
		{
			BinaryExpression expr = Expression.Add (Expression.Constant (1), Expression.Constant (2), typeof(S).GetMethod("MyAdder"));
			Expression<Func<int>> l = Expression.Lambda<Func<int>> (expr);

			Func<int> compiled = l.Compile ();
			Assert.AreEqual (1000, compiled ());
		}

		[Test]
		public void CompileAdd ()
		{
			var left = Expression.Parameter (typeof (int), "l");
			var right = Expression.Parameter (typeof (int), "r");
			var l = Expression.Lambda<Func<int, int, int>> (
				Expression.Add (left, right), left, right);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (int), be.Type);
			Assert.IsFalse (be.IsLifted);
			Assert.IsFalse (be.IsLiftedToNull);

			var add = l.Compile ();

			Assert.AreEqual (12, add (6, 6));
			Assert.AreEqual (0, add (-1, 1));
			Assert.AreEqual (-2, add (1, -3));
		}

		[Test]
		public void AddLifted ()
		{
			var b = Expression.Add (
				Expression.Constant (null, typeof (int?)),
				Expression.Constant (null, typeof (int?)));

			Assert.AreEqual (typeof (int?), b.Type);
			Assert.IsTrue (b.IsLifted);
			Assert.IsTrue (b.IsLiftedToNull);
		}

		[Test]
		public void AddNotLifted ()
		{
			var b = Expression.Add (
				Expression.Constant (1, typeof (int)),
				Expression.Constant (1, typeof (int)));

			Assert.AreEqual (typeof (int), b.Type);
			Assert.IsFalse (b.IsLifted);
			Assert.IsFalse (b.IsLiftedToNull);
		}

		[Test]
		public void AddTestNullable ()
		{
			var a = Expression.Parameter (typeof (int?), "a");
			var b = Expression.Parameter (typeof (int?), "b");
			var l = Expression.Lambda<Func<int?, int?, int?>> (
				Expression.Add (a, b), a, b);

			var be = l.Body as BinaryExpression;
			Assert.IsNotNull (be);
			Assert.AreEqual (typeof (int?), be.Type);
			Assert.IsTrue (be.IsLifted);
			Assert.IsTrue (be.IsLiftedToNull);

			var c = l.Compile ();

			Assert.AreEqual (null, c (1, null), "a1");
			Assert.AreEqual (null, c (null, null), "a2");
			Assert.AreEqual (null, c (null, 2), "a3");
			Assert.AreEqual (3,    c (1, 2), "a4");
		}

		struct Slot {
			public int Value;

			public Slot (int value)
			{
				this.Value = value;
			}

			public static Slot operator + (Slot a, Slot b)
			{
				return new Slot (a.Value + b.Value);
			}
		}

		[Test]
		public void UserDefinedAdd ()
		{
			var l = Expression.Parameter (typeof (Slot), "l");
			var r = Expression.Parameter (typeof (Slot), "r");

			var node = Expression.Add (l, r);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (Slot), node.Type);

			var add = Expression.Lambda<Func<Slot, Slot, Slot>> (node, l, r).Compile ();

			Assert.AreEqual (new Slot (42), add (new Slot (21), new Slot (21)));
			Assert.AreEqual (new Slot (0), add (new Slot (1), new Slot (-1)));
		}

		[Test]
		public void UserDefinedAddLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.Add (l, r);

			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (Slot?), node.Type);

			var add = Expression.Lambda<Func<Slot?, Slot?, Slot?>> (node, l, r).Compile ();

			Assert.AreEqual (null, add (null, null));
			Assert.AreEqual ((Slot?) new Slot (42), add ((Slot?) new Slot (21), (Slot?) new Slot (21)));
		}

		struct SlotToNullable {
			public int Value;

			public SlotToNullable (int value)
			{
				this.Value = value;
			}

			public static SlotToNullable? operator + (SlotToNullable a, SlotToNullable b)
			{
				return new SlotToNullable (a.Value + b.Value);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UserDefinedToNullableAddFromNullable ()
		{
			Expression.Add (
				Expression.Parameter (typeof (SlotToNullable?), "l"),
				Expression.Parameter (typeof (SlotToNullable?), "r"));
		}

		[Test]
		public void UserDefinedToNullableAdd ()
		{
			var l = Expression.Parameter (typeof (SlotToNullable), "l");
			var r = Expression.Parameter (typeof (SlotToNullable), "r");

			var node = Expression.Add (l, r);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (SlotToNullable?), node.Type);
			Assert.IsNotNull (node.Method);

			var add = Expression.Lambda<Func<SlotToNullable, SlotToNullable, SlotToNullable?>> (node, l, r).Compile ();

			Assert.AreEqual ((SlotToNullable?) new SlotToNullable (4), add (new SlotToNullable (2), new SlotToNullable (2)));
			Assert.AreEqual ((SlotToNullable?) new SlotToNullable (0), add (new SlotToNullable (2), new SlotToNullable (-2)));
		}

		/*struct SlotFromNullableToNullable {
			public int Value;

			public SlotFromNullableToNullable (int value)
			{
				this.Value = value;
			}

			public static SlotFromNullableToNullable? operator + (SlotFromNullableToNullable? a, SlotFromNullableToNullable? b)
			{
				if (a.HasValue && b.HasValue)
					return (SlotFromNullableToNullable?) new SlotFromNullableToNullable (
						a.Value.Value + b.Value.Value);
				else
					return null;
			}
		}

		[Test]
		public void UserDefinedFromNullableToNullableAdd ()
		{
			var l = Expression.Parameter (typeof (SlotFromNullableToNullable?), "l");
			var r = Expression.Parameter (typeof (SlotFromNullableToNullable?), "r");

			var node = Expression.Add (l, r);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (SlotFromNullableToNullable?), node.Type);
			Assert.IsNotNull (node.Method);

			var add = Expression.Lambda<Func<SlotFromNullableToNullable?, SlotFromNullableToNullable?, SlotFromNullableToNullable?>> (node, l, r).Compile ();

			Assert.AreEqual ((SlotFromNullableToNullable?) null, add (null, null));
			Assert.AreEqual ((SlotFromNullableToNullable?) null, add (new SlotFromNullableToNullable (2), null));
			Assert.AreEqual ((SlotFromNullableToNullable?) null, add (null, new SlotFromNullableToNullable (2)));
			Assert.AreEqual ((SlotFromNullableToNullable?) new SlotFromNullableToNullable (4), add (new SlotFromNullableToNullable (2), new SlotFromNullableToNullable (2)));
			Assert.AreEqual ((SlotFromNullableToNullable?) new SlotFromNullableToNullable (0), add (new SlotFromNullableToNullable (2), new SlotFromNullableToNullable (-2)));
		}*/

		[Test]
		public void AddStrings ()
		{
			var l = Expression.Parameter (typeof (string), "l");
			var r = Expression.Parameter (typeof (string), "r");

			var meth = typeof (string).GetMethod ("Concat", new [] { typeof (object), typeof (object) });

			var node = Expression.Add (l, r, meth);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (string), node.Type);
			Assert.AreEqual (meth, node.Method);

			var concat = Expression.Lambda<Func<string, string, string>> (node, l, r).Compile ();

			Assert.AreEqual (string.Empty, concat (null, null));
			Assert.AreEqual ("foobar", concat ("foo", "bar"));
		}

		[Test]
		public void AddDecimals ()
		{
			var l = Expression.Parameter (typeof (decimal), "l");
			var r = Expression.Parameter (typeof (decimal), "r");

			var meth = typeof (decimal).GetMethod ("op_Addition", new [] { typeof (decimal), typeof (decimal) });

			var node = Expression.Add (l, r);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal), node.Type);
			Assert.AreEqual (meth, node.Method);

			var add = Expression.Lambda<Func<decimal, decimal, decimal>> (node, l, r).Compile ();

			Assert.AreEqual (2m, add (1m, 1m));
		}

		[Test]
		public void AddLiftedDecimals ()
		{
			var l = Expression.Parameter (typeof (decimal?), "l");
			var r = Expression.Parameter (typeof (decimal?), "r");

			var meth = typeof (decimal).GetMethod ("op_Addition", new [] { typeof (decimal), typeof (decimal) });

			var node = Expression.Add (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal?), node.Type);
			Assert.AreEqual (meth, node.Method);

			var add = Expression.Lambda<Func<decimal?, decimal?, decimal?>> (node, l, r).Compile ();

			Assert.AreEqual (2m, add (1m, 1m));
			Assert.AreEqual (null, add (1m, null));
			Assert.AreEqual (null, add (null, null));
		}
	}
}
