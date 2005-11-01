//
// CodeGeneratorCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.CodeGenerator
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom.Compiler {

	class CodeGeneratorTest: CodeGenerator {

		public CodeGeneratorTest ()
		{
		}

		protected override string CreateEscapedIdentifier (string value)
		{
			return String.Empty;
		}

		protected override string CreateValidIdentifier (string value)
		{
			return String.Empty;
		}

		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e)
		{
		}

		protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression e)
		{
		}

		protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e)
		{
		}

		protected override void GenerateAssignStatement (CodeAssignStatement e)
		{
		}

		protected override void GenerateAttachEventStatement (CodeAttachEventStatement e)
		{
		}

		protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e)
		{
		}

		protected override void GenerateCastExpression (CodeCastExpression e)
		{
		}

		protected override void GenerateComment (CodeComment e)
		{
		}

		protected override void GenerateConditionStatement (CodeConditionStatement e)
		{
		}

		protected override void GenerateConstructor (CodeConstructor e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e)
		{
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e)
		{
		}

		protected override void GenerateEntryPointMethod (CodeEntryPointMethod e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateEvent (CodeMemberEvent e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression e)
		{
		}

		protected override void GenerateExpressionStatement (CodeExpressionStatement e)
		{
		}

		protected override void GenerateField (CodeMemberField e)
		{
		}

		protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e)
		{
		}

		protected override void GenerateGotoStatement (CodeGotoStatement e)
		{
		}

		protected override void GenerateIndexerExpression (CodeIndexerExpression e)
		{
		}

		protected override void GenerateIterationStatement (CodeIterationStatement e)
		{
		}

		protected override void GenerateLabeledStatement (CodeLabeledStatement e)
		{
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma e)
		{
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma e)
		{
		}

		protected override void GenerateMethod (CodeMemberMethod e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e)
		{
		}

		protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e)
		{
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement e)
		{
		}

		protected override void GenerateNamespaceEnd (CodeNamespace e)
		{
		}

		protected override void GenerateNamespaceImport (CodeNamespaceImport e)
		{
		}

		protected override void GenerateNamespaceStart (CodeNamespace e)
		{
		}

		protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression e)
		{
		}

		protected override void GenerateProperty (CodeMemberProperty e, CodeTypeDeclaration c)
		{
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e)
		{
		}

		protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e)
		{
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement e)
		{
		}

		protected override void GenerateSnippetExpression (CodeSnippetExpression e)
		{
		}

		protected override void GenerateSnippetMember (CodeSnippetTypeMember e)
		{
		}

		protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression e)
		{
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement e)
		{
		}

		protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement e)
		{
		}

		protected override void GenerateTypeConstructor (CodeTypeConstructor e)
		{
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration e)
		{
		}

		protected override void GenerateTypeStart (CodeTypeDeclaration e)
		{
		}

		protected override void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e)
		{
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e)
		{
		}

		protected override string GetTypeOutput (CodeTypeReference value)
		{
			return String.Empty;
		}

		protected override bool IsValidIdentifier (string value)
		{
			return true;
		}

		protected override string NullToken {
			get {
				return String.Empty;
			}
		}

		protected override void OutputType (CodeTypeReference typeRef)
		{
		}

		protected override string QuoteSnippetString (string value)
		{
			return String.Empty;
		}

		protected override bool Supports (GeneratorSupport support)
		{
			return true;
		}

		public void TestProtectedProperties ()
		{
#if NET_2_0
			Assert.IsNull (CurrentClass, "CurrentClass");
#endif
			Assert.IsNull (CurrentMember, "CurrentMember");
			Assert.AreEqual ("<% unknown %>", CurrentMemberName, "CurrentMemberName");
			Assert.AreEqual ("<% unknown %>", CurrentTypeName, "CurrentTypeName");

			try {
				Assert.AreEqual (0, Indent, "Indent");
			}
			catch (NullReferenceException) {
			}

			try {
				Indent = Int32.MinValue;
			}
			catch (NullReferenceException) {
			}

			Assert.IsFalse (IsCurrentClass, "IsCurrentClass");
			Assert.IsFalse (IsCurrentDelegate, "IsCurrentDelegate");
			Assert.IsFalse (IsCurrentEnum, "IsCurrentEnum");
			Assert.IsFalse (IsCurrentInterface, "IsCurrentInterface");
			Assert.IsFalse (IsCurrentStruct, "IsCurrentStruct");
			Assert.AreEqual (String.Empty, NullToken, "NullToken");
			Assert.IsNull (Options, "Options");
			Assert.IsNull (Output, "Output");
		}

		public void TestProtectedMethods ()
		{
			try {
				ContinueOnNewLine (String.Empty);
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateBinaryOperatorExpression (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateCommentStatement (null);
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateCommentStatements (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateCompileUnit (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateCompileUnitEnd (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateCompileUnitStart (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateDecimalValue (0); 
			}
			catch (NullReferenceException) {
			}
#if NET_2_0
			try {
				GenerateDefaultValueExpression (null);
			}
			catch (NotImplementedException) {
				// both mono & ms
			}
#endif
			try {
				GenerateDirectionExpression (null); 
			}
			catch (NullReferenceException) {
			}
#if NET_2_0
			GenerateDirectives (null);
#endif
			try {
				GenerateDoubleValue (Double.MaxValue); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateExpression (null);
			}
			catch (ArgumentNullException) {
			}
			try {
				GenerateNamespace (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateNamespaceImports (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateNamespaces (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateParameterDeclarationExpression (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GeneratePrimitiveExpression (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateSingleFloatValue (Single.MinValue);
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateSnippetCompileUnit (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateSnippetStatement (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateStatement (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateStatements (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateTypeOfExpression (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateTypeReferenceExpression (null); 
			}
			catch (NullReferenceException) {
			}
			try {
				GenerateTypes (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputAttributeArgument (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputAttributeDeclarations (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputDirection (FieldDirection.In);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputExpressionList (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputExpressionList (null, true);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputFieldScopeModifier (MemberAttributes.Abstract);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputIdentifier (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputMemberAccessModifier (MemberAttributes.Abstract);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputMemberScopeModifier (MemberAttributes.Abstract);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputOperator (CodeBinaryOperatorType.Add);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputParameters (null);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputTypeAttributes (TypeAttributes.Abstract, false, false);
			}
			catch (NullReferenceException) {
			}
			try {
				OutputTypeNamePair (null, null);
			}
			catch (NullReferenceException) {
			}
			try {
				Supports (GeneratorSupport.ArraysOfArrays);
			}
			catch (NullReferenceException) {
			}
			try {
				ValidateIdentifier (null);
			}
			catch (NullReferenceException) {
			}
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class CodeGeneratorCas {

		private StringWriter writer;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at full trust
			writer = new StringWriter ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Public ()
		{
			CodeGeneratorTest cg = new CodeGeneratorTest ();
			try {
				cg.GenerateCodeFromMember (new CodeTypeMember (), writer, null);
			}
			catch (NotImplementedException) {
				// mono
			}
		}
#endif

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Protected ()
		{
			CodeGeneratorTest cg = new CodeGeneratorTest ();
			// test protected (but not abstract) stuff from within the class itself
			cg.TestProtectedProperties ();
			cg.TestProtectedMethods ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StaticMethods ()
		{
			Assert.IsFalse (CodeGenerator.IsValidLanguageIndependentIdentifier ("@"), "IsValidLanguageIndependentIdentifier");
			try {
				CodeGenerator.ValidateIdentifiers (new CodeCompileUnit ());
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeGeneratorTest).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
