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
	}
}
