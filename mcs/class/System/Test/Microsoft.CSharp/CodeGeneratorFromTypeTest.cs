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
	///	Test ICodeGenerator's GenerateCodeFromType, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromTypeTest: CodeGeneratorTestBase
	{
		CodeTypeDeclaration type = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			type = new CodeTypeDeclaration ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromType (type, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultTypeTest ()
		{
			Generate ();
			Assertion.AssertEquals ("public class  {\n}\n", Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullTypeTest ()
		{
			type = null;
			Generate ();
		}

		[Test]
		public void SimpleTypeTest ()
		{
			type.Name = "Test1";
			Generate ();
			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributesAndTypeTest ()
		{
			type.Name = "Test1";

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			type.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			type.CustomAttributes.Add (attrDec);

			Generate ();
			Assertion.AssertEquals (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}[B()]{0}public class Test1 {{{0}}}{0}", writer.NewLine), Code);
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
