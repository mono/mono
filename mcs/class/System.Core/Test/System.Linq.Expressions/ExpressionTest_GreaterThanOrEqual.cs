//
// ExpressionTest_GreaterThanOrEqual.cs
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
	[Category("SRE")]
	public class ExpressionTest_GreaterThanOrEqual
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.GreaterThanOrEqual (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.GreaterThanOrEqual (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.GreaterThanOrEqual (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		public void Double ()
		{
			var expr = Expression.GreaterThanOrEqual (Expression.Constant (2.0), Expression.Constant (1.0));
			Assert.AreEqual (ExpressionType.GreaterThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 >= 1)", expr.ToString ());
		}

		[Test]
		public void Integer ()
		{
			var expr = Expression.GreaterThanOrEqual (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.GreaterThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 >= 1)", expr.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.GreaterThanOrEqual (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.GreaterThanOrEqual (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void UserDefinedClass ()
		{
			MethodInfo mi = typeof (OpClass).GetMethod ("op_GreaterThanOrEqual");

			Assert.IsNotNull (mi);

			BinaryExpression expr = Expression.GreaterThanOrEqual (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.GreaterThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_GreaterThanOrEqual", expr.Method.Name);
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) >= value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		[Test]
		public void NullableInt32GreaterThanOrEqual ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var gte = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.GreaterThanOrEqual (l, r), l, r).Compile ();

			Assert.IsFalse (gte (null, null));
			Assert.IsFalse (gte (null, 1));
			Assert.IsFalse (gte (null, -1));
			Assert.IsFalse (gte (1, null));
			Assert.IsFalse (gte (-1, null));
			Assert.IsFalse (gte (1, 2));
			Assert.IsTrue (gte (2, 1));
			Assert.IsTrue (gte (1, 1));
		}

		[Test]
		public void NullableInt32GreaterThanOrEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var gte = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.GreaterThanOrEqual (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, gte (null, null));
			Assert.AreEqual ((bool?) null, gte (null, 1));
			Assert.AreEqual ((bool?) null, gte (null, -1));
			Assert.AreEqual ((bool?) null, gte (1, null));
			Assert.AreEqual ((bool?) null, gte (-1, null));
			Assert.AreEqual ((bool?) false, gte (1, 2));
			Assert.AreEqual ((bool?) true, gte (2, 1));
			Assert.AreEqual ((bool?) true, gte (1, 1));
		}

		struct Slot {
			public int Value;

			public Slot (int val)
			{
				Value = val;
			}

			public static bool operator >= (Slot a, Slot b)
			{
				return a.Value >= b.Value;
			}

			public static bool operator <= (Slot a, Slot b)
			{
				return a.Value <= b.Value;
			}
		}

		[Test]
		public void UserDefinedGreaterThanOrEqualLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.GreaterThanOrEqual (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var gte = Expression.Lambda<Func<Slot?, Slot?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, gte (new Slot (1), new Slot (0)));
			Assert.AreEqual (false, gte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (true, gte (new Slot (1), new Slot (1)));
			Assert.AreEqual (false, gte (null, new Slot (1)));
			Assert.AreEqual (false, gte (new Slot (1), null));
			Assert.AreEqual (false, gte (null, null));
		}

		[Test]
		public void UserDefinedGreaterThanOrEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.GreaterThanOrEqual (l, r, true, null);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var gte = Expression.Lambda<Func<Slot?, Slot?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual (true, gte (new Slot (1), new Slot (0)));
			Assert.AreEqual (false, gte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (true, gte (new Slot (1), new Slot (1)));
			Assert.AreEqual (null, gte (null, new Slot (1)));
			Assert.AreEqual (null, gte (new Slot (1), null));
			Assert.AreEqual (null, gte (null, null));
		}
	}
}
