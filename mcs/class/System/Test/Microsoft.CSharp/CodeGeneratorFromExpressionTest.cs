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
	///	Test ICodeGenerator's GenerateCodeFromExpression, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromExpressionTest: CodeGeneratorTestBase
	{
		CodeExpression expression = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			expression = new CodeExpression ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromExpression (expression, writer, options);
			writer.Close ();
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefaultExpressionTest ()
		{
			Generate ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullExpressionTest ()
		{
			expression = null;
			Generate ();
		}

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
