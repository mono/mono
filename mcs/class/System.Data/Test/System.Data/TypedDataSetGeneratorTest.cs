//
// TypedDataSetGeneratorTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell inc.
//
using System;
using System.CodeDom.Compiler;
using System.Data;
using NUnit.Framework;
using Microsoft.CSharp;

namespace MonoTests.System.Data
{
	public class TypedDataSetGeneratorTest : Assertion
	{
		private ICodeGenerator gen;

		public TypedDataSetGeneratorTest ()
		{
			gen = new CSharpCodeProvider ().CreateGenerator ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestGenerateIdNameNullName ()
		{
			TypedDataSetGenerator.GenerateIdName (null, gen);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestGenerateIdNameNullProvider ()
		{
			TypedDataSetGenerator.GenerateIdName ("a", null);
		}

		[Test]
		public void TestGenerateIdName ()
		{
		
			AssertEquals ("a", TypedDataSetGenerator.GenerateIdName ("a", gen));
			AssertEquals ("_int", TypedDataSetGenerator.GenerateIdName ("int", gen));
			AssertEquals ("_", TypedDataSetGenerator.GenerateIdName ("_", gen));
			AssertEquals ("1", TypedDataSetGenerator.GenerateIdName ("1", gen));
			AssertEquals ("1a", TypedDataSetGenerator.GenerateIdName ("1a", gen));
			AssertEquals ("1*2", TypedDataSetGenerator.GenerateIdName ("1*2", gen));
			AssertEquals ("-", TypedDataSetGenerator.GenerateIdName ("-", gen));
			AssertEquals ("+", TypedDataSetGenerator.GenerateIdName ("+", gen));
			AssertEquals ("", TypedDataSetGenerator.GenerateIdName ("", gen));
			AssertEquals ("--", TypedDataSetGenerator.GenerateIdName ("--", gen));
			AssertEquals ("++", TypedDataSetGenerator.GenerateIdName ("++", gen));
			AssertEquals ("\u3042", TypedDataSetGenerator.GenerateIdName ("\u3042", gen));
		}

	}
}
