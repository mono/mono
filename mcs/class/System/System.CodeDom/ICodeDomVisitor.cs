//
// System.CodeDom.ICodeDomVisitor.cs
//
// Author:
//   Juraj Skripsky (js@hotfeet.ch)
//
// Copyright (C) 2008 HotFeet GmbH (http://www.hotfeet.ch)
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

namespace System.CodeDom {
	internal interface ICodeDomVisitor {
		// CodeExpression
		void Visit (CodeArgumentReferenceExpression o);
		void Visit (CodeArrayCreateExpression o);
		void Visit (CodeArrayIndexerExpression o);
		void Visit (CodeBaseReferenceExpression o);
		void Visit (CodeBinaryOperatorExpression o);
		void Visit (CodeCastExpression o);
		void Visit (CodeDefaultValueExpression o);
		void Visit (CodeDelegateCreateExpression o);
		void Visit (CodeDelegateInvokeExpression o);
		void Visit (CodeDirectionExpression o);
		void Visit (CodeEventReferenceExpression o);
		void Visit (CodeFieldReferenceExpression o);
		void Visit (CodeIndexerExpression o);
		void Visit (CodeMethodInvokeExpression o);
		void Visit (CodeMethodReferenceExpression o);
		void Visit (CodeObjectCreateExpression o);
		void Visit (CodeParameterDeclarationExpression o);
		void Visit (CodePrimitiveExpression o);
		void Visit (CodePropertyReferenceExpression o);
		void Visit (CodePropertySetValueReferenceExpression o);
		void Visit (CodeSnippetExpression o);
		void Visit (CodeThisReferenceExpression o);
		void Visit (CodeTypeOfExpression o);
		void Visit (CodeTypeReferenceExpression o);
		void Visit (CodeVariableReferenceExpression o);

		// CodeStatement
		void Visit (CodeAssignStatement o);
		void Visit (CodeAttachEventStatement o);
		void Visit (CodeCommentStatement o);
		void Visit (CodeConditionStatement o);
		void Visit (CodeExpressionStatement o);
		void Visit (CodeGotoStatement o);
		void Visit (CodeIterationStatement o);
		void Visit (CodeLabeledStatement o);
		void Visit (CodeMethodReturnStatement o);
		void Visit (CodeRemoveEventStatement o);
		void Visit (CodeThrowExceptionStatement o);
		void Visit (CodeTryCatchFinallyStatement o);
		void Visit (CodeVariableDeclarationStatement o);

		// CodeTypeMember
		void Visit (CodeConstructor o);
		void Visit (CodeEntryPointMethod o);
		void Visit (CodeMemberEvent o);
		void Visit (CodeMemberField o);
		void Visit (CodeMemberMethod o);
		void Visit (CodeMemberProperty o);
		void Visit (CodeSnippetTypeMember o);
		void Visit (CodeTypeConstructor o);
	}
}
