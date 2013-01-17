//
// CodeGenerator unit tests
//
// Authors:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.CodeDom.Compiler
{
	[TestFixture]
	public class MockCodeGenerator : CodeGenerator
	{
		[Test]
		public void IsCurrentTest ()
		{
			MockCodeGenerator codeGenerator = new MockCodeGenerator ();
			Assert.AreEqual (false, codeGenerator.IsCurrentClass, "#A1");
			Assert.AreEqual (false, codeGenerator.IsCurrentDelegate, "#A2");
			Assert.AreEqual (false, codeGenerator.IsCurrentEnum, "#A3");
			Assert.AreEqual (false, codeGenerator.IsCurrentInterface, "#A4");
			Assert.AreEqual (false, codeGenerator.IsCurrentStruct, "#A5");

			((ICodeGenerator) codeGenerator).GenerateCodeFromType (GetClassType (),
				new StringWriter (), new CodeGeneratorOptions ());
			Assert.AreEqual (true, codeGenerator.IsCurrentClass, "#B1");
			Assert.AreEqual (false, codeGenerator.IsCurrentDelegate, "#B2");
			Assert.AreEqual (false, codeGenerator.IsCurrentEnum, "#B3");
			Assert.AreEqual (false, codeGenerator.IsCurrentInterface, "#B4");
			Assert.AreEqual (false, codeGenerator.IsCurrentStruct, "#B5");

			((ICodeGenerator) codeGenerator).GenerateCodeFromType (GetDelegateType (),
				new StringWriter (), new CodeGeneratorOptions ());
			Assert.AreEqual (false, codeGenerator.IsCurrentClass, "#C1");
			Assert.AreEqual (true, codeGenerator.IsCurrentDelegate, "#C2");
			Assert.AreEqual (false, codeGenerator.IsCurrentEnum, "#C3");
			Assert.AreEqual (false, codeGenerator.IsCurrentInterface, "#C4");
			Assert.AreEqual (false, codeGenerator.IsCurrentStruct, "#C5");

			((ICodeGenerator) codeGenerator).GenerateCodeFromType (GetEnumType (),
				new StringWriter (), new CodeGeneratorOptions ());
			Assert.AreEqual (false, codeGenerator.IsCurrentClass, "#D1");
			Assert.AreEqual (false, codeGenerator.IsCurrentDelegate, "#D2");
			Assert.AreEqual (true, codeGenerator.IsCurrentEnum, "#D3");
			Assert.AreEqual (false, codeGenerator.IsCurrentInterface, "#D4");
			Assert.AreEqual (false, codeGenerator.IsCurrentStruct, "#D5");

			((ICodeGenerator) codeGenerator).GenerateCodeFromType (GetInterfaceType (),
				new StringWriter (), new CodeGeneratorOptions ());
			Assert.AreEqual (false, codeGenerator.IsCurrentClass, "#E1");
			Assert.AreEqual (false, codeGenerator.IsCurrentDelegate, "#E2");
			Assert.AreEqual (false, codeGenerator.IsCurrentEnum, "#E3");
			Assert.AreEqual (true, codeGenerator.IsCurrentInterface, "#E4");
			Assert.AreEqual (false, codeGenerator.IsCurrentStruct, "#E5");

			((ICodeGenerator) codeGenerator).GenerateCodeFromType (GetStructType (),
				new StringWriter (), new CodeGeneratorOptions ());
			Assert.AreEqual (false, codeGenerator.IsCurrentClass, "#F1");
			Assert.AreEqual (false, codeGenerator.IsCurrentDelegate, "#F2");
			Assert.AreEqual (false, codeGenerator.IsCurrentEnum, "#F3");
			Assert.AreEqual (false, codeGenerator.IsCurrentInterface, "#F4");
			Assert.AreEqual (true, codeGenerator.IsCurrentStruct, "#F5");
		}

		[Test]
		public void IsValidLanguageIndependentIdentifierTest ()
		{
			Assert.AreEqual (true,
				CodeGenerator.IsValidLanguageIndependentIdentifier("hello"), "#G1");
			Assert.AreEqual (true,
				CodeGenerator.IsValidLanguageIndependentIdentifier("good1"), "#G2");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier("2bad"), "#G3");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier("@start"), "#G4");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier("not@start"), "#G5");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier(" spaceatstart"), "#G6");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier("spaceatend "), "#G7");
			Assert.AreEqual (false,
				CodeGenerator.IsValidLanguageIndependentIdentifier("a spacein2ndplace"), "#G8");
		}

		[Test]
		public void CurrentMemberShouldBeClearedBetweenTypes ()
		{
			ICodeGenerator codeGenerator = new MockCodeGenerator ();
			var o = new StringWriter () {
				NewLine = "\n",
			};

			var t = new CodeTypeDeclaration() {
				Name = "Foo",
				Members = {
					new CodeMemberField() {
						Name = "f",
					},
					new CodeMemberMethod() {
						Name = "M",
						StartDirectives = {
							new CodeRegionDirective(CodeRegionMode.Start, "foo..."),
						},
						EndDirectives = {
							new CodeRegionDirective(CodeRegionMode.End, null),
						},
					}
				}
			};

			var opts = new CodeGeneratorOptions () {
				IndentString = "\t",
			};

			codeGenerator.GenerateCodeFromType (t, o, opts);
			t.Name = "Bar";
			codeGenerator.GenerateCodeFromType (t, o, opts);

			var expected = 
				"(type Foo\n" + 
				"\t\n" +
				"\t(field f (TypeRef System.Void))\n" +
				"\t\n" +
				"\t(region foo...\n" +
				"\t\t(method M)))\n" +
				"(type Bar\n" + 
				"\t\n" +
				"\t(field f (TypeRef System.Void))\n" +
				"\t\n" +
				"\t(region foo...\n" +
				"\t\t(method M)))\n";

			Assert.AreEqual (expected, o.ToString ());
		}

		private CodeTypeDeclaration GetClassType ()
		{
			return new CodeTypeDeclaration ();
		}

		private CodeTypeDeclaration GetDelegateType ()
		{
			CodeTypeDeclaration type = new CodeTypeDelegate ();
			return type;
		}

		private CodeTypeDeclaration GetEnumType ()
		{
			CodeTypeDeclaration type = new CodeTypeDeclaration ();
			type.IsEnum = true;
			return type;
		}

		private CodeTypeDeclaration GetInterfaceType ()
		{
			CodeTypeDeclaration type = new CodeTypeDeclaration ();
			type.IsInterface = true;
			return type;
		}

		private CodeTypeDeclaration GetStructType ()
		{
			CodeTypeDeclaration type = new CodeTypeDeclaration ();
			type.IsStruct = true;
			return type;
		}

		#region Override implementation of CodeGenerator

		protected override string NullToken
		{
			get { return "zip"; }
		}

		protected override void OutputType (CodeTypeReference typeRef)
		{
			Output.Write ("(TypeRef ");
			Output.Write (typeRef.BaseType);
			Output.Write (")");
		}

		protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression e)
		{
		}

		protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e)
		{
		}

		protected override void GenerateCastExpression (CodeCastExpression e)
		{
		}

		protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e)
		{
		}

		protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e)
		{
		}

		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e)
		{
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e)
		{
		}

		protected override void GenerateIndexerExpression (CodeIndexerExpression e)
		{
		}

		protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e)
		{
		}

		protected override void GenerateSnippetExpression (CodeSnippetExpression e)
		{
		}

		protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e)
		{
		}

		protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e)
		{
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression e)
		{
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e)
		{
		}

		protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression e)
		{
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e)
		{
		}

		protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e)
		{
		}

		protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression e)
		{
		}

		protected override void GenerateExpressionStatement (CodeExpressionStatement e)
		{
		}

		protected override void GenerateIterationStatement (CodeIterationStatement e)
		{
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement e)
		{
		}

		protected override void GenerateComment (CodeComment e)
		{
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement e)
		{
		}

		protected override void GenerateConditionStatement (CodeConditionStatement e)
		{
		}

		protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement e)
		{
		}

		protected override void GenerateAssignStatement (CodeAssignStatement e)
		{
		}

		protected override void GenerateAttachEventStatement (CodeAttachEventStatement e)
		{
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement e)
		{
		}

		protected override void GenerateLabeledStatement (CodeLabeledStatement e)
		{
		}

		protected override void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e)
		{
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma e)
		{
		}

		protected override void GenerateGotoStatement (CodeGotoStatement e)
		{
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma e)
		{
		}

		protected override void GenerateEvent (CodeMemberEvent e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateField (CodeMemberField e)
		{
			Output.WriteLine ();
			Output.Write ("(field ");
			Output.Write (e.Name);
			Output.Write (" ");
			OutputType (e.Type);
			Output.Write (")");
		}

		protected override void GenerateSnippetMember (CodeSnippetTypeMember e)
		{
		}

		protected override void GenerateEntryPointMethod (CodeEntryPointMethod e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateMethod (CodeMemberMethod e, CodeTypeDeclaration c)
		{
			Output.WriteLine ();
			Output.Write ("(method ");
			Output.Write (e.Name);
			Output.Write (")");
		}

		protected override void GenerateProperty (CodeMemberProperty e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateConstructor (CodeConstructor e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateTypeConstructor (CodeTypeConstructor e)
		{
		}

		protected override void GenerateTypeStart (CodeTypeDeclaration e)
		{
			Output.Write ("(type ");
			Output.Write (e.Name);
			++Indent;
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration e)
		{
			Output.WriteLine (")");
			--Indent;
		}

		protected override void GenerateNamespaceStart (CodeNamespace e)
		{
		}

		protected override void GenerateNamespaceEnd (CodeNamespace e)
		{
		}

		protected override void GenerateNamespaceImport (CodeNamespaceImport e)
		{
		}

		protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override bool Supports (GeneratorSupport support)
		{
			return true;
		}

		protected override bool IsValidIdentifier (string value)
		{
			return true;
		}

		protected override string CreateEscapedIdentifier (string value)
		{
			return value;
		}

		protected override string CreateValidIdentifier (string value)
		{
			return value;
		}

		protected override string GetTypeOutput (CodeTypeReference value)
		{
			return "typeoutput";
		}

		protected override string QuoteSnippetString (string value)
		{
			return value;
		}

#if NET_2_0
		protected override void GenerateDirectives (CodeDirectiveCollection directives)
		{
			foreach (CodeDirective d in directives) {
				var r = d as CodeRegionDirective;
				if (r != null) {
					if (r.RegionMode == CodeRegionMode.Start) {
						Output.WriteLine ();
						Output.Write ("(region ");
						Output.Write (r.RegionText);
						++Indent;
					}
					else if (r.RegionMode == CodeRegionMode.End) {
						Output.Write (")");
						--Indent;
					}
				}
			}
		}
#endif

		#endregion Override implementation of CodeGenerator
	}
}
