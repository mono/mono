//
// ExpressionTest_Lift: this contains tests for the various lifting settings in binary expressions
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
	[Category("SRE")]
	public class ExpressionTest_Lifting
	{
		[Test]
		public void TestLiftOnEqual ()
		{
			ConstantExpression a = Expression.Constant (1, typeof (int?));
			ConstantExpression b = Expression.Constant (1, typeof (int?));

			BinaryExpression cmp = Expression.Equal (a, b);

			Assert.AreEqual (true, cmp.IsLifted, "IsLifted");
			Assert.AreEqual (false, cmp.IsLiftedToNull, "IsLiftedToNull");
			Assert.AreEqual (typeof(bool), cmp.Type, "type");
		}

		[Test]
		public void TestLiftOnEqual_ForcedLifted ()
		{
			ConstantExpression a = Expression.Constant (1, typeof (int?));
			ConstantExpression b = Expression.Constant (1, typeof (int?));

			// Force the lift on equal
			BinaryExpression cmp = Expression.Equal (a, b, true, null);

			Assert.AreEqual (true, cmp.IsLifted, "IsLifted");
			Assert.AreEqual (true, cmp.IsLiftedToNull, "IsLiftedToNull");
			Assert.AreEqual (typeof(bool?), cmp.Type);
		}

		static MethodInfo GM(string n)
		{
			MethodInfo [] m = typeof(ExpressionTest_Lifting).GetMethods(BindingFlags.Static | BindingFlags.Public);

			foreach (MethodInfo mm in m)
				if (mm.Name == n)
					return mm;

			throw new Exception("No method found: " + n);
		}

		static public int MyCompare (OpStruct a, OpStruct b)
		{
			return 1;
		}

		[Test]
		public void TestLiftOnEqual_WithMethodInfo ()
		{
			ConstantExpression a = Expression.Constant (new OpStruct (), typeof (OpStruct?));
			ConstantExpression b = Expression.Constant (null, typeof (OpStruct?));

			// Force the lift on equal
			BinaryExpression cmp = Expression.Equal (a, b, true, GM("MyCompare"));

			Assert.AreEqual (true, cmp.IsLifted, "IsLifted");
			Assert.AreEqual (true, cmp.IsLiftedToNull, "IsLiftedToNull");

			BinaryExpression cmp2 = Expression.Equal (a, b, false, GM("MyCompare"));

			//
			// When we use a MethodInfo, that has a non-bool return type,
			// the result is always Nullable<returntype> regardless of the
			// setting of "liftToNull"
			//
			Assert.AreEqual (typeof(int?), cmp.Type);
			Assert.AreEqual (typeof(int?), cmp2.Type);

		}
	}
}
