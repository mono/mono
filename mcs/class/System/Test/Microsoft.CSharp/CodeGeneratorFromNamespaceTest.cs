//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromNamespace, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromNamespaceTest : CodeGeneratorTestBase
	{
		CodeNamespace codeNamespace = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			codeNamespace = new CodeNamespace ();
		}
		
		protected override string Generate (CodeGeneratorOptions options)
		{
			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			generator.GenerateCodeFromNamespace (codeNamespace, writer, options);
			writer.Close ();
			return writer.ToString ();
		}
		
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullNamespaceTest ()
		{
			codeNamespace = null;
			Generate ();
		}

		[Test]
		public void NullNamespaceNameTest ()
		{
			codeNamespace.Name = null;
			Assert.AreEqual ("\n", Generate ());
		}

		
		[Test]
		public void DefaultNamespaceTest ()
		{
			Assert.AreEqual ("\n", Generate ());
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			string code = null;

			codeNamespace.Name = "A";
			code = Generate ();
			Assert.AreEqual ("namespace A {\n    \n}\n", code, "#1");

			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";
			code = Generate (options);
			Assert.AreEqual ("namespace A\n{\n    \n}\n", code, "#2");
		}

		[Test]
		public void InvalidNamespaceTest ()
		{
			codeNamespace.Name = "A,B";
			Assert.AreEqual ("namespace A,B {\n    \n}\n", Generate ());
		}

		[Test]
		public void CommentOnlyNamespaceTest ()
		{
			CodeCommentStatement comment = new CodeCommentStatement ("a");
			codeNamespace.Comments.Add (comment);
			Assert.AreEqual ("// a\n\n", Generate ());
		}
	}
}
