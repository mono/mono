//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromNamespace, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
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
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromNamespace (codeNamespace, writer, options);
			writer.Close ();
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
			Generate ();
			Assertion.AssertEquals ("\n", Code);
		}

		
		[Test]
		public void DefaultNamespaceTest ()
		{
			Generate ();
			Assertion.AssertEquals ("\n", Code);
		}

		[Test]
		[Ignore ("This only differs in 4 spaces")]
		public void SimpleNamespaceTest ()
		{
			codeNamespace.Name = "A";
			Generate();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		[Ignore ("This only differs in 4 spaces")]
		public void InvalidNamespaceTest ()
		{
			codeNamespace.Name = "A,B";
			Generate();
			Assertion.AssertEquals ("namespace A,B {\n    \n}\n", Code);
		}


		[Test]
		public void CommentOnlyNamespaceTest ()
		{
			CodeCommentStatement comment = new CodeCommentStatement ("a");
			codeNamespace.Comments.Add (comment);
			Generate ();
			Assertion.AssertEquals ("// a\n\n", Code);
		}
	}

	// FIXME implement tests for these methods:
	// GenerateCodeFromType
	// GenerateCodeFromExpression
	// GenerateCodeFromStatement

}

