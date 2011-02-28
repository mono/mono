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
		public void GG ()
		{
			Evaluator.Run ("public class D { int x; public int X { get { return x; } set { x = value;} } };");
			Evaluator.Run ("public class C { public int Speed{get;set;}};");
		}
	}
}