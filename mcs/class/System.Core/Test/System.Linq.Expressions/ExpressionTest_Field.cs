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
	public class ExpressionTest_Field
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Field (null, "NoField");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null1 ()
		{
			Expression.Field (Expression.Constant (new MemberClass()), (string)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null2 ()
		{
			Expression.Field (Expression.Constant (new MemberClass()), (FieldInfo)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NoField ()
		{
			Expression.Field (Expression.Constant (new MemberClass()), "NoField");
		}

		[Test]
		public void InstanceField ()
		{
			MemberExpression expr = Expression.Field (Expression.Constant (new MemberClass()), "TestField1");
			Assert.AreEqual (ExpressionType.MemberAccess, expr.NodeType, "Field#01");
			Assert.AreEqual (typeof (int), expr.Type, "Field#02");
			Assert.AreEqual ("value(MonoTests.System.Linq.Expressions.MemberClass).TestField1", expr.ToString(), "Field#03");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void StaticField1 ()
		{
			// This will fail because access to a static field should be created using a FieldInfo and
			// not an instance plus the field name.
			Expression.Field (Expression.Constant (new MemberClass()), "StaticField");
		}

		[Test]
		public void StaticField2 ()
		{
			MemberExpression expr = Expression.Field (null, MemberClass.GetStaticFieldInfo());
			Assert.AreEqual (ExpressionType.MemberAccess, expr.NodeType, "Field#07");
			Assert.AreEqual (typeof (int), expr.Type, "Field#08");
			Assert.AreEqual ("MemberClass.StaticField", expr.ToString(), "Field#09");
		}

		[Test]
		[Category ("NotDotNet")] // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=339351
		[ExpectedException (typeof (ArgumentException))]
		public void StaticFieldWithInstanceArgument ()
		{
			Expression.Field (
				Expression.Parameter (GetType (), "t"),
				GetType ().GetField ("foo"));
		}

		public static string foo = "foo";

		[Test]
		public void CompileStaticField ()
		{
			var foo = Expression.Lambda<Func<string>> (
				Expression.Field (null, GetType ().GetField (
					"foo", BindingFlags.Static | BindingFlags.Public))).Compile ();

			Assert.AreEqual ("foo", foo ());
		}

		public class Bar {
			public string baz;

			public Bar ()
			{
				baz = "baz";
			}
		}

		[Test]
		public void CompileInstanceField ()
		{
			var p = Expression.Parameter (typeof (Bar), "bar");
			var baz = Expression.Lambda<Func<Bar, string>> (
				Expression.Field (p, typeof (Bar).GetField (
					"baz", BindingFlags.Public | BindingFlags.Instance)), p).Compile ();

			Assert.AreEqual ("baz", baz (new Bar ()));
		}

		public struct Gazonk {
			public string Tzap;

			public Gazonk (string tzap)
			{
				Tzap = tzap;
			}
		}

		[Test]
		public void CompileStructInstanceField ()
		{
			var p = Expression.Parameter (typeof (Gazonk), "gazonk");
			var gazonker = Expression.Lambda<Func<Gazonk, string>> (
				Expression.Field (p, typeof (Gazonk).GetField ("Tzap")), p).Compile ();

			Assert.AreEqual ("bang", gazonker (new Gazonk ("bang")));
		}
	}
}
