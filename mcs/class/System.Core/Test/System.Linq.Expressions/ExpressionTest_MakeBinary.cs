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
//   Miguel de Icaza <miguel@novell.com>
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
	public class ExpressionTest_MakeBinary {

		public static int GoodMethod (string a, double d)
		{
			return 1;
		}

		public static int BadMethodSig_1 ()
		{
			return 1;
		}

		public static int BadMethodSig_2 (int a)
		{
			return 1;
		}

		public static int BadMethodSig_3 (int a, int b, int c)
		{
			return 1;
		}

		static MethodInfo GM (string n)
		{
			MethodInfo [] methods = typeof (ExpressionTest_MakeBinary).GetMethods (
				BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

			foreach (MethodInfo m in methods)
				if (m.Name == n)
					return m;

			throw new Exception (String.Format ("Method {0} not found", n));
		}

		[Test]
		public void MethodChecks ()
		{
			Expression left = Expression.Constant ("");
			Expression right = Expression.Constant (1.0);

			BinaryExpression r = Expression.Add (left, right, GM ("GoodMethod"));
			Assert.AreEqual (r.Type, typeof (int));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MethodCheck_BadArgs ()
		{
			Expression left = Expression.Constant ("");
			Expression right = Expression.Constant (1.0);

			Expression.Add (left, right, GM ("BadMethodSig_1"));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MethodCheck_BadArgs2 ()
		{
			Expression left = Expression.Constant ("");
			Expression right = Expression.Constant (1.0);

			Expression.Add (left, right, GM ("BadMethodSig_2"));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MethodCheck_BadArgs3 ()
		{
			Expression left = Expression.Constant ("");
			Expression right = Expression.Constant (1.0);

			Expression.Add (left, right, GM ("BadMethodSig_3"));
		}

		static void PassInt (ExpressionType nt)
		{
			Expression left = Expression.Constant (1);
			Expression right = Expression.Constant (1);

			Expression.MakeBinary (nt, left, right);
		}

		static void FailInt (ExpressionType nt)
		{
			Expression left = Expression.Constant (1);
			Expression right = Expression.Constant (1);

			try {
				Expression.MakeBinary (nt, left, right);
			} catch (ArgumentException) {
				return;
			} catch (InvalidOperationException) {
				return;
			}
			// If we get here, there was an error
			Assert.Fail ("FailInt failed while creating an {0}", nt);
		}

		//
		// Checks that we complain on the proper ExpressionTypes
		//
		[Test]
		public void TestBinaryCtor ()
		{
			PassInt (ExpressionType.Add);
			PassInt (ExpressionType.AddChecked);
			PassInt (ExpressionType.And);
			PassInt (ExpressionType.Divide);
			PassInt (ExpressionType.Equal);
			PassInt (ExpressionType.ExclusiveOr);
			PassInt (ExpressionType.GreaterThan);
			PassInt (ExpressionType.GreaterThanOrEqual);
			PassInt (ExpressionType.LeftShift);
			PassInt (ExpressionType.LessThan);
			PassInt (ExpressionType.LessThanOrEqual);
			PassInt (ExpressionType.Multiply);
			PassInt (ExpressionType.MultiplyChecked);
			PassInt (ExpressionType.NotEqual);
			PassInt (ExpressionType.Or);
			PassInt (ExpressionType.Modulo);
			PassInt (ExpressionType.RightShift);
			PassInt (ExpressionType.Subtract);
			PassInt (ExpressionType.SubtractChecked);

			FailInt (ExpressionType.AndAlso);
			FailInt (ExpressionType.OrElse);
			FailInt (ExpressionType.Power);
			FailInt (ExpressionType.ArrayLength);
			FailInt (ExpressionType.ArrayIndex);
			FailInt (ExpressionType.Call);
			FailInt (ExpressionType.Coalesce);
			FailInt (ExpressionType.Conditional);
			FailInt (ExpressionType.Constant);
			FailInt (ExpressionType.Convert);
			FailInt (ExpressionType.ConvertChecked);
			FailInt (ExpressionType.Invoke);
			FailInt (ExpressionType.Lambda);
			FailInt (ExpressionType.ListInit);
			FailInt (ExpressionType.MemberAccess);
			FailInt (ExpressionType.MemberInit);
			FailInt (ExpressionType.Negate);
			FailInt (ExpressionType.UnaryPlus);
			FailInt (ExpressionType.NegateChecked);
			FailInt (ExpressionType.New);
			FailInt (ExpressionType.NewArrayInit);
			FailInt (ExpressionType.NewArrayBounds);
			FailInt (ExpressionType.Not);
			FailInt (ExpressionType.Parameter);
			FailInt (ExpressionType.Quote);
			FailInt (ExpressionType.TypeAs);
			FailInt (ExpressionType.TypeIs);
		}

		public T CodeGen<T> (Func<Expression, Expression, Expression> bin, T v1, T v2)
		{
			var lambda = Expression.Lambda<Func<T>> (bin (v1.ToConstant (), v2.ToConstant ())).Compile ();
			return lambda ();
		}

		[Test]
		public void TestOperations ()
		{
			Assert.AreEqual (30, CodeGen<int> ((a, b) => Expression.Add (a, b), 10, 20));
			Assert.AreEqual (-12, CodeGen<int> ((a, b) => Expression.Subtract (a, b), 11, 23));
			Assert.AreEqual (253, CodeGen<int> ((a, b) => Expression.Multiply (a, b), 11, 23));
			Assert.AreEqual (33, CodeGen<int> ((a, b) => Expression.Divide (a, b), 100, 3));
			Assert.AreEqual (100.0/3, CodeGen<double> ((a, b) => Expression.Divide (a, b), 100, 3));
		}

		void CTest<T> (ExpressionType node, bool r, T a, T b)
		{
			ParameterExpression pa = Expression.Parameter(typeof(T), "a");
			ParameterExpression pb = Expression.Parameter(typeof(T), "b");

			BinaryExpression p = Expression.MakeBinary (node, Expression.Constant (a), Expression.Constant(b));
			Expression<Func<T,T,bool>> pexpr = Expression.Lambda<Func<T,T,bool>> (
				p, new ParameterExpression [] { pa, pb });

			Func<T,T,bool> compiled = pexpr.Compile ();
			Assert.AreEqual (r, compiled (a, b), String.Format ("{0} ({1},{2}) == {3}", node, a, b, r));
		}

		[Test]
		public void ComparisonTests ()
		{
			ExpressionType t = ExpressionType.Equal;

			CTest<byte>   (t, true,   10,  10);
			CTest<sbyte>  (t, false,   1,   5);
			CTest<sbyte>  (t, true,    1,   1);
			CTest<int>    (t, true,    1,   1);
			CTest<double> (t, true,  1.0, 1.0);
			CTest<string> (t, true,  "",  "");
			CTest<string> (t, true,  "Hey",  "Hey");
			CTest<string> (t, false,  "Hey",  "There");

			t = ExpressionType.NotEqual;

			CTest<byte>   (t, false,   10,  10);
			CTest<sbyte>  (t, true,   1,   5);
			CTest<sbyte>  (t, false,    1,   1);
			CTest<int>    (t, false,    1,   1);
			CTest<double> (t, false,  1.0, 1.0);
			CTest<double> (t, false,  1.0, 1.0);
			CTest<string> (t, false,  "",  "");
			CTest<string> (t, false,  "Hey",  "Hey");
			CTest<string> (t, true,  "Hey",  "There");

			t = ExpressionType.GreaterThan;
			CTest<byte>   (t, true,   5,  1);
			CTest<byte>   (t, false,   10,  10);
			CTest<sbyte>  (t, false,   1,   5);
			CTest<sbyte>  (t, false,    1,   1);
			CTest<int>    (t, false,    1,   1);
			CTest<uint>   (t, true,     1,   0);
			CTest<ulong>  (t, true,     Int64.MaxValue,  0);
			CTest<double> (t, false,  1.0, 1.0);
			CTest<double> (t, false,  1.0, 1.0);


			t = ExpressionType.LessThan;
			CTest<byte>   (t, false,   5,  1);
			CTest<byte>   (t, false,   10,  10);
			CTest<sbyte>  (t, true,   1,   5);
			CTest<sbyte>  (t, false,    1,   1);
			CTest<int>    (t, false,    1,   1);
			CTest<uint>   (t, false,     1,   0);
			CTest<ulong>  (t, false,     Int64.MaxValue,  0);
			CTest<double> (t, false,  1.0, 1.0);
			CTest<double> (t, false,  1.0, 1.0);

			t = ExpressionType.GreaterThanOrEqual;
			CTest<byte>   (t, true,   5,  1);
			CTest<byte>   (t, true,   10,  10);
			CTest<sbyte>  (t, false,   1,   5);
			CTest<sbyte>  (t, true,    1,   1);
			CTest<int>    (t, true,    1,   1);
			CTest<uint>   (t, true,     1,   0);
			CTest<ulong>  (t, true,     Int64.MaxValue,  0);
			CTest<double> (t, true,  1.0, 1.0);
			CTest<double> (t, true,  1.0, 1.0);


			t = ExpressionType.LessThanOrEqual;
			CTest<byte>   (t, false,   5,  1);
			CTest<byte>   (t, true,   10,  10);
			CTest<sbyte>  (t, true,   1,   5);
			CTest<sbyte>  (t, true,    1,   1);
			CTest<int>    (t, true,    1,   1);
			CTest<uint>   (t, false,     1,   0);
			CTest<ulong>  (t, false,     Int64.MaxValue,  0);
			CTest<double> (t, true,  1.0, 1.0);
			CTest<double> (t, true,  1.0, 1.0);

		}

		[Test]
		public void MakeArrayIndex ()
		{
			var array = Expression.Constant (new int [] { 1, 2 }, typeof (int []));
			var index = Expression.Constant (1);

			var array_index = Expression.MakeBinary (
				ExpressionType.ArrayIndex,
				array,
				index);

			Assert.AreEqual (ExpressionType.ArrayIndex, array_index.NodeType);
		}
	}
}
