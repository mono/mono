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
//    Miguel de Icaza (miguel@novell.com)
//    Jb Evain (jbevain@novell.com)
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Equal
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Equal (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Equal (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.Equal (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		public void ReferenceCompare ()
		{
			Expression.Equal (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		public struct D {
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.Equal (Expression.Constant (new D ()), Expression.Constant (new D ()));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.Equal (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.Equal, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
#if !NET_4_0
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
#endif
		}

		[Test]
		public void Nullable_LiftToNull_SetToFalse ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.Equal (Expression.Constant (a, typeof(int?)),
								  Expression.Constant (b, typeof(int?)),
								  false, null);
			Assert.AreEqual (ExpressionType.Equal, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (true, expr.IsLifted);
			Assert.AreEqual (false, expr.IsLiftedToNull);
			Assert.IsNull (expr.Method);
#if !NET_4_0
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
#endif
		}

		[Test]
		public void Nullable_LiftToNull_SetToTrue ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.Equal (Expression.Constant (a, typeof(int?)),
								  Expression.Constant (b, typeof(int?)),
								  true, null);
			Assert.AreEqual (ExpressionType.Equal, expr.NodeType);
			Assert.AreEqual (typeof (bool?), expr.Type);
			Assert.AreEqual (true, expr.IsLifted);
			Assert.AreEqual (true, expr.IsLiftedToNull);
			Assert.IsNull (expr.Method);
#if !NET_4_0
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
#endif
		}

		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void Nullable_Mixed ()
		{
			int? a = 1;
			int b = 2;

			Expression.Equal (Expression.Constant (a, typeof (int?)),
					  Expression.Constant (b, typeof (int)));
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Equality");

			BinaryExpression expr = Expression.Equal (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Equal, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_Equality", expr.Method.Name);
#if !NET_4_0
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) = value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
#endif
		}

		[Test]
		public void NullableInt32Equal ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var eq = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.Equal (l, r), l, r).Compile ();

			Assert.IsTrue (eq (null, null));
			Assert.IsFalse (eq (null, 1));
			Assert.IsFalse (eq (1, null));
			Assert.IsFalse (eq (1, 2));
			Assert.IsTrue (eq (1, 1));
			Assert.IsFalse (eq (null, 0));
			Assert.IsFalse (eq (0, null));
		}

		[Test]
		public void NullableInt32EqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var eq = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.Equal (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, eq (null, null));
			Assert.AreEqual ((bool?) null, eq (null, 1));
			Assert.AreEqual ((bool?) null, eq (1, null));
			Assert.AreEqual ((bool?) false, eq (1, 2));
			Assert.AreEqual ((bool?) true, eq (1, 1));
			Assert.AreEqual ((bool?) null, eq (null, 0));
			Assert.AreEqual ((bool?) null, eq (0, null));
		}

		struct Slot {
			public int Value;

			public Slot (int value)
			{
				this.Value = value;
			}

			public override bool Equals (object obj)
			{
				if (!(obj is Slot))
					return false;

				var other = (Slot) obj;
				return other.Value == this.Value;
			}

			public override int GetHashCode ()
			{
				return Value;
			}

			public static bool operator == (Slot a, Slot b)
			{
				return a.Value == b.Value;
			}

			public static bool operator != (Slot a, Slot b)
			{
				return a.Value != b.Value;
			}
		}

		[Test]
		public void UserDefinedEqual ()
		{
			var l = Expression.Parameter (typeof (Slot), "l");
			var r = Expression.Parameter (typeof (Slot), "r");

			var node = Expression.Equal (l, r);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var eq = Expression.Lambda<Func<Slot, Slot, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, eq (new Slot (21), new Slot (21)));
			Assert.AreEqual (false, eq (new Slot (1), new Slot (-1)));
		}

		[Test]
		public void UserDefinedEqualLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.Equal (l, r);

			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var eq = Expression.Lambda<Func<Slot?, Slot?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, eq (null, null));
			Assert.AreEqual (false, eq ((Slot?) new Slot (2), null));
			Assert.AreEqual (false, eq (null, (Slot?) new Slot (2)));
			Assert.AreEqual (true, eq ((Slot?) new Slot (21), (Slot?) new Slot (21)));
		}

		[Test]
		public void UserDefinedEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.Equal (l, r, true, null);

			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var eq = Expression.Lambda<Func<Slot?, Slot?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual ((bool?) null, eq (null, null));
			Assert.AreEqual ((bool?) null, eq ((Slot?) new Slot (2), null));
			Assert.AreEqual ((bool?) null, eq (null, (Slot?) new Slot (2)));
			Assert.AreEqual ((bool?) true, eq ((Slot?) new Slot (21), (Slot?) new Slot (21)));
			Assert.AreEqual ((bool?) false, eq ((Slot?) new Slot (21), (Slot?) new Slot (-21)));
		}

		struct SlotToNullable {
			public int Value;

			public SlotToNullable (int value)
			{
				this.Value = value;
			}

			public override int GetHashCode ()
			{
				return Value;
			}

			public override bool Equals (object obj)
			{
				if (!(obj is SlotToNullable))
					return false;

				var other = (SlotToNullable) obj;
				return other.Value == this.Value;
			}

			public static bool? operator == (SlotToNullable a, SlotToNullable b)
			{
				return (bool?) (a.Value == b.Value);
			}

			public static bool? operator != (SlotToNullable a, SlotToNullable b)
			{
				return (bool?) (a.Value != b.Value);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UserDefinedToNullableEqualFromNullable ()
		{
			Expression.Equal (
				Expression.Parameter (typeof (SlotToNullable?), "l"),
				Expression.Parameter (typeof (SlotToNullable?), "r"));
		}

		[Test]
		public void UserDefinedToNullableEqual ()
		{
			var l = Expression.Parameter (typeof (SlotToNullable), "l");
			var r = Expression.Parameter (typeof (SlotToNullable), "r");

			var node = Expression.Equal (l, r, false, null);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var eq = Expression.Lambda<Func<SlotToNullable, SlotToNullable, bool?>> (node, l, r).Compile ();

			Assert.AreEqual ((bool?) true, eq (new SlotToNullable (2), new SlotToNullable (2)));
			Assert.AreEqual ((bool?) false, eq (new SlotToNullable (2), new SlotToNullable (-2)));
		}

		/*struct SlotFromNullableToNullable {
			public int Value;

			public SlotFromNullableToNullable (int value)
			{
				this.Value = value;
			}

			public override bool Equals (object obj)
			{
				if (!(obj is SlotFromNullableToNullable))
					return false;

				var other = (SlotFromNullableToNullable) obj;
				return other.Value == this.Value;
			}

			public override int GetHashCode ()
			{
				return Value;
			}

			public static bool? operator == (SlotFromNullableToNullable? a, SlotFromNullableToNullable? b)
			{
				if (a.HasValue && b.HasValue)
					return (bool?) (a.Value.Value == b.Value.Value);
				else
					return null;
			}

			public static bool? operator != (SlotFromNullableToNullable? a, SlotFromNullableToNullable? b)
			{
				return !(a == b);
			}
		}

		[Test]
		public void UserDefinedFromNullableToNullableEqual ()
		{
			var l = Expression.Parameter (typeof (SlotFromNullableToNullable?), "l");
			var r = Expression.Parameter (typeof (SlotFromNullableToNullable?), "r");

			var node = Expression.Equal (l, r);

			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var eq = Expression.Lambda<Func<SlotFromNullableToNullable?, SlotFromNullableToNullable?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual ((bool?) null, eq (null, null));
			Assert.AreEqual ((bool?) null, eq (new SlotFromNullableToNullable (2), null));
			Assert.AreEqual ((bool?) null, eq (null, new SlotFromNullableToNullable (2)));
			Assert.AreEqual ((bool?) true, eq (new SlotFromNullableToNullable (2), new SlotFromNullableToNullable (2)));
			Assert.AreEqual ((bool?) false, eq (new SlotFromNullableToNullable (2), new SlotFromNullableToNullable (-2)));
		}*/

		[Test]
		public void NullableBoolEqualToBool ()
		{
			var l = Expression.Parameter (typeof (bool?), "l");
			var r = Expression.Parameter (typeof (bool?), "r");

			var node = Expression.Equal (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var eq = Expression.Lambda<Func<bool?, bool?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (false, eq (true, null));
			Assert.AreEqual (true, eq (null, null));
			Assert.AreEqual (true, eq (false, false));
		}

		public enum Foo {
			Bar,
			Baz,
		}

		[Test]
		public void EnumEqual ()
		{
			var l = Expression.Parameter (typeof (Foo), "l");
			var r = Expression.Parameter (typeof (Foo), "r");

			var node = Expression.Equal (l, r);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var eq = Expression.Lambda<Func<Foo, Foo, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, eq (Foo.Bar, Foo.Bar));
			Assert.AreEqual (false, eq (Foo.Bar, Foo.Baz));
		}

		[Test]
		public void LiftedEnumEqual ()
		{
			var l = Expression.Parameter (typeof (Foo?), "l");
			var r = Expression.Parameter (typeof (Foo?), "r");

			var node = Expression.Equal (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var eq = Expression.Lambda<Func<Foo?, Foo?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, eq (Foo.Bar, Foo.Bar));
			Assert.AreEqual (false, eq (Foo.Bar, Foo.Baz));
			Assert.AreEqual (false, eq (Foo.Bar, null));
			Assert.AreEqual (true, eq (null, null));
		}

		[Test]
		public void NullableNullEqual ()
		{
			var param = Expression.Parameter (typeof (DateTime?), "x");

			var node = Expression.Equal (param, Expression.Constant (null));

			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var eq = Expression.Lambda<Func<DateTime?, bool>> (node, new [] { param }).Compile ();

			Assert.AreEqual (true, eq (null));
			Assert.AreEqual (false, eq (DateTime.Now));
		}
	}
}
