//
// ExpressionTest_Negate.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Negate
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Negate (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgNotStatic ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryNotStatic"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgParameterCount ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryParameterCount"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgReturnsVoid ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryReturnVoid"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NegateBool ()
		{
			Expression.UnaryPlus (true.ToConstant ());
		}

		[Test]
		public void Number ()
		{
			var up = Expression.Negate (Expression.Constant (1));
			Assert.AreEqual ("-1", up.ToString ());
		}

		[Test]
		public void UserDefinedClass ()
		{
			var mi = typeof (OpClass).GetMethod ("op_UnaryNegation");

			var expr = Expression.Negate (Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Negate, expr.NodeType);
			Assert.AreEqual (typeof (OpClass), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_UnaryNegation", expr.Method.Name);
			Assert.AreEqual ("-value(MonoTests.System.Linq.Expressions.OpClass)",	expr.ToString ());
		}

		[Test]
		public void NegateNullableInt32 ()
		{
			var n = Expression.Negate (Expression.Parameter (typeof (int?), ""));
			Assert.AreEqual (typeof (int?), n.Type);
			Assert.IsTrue (n.IsLifted);
			Assert.IsTrue (n.IsLiftedToNull);
			Assert.IsNull (n.Method);
		}

		[Test]
		public void CompileNegateInt32 ()
		{
			var p = Expression.Parameter (typeof (int), "i");
			var negate = Expression.Lambda<Func<int, int>> (Expression.Negate (p), p).Compile ();

			Assert.AreEqual (-2, negate (2));
			Assert.AreEqual (0, negate (0));
			Assert.AreEqual (3, negate (-3));
		}

		[Test]
		public void CompiledNegateNullableInt32 ()
		{
			var p = Expression.Parameter (typeof (int?), "i");
			var negate = Expression.Lambda<Func<int?, int?>> (Expression.Negate (p), p).Compile ();

			Assert.AreEqual (null, negate (null));
			Assert.AreEqual ((int?) -2, negate (2));
			Assert.AreEqual ((int?) 0, negate (0));
			Assert.AreEqual ((int?) 3, negate (-3));
		}

		struct Slot {
			public int Value;

			public Slot (int value)
			{
				this.Value = value;
			}

			public static Slot operator - (Slot s)
			{
				return new Slot (-s.Value);
			}
		}

		[Test]
		public void UserDefinedNegate ()
		{
			var s = Expression.Parameter (typeof (Slot), "s");
			var node = Expression.Negate (s);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (Slot), node.Type);

			var negate = Expression.Lambda<Func<Slot, Slot>> (node, s).Compile ();

			Assert.AreEqual (new Slot (-2), negate (new Slot (2)));
			Assert.AreEqual (new Slot (42), negate (new Slot (-42)));
		}

		[Test]
		public void UserDefinedNotNullableNegateNullable ()
		{
			var s = Expression.Parameter (typeof (Slot?), "s");
			var node = Expression.Negate (s);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (Slot?), node.Type);

			var negate = Expression.Lambda<Func<Slot?, Slot?>> (node, s).Compile ();

			Assert.AreEqual (null, negate (null));
			Assert.AreEqual (new Slot (42), negate (new Slot (-42)));
			Assert.AreEqual (new Slot (-2), negate (new Slot (2)));
		}

		struct SlotToNullable {
			public int Value;

			public SlotToNullable (int value)
			{
				this.Value = value;
			}

			public static SlotToNullable? operator - (SlotToNullable s)
			{
				return new SlotToNullable (-s.Value);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UserDefinedToNullableNegateFromNullable ()
		{
			Expression.Negate (Expression.Parameter (typeof (SlotToNullable?), "s"));
		}

		[Test]
		public void UserDefinedToNullableNegateNullable ()
		{
			var s = Expression.Parameter (typeof (SlotToNullable), "s");
			var node = Expression.Negate (s);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (SlotToNullable?), node.Type);

			var negate = Expression.Lambda<Func<SlotToNullable, SlotToNullable?>> (node, s).Compile ();

			Assert.AreEqual ((SlotToNullable?) new SlotToNullable (42), negate (new SlotToNullable (-42)));
			Assert.AreEqual ((SlotToNullable?) new SlotToNullable (-2), negate (new SlotToNullable (2)));
		}

		struct SlotFromNullable {
			public int Value;

			public SlotFromNullable (int value)
			{
				this.Value = value;
			}

			public static SlotFromNullable operator - (SlotFromNullable? s)
			{
				if (s.HasValue)
					return new SlotFromNullable (-s.Value.Value);
				else
					return new SlotFromNullable (-1);
			}
		}

		[Test]
		public void UserDefinedNegateFromNullable ()
		{
			var s = Expression.Parameter (typeof (SlotFromNullable?), "s");
			var node = Expression.Negate (s);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (SlotFromNullable), node.Type);

			var negate = Expression.Lambda<Func<SlotFromNullable?, SlotFromNullable>> (node, s).Compile ();

			Assert.AreEqual (new SlotFromNullable (-2), negate (new SlotFromNullable (2)));
			Assert.AreEqual (new SlotFromNullable (42), negate (new SlotFromNullable (-42)));
			Assert.AreEqual (new SlotFromNullable (-1), negate (null));
		}

		struct SlotFromNullableToNullable {
			public int Value;

			public SlotFromNullableToNullable (int value)
			{
				this.Value = value;
			}

			public static SlotFromNullableToNullable? operator - (SlotFromNullableToNullable? s)
			{
				if (s.HasValue)
					return new SlotFromNullableToNullable (-s.Value.Value);
				else
					return s;
			}
		}

		[Test]
		public void UserDefinedNegateFromNullableNotNullable ()
		{
			var s = Expression.Parameter (typeof (SlotFromNullableToNullable?), "s");
			var node = Expression.Negate (s);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (SlotFromNullableToNullable?), node.Type);

			var negate = Expression.Lambda<Func<SlotFromNullableToNullable?, SlotFromNullableToNullable?>> (
				node, s).Compile ();

			Assert.AreEqual (new SlotFromNullableToNullable (-2), negate (new SlotFromNullableToNullable (2)));
			Assert.AreEqual (new SlotFromNullableToNullable (42), negate (new SlotFromNullableToNullable (-42)));
			Assert.AreEqual (null, negate (null));
		}

		[Test]
		public void NegateDecimal ()
		{
			var d = Expression.Parameter (typeof (decimal), "l");

			var meth = typeof (decimal).GetMethod ("op_UnaryNegation", new [] { typeof (decimal) });

			var node = Expression.Negate (d);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal), node.Type);
			Assert.AreEqual (meth, node.Method);

			var neg = Expression.Lambda<Func<decimal, decimal>> (node, d).Compile ();

			Assert.AreEqual (-2m, neg (2m));
		}

		[Test]
		public void NegateLiftedDecimal ()
		{
			var d = Expression.Parameter (typeof (decimal?), "l");

			var meth = typeof (decimal).GetMethod ("op_UnaryNegation", new [] { typeof (decimal) });

			var node = Expression.Negate (d);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal?), node.Type);
			Assert.AreEqual (meth, node.Method);

			var neg = Expression.Lambda<Func<decimal?, decimal?>> (node, d).Compile ();

			Assert.AreEqual (-2m, neg (2m));
			Assert.AreEqual (null, neg (null));
		}
	}
}
