//
// ExpressionTest_New.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_New {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullType ()
		{
			Expression.New (null as Type);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullConstructor ()
		{
			Expression.New (null as ConstructorInfo);
		}

		public class Foo {

			public Foo (string s)
			{
			}
		}

		public class Bar {

			public string Value { get; set; }

			public Bar ()
			{
			}
		}

		public struct Baz {
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NoParameterlessConstructor ()
		{
			Expression.New (typeof (Foo));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorHasTooMuchParameters ()
		{
			Expression.New (typeof (Foo).GetConstructor (new [] { typeof (string) }));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HasNullArgument ()
		{
			Expression.New (typeof (Foo).GetConstructor (new [] { typeof (string) }), null as Expression);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HasWrongArgument ()
		{
			Expression.New (typeof (Foo).GetConstructor (new [] { typeof (string) }), Expression.Constant (12));
		}

		[Test]
		public void NewFoo ()
		{
			var n = Expression.New (typeof (Foo).GetConstructor (new [] { typeof (string) }), Expression.Constant ("foo"));

			Assert.AreEqual (ExpressionType.New, n.NodeType);
			Assert.AreEqual (typeof (Foo), n.Type);
			Assert.AreEqual ("new Foo(\"foo\")", n.ToString ());
		}

		[Test]
		public void NewBar ()
		{
			var n = Expression.New (typeof (Bar));

			Assert.IsNull (n.Constructor);
			Assert.AreEqual ("new Bar()", n.ToString ());

			n = Expression.New (typeof (Bar).GetConstructor (Type.EmptyTypes));

			Assert.AreEqual ("new Bar()", n.ToString ());
		}

		public class Gazonk {

			string value;

			public Gazonk (string s)
			{
				value = s;
			}

			public override bool Equals (object obj)
			{
				var o = obj as Gazonk;
				if (o == null)
					return false;

				return value == o.value;
			}

			public override int GetHashCode ()
			{
				return value.GetHashCode ();
			}
		}

		[Test]
		public void CompileNewClass ()
		{
			var p = Expression.Parameter (typeof (string), "p");
			var n = Expression.New (typeof (Gazonk).GetConstructor (new [] { typeof (string) }), p);
			var fgaz = Expression.Lambda<Func<string, Gazonk>> (n, p).Compile ();

			var g1 = new Gazonk ("foo");
			var g2 = new Gazonk ("bar");

			Assert.IsNotNull (g1);
			Assert.AreEqual (g1, fgaz ("foo"));
			Assert.IsNotNull (g2);
			Assert.AreEqual (g2, fgaz ("bar"));

			n = Expression.New (typeof (Bar));
			var lbar = Expression.Lambda<Func<Bar>> (n).Compile ();

			var bar = lbar ();

			Assert.IsNotNull (bar);
			Assert.IsNull (bar.Value);
		}
	}
}
