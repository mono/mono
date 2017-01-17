//
// CodeGeneratorIdentifierTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell inc.
//
using System;
using System.CodeDom.Compiler;
using NUnit.Framework;
using Microsoft.CSharp;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CodeGeneratorIdentifierTest
	{
		private ICodeGenerator gen;

		public CodeGeneratorIdentifierTest ()
		{
			gen = new CSharpCodeProvider ().CreateGenerator ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestCreateValidIdentifierNullArg ()
		{
			gen.CreateValidIdentifier (null);
		}

		[Test]
		public void TestCreateValidIdentifier ()
		{
		
			Assert.AreEqual ("a", gen.CreateValidIdentifier ("a"));
			Assert.AreEqual ("_int", gen.CreateValidIdentifier ("int"));
			Assert.AreEqual ("_", gen.CreateValidIdentifier ("_"));
			Assert.AreEqual ("1", gen.CreateValidIdentifier ("1"));
			Assert.AreEqual ("1a", gen.CreateValidIdentifier ("1a"));
			Assert.AreEqual ("1*2", gen.CreateValidIdentifier ("1*2"));
			Assert.AreEqual ("-", gen.CreateValidIdentifier ("-"));
			Assert.AreEqual ("+", gen.CreateValidIdentifier ("+"));
			Assert.AreEqual ("", gen.CreateValidIdentifier (""));
			Assert.AreEqual ("--", gen.CreateValidIdentifier ("--"));
			Assert.AreEqual ("++", gen.CreateValidIdentifier ("++"));
			Assert.AreEqual ("\u3042", gen.CreateValidIdentifier ("\u3042"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCreateEscapedIdentifierNullArg ()
		{
			gen.CreateEscapedIdentifier (null);
		}

		[Test]
		public void TestCreateEscapedIdentifier ()
		{
		
			Assert.AreEqual ("a", gen.CreateEscapedIdentifier ("a"));
			Assert.AreEqual ("@int", gen.CreateEscapedIdentifier ("int"));
			Assert.AreEqual ("_", gen.CreateEscapedIdentifier ("_"));
			Assert.AreEqual ("1", gen.CreateEscapedIdentifier ("1"));
			Assert.AreEqual ("1a", gen.CreateEscapedIdentifier ("1a"));
			Assert.AreEqual ("1*2", gen.CreateEscapedIdentifier ("1*2"));
			Assert.AreEqual ("-", gen.CreateEscapedIdentifier ("-"));
			Assert.AreEqual ("+", gen.CreateEscapedIdentifier ("+"));
			Assert.AreEqual ("", gen.CreateEscapedIdentifier (""));
			Assert.AreEqual ("--", gen.CreateEscapedIdentifier ("--"));
			Assert.AreEqual ("++", gen.CreateEscapedIdentifier ("++"));
			Assert.AreEqual ("\u3042", gen.CreateEscapedIdentifier ("\u3042"));
		}

		[Test]
		public void TestIsValidIdentifier ()
		{
			Assert.AreEqual (true, gen.IsValidIdentifier ("_a"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("_"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("@return"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("d1"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("_1"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("_a_1"));
			Assert.AreEqual (true, gen.IsValidIdentifier ("@a"));
			Assert.AreEqual (false, gen.IsValidIdentifier ("1"));
			Assert.AreEqual (false, gen.IsValidIdentifier (" "));
			Assert.AreEqual (false, gen.IsValidIdentifier ("?"));
			Assert.AreEqual (false, gen.IsValidIdentifier (":_:"));
			Assert.AreEqual (false, gen.IsValidIdentifier ("_ "));
			Assert.AreEqual (false, gen.IsValidIdentifier ("@ "));
			Assert.AreEqual (false, gen.IsValidIdentifier ("1*2"));
			Assert.AreEqual (false, gen.IsValidIdentifier ("1_2"));
			Assert.AreEqual (gen.IsValidIdentifier ("a, b"), false);
		}
	}
}
