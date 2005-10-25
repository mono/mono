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
			Assert.AreEqual ("", Code);
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
			Assert.AreEqual ("", Code);
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}Namespace A{0}End Namespace{0}", writer.NewLine), Code);
		}

		[Test]
		public void ReferenceAndSimpleNamespaceTest()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			codeUnit.ReferencedAssemblies.Add ("using System;");
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}Namespace A{0}End Namespace{0}", writer.NewLine), Code);
		}

		[Test]
		public void SimpleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<Assembly: A()> {0}", writer.NewLine), Code);
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
			Generate ();
			Assert.AreEqual ("<Assembly: A(A1:=false, A2:=true, true, false)> " + 
				writer.NewLine, Code);
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
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture, 
				"<Assembly: A(),  _{0} Assembly: B()> {0}", writer.NewLine),
				Code);
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

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<Assembly: A(),  _{0} Assembly: B()> {0}{0}Namespace A{0}End "
				+ "Namespace{0}",writer.NewLine), Code);
		}

		[Test]
		public void CodeSnippetTest ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append ("Public Class Test1");
			sb.Append (Environment.NewLine);
			sb.Append ("End Class");

			codeUnit = new CodeSnippetCompileUnit (sb.ToString ());
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assert.AreEqual (sb.ToString () + writer.NewLine, 
						writer.ToString());
		}
	}
}
