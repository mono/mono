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
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void NewVoid ()
		{
			Expression.New (typeof (void));
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
			Assert.AreEqual (1, n.Arguments.Count);
			Assert.IsNull (n.Members);
			Assert.AreEqual ("new Foo(\"foo\")", n.ToString ());
		}

		[Test]
		public void NewBar ()
		{
			var n = Expression.New (typeof (Bar));

			Assert.IsNotNull (n.Constructor);
			Assert.IsNotNull (n.Arguments);
			Assert.IsNull (n.Members); // wrong doc

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

		public class FakeAnonymousType {

			public string Foo { get; set; }
			public string Bar { get; set; }
			public string Baz { get; set; }
			public int Gazonk { get; set; }
			public string Tzap { set {} }

			public FakeAnonymousType (string foo)
			{
				Foo = foo;
			}

			public FakeAnonymousType (string foo, string bar, string baz)
			{
				Foo = foo;
				Bar = bar;
				Baz = baz;
			}
		}

		[Test]
		public void NewFakeAnonymousType ()
		{
			var n = Expression.New (
				typeof (FakeAnonymousType).GetConstructor (new [] { typeof (string), typeof (string), typeof (string) } ),
				new [] { "Foo".ToConstant (), "Bar".ToConstant (), "Baz".ToConstant () },
				new [] { typeof (FakeAnonymousType).GetProperty ("Foo"), typeof (FakeAnonymousType).GetProperty ("Bar"), typeof (FakeAnonymousType).GetProperty ("Baz") });

			Assert.IsNotNull (n.Constructor);
			Assert.IsNotNull (n.Arguments);
			Assert.IsNotNull (n.Members);
			Assert.AreEqual ("new FakeAnonymousType(Foo = \"Foo\", Bar = \"Bar\", Baz = \"Baz\")", n.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullMember ()
		{
			Expression.New (
				typeof (FakeAnonymousType).GetConstructor (new [] { typeof (string) }),
				new [] { "Foo".ToConstant () },
				new MemberInfo [] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberArgumentMiscount ()
		{
			Expression.New (
				typeof (FakeAnonymousType).GetConstructor (new [] { typeof (string) }),
				new [] { "Foo".ToConstant () },
				new [] { typeof (FakeAnonymousType).GetProperty ("Foo"), typeof (FakeAnonymousType).GetProperty ("Bar") });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberArgumentMismatch ()
		{
			Expression.New (
				typeof (FakeAnonymousType).GetConstructor (new [] { typeof (string) }),
				new [] { "Foo".ToConstant () },
				new [] { typeof (FakeAnonymousType).GetProperty ("Gazonk") });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberHasNoGetter ()
		{
			Expression.New (
				typeof (FakeAnonymousType).GetConstructor (new [] { typeof (string) }),
				new [] { "Foo".ToConstant () },
				new [] { typeof (FakeAnonymousType).GetProperty ("Tzap") });
		}

		public struct EineStrukt {
			public int left;
			public int right;

			public EineStrukt (int left, int right)
			{
				this.left = left;
				this.right = right;
			}
		}

		[Test]
		public void CompileNewStruct ()
		{
			var create = Expression.Lambda<Func<EineStrukt>> (
				Expression.New (typeof (EineStrukt))).Compile ();

			var s = create ();
			Assert.AreEqual (0, s.left);
			Assert.AreEqual (0, s.right);
		}

		[Test]
		public void CompileNewStructWithParameters ()
		{
			var pl = Expression.Parameter (typeof (int), "left");
			var pr = Expression.Parameter (typeof (int), "right");

			var create = Expression.Lambda<Func<int, int, EineStrukt>> (
				Expression.New (typeof (EineStrukt).GetConstructor (new [] { typeof (int), typeof (int) }), pl, pr), pl, pr).Compile ();

			var s = create (42, 12);

			Assert.AreEqual (42, s.left);
			Assert.AreEqual (12, s.right);
		}

		public class EineKlass {

			public string Left { get; set; }
			public string Right { get; set; }

			public EineKlass ()
			{
			}

			public EineKlass (string l, string r)
			{
				Left = l;
				Right = r;
			}
		}

		[Test]
		public void CompileNewClassEmptyConstructor ()
		{
			var create = Expression.Lambda<Func<EineKlass>> (
				Expression.New (typeof (EineKlass))).Compile ();

			var k = create ();
			Assert.IsNull (k.Left);
			Assert.IsNull (k.Right);
		}

		[Test]
		public void CompileNewClassWithParameters ()
		{
			var pl = Expression.Parameter (typeof (string), "left");
			var pr = Expression.Parameter (typeof (string), "right");

			var create = Expression.Lambda<Func<string, string, EineKlass>> (
				Expression.New (typeof (EineKlass).GetConstructor (new [] { typeof (string), typeof (string) }), pl, pr), pl, pr).Compile ();

			var k = create ("foo", "bar");

			Assert.AreEqual ("foo", k.Left);
			Assert.AreEqual ("bar", k.Right);
		}
	}
}
