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
using System.IO;
using System.Collections;

namespace Microsoft.JScript {

	[MonoTODO]
	sealed class JScriptCodeGenerator : CodeCompiler {
		// It is used for beautiful "for" syntax -- in the future
		bool dont_write_semicolon = false;

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
			if (name == null)
				throw new NullReferenceException ("Argument identifier is null.");

			return GetSafeName (name);
		}

		protected override string CreateValidIdentifier(string name) {
			if (name == null)
				throw new NullReferenceException ();

			return GetSafeName (name);
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
			TextWriter output = Output;
			GenerateExpression (e.Left);
			output.Write (" = ");
			GenerateExpression (e.Right);
			if (dont_write_semicolon)
				return;
			output.WriteLine (';');
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
			TextWriter output = Output;
			string [] lines = e.Text.Split ('\n');
			bool first = true;
			foreach (string line in lines) {
				if (e.DocComment)
					output.Write ("///");
				else
					output.Write ("//");
				if (first) {
					output.Write (' ');
					first = false;
				}
				output.WriteLine (line);
			}
		}

		protected override void GenerateCompileUnitStart(CodeCompileUnit e) {
			throw new NotImplementedException();
		}

		protected override void GenerateConditionStatement(CodeConditionStatement e) {
			TextWriter output = Output;
			output.Write ("if (");

			GenerateExpression (e.Condition);

			output.WriteLine (") {");
			++Indent;
			GenerateStatements (e.TrueStatements);
			--Indent;

			CodeStatementCollection falses = e.FalseStatements;
			if (falses.Count > 0) {
				output.Write ('}');
				if (Options.ElseOnClosing)
					output.Write (' ');
				else
					output.WriteLine ();
				output.WriteLine ("else {");
				++Indent;
				GenerateStatements (falses);
				--Indent;
			}
			output.WriteLine ('}');
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
			TextWriter output = Output;

			if (e.Expression != null) {
				output.Write ("return ");
				GenerateExpression (e.Expression);
				output.WriteLine (";");
			} else
				output.WriteLine ("return;");
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
			Output.Write ("this");
		}

		protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e) {
			Output.Write ("throw");
			if (e.ToThrow != null) {
				Output.Write (' ');
				GenerateExpression (e.ToThrow);
			}
			Output.WriteLine (";");
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
			TextWriter output = Output;

			output.Write ("var ");
			output.Write (GetSafeName (e.Name));

			CodeExpression initExpression = e.InitExpression;
			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine (';');
		}

		protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e) {
			throw new NotImplementedException();
		}

		protected override string GetTypeOutput(CodeTypeReference typeRef) {
			throw new NotImplementedException();
		}

		protected override bool IsValidIdentifier(string value) {
			if (keywordsTable == null)
				FillKeywordTable ();

			return !keywordsTable.Contains (value);
		}

		protected override void OutputType(CodeTypeReference typeRef) {
			throw new NotImplementedException();
		}

		protected override void ProcessCompilerOutputLine(CompilerResults results, string line) {
			throw new NotImplementedException();
		}

		[MonoTODO ("Implement missing special characters")]
		protected override string QuoteSnippetString (string value)
		{
			// FIXME: this is weird, but works.
			string output = value.Replace ("\\", "\\\\");
			output = output.Replace ("\"", "\\\"");
			output = output.Replace ("\t", "\\t");
			output = output.Replace ("\r", "\\r");
			output = output.Replace ("\n", "\\n");

			return "\"" + output + "\"";
		}

		protected override bool Supports(GeneratorSupport support) {
			throw new NotImplementedException();
		}

		string GetSafeName (string id)
		{
			if (keywordsTable == null)
				FillKeywordTable ();

			if (keywordsTable.Contains (id))
				return "_" + id;
			else
				return id;
		}

		static void FillKeywordTable ()
		{
			keywordsTable = new Hashtable ();
			foreach (string keyword in keywords)
				keywordsTable.Add (keyword, keyword);
		}

		static Hashtable keywordsTable;
		static string [] keywords = new string [] {
			"break", "else", "new", "var",
			"case", "finally", "return", "void",
			"catch", "for", "switch", "while",
			"continue", "function", "this", "with",
			"default", "if", "throw",
			"delete", "in", "try",
			"do", "instanceof", "typeof",
			// Future reserved keywords
			"abstract", "enum", "int", "short",
			"boolean", "export", "interface", "static",
			"byte", "extends", "long", "super",
			"char", "final", "native", "synchronized",
			"class", "float", "package", "throws",
			"const", "goto", "private", "transient",
			"debugger", "implements", "protected", "volatile",
			"double", "import", "public"
		};
	}
}