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
//	Miguel de Icaza (miguel@novell.com)
//	Jb Evain (jbevain@novell.com)
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_NotEqual
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.NotEqual (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.NotEqual (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.NotEqual (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		public void ReferenceCompare ()
		{
			Expression.NotEqual (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		public struct D {
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.NotEqual (Expression.Constant (new D ()), Expression.Constant (new D ()));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.NotEqual (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.NotEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(1 != 2)", expr.ToString ());
		}

		[Test]
		public void Nullable_LiftToNull_SetToFalse ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.NotEqual (Expression.Constant (a, typeof(int?)),
								  Expression.Constant (b, typeof(int?)),
								  false, null);
			Assert.AreEqual (ExpressionType.NotEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (true, expr.IsLifted);
			Assert.AreEqual (false, expr.IsLiftedToNull);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(1 != 2)", expr.ToString ());
		}

		[Test]
		public void Nullable_LiftToNull_SetToTrue ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.NotEqual (Expression.Constant (a, typeof(int?)),
								  Expression.Constant (b, typeof(int?)),
								  true, null);
			Assert.AreEqual (ExpressionType.NotEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool?), expr.Type);
			Assert.AreEqual (true, expr.IsLifted);
			Assert.AreEqual (true, expr.IsLiftedToNull);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(1 != 2)", expr.ToString ());
		}

		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void Nullable_Mixed ()
		{
			int? a = 1;
			int b = 2;

			Expression.NotEqual (Expression.Constant (a, typeof (int?)),
					  Expression.Constant (b, typeof (int)));
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Inequality");

			BinaryExpression expr = Expression.NotEqual (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.NotEqual, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_Inequality", expr.Method.Name);

			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) != value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		[Test]
		public void NullableInt32NotEqual ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var neq = Expression.Lambda<Func<int?, int?, bool>> (
				Expression.NotEqual (l, r), l, r).Compile ();

			Assert.IsFalse (neq (null, null));
			Assert.IsTrue (neq (null, 1));
			Assert.IsTrue (neq (1, null));
			Assert.IsTrue (neq (1, 2));
			Assert.IsFalse (neq (1, 1));
			Assert.IsTrue (neq (null, 0));
			Assert.IsTrue (neq (0, null));
		}

		[Test]
		public void NullableInt32NotEqualLiftedToNull ()
		{
			var l = Expression.Parameter (typeof (int?), "l");
			var r = Expression.Parameter (typeof (int?), "r");

			var neq = Expression.Lambda<Func<int?, int?, bool?>> (
				Expression.NotEqual (l, r, true, null), l, r).Compile ();

			Assert.AreEqual ((bool?) null, neq (null, null));
			Assert.AreEqual ((bool?) null, neq (null, 1));
			Assert.AreEqual ((bool?) null, neq (1, null));
			Assert.AreEqual ((bool?) true, neq (1, 2));
			Assert.AreEqual ((bool?) false, neq (1, 1));
			Assert.AreEqual ((bool?) null, neq (null, 0));
			Assert.AreEqual ((bool?) null, neq (0, null));
		}


		public enum Foo {
			Bar,
			Baz,
		}

		[Test]
		public void EnumNotEqual ()
		{
			var l = Expression.Parameter (typeof (Foo), "l");
			var r = Expression.Parameter (typeof (Foo), "r");

			var node = Expression.NotEqual (l, r);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var neq = Expression.Lambda<Func<Foo, Foo, bool>> (node, l, r).Compile ();

			Assert.AreEqual (false, neq (Foo.Bar, Foo.Bar));
			Assert.AreEqual (true, neq (Foo.Bar, Foo.Baz));
		}

		[Test]
		public void LiftedEnumNotEqual ()
		{
			var l = Expression.Parameter (typeof (Foo?), "l");
			var r = Expression.Parameter (typeof (Foo?), "r");

			var node = Expression.NotEqual (l, r);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool), node.Type);
			Assert.IsNull (node.Method);

			var neq = Expression.Lambda<Func<Foo?, Foo?, bool>> (node, l, r).Compile ();

			Assert.AreEqual (false, neq (Foo.Bar, Foo.Bar));
			Assert.AreEqual (true, neq (Foo.Bar, Foo.Baz));
			Assert.AreEqual (true, neq (Foo.Bar, null));
			Assert.AreEqual (false, neq (null, null));
		}
	}
}
