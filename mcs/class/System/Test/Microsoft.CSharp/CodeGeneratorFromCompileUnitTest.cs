//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
// 	Ilker Cetinkaya (mail@ilker.de)
//
// (c) 2003 Erik LeBel
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromCompileUnit, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromCompileUnitTest : CodeGeneratorTestBase
	{
		private string codeUnitHeader = string.Empty;
		private CodeCompileUnit codeUnit;

		public CodeGeneratorFromCompileUnitTest ()
		{
			Init();
			codeUnitHeader = Generate ();
		}
		
		[SetUp]
		public void Init ()
		{
			InitBase ();
			codeUnit = new CodeCompileUnit ();
		}
		
		protected override string Generate (CodeGeneratorOptions options)
		{
			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			return writer.ToString ().Substring (codeUnitHeader.Length);
		}
		
		[Test]
		public void DefaultCodeUnitTest ()
		{
			Assert.AreEqual (string.Empty, Generate ());
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullCodeUnitTest ()
		{
			codeUnit = null;
			Generate ();
		}

		[Test]
		public void ReferencedTest ()
		{
			codeUnit.ReferencedAssemblies.Add ("System.dll");
			Assert.AreEqual (string.Empty, Generate ());
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			string code = null;

			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			code = Generate ();
			Assert.AreEqual ("namespace A {\n    \n}\n", code, "#1");

			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";
			code = Generate (options);
			Assert.AreEqual ("namespace A\n{\n    \n}\n", code, "#2");
		}

		[Test]
		public void ReferenceAndSimpleNamespaceTest()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			codeUnit.ReferencedAssemblies.Add ("using System;");
			Assert.AreEqual ("namespace A {\n    \n}\n", Generate ());
		}

		[Test]
		public void SimpleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}{0}", NewLine), Generate ());
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
			// null name should not be output
			attrDec.Arguments.Add (new CodeAttributeArgument (null,
				new CodePrimitiveExpression (true)));
			// zero length name should not be output
			attrDec.Arguments.Add (new CodeAttributeArgument (string.Empty,
				new CodePrimitiveExpression (false)));

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A(A1=false, A2=true, true, false)]{0}{0}", NewLine), 
				Generate ());
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
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}[assembly: B()]{0}{0}", NewLine),
				Generate ());
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

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[assembly: A()]{0}[assembly: B()]{0}{0}namespace A {{{0}    {0}"
				+ "}}{0}", NewLine), Generate ());
		}

		[Test]
		public void CodeSnippetTest ()
		{
			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			codeUnit = new CodeSnippetCompileUnit ("public class Test1 {}");
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assert.AreEqual ("public class Test1 {}" + writer.NewLine, writer.ToString ());
		}

		[Test]
		public void AttributeAndGlobalNamespaceWithImportTest ()
		{
			var import = new CodeNamespaceImport ("Z");
			AddGlobalNamespaceWithImport (codeUnit, import);
			AddAssemblyAttribute (codeUnit, "A");

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"using Z;{0}{0}[assembly: A()]{0}{0}", NewLine), Generate ());
		}

		private static void AddGlobalNamespaceWithImport (CodeCompileUnit codeUnit, CodeNamespaceImport import) {
			CodeNamespace ns = new CodeNamespace ();
			ns.Imports.Add (import);
			codeUnit.Namespaces.Add (ns);
		}

		private static void AddAssemblyAttribute (CodeCompileUnit codeUnit, string attributeName) {
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = attributeName;
			codeUnit.AssemblyCustomAttributes.Add (attrDec);
		}
	}
}
