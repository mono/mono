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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Bind
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Bind (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Bind (MemberClass.GetRwFieldInfo (), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Method1 ()
		{
			// This tests the MethodInfo version of Bind(): should raise an exception
			// because the method is not an accessor.
			Expression.Bind (MemberClass.GetMethodInfo (), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Method2 ()
		{
			// This tests the MemberInfo version of Bind(): should raise an exception
			// because the argument is not a field or property accessor.
			Expression.Bind ((MemberInfo)MemberClass.GetMethodInfo (), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Event ()
		{
			Expression.Bind (MemberClass.GetEventInfo (), Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PropertyRo ()
		{
			Expression.Bind (MemberClass.GetRoPropertyInfo (), Expression.Constant (1));
		}

		[Test]
		public void FieldRo ()
		{
			MemberAssignment expr = Expression.Bind (MemberClass.GetRoFieldInfo (), Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#01");
			Assert.AreEqual ("TestField1 = 1", expr.ToString(), "Bind#02");
		}

		[Test]
		public void FieldRw ()
		{
			MemberAssignment expr = Expression.Bind (MemberClass.GetRwFieldInfo (), Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#03");
			Assert.AreEqual ("TestField2 = 1", expr.ToString(), "Bind#04");
		}

		[Test]
		public void FieldStatic ()
		{
			MemberAssignment expr = Expression.Bind (MemberClass.GetStaticFieldInfo (), Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#05");
			Assert.AreEqual ("StaticField = 1", expr.ToString(), "Bind#06");
		}

		[Test]
		public void PropertyRw ()
		{
			MemberAssignment expr = Expression.Bind (MemberClass.GetRwPropertyInfo (), Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#07");
			Assert.AreEqual ("TestProperty2 = 1", expr.ToString(), "Bind#08");
		}

		[Test]
		public void PropertyStatic ()
		{
			MemberAssignment expr = Expression.Bind (MemberClass.GetStaticPropertyInfo (), Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#09");
			Assert.AreEqual ("StaticProperty = 1", expr.ToString(), "Bind#10");
		}

		[Test]
		public void PropertyAccessor ()
		{
			MethodInfo mi = typeof(MemberClass).GetMethod("get_TestProperty2");

			MemberAssignment expr = Expression.Bind (mi, Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#11");
			Assert.AreEqual ("TestProperty2 = 1", expr.ToString(), "Bind#12");
			Assert.AreEqual (MemberClass.GetRwPropertyInfo(), expr.Member, "Bind#13");
		}

		[Test]
		public void PropertyAccessorStatic ()
		{
			MethodInfo mi = typeof(MemberClass).GetMethod("get_StaticProperty");

			MemberAssignment expr = Expression.Bind (mi, Expression.Constant (1));
			Assert.AreEqual (MemberBindingType.Assignment, expr.BindingType, "Bind#14");
			Assert.AreEqual ("StaticProperty = 1", expr.ToString(), "Bind#15");
			Assert.AreEqual (MemberClass.GetStaticPropertyInfo(), expr.Member, "Bind#16");
		}

		struct Slot {
			public int Integer { get; set; }
			public short Short { get; set; }
		}

		[Test]
		public void BindValueTypes ()
		{
			var i = Expression.Parameter (typeof (int), "i");
			var s = Expression.Parameter (typeof (short), "s");

			var gslot = Expression.Lambda<Func<int, short, Slot>> (
				Expression.MemberInit (
					Expression.New (typeof (Slot)),
					Expression.Bind (typeof (Slot).GetProperty ("Integer"), i),
					Expression.Bind (typeof (Slot).GetProperty ("Short"), s)), i, s).Compile ();

			Assert.AreEqual (new Slot { Integer = 42, Short = -1 }, gslot (42, -1));
		}
	}
}
