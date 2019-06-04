//
// ExpressionTest_LessThanOrEqual.cs
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
	public class ExpressionTest_LessThanOrEqual
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.LessThanOrEqual (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.LessThanOrEqual (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.LessThanOrEqual (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		public void Double ()
		{
			var expr = Expression.LessThanOrEqual (Expression.Constant (2.0), Expression.Constant (1.0));
			Assert.AreEqual (ExpressionType.LessThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 <= 1)", expr.ToString ());
		}

		[Test]
		public void Integer ()
		{
			var expr = Expression.LessThanOrEqual (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.LessThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 <= 1)", expr.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.LessThanOrEqual (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.LessThanOrEqual (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void UserDefinedClass ()
		{
			MethodInfo mi = typeof (OpClass).GetMethod ("op_LessThanOrEqual");

			Assert.IsNotNull (mi);

			BinaryExpression expr = Expression.LessThanOrEqual (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.LessThanOrEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_LessThanOrEqual", expr.Method.Name);
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) <= value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		[Test]
		public void NullableInt32LessThanOrEqual ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var lte = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.LessThanOrEqual (l, r), l, r).Compile ();

			Assert.IsFalse (lte (null, null));
			Assert.IsFalse (lte (null, 1));
			Assert.IsFalse (lte (null, -1));
			Assert.IsFalse (lte (1, null));
			Assert.IsFalse (lte (-1, null));
			Assert.IsTrue (lte (1, 2));
			Assert.IsFalse (lte (2, 1));
			Assert.IsTrue (lte (1, 1));
		}

		[Test]
		public void NullableInt32LessThanOrEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var lte = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.LessThanOrEqual (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, lte (null, null));
			Assert.AreEqual ((bool?) null, lte (null, 1));
			Assert.AreEqual ((bool?) null, lte (null, -1));
			Assert.AreEqual ((bool?) null, lte (1, null));
			Assert.AreEqual ((bool?) null, lte (-1, null));
			Assert.AreEqual ((bool?) true, lte (1, 2));
			Assert.AreEqual ((bool?) false, lte (2, 1));
			Assert.AreEqual ((bool?) true, lte (1, 1));
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
		public void UserDefinedLessThanOrEqualLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.LessThanOrEqual (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var lte = Expression.Lambda<Func<Slot?, Slot?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (false, lte (new Slot (1), new Slot (0)));
			Assert.AreEqual (true, lte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (true, lte (new Slot (1), new Slot (1)));
			Assert.AreEqual (false, lte (null, new Slot (1)));
			Assert.AreEqual (false, lte (new Slot (1), null));
			Assert.AreEqual (false, lte (null, null));
		}

		[Test]
		public void UserDefinedLessThanOrEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.LessThanOrEqual (l, r, true, null);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var lte = Expression.Lambda<Func<Slot?, Slot?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual (false, lte (new Slot (1), new Slot (0)));
			Assert.AreEqual (true, lte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (true, lte (new Slot (1), new Slot (1)));
			Assert.AreEqual (null, lte (null, new Slot (1)));
			Assert.AreEqual (null, lte (new Slot (1), null));
			Assert.AreEqual (null, lte (null, null));
		}
	}
}
