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
	///	Test ICodeGenerator's GenerateCodeFromStatement, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromStatementTest: CodeGeneratorTestBase
	{
		CodeStatement statement = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			statement = new CodeStatement ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromStatement (statement, writer, options);
			writer.Close ();
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefaultStatementTest ()
		{
			Generate ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullStatementTest ()
		{
			statement = null;
			Generate ();
		}

		[Test]
		public void DefaultCodeCommentStatementTest ()
		{
			CodeCommentStatement commentStatement = new CodeCommentStatement ();
			CodeComment comment = new CodeComment ();
			
			commentStatement.Comment = comment;
			statement = commentStatement;
			
			Generate ();
			Assertion.AssertEquals ("// \n", Code);
		}

		[Test]
		public void MultiLineCodeCommentStatementTest ()
		{
			CodeCommentStatement commentStatement = new CodeCommentStatement ();
			CodeComment comment = new CodeComment ();
			
			comment.Text = "a\nb";
			commentStatement.Comment = comment;
			statement = commentStatement;
			
			Generate ();
			Assertion.AssertEquals ("// a\n//b\n", Code);
		}

		[Test]
		public void DefaultThrowExceptionStatementTest ()
		{
			CodeThrowExceptionStatement throwStatement = new CodeThrowExceptionStatement ();

			statement = throwStatement;

			Generate ();
			Assertion.AssertEquals ("throw;\n", Code);
		}
		
		/*
		[Test]
		public void ThrowExceptionStatementTest ()
		{
			CodeThrowExceptionStatement throwStatement = new CodeThrowExceptionStatement ();
			throwStatement.ToThrow = ... expression
			statement = throwStatement ();

			Generate();
			Assertion.AssertEquals ("", Code);
		}
		*/
				
		/*
		System.Object
		   System.CodeDom.CodeObject
		      System.CodeDom.CodeStatement
			 System.CodeDom.CodeAssignStatement
			 System.CodeDom.CodeAttachEventStatement
			 - System.CodeDom.CodeCommentStatement
			 System.CodeDom.CodeConditionStatement
			 System.CodeDom.CodeExpressionStatement
			 System.CodeDom.CodeGotoStatement
			 System.CodeDom.CodeIterationStatement
			 System.CodeDom.CodeLabeledStatement
			 System.CodeDom.CodeMethodReturnStatement
			 System.CodeDom.CodeRemoveEventStatement
			 System.CodeDom.CodeSnippetStatement
			 System.CodeDom.CodeThrowExceptionStatement
			 System.CodeDom.CodeTryCatchFinallyStatement
			 System.CodeDom.CodeVariableDeclarationStatement

		*/

		
		/*
		[Test]
		public void ReferencedTest ()
		{
			codeUnit.ReferencedAssemblies.Add ("System.dll");
			Generate ();
			Assertion.AssertEquals ("", Code);
		}
		*/
	}
}

