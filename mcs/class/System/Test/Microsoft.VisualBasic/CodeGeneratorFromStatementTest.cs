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
		public void CodeAssignStatementTest ()
		{
			CodeSnippetExpression cse1 = new CodeSnippetExpression ("A");
			CodeSnippetExpression cse2 = new CodeSnippetExpression ("B");

			CodeAssignStatement assignStatement = new CodeAssignStatement (cse1, cse2);
			statement = assignStatement;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"A = B{0}", NewLine), Generate (), "#1");

			assignStatement.Left = null;
			try {
				Generate ();
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			assignStatement.Left = cse1;
			Generate ();

			assignStatement.Right = null;
			try {
				Generate ();
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}

			assignStatement.Right = cse2;
			Generate ();
		}

		[Test]
		public void CodeAttachEventStatementTest ()
		{
			CodeEventReferenceExpression cere = new CodeEventReferenceExpression (
				new CodeSnippetExpression ("A"), "class");
			CodeSnippetExpression handler = new CodeSnippetExpression ("EventHandler");

			CodeAttachEventStatement attachEventStatement = new CodeAttachEventStatement ();
			statement = attachEventStatement;

			try {
				Generate ();
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			attachEventStatement.Event = cere;
			try {
				Generate ();
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			attachEventStatement.Event = null;
			attachEventStatement.Listener = handler;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler , EventHandler{0}", NewLine), Generate (), "#3");

			attachEventStatement.Event = cere;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler A.[class], EventHandler{0}", NewLine), Generate (), "#4");

			attachEventStatement.Event = new CodeEventReferenceExpression (
				new CodeSnippetExpression ((string) null), "");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler ., EventHandler{0}", NewLine), Generate (), "#5");

			attachEventStatement.Listener = new CodeSnippetExpression ("");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler ., {0}", NewLine), Generate (), "#6");
		}
		
		[Test]
		public void CodeAttachEventStatementKeywordTest ()
		{
			CodeEventReferenceExpression cere = new CodeEventReferenceExpression (
				new CodeSnippetExpression ("Set"), "Event");
			CodeSnippetExpression handler = new CodeSnippetExpression ("Handles");

			CodeAttachEventStatement attachEventStatement = new CodeAttachEventStatement ();
			statement = attachEventStatement;

			try {
				Generate ();
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			attachEventStatement.Event = cere;
			try {
				Generate ();
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			attachEventStatement.Event = null;
			attachEventStatement.Listener = handler;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler , Handles{0}", NewLine), Generate (), "#3");

			attachEventStatement.Event = cere;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler Set.[Event], Handles{0}", NewLine), Generate (), "#4");

			attachEventStatement.Event = new CodeEventReferenceExpression (
				new CodeSnippetExpression ((string) null), "");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler ., Handles{0}", NewLine), Generate (), "#5");

			attachEventStatement.Listener = new CodeSnippetExpression ("");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"AddHandler ., {0}", NewLine), Generate (), "#6");
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
		public void CodeConditionStatementTest ()
		{
			CodeStatement[] trueStatements = new CodeStatement[] {
				new CodeExpressionStatement (new CodeSnippetExpression ("DoA()")),
				new CodeExpressionStatement (new CodeSnippetExpression (";")),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoB()")),
				new CodeExpressionStatement (new CodeSnippetExpression ("")),
				new CodeSnippetStatement ("A"),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoC()")) };

			CodeStatement[] falseStatements = new CodeStatement[] {
				new CodeExpressionStatement (new CodeSnippetExpression ("DoD()")),
				new CodeSnippetStatement ("B"),
				new CodeExpressionStatement (new CodeSnippetExpression (";")),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoE()")),
				new CodeExpressionStatement (new CodeSnippetExpression ("")),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoF()")) };

			CodeConditionStatement conditionStatement = new CodeConditionStatement ();
			statement = conditionStatement;

			try {
				Generate ();
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			conditionStatement.Condition = new CodeSnippetExpression ("");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If  Then{0}" +
				"End If{0}", NewLine), Generate (), "#2");

			conditionStatement.Condition = new CodeSnippetExpression ("true == false");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"End If{0}", NewLine), Generate (), "#3");

			conditionStatement.TrueStatements.AddRange (trueStatements);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"    DoA(){0}" +
				"    ;{0}" +
				"    DoB(){0}" +
				"    {0}" +
#if NET_2_0
				"A{0}" +
#else
				"    A{0}" +
#endif
				"    DoC(){0}" +
				"End If{0}", NewLine), Generate (), "#3");

			conditionStatement.FalseStatements.AddRange (falseStatements);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"    DoA(){0}" +
				"    ;{0}" +
				"    DoB(){0}" +
				"    {0}" +
#if NET_2_0
				"A{0}" +
#else
				"    A{0}" +
#endif
				"    DoC(){0}" +
				"Else{0}" +
				"    DoD(){0}" +
#if NET_2_0
				"B{0}" +
#else
				"    B{0}" +
#endif
				"    ;{0}" +
				"    DoE(){0}" +
				"    {0}" +
				"    DoF(){0}" +
				"End If{0}", NewLine), Generate (), "#4");

			options.ElseOnClosing = true;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"    DoA(){0}" +
				"    ;{0}" +
				"    DoB(){0}" +
				"    {0}" +
