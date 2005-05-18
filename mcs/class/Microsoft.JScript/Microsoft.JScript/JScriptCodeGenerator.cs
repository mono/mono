//
// JScriptCodeGenerator.cs:
//
// Author: 
//

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

using System;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Microsoft.JScript {

	[MonoTODO]
	sealed class JScriptCodeGenerator : CodeCompiler {
		[MonoTODO]
		protected override string CompilerName {
			get {
				throw new NotImplementedException ();
			}
		}

		protected override string FileExtension {
			get {
				return ".js";
			}
		}

		protected override string NullToken {
			get {
				return "null";
			}
		}

		protected override string CmdArgsFromParameters(CompilerParameters options) {
			throw new NotImplementedException();
		}

		protected override string CreateEscapedIdentifier(string name) {
			throw new NotImplementedException();
		}

		protected override string CreateValidIdentifier(string name) {
			throw new NotImplementedException();
		}

		protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames) {
			throw new NotImplementedException();
		}

		protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateAssignStatement(CodeAssignStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateAttachEventStatement(CodeAttachEventStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes) {
			throw new NotImplementedException();
		}

		protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes) {
			throw new NotImplementedException();
		}

		protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateCastExpression(CodeCastExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateComment(CodeComment e) {
			throw new NotImplementedException();
		}

		protected override void GenerateCompileUnitStart(CodeCompileUnit e) {
			throw new NotImplementedException();
		}

		protected override void GenerateConditionStatement(CodeConditionStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c) {
			throw new NotImplementedException();
		}

		protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c) {
			throw new NotImplementedException();
		}

		protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c) {
			throw new NotImplementedException();
		}

		protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateExpressionStatement(CodeExpressionStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateField(CodeMemberField e) {
			throw new NotImplementedException();
		}

		protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateGotoStatement(CodeGotoStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateIndexerExpression(CodeIndexerExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateIterationStatement(CodeIterationStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateLabeledStatement(CodeLabeledStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateLinePragmaEnd(CodeLinePragma e) {
			throw new NotImplementedException();
		}

		protected override void GenerateLinePragmaStart(CodeLinePragma e) {
			throw new NotImplementedException();
		}

		protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c) {
			throw new NotImplementedException();
		}

		protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateNamespace(CodeNamespace e) {
			GenerateNamespaceStart (e);
			base.GenerateNamespace (e);
			GenerateNamespaceEnd (e);
		}

		protected override void GenerateNamespaceEnd(CodeNamespace e) {
			throw new NotImplementedException();
		}

		protected override void GenerateNamespaceImport(CodeNamespaceImport e) {
			throw new NotImplementedException();
		}

		protected override void GenerateNamespaceStart(CodeNamespace e) {
			throw new NotImplementedException();
		}

		protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e) {
			throw new NotImplementedException();
		}

		protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c) {
			throw new NotImplementedException();
		}

		protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateSingleFloatValue(float s) {
			throw new NotImplementedException();
		}

		protected override void GenerateSnippetExpression(CodeSnippetExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateSnippetMember(CodeSnippetTypeMember e) {
			throw new NotImplementedException();
		}

		protected override void GenerateSnippetStatement(CodeSnippetStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateTypeConstructor(CodeTypeConstructor e) {
			throw new NotImplementedException();
		}

		protected override void GenerateTypeEnd(CodeTypeDeclaration e) {
			throw new NotImplementedException();
		}

		protected override void GenerateTypeOfExpression(CodeTypeOfExpression e) {
			throw new NotImplementedException();
		}

		protected override void GenerateTypeStart(CodeTypeDeclaration e) {
			throw new NotImplementedException();
		}

		protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e) {
			throw new NotImplementedException();
		}

		protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override string GetTypeOutput(CodeTypeReference typeRef) {
			throw new NotImplementedException();
		}

		protected override bool IsValidIdentifier(string value) {
			throw new NotImplementedException();
		}

		protected override void OutputType(CodeTypeReference typeRef) {
			throw new NotImplementedException();
		}

		protected override void ProcessCompilerOutputLine(CompilerResults results, string line) {
			throw new NotImplementedException();
		}

		protected override string QuoteSnippetString(string value) {
			throw new NotImplementedException();
		}

		protected override bool Supports(GeneratorSupport support) {
			throw new NotImplementedException();
		}
	}
}