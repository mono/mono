//
// TypedDataSetGeneratorTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !MOBILE

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data;
using NUnit.Framework;
using Microsoft.CSharp;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class TypedDataSetGeneratorTest : Assertion
	{
		private ICodeGenerator gen;
		private ICodeCompiler compiler;

		public TypedDataSetGeneratorTest ()
		{
			CodeDomProvider p = new CSharpCodeProvider ();
			gen = p.CreateGenerator ();
			compiler = p.CreateCompiler ();
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
		
			AssertEquals ("#1", "a", TypedDataSetGenerator.GenerateIdName ("a", gen));
			AssertEquals ("#2", "_int", TypedDataSetGenerator.GenerateIdName ("int", gen));
			AssertEquals ("#3", "_", TypedDataSetGenerator.GenerateIdName ("_", gen));
			AssertEquals ("#3-2", "_", TypedDataSetGenerator.GenerateIdName ("_", gen));
			AssertEquals ("#4", "_1", TypedDataSetGenerator.GenerateIdName ("1", gen));
			AssertEquals ("#4-2", "_1", TypedDataSetGenerator.GenerateIdName ("1", gen));
			AssertEquals ("#5", "_1a", TypedDataSetGenerator.GenerateIdName ("1a", gen));
			AssertEquals ("#6", "_1_2", TypedDataSetGenerator.GenerateIdName ("1*2", gen));
			AssertEquals ("#7", "__", TypedDataSetGenerator.GenerateIdName ("-", gen));
			AssertEquals ("#8", "__", TypedDataSetGenerator.GenerateIdName ("+", gen));
			AssertEquals ("#9", "_", TypedDataSetGenerator.GenerateIdName ("", gen));
			AssertEquals ("#10", "___", TypedDataSetGenerator.GenerateIdName ("--", gen));
			AssertEquals ("#11", "___", TypedDataSetGenerator.GenerateIdName ("++", gen));
			AssertEquals ("#12", "\u3042", TypedDataSetGenerator.GenerateIdName ("\u3042", gen));
		}

		[Test]
		[Ignore ("We cannot depend on CodeCompiler since it expects mcs to exist.")]
		public void RelationConnectsSameTable ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/schemas/bug77248.xsd");
			CodeNamespace cns = new CodeNamespace ();
			TypedDataSetGenerator.Generate (ds, cns, gen);
			CodeCompileUnit ccu = new CodeCompileUnit ();
			ccu.Namespaces.Add (cns);
			CompilerResults r = compiler.CompileAssemblyFromDom (
				new CompilerParameters (), ccu);
			AssertEquals (0, r.Errors.Count);
		}
	}
}

#endif