#if NET_2_0
				"A{0}" +
#else
				"    A{0}" +
#endif
				"    DoC(){0}" +
				"Else{0}" +
				"    DoD(){0}" +
#if NET_2_0
				"B{0}" +
#else
				"    B{0}" +
#endif
				"    ;{0}" +
				"    DoE(){0}" +
				"    {0}" +
				"    DoF(){0}" +
				"End If{0}", NewLine), Generate (), "#5");

			options.ElseOnClosing = false;

			conditionStatement.TrueStatements.Clear ();

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"Else{0}" +
				"    DoD(){0}" +
#if NET_2_0
				"B{0}" +
#else
				"    B{0}" +
#endif
				"    ;{0}" +
				"    DoE(){0}" +
				"    {0}" +
				"    DoF(){0}" +
				"End If{0}", NewLine), Generate (), "#6");

			conditionStatement.TrueStatements.AddRange (trueStatements);
			conditionStatement.FalseStatements.Clear ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"If true == false Then{0}" +
				"    DoA(){0}" +
				"    ;{0}" +
				"    DoB(){0}" +
				"    {0}" +
#if NET_2_0
				"A{0}" +
#else
				"    A{0}" +
#endif
				"    DoC(){0}" +
				"End If{0}", NewLine), Generate (), "#7");
		}

		[Test]
		public void CodeExpressionStatementTest ()
		{
			CodeExpressionStatement ces = new CodeExpressionStatement ();
			statement = ces;

			try {
				Generate ();
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			ces.Expression = new CodeSnippetExpression ("something");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"something{0}", NewLine), Generate (), "#2");
		}

		[Test]
		public void CodeGotoStatementTest ()
		{
			statement = new CodeGotoStatement ("something");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"goto something{0}", NewLine), Generate ());
		}

		[Test]
		public void CodeIterationStatementTest ()
		{
			CodeIterationStatement cis = new CodeIterationStatement ();
			statement = cis;

			try {
				Generate ();
				Assert.Fail ("#1: null InitStatement should cause NRE");
			} catch (NullReferenceException) {
			}

			cis.InitStatement = new CodeVariableDeclarationStatement (typeof (int),
				"testInt", new CodePrimitiveExpression (1));
			try {
				Generate ();
				Assert.Fail ("#2: null TestExpression should cause ArgumentNullException");
			} catch (ArgumentNullException) {
			}

			cis.TestExpression = new CodeBinaryOperatorExpression (
				new CodeVariableReferenceExpression ("testInt"),
				CodeBinaryOperatorType.LessThan,
				new CodePrimitiveExpression (10));
			try {
				Generate ();
				Assert.Fail ("#3: null IncrementStatement should cause NRE");
			} catch (NullReferenceException) {
			}

			cis.IncrementStatement = new CodeAssignStatement (
				new CodeVariableReferenceExpression ("testInt"),
				new CodeBinaryOperatorExpression (
					new CodeVariableReferenceExpression ("testInt"),
					CodeBinaryOperatorType.Add,
					new CodePrimitiveExpression (1)));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim testInt As Integer = 1{0}" +
				"Do While (testInt < 10){0}" +
				"    testInt = (testInt + 1){0}" +
				"Loop{0}", NewLine), Generate (), "#4");

			cis.Statements.AddRange (new CodeStatement[] {
				new CodeExpressionStatement (new CodeSnippetExpression ("DoA()")),
				new CodeExpressionStatement (new CodeSnippetExpression (";")),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoB()")),
				new CodeLabeledStatement ("test", new CodeSnippetStatement ("C")),
				new CodeExpressionStatement (new CodeSnippetExpression ("")),
				new CodeSnippetStatement ("A"),
				new CodeExpressionStatement (new CodeSnippetExpression ("DoC()")) });
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim testInt As Integer = 1{0}" +
				"Do While (testInt < 10){0}" +
				"    DoA(){0}" +
				"    ;{0}" +
				"    DoB(){0}" +
				"test:{0}" +
