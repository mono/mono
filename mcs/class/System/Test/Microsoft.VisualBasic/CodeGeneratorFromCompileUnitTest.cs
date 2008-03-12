//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	[TestFixture]
	public class CodeGeneratorFromCompileUnitTest : CodeGeneratorTestBase
	{
		string codeUnitHeader = "";
		CodeCompileUnit codeUnit = null;

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
			Assert.AreEqual ("", Generate ());
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
			Assert.AreEqual ("", Generate ());
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}Namespace A{0}End Namespace{0}", NewLine), Generate ());
		}

		[Test]
		public void ReferenceAndSimpleNamespaceTest()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			codeUnit.ReferencedAssemblies.Add ("using System;");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}Namespace A{0}End Namespace{0}", NewLine), Generate ());
		}

		[Test]
		public void SimpleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<Assembly: A()> {0}", NewLine), Generate ());
		}

		[Test]
		public void AttributeWithValueTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			attrDec.Arguments.Add (new CodeAttributeArgument("A1", 
				new CodePrimitiveExpression(false)));
			attrDec.Arguments.Add (new CodeAttributeArgument("A2", 
				new CodePrimitiveExpression(true)));
			// null name should not be output
			attrDec.Arguments.Add (new CodeAttributeArgument (null,
				new CodePrimitiveExpression (true)));
			// zero length name should not be output
			attrDec.Arguments.Add (new CodeAttributeArgument (string.Empty,
				new CodePrimitiveExpression (false)));

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Assert.AreEqual ("<Assembly: A(A1:=false, A2:=true, true, false)> " +
				NewLine, Generate ());
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
				"<Assembly: A(),  _{0} Assembly: B()> {0}", NewLine),
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
				"<Assembly: A(),  _{0} Assembly: B()> {0}{0}Namespace A{0}End "
				+ "Namespace{0}", NewLine), Generate ());
		}

		[Test]
		public void CodeSnippetTest ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append ("Public Class Test1");
			sb.Append (Environment.NewLine);
			sb.Append ("End Class");

			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			codeUnit = new CodeSnippetCompileUnit (sb.ToString ());
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assert.AreEqual (sb.ToString () + NewLine, writer.ToString());
		}
		
		[Test]
		public void ExternalSourceTest ()
		{
			CodeSnippetCompileUnit snippet;
			
			StringBuilder sb = new StringBuilder();
			sb.Append ("\n");
			sb.Append ("#ExternalSource(\"file.vb\",123)");
			sb.Append ("\n");
			sb.Append ("\n");
			sb.Append ("\n");
			sb.Append ("#End ExternalSource");
			sb.Append ("\n");

			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			codeUnit = new CodeSnippetCompileUnit ("");
			snippet = (CodeSnippetCompileUnit) codeUnit;
			snippet.LinePragma = new CodeLinePragma ("file.vb", 123);
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assert.AreEqual (sb.ToString (), writer.ToString());
		}
	}
}
