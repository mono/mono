//
// CodeGenerator.GenerateFromCompileUnit Tests
//
// Authors:
// Ilker Cetinkaya (mail@ilker.de)
//
// This is a specific test for an issue on GenerateFromCompileUnit.
// Up until 2012 (version 2.10.n) the method emits attribute first
// and imports afterwards on global namespace scope. Example:
//
// ~~~~
// [assembly: AssemblyVersion("1.0")]
// using System.Reflection;
// ~~~~
//
// This in particular causes compiler to bail with CS1529.
// Albeit well aware that this is a _language specific_ issue
// (or requirement), the actual fix is aimed on CodeGenerator since
// the wrong emit order is as well in GenerateFromCompileUnit of abstract
// base. The probability to harm any other language generators
// is very low. It's near common sense to have imports on top
// on global namespace / file level.
//
// The test is being repeated for the internal `CSharpCodeGenerator`.
// See `Microsoft.CSharp` (Mono.CSharp namespace) for details.
//
// This test verifies the issue as well as describes correct expectation.

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.CodeDom.Compiler
{
	[TestFixture]
	public class CodeGeneratorGenerateFromCompileUnitTest {
		[Test]
		public void When_Having_AssemblyAttribute_And_Using_Namespace_It_Should_Generate_Namespace_First_And_Attribute_Afterwards () {
			ICodeGenerator generator = new SampleCodeGenerator ();
			var compileUnit = ACompileUnitWithAttributeAndNamespace ();
			var writer = new StringWriter ();
			var options = new CodeGeneratorOptions ();

			generator.GenerateCodeFromCompileUnit (compileUnit, writer, options);

			string result = writer.ToString ();
			
			int importPosition = result.IndexOf (IMPORT);
			int attributePosition = result.IndexOf (ATTRIBUTE);

			Assert.Greater (attributePosition, importPosition, "Actual order: " + result);
		}

		[Test]
		public void CodeSnippetBlankLines ()
		{
			var opt = new CodeGeneratorOptions () {
				BlankLinesBetweenMembers = false,
				VerbatimOrder = false
			};

			var ccu = new CodeCompileUnit ();
			var ns = new CodeNamespace ("Foo");
			ccu.Namespaces.Add (ns);
			var t = new CodeTypeDeclaration ("Bar");
			ns.Types.Add (t);

			t.Members.Add (new CodeSnippetTypeMember ("#line hidden"));
			t.Members.Add (new CodeSnippetTypeMember ("#line hidden2"));
	
			t.Members.Add (new CodeMemberMethod () { Name = "Foo" });

			using (var sw = new StringWriter ()) {
				new CSharpCodeProvider ().GenerateCodeFromCompileUnit (ccu, sw, opt);
				var str = sw.ToString ();

				Assert.IsFalse (str.Contains ("hidden2private"), "#0");
				Assert.IsTrue (str.Contains( "#line hidden#line hidden2"), "#1");
			}
		}

		[Test]
		public void CodeSnippetBlankLinesVerbatimOrder ()
		{
			var opt = new CodeGeneratorOptions () {
				BlankLinesBetweenMembers = false,
				VerbatimOrder = true
			};

			var ccu = new CodeCompileUnit ();
			var ns = new CodeNamespace ("Foo");
			ccu.Namespaces.Add (ns);
			var t = new CodeTypeDeclaration ("Bar");
			ns.Types.Add (t);

			t.Members.Add (new CodeSnippetTypeMember ("#line hidden"));
			t.Members.Add (new CodeSnippetTypeMember ("#line hidden2"));
	
			t.Members.Add (new CodeMemberMethod () { Name = "Foo" });

			using (var sw = new StringWriter ()) {
				new CSharpCodeProvider ().GenerateCodeFromCompileUnit (ccu, sw, opt);
				var str = sw.ToString ();

				Assert.IsFalse (str.Contains ("hidden2private"), "#0");
				Assert.IsFalse (str.Contains( "#line hidden#line hidden2"), "#1");
				Assert.IsTrue (str.Contains( "#line hidden" + Environment.NewLine), "#2");
				Assert.IsTrue (str.Contains( "#line hidden2" + Environment.NewLine), "#3");
			}
		}

		private const string ATTRIBUTE = "ATTRIBUTE";
		private const string IMPORT = "IMPORT";

		private CodeCompileUnit ACompileUnitWithAttributeAndNamespace () {
			var compileUnit = new CodeCompileUnit ();
			var importNs = new CodeNamespace ();

			importNs.Imports.Add (new CodeNamespaceImport (IMPORT));

			compileUnit.AssemblyCustomAttributes.Add (new CodeAttributeDeclaration (ATTRIBUTE));
			compileUnit.Namespaces.Add (importNs);

			return compileUnit;
		}

		private class SampleCodeGenerator : CodeGenerator {
			/* test overrides */
			protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes) { Output.Write ("ATTRIBUTE"); }
			protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes) {}
			protected override void GenerateNamespaceImport (CodeNamespaceImport i) { Output.Write ("IMPORT"); }
			/* must overrides */
			protected override string NullToken { get { return string.Empty; } }
			protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e) {}
			protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression e) {}
			protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e) {}
			protected override void GenerateAssignStatement (CodeAssignStatement s) {}
			protected override void GenerateAttachEventStatement (CodeAttachEventStatement s) {}
			protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e) {}
			protected override void GenerateCastExpression (CodeCastExpression e) {}
			protected override void GenerateComment (CodeComment comment) {}
			protected override void GenerateConditionStatement (CodeConditionStatement s) {}
			protected override void GenerateConstructor (CodeConstructor x, CodeTypeDeclaration d) {}
			protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e) {}
			protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e) {}
			protected override void GenerateEntryPointMethod (CodeEntryPointMethod m, CodeTypeDeclaration d) {}
			protected override void GenerateEvent (CodeMemberEvent ev, CodeTypeDeclaration d) {}
			protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression e) {}
			protected override void GenerateExpressionStatement (CodeExpressionStatement statement) {}
			protected override void GenerateField (CodeMemberField f) {}
			protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e) {}
			protected override void GenerateGotoStatement (CodeGotoStatement statement) {}
			protected override void GenerateIndexerExpression (CodeIndexerExpression e) {}
			protected override void GenerateIterationStatement (CodeIterationStatement s) {}
			protected override void GenerateLabeledStatement (CodeLabeledStatement statement) {}
			protected override void GenerateLinePragmaStart (CodeLinePragma p) {}
			protected override void GenerateLinePragmaEnd (CodeLinePragma p) {}
			protected override void GenerateMethod (CodeMemberMethod m, CodeTypeDeclaration d) {}
			protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e) {}
			protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e) {}
			protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement e) {}
			protected override void GenerateNamespaceStart (CodeNamespace ns) {}
			protected override void GenerateNamespaceEnd (CodeNamespace ns) {}
			protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression e) {}
			protected override void GenerateProperty (CodeMemberProperty p, CodeTypeDeclaration d) {}
			protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e) {}
			protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e) {}
			protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement statement) {}
			protected override void GenerateSnippetExpression (CodeSnippetExpression e) {}
			protected override void GenerateSnippetMember (CodeSnippetTypeMember m) {}
			protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression e) {}
			protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement s) {}
			protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement s) {}
			protected override void GenerateTypeEnd (CodeTypeDeclaration declaration) {}
			protected override void GenerateTypeConstructor (CodeTypeConstructor constructor) {}
			protected override void GenerateTypeStart (CodeTypeDeclaration declaration) {}
			protected override void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e) {}
			protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e) {}
			protected override void OutputType (CodeTypeReference t) {}
			protected override string QuoteSnippetString (string value) { return string.Empty; }
			protected override string CreateEscapedIdentifier (string value) { return string.Empty; }
			protected override string CreateValidIdentifier (string value) { return string.Empty; }
			protected override string GetTypeOutput (CodeTypeReference type) { return string.Empty; }
			protected override bool IsValidIdentifier (string value) { return false; }
			protected override bool Supports (GeneratorSupport supports) { return false; }
		}
	}
}
