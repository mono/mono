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

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
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
			ParameterExpression left = Expression.Parameter (typeof (int), "l"), right = Expression.Parameter (typeof (int), "r");
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
			ParameterExpression a = Expression.Parameter (typeof (int?), "a"), b = Expression.Parameter (typeof (int?), "b");
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

		struct EineStrukt {
			int i;

			public int I {
				get { return i; }
			}

			public EineStrukt (int i)
			{
				this.i = i;
			}

			public static EineStrukt operator + (EineStrukt a, EineStrukt b)
			{
				return new EineStrukt (a.i + b.i);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void AddNullableStruct ()
		{
			var a = Expression.Parameter (typeof (EineStrukt?), "a");
			var b = Expression.Parameter (typeof (EineStrukt?), "b");

			var body = Expression.Add (a, b);
			var lambda = Expression.Lambda<Func<EineStrukt?, EineStrukt?, EineStrukt?>> (body, a, b);

			Assert.AreEqual (typeof (EineStrukt?), body.Type);
			Assert.IsTrue (body.IsLifted);
			Assert.IsTrue (body.IsLiftedToNull);

			var add = lambda.Compile ();

			var res = add (new EineStrukt (2), new EineStrukt (3));
			Assert.IsTrue (res.HasValue);
			Assert.AreEqual (5, res.Value.I);

			res = add (null, null);
			Assert.IsFalse (res.HasValue);
		}
	}
}