#if NET_2_0
				"C{0}" +
#else
				"    C{0}" +
#endif
				"    {0}" +
#if NET_2_0
				"A{0}" +
#else
				"    A{0}" +
#endif
				"    DoC(){0}" +
				"    testInt = (testInt + 1){0}" +
				"Loop{0}", NewLine), Generate (), "#5");
		}

		[Test]
		public void CodeLabeledStatementTest ()
		{
			CodeLabeledStatement cls = new CodeLabeledStatement ();
			statement = cls;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				":{0}", NewLine), Generate (), "#1");

			cls.Label = "class";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"class:{0}", NewLine), Generate (), "#2");

			cls.Statement = new CodeSnippetStatement ("A");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"class:{0}" +
#if NET_2_0
				"A{0}",
#else
				"    A{0}",
#endif
				NewLine), Generate (), "#3");
		}

		[Test]
		public void CodeMethodReturnStatementTest ()
		{
			CodeMethodReturnStatement cmrs = new CodeMethodReturnStatement ();
			statement = cmrs;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Return{0}", NewLine), Generate (), "#1");

			cmrs.Expression = new CodePrimitiveExpression (1);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Return 1{0}", NewLine), Generate (), "#2");
		}

		[Test]
		public void CodeRemoveEventStatementTest ()
		{
			CodeEventReferenceExpression cere = new CodeEventReferenceExpression (
				new CodeSnippetExpression ("A"), "class");
			CodeSnippetExpression handler = new CodeSnippetExpression ("EventHandler");

			CodeRemoveEventStatement cres = new CodeRemoveEventStatement ();
			statement = cres;

			try {
				Generate ();
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			cres.Event = cere;
			try {
				Generate ();
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			cres.Event = null;
			cres.Listener = handler;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"RemoveHandler , EventHandler{0}", NewLine), Generate (), "#3");

			cres.Event = cere;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"RemoveHandler A.[class], EventHandler{0}", NewLine), Generate (), "#4");

			cres.Event = new CodeEventReferenceExpression (
				new CodeSnippetExpression ((string) null), "");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"RemoveHandler ., EventHandler{0}", NewLine), Generate (), "#5");

			cres.Listener = new CodeSnippetExpression ("");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"RemoveHandler ., {0}", NewLine), Generate (), "#6");
		}

		[Test]
		public void CodeSnippetStatementTest ()
		{
			CodeSnippetStatement css = new CodeSnippetStatement ();
			statement = css;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}", NewLine), Generate (), "#1");

			css.Value = "class";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"class{0}", NewLine), Generate (), "#2");

			css.Value = null;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}", NewLine), Generate (), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CodeStatement ()
		{
			CodeStatement cs = new CodeStatement ();
			statement = cs;

			Generate ();
		}

		[Test]
		public void CodeThrowExceptionStatementTest ()
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
		public void CodeTryCatchFinallyStatementTest ()
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
		public void CodeVariableDeclarationStatementTest ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ();
			statement = cvds;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"Dim __exception As System.Void{0}", 
#else
				"Dim  As System.Void{0}", 
#endif
				NewLine), Generate (), "#1");

			cvds.Name = "class";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim [class] As System.Void{0}", NewLine), Generate (), "#2");

			cvds.Name = "A";
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim A As System.Void{0}", NewLine), Generate (), "#3");

			cvds.Type = new CodeTypeReference (typeof (int));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim A As Integer{0}", NewLine), Generate (), "#4");

			cvds.InitExpression = new CodePrimitiveExpression (25);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Dim A As Integer = 25{0}", NewLine), Generate (), "#5");

			cvds.Name = null;
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"Dim __exception As Integer = 25{0}", 
#else
				"Dim  As Integer = 25{0}", 
#endif
				NewLine), Generate (), "#6");
		}
	}
}
