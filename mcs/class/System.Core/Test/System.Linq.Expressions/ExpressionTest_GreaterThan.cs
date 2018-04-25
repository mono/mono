//
// ExpressionTest_GreaterThan.cs
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
	public class ExpressionTest_GreaterThan
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.GreaterThan (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.GreaterThan (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.GreaterThan (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		public void Double ()
		{
			var expr = Expression.GreaterThan (Expression.Constant (2.0), Expression.Constant (1.0));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 > 1)", expr.ToString ());
		}

		[Test]
		public void Integer ()
		{
			var expr = Expression.GreaterThan (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 > 1)", expr.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.GreaterThan (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.GreaterThan (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void StringS ()
		{
			Expression.GreaterThan (Expression.Constant (""), Expression.Constant (""));
		}

		[Test]
		public void UserDefinedClass ()
		{
			MethodInfo mi = typeof (OpClass).GetMethod ("op_GreaterThan");

			Assert.IsNotNull (mi);

			BinaryExpression expr = Expression.GreaterThan (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_GreaterThan", expr.Method.Name);
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) > value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		[Test]
		public void TestCompiled ()
		{
			ParameterExpression a = Expression.Parameter(typeof(int), "a");
			ParameterExpression b = Expression.Parameter(typeof(int), "b");

			BinaryExpression p = Expression.GreaterThan (a, b);

			Expression<Func<int,int,bool>> pexpr = Expression.Lambda<Func<int,int,bool>> (
				p, new ParameterExpression [] { a, b });

			Func<int,int,bool> compiled = pexpr.Compile ();
			Assert.AreEqual (true, compiled (10, 1), "tc1");
			Assert.AreEqual (true, compiled (1, 0), "tc2");
			Assert.AreEqual (true, compiled (Int32.MinValue+1, Int32.MinValue), "tc3");
			Assert.AreEqual (false, compiled (-1, 0), "tc4");
			Assert.AreEqual (false, compiled (0, Int32.MaxValue), "tc5");
		}

		[Test]
		public void NullableInt32GreaterThan ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var gt = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.GreaterThan (l, r), l, r).Compile ();

			Assert.IsFalse (gt (null, null));
			Assert.IsFalse (gt (null, 1));
			Assert.IsFalse (gt (null, -1));
			Assert.IsFalse (gt (1, null));
			Assert.IsFalse (gt (-1, null));
			Assert.IsFalse (gt (1, 2));
			Assert.IsTrue (gt (2, 1));
			Assert.IsFalse (gt (1, 1));
		}

		[Test]
		public void NullableInt32GreaterThanLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var gt = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.GreaterThan (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, gt (null, null));
			Assert.AreEqual ((bool?) null, gt (null, 1));
			Assert.AreEqual ((bool?) null, gt (null, -1));
			Assert.AreEqual ((bool?) null, gt (1, null));
			Assert.AreEqual ((bool?) null, gt (-1, null));
			Assert.AreEqual ((bool?) false, gt (1, 2));
			Assert.AreEqual ((bool?) true, gt (2, 1));
			Assert.AreEqual ((bool?) false, gt (1, 1));
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
		public void UserDefinedGreaterThanLifted ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.GreaterThan (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNotNull (node.Method);

			var gte = Expression.Lambda<Func<Slot?, Slot?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (true, gte (new Slot (1), new Slot (0)));
			Assert.AreEqual (false, gte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (false, gte (new Slot (1), new Slot (1)));
			Assert.AreEqual (false, gte (null, new Slot (1)));
			Assert.AreEqual (false, gte (new Slot (1), null));
			Assert.AreEqual (false, gte (null, null));
		}

		[Test]
		public void UserDefinedGreaterThanLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (Slot?), "l");
			var r = Expression.Parameter (typeof (Slot?), "r");

			var node = Expression.GreaterThan (l, r, true, null);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.IsNotNull (node.Method);

			var gte = Expression.Lambda<Func<Slot?, Slot?, bool?>> (node, l, r).Compile ();

			Assert.AreEqual (true, gte (new Slot (1), new Slot (0)));
			Assert.AreEqual (false, gte (new Slot (-1), new Slot (1)));
			Assert.AreEqual (false, gte (new Slot (1), new Slot (1)));
			Assert.AreEqual (null, gte (null, new Slot (1)));
			Assert.AreEqual (null, gte (new Slot (1), null));
			Assert.AreEqual (null, gte (null, null));
		}

		enum Foo {
			Bar,
			Baz
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EnumGreaterThan ()
		{
			Expression.GreaterThan (
				Foo.Bar.ToConstant (),
				Foo.Baz.ToConstant ());
		}
	}
}
