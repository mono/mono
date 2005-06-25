//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.Globalization;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromCompileUnit, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromCompileUnitTest : CodeGeneratorTestBase
	{
		string codeUnitHeader = "";
		CodeCompileUnit codeUnit = null;

		public CodeGeneratorFromCompileUnitTest ()
		{
			Init();
			Generate();
			codeUnitHeader = Code;
		}
		
		[SetUp]
		public void Init ()
		{
			InitBase ();
			codeUnit = new CodeCompileUnit ();
		}
		
		protected override string Code {
			get { return base.Code.Substring (codeUnitHeader.Length); }
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultCodeUnitTest ()
		{
			Generate ();
			Assertion.AssertEquals ("", Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullCodeUnitTest ()
		{
			codeUnit = null;
			Generate();
		}

		[Test]
		public void ReferencedTest ()
		{
			codeUnit.ReferencedAssemblies.Add ("System.dll");
			Generate();
			Assertion.AssertEquals ("", Code);
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			Generate ();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		public void ReferenceAndSimpleNamespaceTest()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			codeUnit.ReferencedAssemblies.Add ("using System;");
			Generate ();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		public void SimpleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributeWithValueTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A(A1=false, A2=true)]{0}{0}", writer.NewLine), Code);
		}

		[Test]
		public void MultipleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			codeUnit.AssemblyCustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}[assembly: B()]{0}{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributeAndSimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			codeUnit.AssemblyCustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			codeUnit.AssemblyCustomAttributes.Add (attrDec);

			Generate ();

			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}[assembly: B()]{0}{0}namespace A {{{0}    {0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void CodeSnippetTest ()
		{
			codeUnit = new CodeSnippetCompileUnit ("public class Test1 {}");
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assertion.AssertEquals ("public class Test1 {}" + writer.NewLine, writer.ToString ());
		}
	}
}
