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
	[Category("SRE")]
	public class ExpressionTest_PropertyOrField
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.PropertyOrField (null, "NoPropertyOrField");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.PropertyOrField (Expression.Constant (new MemberClass()), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NoPropertyOrField ()
		{
			Expression.PropertyOrField (Expression.Constant (new MemberClass()), "NoPropertyOrField");
		}

		[Test]
		public void InstanceProperty ()
		{
			MemberExpression expr = Expression.PropertyOrField (Expression.Constant (new MemberClass()), "TestProperty1");
			Assert.AreEqual (ExpressionType.MemberAccess, expr.NodeType, "PropertyOrField#01");
			Assert.AreEqual (typeof (int), expr.Type, "PropertyOrField#02");
			Assert.AreEqual ("value(MonoTests.System.Linq.Expressions.MemberClass).TestProperty1", expr.ToString(), "PropertyOrField#04");
		}
	}
}
