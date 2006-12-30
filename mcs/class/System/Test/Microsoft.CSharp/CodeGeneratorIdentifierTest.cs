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
	public class CodeGeneratorIdentifierTest : Assertion
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
		
			AssertEquals ("a", gen.CreateValidIdentifier ("a"));
			AssertEquals ("_int", gen.CreateValidIdentifier ("int"));
			AssertEquals ("_", gen.CreateValidIdentifier ("_"));
			AssertEquals ("1", gen.CreateValidIdentifier ("1"));
			AssertEquals ("1a", gen.CreateValidIdentifier ("1a"));
			AssertEquals ("1*2", gen.CreateValidIdentifier ("1*2"));
			AssertEquals ("-", gen.CreateValidIdentifier ("-"));
			AssertEquals ("+", gen.CreateValidIdentifier ("+"));
			AssertEquals ("", gen.CreateValidIdentifier (""));
			AssertEquals ("--", gen.CreateValidIdentifier ("--"));
			AssertEquals ("++", gen.CreateValidIdentifier ("++"));
			AssertEquals ("\u3042", gen.CreateValidIdentifier ("\u3042"));
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestCreateEscapedIdentifierNullArg ()
		{
			gen.CreateEscapedIdentifier (null);
		}

		[Test]
		public void TestCreateEscapedIdentifier ()
		{
		
			AssertEquals ("a", gen.CreateEscapedIdentifier ("a"));
			AssertEquals ("@int", gen.CreateEscapedIdentifier ("int"));
			AssertEquals ("_", gen.CreateEscapedIdentifier ("_"));
			AssertEquals ("1", gen.CreateEscapedIdentifier ("1"));
			AssertEquals ("1a", gen.CreateEscapedIdentifier ("1a"));
			AssertEquals ("1*2", gen.CreateEscapedIdentifier ("1*2"));
			AssertEquals ("-", gen.CreateEscapedIdentifier ("-"));
			AssertEquals ("+", gen.CreateEscapedIdentifier ("+"));
			AssertEquals ("", gen.CreateEscapedIdentifier (""));
			AssertEquals ("--", gen.CreateEscapedIdentifier ("--"));
			AssertEquals ("++", gen.CreateEscapedIdentifier ("++"));
			AssertEquals ("\u3042", gen.CreateEscapedIdentifier ("\u3042"));
		}

		[Test]
		public void TestIsValidIdentifier ()
		{
			AssertEquals (true, gen.IsValidIdentifier ("_a"));
			AssertEquals (true, gen.IsValidIdentifier ("_"));
			AssertEquals (true, gen.IsValidIdentifier ("@return"));
			AssertEquals (true, gen.IsValidIdentifier ("d1"));
			AssertEquals (true, gen.IsValidIdentifier ("_1"));
			AssertEquals (true, gen.IsValidIdentifier ("_a_1"));
			AssertEquals (true, gen.IsValidIdentifier ("@a"));
			AssertEquals (false, gen.IsValidIdentifier ("1"));
			AssertEquals (false, gen.IsValidIdentifier (" "));
			AssertEquals (false, gen.IsValidIdentifier ("?"));
			AssertEquals (false, gen.IsValidIdentifier (":_:"));
			AssertEquals (false, gen.IsValidIdentifier ("_ "));
			AssertEquals (false, gen.IsValidIdentifier ("@ "));
			AssertEquals (false, gen.IsValidIdentifier ("1*2"));
			AssertEquals (false, gen.IsValidIdentifier ("1_2"));
			AssertEquals (false, gen.IsValidIdentifier ("a,b"));
		}
	}
}
