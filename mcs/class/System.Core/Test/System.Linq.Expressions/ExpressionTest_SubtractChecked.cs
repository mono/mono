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
	public class ExpressionTest_SubtractChecked
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.SubtractChecked (null, Expression.Constant(1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.SubtractChecked (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgTypesDifferent ()
		{
			Expression.SubtractChecked (Expression.Constant (1), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.SubtractChecked (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.SubtractChecked (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void Numeric ()
		{
			BinaryExpression expr = Expression.SubtractChecked (Expression.Constant (1), Expression.Constant (2));
			Assert.AreEqual (ExpressionType.SubtractChecked, expr.NodeType, "SubtractChecked#01");
			Assert.AreEqual (typeof (int), expr.Type, "SubtractChecked#02");
			Assert.IsNull (expr.Method, "SubtractChecked#03");
			Assert.AreEqual ("(1 - 2)", expr.ToString(), "SubtractChecked#15");
		}

		[Test]
		public void Nullable ()
		{
			int? a = 1;
			int? b = 2;

			BinaryExpression expr = Expression.SubtractChecked (Expression.Constant (a), Expression.Constant (b));
			Assert.AreEqual (ExpressionType.SubtractChecked, expr.NodeType, "SubtractChecked#04");
			Assert.AreEqual (typeof (int), expr.Type, "SubtractChecked#05");
			Assert.IsNull (expr.Method, null, "SubtractChecked#06");
			Assert.AreEqual ("(1 - 2)", expr.ToString(), "SubtractChecked#16");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_Subtraction");

			BinaryExpression expr = Expression.SubtractChecked (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.SubtractChecked, expr.NodeType, "SubtractChecked#07");
			Assert.AreEqual (typeof (OpClass), expr.Type, "SubtractChecked#08");
			Assert.AreEqual (mi, expr.Method, "SubtractChecked#09");
			Assert.AreEqual ("op_Subtraction", expr.Method.Name, "SubtractChecked#10");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) - value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "SubtractChecked#17");
		}

		[Test]
		public void UserDefinedStruct ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpStruct).GetMethod ("op_Subtraction");

			BinaryExpression expr = Expression.SubtractChecked (Expression.Constant (new OpStruct ()), Expression.Constant (new OpStruct ()));
			Assert.AreEqual (ExpressionType.SubtractChecked, expr.NodeType, "SubtractChecked#11");
			Assert.AreEqual (typeof (OpStruct), expr.Type, "SubtractChecked#12");
			Assert.AreEqual (mi, expr.Method, "SubtractChecked#13");
			Assert.AreEqual ("op_Subtraction", expr.Method.Name, "SubtractChecked#14");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpStruct) - value(MonoTests.System.Linq.Expressions.OpStruct))",
				expr.ToString(), "SubtractChecked#18");
		}

		//
		// This method makes sure that compiling an AddChecked on two values
		// throws an OverflowException, if it doesnt, it fails
		//
		static void MustOverflow<T> (T v1, T v2)
		{
			Expression<Func<T>> l = Expression.Lambda<Func<T>> (
				Expression.SubtractChecked (Expression.Constant (v1), Expression.Constant (v2)));
			Func<T> del = l.Compile ();
			T res = default (T);
			try {
				res = del ();
			} catch (OverflowException){
				// OK
				return;
			}
			throw new Exception (String.Format ("SubtractChecked on {2} should have thrown an exception with values {0} {1}, result was: {3}",
							    v1, v2, v1.GetType (), res));
		}

		//
		// This routine should execute the code, but not throw an
		// overflow exception
		//
		static void MustNotOverflow<T> (T v1, T v2)
		{
			Expression<Func<T>> l = Expression.Lambda<Func<T>> (
				Expression.SubtractChecked (Expression.Constant (v1), Expression.Constant (v2)));
			Func<T> del = l.Compile ();
			del ();
		}

		//
		// SubtractChecked is not defined for small types (byte, sbyte)
		//
		static void InvalidOperation<T> (T v1, T v2)
		{
			try {
				Expression.Lambda<Func<T>> (
					Expression.SubtractChecked (Expression.Constant (v1), Expression.Constant (v2)));
			} catch (InvalidOperationException){
				// OK
				return;
			}
			throw new Exception (String.Format ("SubtractChecked should have thrown for the creation of a tree with {0} operands", v1.GetType ()));
		}

		[Test]
		public void TestOverflows ()
		{
			// These should overflow, check the various types and codepaths
			// in BinaryExpression:
			MustOverflow<int> (Int32.MinValue, 1);
			MustOverflow<int> (Int32.MaxValue, -1);
			MustOverflow<long> (Int64.MinValue, 1);
			MustOverflow<long> (Int64.MaxValue, -1);

			MustOverflow<ushort> (UInt16.MinValue, 1);

			// unsigned values use Sub_Ovf_Un, check that too:
			MustOverflow<ulong> (0, 1);
			MustOverflow<uint>  (0, 1);
		}

#if !NET_4_0 // fixed in net_4_0
		[Test]
		public void TestBugCompatibility ()
		{
			// This one does not overflow on .NET (!!!)
			// Sounds very odd, should file a bug with MS.
			MustNotOverflow<short> (Int16.MinValue, 1);
		}
#endif

		//
		// These should not overflow
		//
		[Test]
		public void TestNoOverflow ()
		{
			// Simple stuff
			MustNotOverflow<int> (10, 20);

			// There are invalid:
			InvalidOperation<byte> (Byte.MinValue, 1);
			InvalidOperation<sbyte> (SByte.MaxValue, 2);

			MustNotOverflow<short> (Int16.MaxValue, 2);
			MustNotOverflow<ushort> (UInt16.MaxValue, 2);

			// Doubles, floats, do not overflow
			MustNotOverflow<float> (Single.MaxValue, 1);
			MustNotOverflow<double> (Double.MaxValue, 1);
		}
	}
}
