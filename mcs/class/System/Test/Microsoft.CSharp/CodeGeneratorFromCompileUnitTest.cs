//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Eric Lindvall (eric@5stops.com)
//
// (c) 2003 Eric Lindvall
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
		[Ignore ("This only differs in 4 spaces")]
		public void SimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			Generate ();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		[Ignore ("This only differs in 4 spaces")]
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
			Assertion.AssertEquals ("[assembly: A()]\n\n", Code);
		}

		/* FIXME
		[Test]
		public void AttributeWithValueTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals ("[assembly: A()]\n\n", Code);
		}*/

	}
}
