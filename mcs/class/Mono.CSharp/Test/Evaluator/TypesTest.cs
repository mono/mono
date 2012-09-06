//
// TypesTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using NUnit.Framework;
using Mono.CSharp;
using System.Collections;

namespace MonoTests.EvaluatorTest
{
	[TestFixture]
	public class TypesTest : EvaluatorFixture
	{
		[Test]
		public void SimpleTypeStaticMethod ()
		{
			object res;
			Evaluator.Run ("class Y { public static int Test () { return 5; }}");
			res = Evaluator.Evaluate ("Y.Test ();");
			Assert.AreEqual (5, res, "#1");
		}

		[Test]
		public void SameTypeNameRedefinition ()
		{
			Evaluator.Run ("class X { }");
			Evaluator.Run ("class X { public static void Foo () { throw new System.ApplicationException (); } }");
			Evaluator.Run ("class X {}");
			Evaluator.Run ("class X { public static string Foo () { return \"Test\"; } }");
			object res = Evaluator.Evaluate ("X.Foo ();");
			Assert.AreEqual ("Test", res, "#1");
			Evaluator.Run ("class X { public static int Foo () { return 5; } }");
			res = Evaluator.Evaluate ("X.Foo ();");
			Assert.AreEqual (5, res, "#2");
		}

		[Test]
		public void SimpleConstructor ()
		{
			Evaluator.Run ("class Y2 { public int Value; public Y2 (){ Value = 99; } }");
			Evaluator.Run ("var a = new Y2 ();");
			object res = Evaluator.Evaluate ("a.Value;");
			Assert.AreEqual (99, res);
		}

		[Test]
		public void TypeOfType ()
		{
			Evaluator.Run ("struct Z { }");
			object res = Evaluator.Evaluate ("typeof (Z);");
			Assert.AreEqual ("Z", res.ToString ());
		}

		[Test]
		public void UsingAfterType ()
		{
			Evaluator.Run ("struct Z { }");
			Evaluator.Run ("using System;");
			Evaluator.Run ("struct Z { }");
		}

		[Test]
		public void MoreThanOneType ()
		{
			Evaluator.Run ("public class D { int x; public int X { get { return x; } set { x = value;} } };");
			Evaluator.Run ("public class C { public int Speed{get;set;}};");
		}

		[Test]
		public void StructType ()
		{
			Evaluator.Run ("class C { }");
			Evaluator.Run ("struct B { public string foo; public int bar; }");
			Evaluator.Run ("B aStruct = new B { foo = \"foo\", bar = 1 };");
		}
	}
}