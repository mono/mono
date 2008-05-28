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
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
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
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
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
			Assert.AreEqual ("(1 = 2)", expr.ToString ());
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

			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) = value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}

		//
		// Checks for the behavior when the return type for Equal is not
		// bool, and its coping with nullable values.
		//
		[Test]
		public void UserDefinedEqual ()
		{

		}

		[Test]
		[Category ("NotWorking")]
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
		}
	}
}
