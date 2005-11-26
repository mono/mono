//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Gert Driesen
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromStatement, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromStatementTest: CodeGeneratorTestBase
	{
		private CodeStatement statement = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			statement = new CodeStatement ();
		}
		
		protected override string Generate (CodeGeneratorOptions options)
		{
			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			generator.GenerateCodeFromStatement (statement, writer, options);
			writer.Close ();
			return writer.ToString ();
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
		public void CodeCommentStatementTest ()
		{
			CodeCommentStatement commentStatement = new CodeCommentStatement ();
			CodeComment comment = new CodeComment ();
			commentStatement.Comment = comment;
			statement = commentStatement;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"'{0}", NewLine), Generate (), "#1");

			comment.Text = "a\nb";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"'a\n'b{0}", NewLine), Generate (), "#2");

			comment.Text = "a\r\nb";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"'a\r\n'b{0}", NewLine), Generate (), "#3");

			comment.Text = "a\rb";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"'a\r'b{0}", NewLine), Generate (), "#4");
		}

		[Test]
		public void ThrowExceptionStatementTest ()
		{
			CodeThrowExceptionStatement ctet = new CodeThrowExceptionStatement ();
			statement = ctet;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Throw{0}", NewLine), Generate (), "#1");

			ctet.ToThrow = new CodeSnippetExpression ("whatever");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Throw whatever{0}", NewLine), Generate (), "#2");
		}

		[Test]
		public void GotoStatementTest ()
		{
			statement = new CodeGotoStatement ("something");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"goto something{0}", NewLine), Generate ());
		}

		[Test]
		public void TryCatchFinallyStatementTest ()
		{
			CodeStatement cs = new CodeGotoStatement ("exit");
			CodeCatchClause ccc1 = new CodeCatchClause ("ex1", new CodeTypeReference ("System.ArgumentException"));
			CodeCatchClause ccc2 = new CodeCatchClause (null, new CodeTypeReference ("System.ApplicationException"));
			CodeSnippetStatement fin1 = new CodeSnippetStatement ("A");
			CodeSnippetStatement fin2 = new CodeSnippetStatement ("B");

			statement = new CodeTryCatchFinallyStatement (new CodeStatement[] { cs },
				new CodeCatchClause[] { ccc1, ccc2 }, new CodeStatement[] { fin1, fin2 });

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Try {0}" +
				"    goto exit{0}" +
				"Catch ex1 As System.ArgumentException{0}" + 
#if NET_2_0
				"Catch __exception As System.ApplicationException{0}" +
#else
				"Catch  As System.ApplicationException{0}" +
#endif
				"Finally{0}" +
#if NET_2_0
				"A{0}" +
				"B{0}" +
#else
				"    A{0}" +
				"    B{0}" +
#endif
				"End Try{0}", NewLine), Generate (), "#1");

			options.ElseOnClosing = true;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Try {0}" +
				"    goto exit{0}" +
				"Catch ex1 As System.ArgumentException{0}" +
#if NET_2_0
				"Catch __exception As System.ApplicationException{0}" +
#else
				"Catch  As System.ApplicationException{0}" +
#endif
				"Finally{0}" +
#if NET_2_0
				"A{0}" +
				"B{0}" +
#else
				"    A{0}" +
				"    B{0}" +
#endif
				"End Try{0}", NewLine), Generate (), "#2");

			statement = new CodeTryCatchFinallyStatement ();

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Try {0}" +
				"End Try{0}", NewLine), Generate (), "#3");

			options.ElseOnClosing = false;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Try {0}" +
				"End Try{0}", NewLine), Generate (), "#4");
		}

		[Test]
		public void VariableDeclarationStatementTest ()
		{
			statement = new CodeVariableDeclarationStatement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"Dim __exception As System.Void{0}",
#else
				"Dim  As System.Void{0}",
#endif
				NewLine), Generate (), "#1");

			statement = new CodeVariableDeclarationStatement ((string) null,
				(string) null);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"Dim __exception As System.Void{0}",
#else
				"Dim  As System.Void{0}",
#endif
				NewLine), Generate (), "#1");


			statement = new CodeVariableDeclarationStatement ("A", (string) null);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"Dim __exception As A{0}",
#else
				"Dim  As A{0}",
#endif
				NewLine), Generate (), "#1");

			statement = new CodeVariableDeclarationStatement ((string) null, "B");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim B As System.Void{0}", NewLine), Generate (), "#4");

			statement = new CodeVariableDeclarationStatement ("A", "B");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim B As A{0}", NewLine), Generate (), "#5");

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ("A", "B");
			cvds.InitExpression = new CodeSnippetExpression ("C");
			statement = cvds;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim B As A = C{0}", NewLine), Generate (), "#6");
		}
	}
}
