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
	public class ExpressionTest_TypeIs
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.TypeIs (null, typeof (int));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.TypeIs (Expression.Constant (1), null);
		}

		[Test]
		public void Numeric ()
		{
			TypeBinaryExpression expr = Expression.TypeIs (Expression.Constant (1), typeof (int));
			Assert.AreEqual (ExpressionType.TypeIs, expr.NodeType, "TypeIs#01");
			Assert.AreEqual (typeof (bool), expr.Type, "TypeIs#02");
			Assert.AreEqual ("(1 Is Int32)", expr.ToString(), "TypeIs#03");
		}

		[Test]
		public void String ()
		{
			TypeBinaryExpression expr = Expression.TypeIs (Expression.Constant (1), typeof (string));
			Assert.AreEqual (ExpressionType.TypeIs, expr.NodeType, "TypeIs#04");
			Assert.AreEqual (typeof (bool), expr.Type, "TypeIs#05");
			Assert.AreEqual ("(1 Is String)", expr.ToString(), "TypeIs#06");
		}

		[Test]
		public void UserDefinedClass ()
		{
			TypeBinaryExpression expr = Expression.TypeIs (Expression.Constant (new OpClass()), typeof (OpClass));
			Assert.AreEqual (ExpressionType.TypeIs, expr.NodeType, "TypeIs#07");
			Assert.AreEqual (typeof (bool), expr.Type, "TypeIs#08");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) Is OpClass)", expr.ToString(), "TypeIs#09");
		}

		struct Foo {
		}

		class Bar {
		}

		class Baz : Bar {
		}

		static Func<TType, bool> CreateTypeIs<TType, TCandidate> ()
		{
			var p = Expression.Parameter (typeof (TType), "p");

			return Expression.Lambda<Func<TType, bool>> (
				Expression.TypeIs (p, typeof (TCandidate)), p).Compile ();
		}

		[Test]
		public void CompiledTypeIs ()
		{
			var foo_is_bar = CreateTypeIs<Foo, Bar> ();
			var foo_is_foo = CreateTypeIs<Foo, Foo> ();
			var bar_is_bar = CreateTypeIs<Bar, Bar> ();
			var bar_is_foo = CreateTypeIs<Bar, Foo> ();
			var baz_is_bar = CreateTypeIs<Baz, Bar> ();

			Assert.IsTrue (foo_is_foo (new Foo ()));
			Assert.IsFalse (foo_is_bar (new Foo ()));
			Assert.IsTrue (bar_is_bar (new Bar ()));
			Assert.IsFalse (bar_is_foo (new Bar ()));
			Assert.IsTrue (baz_is_bar (new Baz ()));
		}


		public static void TacTac ()
		{
		}

		[Test]
		public void VoidIsObject ()
		{
			var vio = Expression.Lambda<Func<bool>> (
				Expression.TypeIs (
					Expression.Call (GetType ().GetMethod ("TacTac")),
					typeof (object))).Compile ();

			Assert.IsFalse (vio ());
		}
	}
}
