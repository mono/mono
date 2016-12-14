//
// ExpressionTest_LessThan.cs
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
	public class ExpressionTest_LessThan
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.LessThan (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.LessThan (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.LessThan (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		public void Double ()
		{
			var expr = Expression.LessThan (Expression.Constant (2.0), Expression.Constant (1.0));
			Assert.AreEqual (ExpressionType.LessThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 < 1)", expr.ToString ());
		}

		[Test]
		public void Integer ()
		{
			var expr = Expression.LessThan (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.LessThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 < 1)", expr.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.LessThan (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.LessThan (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void UserDefinedClass ()
		{
			MethodInfo mi = typeof (OpClass).GetMethod ("op_LessThan");

			Assert.IsNotNull (mi);

			BinaryExpression expr = Expression.LessThan (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.LessThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_LessThan", expr.Method.Name);
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) < value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		[Test]
		public void NullableInt32LessThan ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var lt = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.LessThan (l, r), l, r).Compile ();

			Assert.IsFalse (lt (null, null));
			Assert.IsFalse (lt (null, 1));
			Assert.IsFalse (lt (null, -1));
			Assert.IsFalse (lt (1, null));
			Assert.IsFalse (lt (-1, null));
			Assert.IsTrue (lt (1, 2));
			Assert.IsFalse (lt (2, 1));
			Assert.IsFalse (lt (1, 1));
		}

		[Test]
		public void NullableInt32LessThanLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var lt = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.LessThan (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, lt (null, null));
			Assert.AreEqual ((bool?) null, lt (null, 1));
			Assert.AreEqual ((bool?) null, lt (null, -1));
			Assert.AreEqual ((bool?) null, lt (1, null));
			Assert.AreEqual ((bool?) null, lt (-1, null));
			Assert.AreEqual ((bool?) true, lt (1, 2));
			Assert.AreEqual ((bool?) false, lt (2, 1));
			Assert.AreEqual ((bool?) false, lt (1, 1));
		}

		struct Slot {
			public int Value;

			public Slot (int val)
			{
				Value = val;
			}

			public static bool operator > (Slot a, Slot b)
			{
				return a.Value > b.Value;
			}

			public static bool operator < (Slot a, Slot b)
			{
				return a.Value < b.Value;
			}
		}

		[Test]
		public void UserDefinedLessThanLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.LessThan (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var lte = Expression.Lambda<Func<Slot?, Slot?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (false, lte (new Slot (1), new Slot (0)));
			Assert.AreEqual (true, lte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (false, lte (new Slot (1), new Slot (1)));
			Assert.AreEqual (false, lte (null, new Slot (1)));
			Assert.AreEqual (false, lte (new Slot (1), null));
			Assert.AreEqual (false, lte (null, null));
		}

		[Test]
		public void UserDefinedLessThanLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.LessThan (l, r, true, null);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var lte = Expression.Lambda<Func<Slot?, Slot?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual (false, lte (new Slot (1), new Slot (0)));
			Assert.AreEqual (true, lte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (false, lte (new Slot (1), new Slot (1)));
			Assert.AreEqual (null, lte (null, new Slot (1)));
			Assert.AreEqual (null, lte (new Slot (1), null));
			Assert.AreEqual (null, lte (null, null));
		}
	}
}
