//
// System.CodeDom.Compiler.CodeGenerator.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001-2003 Ximian, Inc.
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

using System.Globalization;
using System.CodeDom;
using System.Reflection;
using System.IO;
using System.Collections;
	
namespace System.CodeDom.Compiler {

	public abstract class CodeGenerator : ICodeGenerator
	{
		private IndentedTextWriter output;
		private CodeGeneratorOptions options;
		private CodeTypeMember currentMember;
		private CodeTypeDeclaration currentType;
		private Visitor visitor;

		//
		// Constructors
		//
		protected CodeGenerator()
		{
			visitor = new Visitor (this);
		}

		//
		// Properties
		//
#if NET_2_0
		protected
#else
		internal
#endif
		CodeTypeDeclaration CurrentClass {
			get {
				return currentType;
			}
		}

		protected CodeTypeMember CurrentMember {
			get {
				return currentMember;
			}
		}
		
		protected string CurrentMemberName {
			get {
				if (currentMember == null)
					return "<% unknown %>";
				return currentMember.Name;
			}
		}

		protected string CurrentTypeName {
			get {
				if (currentType == null)
					return "<% unknown %>";
				return currentType.Name;
			}
		}
		
		protected int Indent {
			get {
				return output.Indent;
			}
			set {
				output.Indent = value;
			}
		}
		
		protected bool IsCurrentClass {
			get {
				if (currentType == null)
					return false;
				return currentType.IsClass && !(currentType is CodeTypeDelegate);
			}
		}

		protected bool IsCurrentDelegate {
			get {
				return currentType is CodeTypeDelegate;
			}
		}

		protected bool IsCurrentEnum {
			get {
				if (currentType == null)
					return false;
				return currentType.IsEnum;
			}
		}

		protected bool IsCurrentInterface {
			get {
				if (currentType == null)
					return false;
				return currentType.IsInterface;
			}
		}

		protected bool IsCurrentStruct {
			get {
				if (currentType == null)
					return false;
				return currentType.IsStruct;
			}
		}

		protected abstract string NullToken {
			get;
		}
		
		
		protected CodeGeneratorOptions Options {
			get {
				return options;
			}
		}
			
		protected TextWriter Output {
			get {
				return output;
			}
		}

		//
		// Methods
		//
		protected virtual void ContinueOnNewLine (string st)
		{
			output.WriteLine (st);
		}

		/*
		 * Code Generation methods
		 */
		protected abstract void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e);
		protected abstract void GenerateArrayCreateExpression (CodeArrayCreateExpression e);
		protected abstract void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e);
		protected abstract void GenerateAssignStatement (CodeAssignStatement s);
		protected abstract void GenerateAttachEventStatement (CodeAttachEventStatement s);
		protected abstract void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes);
		protected abstract void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes);
		protected abstract void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e);

		protected virtual void GenerateBinaryOperatorExpression (CodeBinaryOperatorExpression e)
		{
			output.Write ('(');
			GenerateExpression (e.Left);
			output.Write (' ');
			OutputOperator (e.Operator);
			output.Write (' ');
			GenerateExpression (e.Right);
			output.Write (')');
		}

		protected abstract void GenerateCastExpression (CodeCastExpression e);
#if NET_2_0
		[MonoTODO]
		public virtual void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException ();
		}
#endif
		protected abstract void GenerateComment (CodeComment comment);

		protected virtual void GenerateCommentStatement (CodeCommentStatement statement)
		{
			GenerateComment (statement.Comment);
		}

		protected virtual void GenerateCommentStatements (CodeCommentStatementCollection statements)
		{
			foreach (CodeCommentStatement comment in statements)
				GenerateCommentStatement (comment);
		}

		protected virtual void GenerateCompileUnit (CodeCompileUnit compileUnit)
		{
			GenerateCompileUnitStart (compileUnit);

			CodeAttributeDeclarationCollection attributes = compileUnit.AssemblyCustomAttributes;
			if (attributes.Count != 0) {
				foreach (CodeAttributeDeclaration att in attributes) {
					GenerateAttributeDeclarationsStart (attributes);
					output.Write ("assembly: ");
					OutputAttributeDeclaration (att);
					GenerateAttributeDeclarationsEnd (attributes);
				}
				output.WriteLine ();
			}

			foreach (CodeNamespace ns in compileUnit.Namespaces)
				GenerateNamespace (ns);

			GenerateCompileUnitEnd (compileUnit);
		}

		protected virtual void GenerateCompileUnitEnd (CodeCompileUnit compileUnit)
		{
#if NET_2_0
			if (compileUnit.EndDirectives.Count > 0)
				GenerateDirectives (compileUnit.EndDirectives);
#endif
		}

		protected virtual void GenerateCompileUnitStart (CodeCompileUnit compileUnit)
		{
#if NET_2_0
			if (compileUnit.StartDirectives.Count > 0) {
				GenerateDirectives (compileUnit.StartDirectives);
				Output.WriteLine ();
			}
#endif
		}

		protected abstract void GenerateConditionStatement (CodeConditionStatement s);
		protected abstract void GenerateConstructor (CodeConstructor x, CodeTypeDeclaration d);

		protected virtual void GenerateDecimalValue (Decimal d)
		{
			Output.Write (d.ToString (CultureInfo.InvariantCulture));
		}
#if NET_2_0
		[MonoTODO]
		protected virtual void GenerateDefaultValueExpression (CodeDefaultValueExpression e)
		{
			throw new NotImplementedException ();
		}
#endif
		protected abstract void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e);
		protected abstract void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e);

		protected virtual void GenerateDirectionExpression (CodeDirectionExpression e)
		{
			OutputDirection (e.Direction);
			output.Write (' ');
			GenerateExpression (e.Expression);
		}

		protected virtual void GenerateDoubleValue (Double d)
		{
			Output.Write (d.ToString (CultureInfo.InvariantCulture));
		}

		protected abstract void GenerateEntryPointMethod (CodeEntryPointMethod m, CodeTypeDeclaration d);
		protected abstract void GenerateEvent (CodeMemberEvent ev, CodeTypeDeclaration d);
		protected abstract void GenerateEventReferenceExpression (CodeEventReferenceExpression e);

		protected void GenerateExpression (CodeExpression e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");

			try {
				e.Accept (visitor);
			} catch (NotImplementedException) {
				throw new ArgumentException ("Element type " + e.GetType () + " is not supported.", "e");
			}
		}

		protected abstract void GenerateExpressionStatement (CodeExpressionStatement statement);
		protected abstract void GenerateField (CodeMemberField f);
		protected abstract void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e);
		protected abstract void GenerateGotoStatement (CodeGotoStatement statement);
		protected abstract void GenerateIndexerExpression (CodeIndexerExpression e);
		protected abstract void GenerateIterationStatement (CodeIterationStatement s);
		protected abstract void GenerateLabeledStatement (CodeLabeledStatement statement);
		protected abstract void GenerateLinePragmaStart (CodeLinePragma p);
		protected abstract void GenerateLinePragmaEnd (CodeLinePragma p);
		protected abstract void GenerateMethod (CodeMemberMethod m, CodeTypeDeclaration d);
		protected abstract void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e);
		protected abstract void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e);
		protected abstract void GenerateMethodReturnStatement (CodeMethodReturnStatement e);

		protected virtual void GenerateNamespace (CodeNamespace ns)
		{
			foreach (CodeCommentStatement statement in ns.Comments)
				GenerateCommentStatement (statement);

			GenerateNamespaceStart (ns);

			foreach (CodeNamespaceImport import in ns.Imports) {
				if (import.LinePragma != null)
					GenerateLinePragmaStart (import.LinePragma);

				GenerateNamespaceImport (import);

				if (import.LinePragma != null)
					GenerateLinePragmaEnd (import.LinePragma);
			}

			output.WriteLine();

			GenerateTypes (ns);

			GenerateNamespaceEnd (ns);
		}

		protected abstract void GenerateNamespaceStart (CodeNamespace ns);
		protected abstract void GenerateNamespaceEnd (CodeNamespace ns);
		protected abstract void GenerateNamespaceImport (CodeNamespaceImport i);
		protected void GenerateNamespaceImports (CodeNamespace e)
		{
			foreach (CodeNamespaceImport import in e.Imports) {
				if (import.LinePragma != null)
					GenerateLinePragmaStart (import.LinePragma);

				GenerateNamespaceImport (import);

				if (import.LinePragma != null)
					GenerateLinePragmaEnd (import.LinePragma);
			}
		}

		protected void GenerateNamespaces (CodeCompileUnit e)
		{
			foreach (CodeNamespace ns in e.Namespaces)
				GenerateNamespace (ns);
		}

		protected abstract void GenerateObjectCreateExpression (CodeObjectCreateExpression e);

		protected virtual void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			OutputType (e.Type);
			output.Write (' ');
			output.Write (e.Name);
		}

		protected virtual void GeneratePrimitiveExpression (CodePrimitiveExpression e)
		{
			object value = e.Value;
			if (value == null) {
				output.Write (NullToken);
				return;
			}
 
			Type type = value.GetType ();
			TypeCode typeCode = Type.GetTypeCode (type);
			switch (typeCode) {
			case TypeCode.Boolean:
				output.Write (value.ToString ().ToLower (CultureInfo.InvariantCulture));
				break;
			case TypeCode.Char:
				output.Write ("'" + value.ToString () + "'");
				break;
			case TypeCode.String:
				output.Write (QuoteSnippetString ((string) value));
				break;
			case TypeCode.Single:
				GenerateSingleFloatValue ((float) value);
				break;
			case TypeCode.Double:
				GenerateDoubleValue ((double) value);
				break;
			case TypeCode.Decimal:
				GenerateDecimalValue ((decimal) value);
				break;
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
				output.Write (((IFormattable)value).ToString (null, CultureInfo.InvariantCulture));
				break;
			default:
				throw new ArgumentException (string.Format(CultureInfo.InvariantCulture,
					"Invalid Primitive Type: {0}. Only CLS compliant primitive " +
					"types can be used. Consider using CodeObjectCreateExpression.",
					type.FullName));
			}
		}

		protected abstract void GenerateProperty (CodeMemberProperty p, CodeTypeDeclaration d);
		protected abstract void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e);
		protected abstract void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e);
		protected abstract void GenerateRemoveEventStatement (CodeRemoveEventStatement statement);

		protected virtual void GenerateSingleFloatValue (Single s)
		{
			output.Write (s.ToString(CultureInfo.InvariantCulture));
		}

		protected virtual void GenerateSnippetCompileUnit (CodeSnippetCompileUnit e)
		{
			if (e.LinePragma != null)
				GenerateLinePragmaStart (e.LinePragma);

			output.WriteLine (e.Value);

			if (e.LinePragma != null)
				GenerateLinePragmaEnd (e.LinePragma);

		}

		protected abstract void GenerateSnippetExpression (CodeSnippetExpression e);
		protected abstract void GenerateSnippetMember (CodeSnippetTypeMember m);
		protected virtual void GenerateSnippetStatement (CodeSnippetStatement s)
		{
			output.WriteLine (s.Value);
		}

		protected void GenerateStatement (CodeStatement s)
		{
#if NET_2_0
			if (s.StartDirectives.Count > 0)
				GenerateDirectives (s.StartDirectives);
#endif
			if (s.LinePragma != null)
				GenerateLinePragmaStart (s.LinePragma);

			CodeSnippetStatement snippet = s as CodeSnippetStatement;
			if (snippet != null) {
#if NET_2_0
				int indent = Indent;
				try {
					Indent = 0;
					GenerateSnippetStatement (snippet);
				} finally {
					Indent = indent;
				}
#else
				GenerateSnippetStatement (snippet);
#endif
			} else {
				try {
					s.Accept (visitor);
				} catch (NotImplementedException) {
					throw new ArgumentException ("Element type " + s.GetType () + " is not supported.", "s");
				}
			}

			if (s.LinePragma != null)
				GenerateLinePragmaEnd (s.LinePragma);

#if NET_2_0
			if (s.EndDirectives.Count > 0)
				GenerateDirectives (s.EndDirectives);
#endif			
		}

		protected void GenerateStatements (CodeStatementCollection c)
		{
			foreach (CodeStatement statement in c)
				GenerateStatement (statement);
		}

		protected abstract void GenerateThisReferenceExpression (CodeThisReferenceExpression e);
		protected abstract void GenerateThrowExceptionStatement (CodeThrowExceptionStatement s);
		protected abstract void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement s);
		protected abstract void GenerateTypeEnd (CodeTypeDeclaration declaration);
		protected abstract void GenerateTypeConstructor (CodeTypeConstructor constructor);

		protected virtual void GenerateTypeOfExpression (CodeTypeOfExpression e)
		{
			output.Write ("typeof(");
			OutputType (e.Type);
			output.Write (")");
		}

		protected virtual void GenerateTypeReferenceExpression (CodeTypeReferenceExpression e)
		{
			OutputType (e.Type);
		}

		protected void GenerateTypes (CodeNamespace e)
		{
			foreach (CodeTypeDeclaration type in e.Types) {
				if (options.BlankLinesBetweenMembers)
					output.WriteLine ();

				GenerateType (type);
			}
		}

		protected abstract void GenerateTypeStart (CodeTypeDeclaration declaration);
		protected abstract void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e);
		protected abstract void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e);

		//
		// Other members
		//
		
		/*
		 * Output Methods
		 */
		protected virtual void OutputAttributeArgument (CodeAttributeArgument argument)
		{
			string name = argument.Name;
			if ((name != null) && (name.Length > 0)) {
				output.Write (name);
				output.Write ('=');
			}
			GenerateExpression (argument.Value);
		}

		private void OutputAttributeDeclaration (CodeAttributeDeclaration attribute)
		{
			output.Write (attribute.Name.Replace ('+', '.'));
			output.Write ('(');
			IEnumerator enumerator = attribute.Arguments.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeAttributeArgument argument = (CodeAttributeArgument)enumerator.Current;
				OutputAttributeArgument (argument);
				
				while (enumerator.MoveNext()) {
					output.Write (',');
					argument = (CodeAttributeArgument)enumerator.Current;
					OutputAttributeArgument (argument);
				}
			}
			output.Write (')');
		}

		protected virtual void OutputAttributeDeclarations (CodeAttributeDeclarationCollection attributes)
		{
			GenerateAttributeDeclarationsStart (attributes);
			
			IEnumerator enumerator = attributes.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeAttributeDeclaration attribute = (CodeAttributeDeclaration)enumerator.Current;

				OutputAttributeDeclaration (attribute);
				
				while (enumerator.MoveNext()) {
					attribute = (CodeAttributeDeclaration)enumerator.Current;

					output.WriteLine (',');
					OutputAttributeDeclaration (attribute);
				}
			}

			GenerateAttributeDeclarationsEnd (attributes);
		}

		protected virtual void OutputDirection (FieldDirection direction)
		{
			switch (direction) {
			case FieldDirection.In:
				//output.Write ("in ");
				break;
			case FieldDirection.Out:
				output.Write ("out ");
				break;
			case FieldDirection.Ref:
				output.Write ("ref ");
				break;
			}
		}

		protected virtual void OutputExpressionList (CodeExpressionCollection expressions)
		{
			OutputExpressionList (expressions, false);
		}

		protected virtual void OutputExpressionList (CodeExpressionCollection expressions,
							     bool newLineBetweenItems)
		{
			++Indent;
			IEnumerator enumerator = expressions.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeExpression expression = (CodeExpression)enumerator.Current;

				GenerateExpression (expression);
				
				while (enumerator.MoveNext()) {
					expression = (CodeExpression)enumerator.Current;
					
					output.Write (',');
					if (newLineBetweenItems)
						output.WriteLine ();
					else
						output.Write (' ');
					
					GenerateExpression (expression);
				}
			}
			--Indent;
		}

		protected virtual void OutputFieldScopeModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				output.Write ("new ");

			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Static:
				output.Write ("static ");
				break;
			case MemberAttributes.Const:
				output.Write ("const ");
				break;
			}
		}

		protected virtual void OutputIdentifier (string ident)
		{
			output.Write (ident);
		}

		protected virtual void OutputMemberAccessModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.AccessMask) {
				case MemberAttributes.Assembly:
					output.Write ("internal ");
					break;
				case MemberAttributes.FamilyAndAssembly:
#if NET_2_0
					output.Write ("internal "); 
#else
					output.Write ("/*FamANDAssem*/ internal "); 
#endif
					break;
				case MemberAttributes.Family:
					output.Write ("protected ");
					break;
				case MemberAttributes.FamilyOrAssembly:
					output.Write ("protected internal ");
					break;
				case MemberAttributes.Private:
					output.Write ("private ");
					break;
				case MemberAttributes.Public:
					output.Write ("public ");
					break;
			}
		}

		protected virtual void OutputMemberScopeModifier (MemberAttributes attributes)
		{
#if NET_2_0
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				output.Write( "new " );
#endif

			switch (attributes & MemberAttributes.ScopeMask) {
				case MemberAttributes.Abstract:
					output.Write ("abstract ");
					break;
				case MemberAttributes.Final:
					// Do nothing
					break;
				case MemberAttributes.Static:
					output.Write ("static ");
					break;
				case MemberAttributes.Override:
					output.Write ("override ");
					break;
				default:
					//
					// FUNNY! if the scope value is
					// rubbish (0 or >Const), and access
					// is public or protected, make it
					// "virtual".
					//
					// i'm not sure whether this is 100%
					// correct, but it seems to be MS
					// behavior.
					//
					// On .NET 2.0, internal members
					// are also marked "virtual".
					//
					MemberAttributes access = attributes & MemberAttributes.AccessMask;
					if (access == MemberAttributes.Public || access == MemberAttributes.Family)
						output.Write ("virtual ");
					break;
			}
		}
				
		protected virtual void OutputOperator (CodeBinaryOperatorType op)
		{
			switch (op) {
			case CodeBinaryOperatorType.Add:
				output.Write ("+");
				break;
			case CodeBinaryOperatorType.Subtract:
				output.Write ("-");
				break;
			case CodeBinaryOperatorType.Multiply:
				output.Write ("*");
				break;
			case CodeBinaryOperatorType.Divide:
				output.Write ("/");
				break;
			case CodeBinaryOperatorType.Modulus:
				output.Write ("%");
				break;
			case CodeBinaryOperatorType.Assign:
				output.Write ("=");
				break;
			case CodeBinaryOperatorType.IdentityInequality:
				output.Write ("!=");
				break;
			case CodeBinaryOperatorType.IdentityEquality:
				output.Write ("==");
				break;
			case CodeBinaryOperatorType.ValueEquality:
				output.Write ("==");
				break;
			case CodeBinaryOperatorType.BitwiseOr:
				output.Write ("|");
				break;
			case CodeBinaryOperatorType.BitwiseAnd:
				output.Write ("&");
				break;
			case CodeBinaryOperatorType.BooleanOr:
				output.Write ("||");
				break;
			case CodeBinaryOperatorType.BooleanAnd:
				output.Write ("&&");
				break;
			case CodeBinaryOperatorType.LessThan:
				output.Write ("<");
				break;
			case CodeBinaryOperatorType.LessThanOrEqual:
				output.Write ("<=");
				break;
			case CodeBinaryOperatorType.GreaterThan:
				output.Write (">");
				break;
			case CodeBinaryOperatorType.GreaterThanOrEqual:
				output.Write (">=");
				break;
			}
		}

		protected virtual void OutputParameters (CodeParameterDeclarationExpressionCollection parameters)
		{
			bool first = true;
			foreach (CodeParameterDeclarationExpression expr in parameters) {
				if (first)
					first = false;
				else
					output.Write (", ");
				GenerateExpression (expr);
			}
		}

		protected abstract void OutputType (CodeTypeReference t);

		protected virtual void OutputTypeAttributes (TypeAttributes attributes,
							     bool isStruct,
							     bool isEnum)
		{
			switch (attributes & TypeAttributes.VisibilityMask) {
			case TypeAttributes.NotPublic:
				// private by default
				break; 

			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				output.Write ("public ");
				break;

			case TypeAttributes.NestedPrivate:
				output.Write ("private ");
				break;
			}

			if (isStruct)
				output.Write ("struct ");

			else if (isEnum)
				output.Write ("enum ");
			else {
				if ((attributes & TypeAttributes.Interface) != 0) 
					output.Write ("interface ");
				else if (currentType is CodeTypeDelegate)
					output.Write ("delegate ");
				else {
					if ((attributes & TypeAttributes.Sealed) != 0)
						output.Write ("sealed ");
					if ((attributes & TypeAttributes.Abstract) != 0)
						output.Write ("abstract ");
					
					output.Write ("class ");
				}
			}
		}

		protected virtual void OutputTypeNamePair (CodeTypeReference type,
							   string name)
		{
			OutputType (type);
			output.Write (' ');
			output.Write (name);
		}

		protected abstract string QuoteSnippetString (string value);

		/*
		 * ICodeGenerator
		 */
		protected abstract string CreateEscapedIdentifier (string value);
		string ICodeGenerator.CreateEscapedIdentifier (string value)
		{
			return CreateEscapedIdentifier (value);
		}

		protected abstract string CreateValidIdentifier (string value);
		string ICodeGenerator.CreateValidIdentifier (string value)
		{
			return CreateValidIdentifier (value);
		}

		private void InitOutput (TextWriter output, CodeGeneratorOptions options)
		{
			if (options == null)
				options = new CodeGeneratorOptions ();
				
			this.output = new IndentedTextWriter (output, options.IndentString);
			this.options = options;
		}

		void ICodeGenerator.GenerateCodeFromCompileUnit (CodeCompileUnit compileUnit,
								 TextWriter output,
								 CodeGeneratorOptions options)
		{
			InitOutput (output, options);

			if (compileUnit is CodeSnippetCompileUnit) {
				GenerateSnippetCompileUnit ((CodeSnippetCompileUnit) compileUnit);
			} else {
				GenerateCompileUnit (compileUnit);
			}
		}

		void ICodeGenerator.GenerateCodeFromExpression (CodeExpression expression,
								TextWriter output,
								CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateExpression (expression);
		}

		void ICodeGenerator.GenerateCodeFromNamespace (CodeNamespace ns,
							       TextWriter output, 
							       CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateNamespace (ns);
		}

		void ICodeGenerator.GenerateCodeFromStatement (CodeStatement statement,
							       TextWriter output, 
							       CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateStatement (statement);
		}

		void ICodeGenerator.GenerateCodeFromType (CodeTypeDeclaration type,
							  TextWriter output,
							  CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateType (type);
		}

		private void GenerateType (CodeTypeDeclaration type)
		{
			this.currentType = type;
			this.currentMember = null;

#if NET_2_0
			if (type.StartDirectives.Count > 0)
				GenerateDirectives (type.StartDirectives);
#endif
			foreach (CodeCommentStatement statement in type.Comments)
				GenerateCommentStatement (statement);

			if (type.LinePragma != null)
				GenerateLinePragmaStart (type.LinePragma);

			GenerateTypeStart (type);

			CodeTypeMember[] members = new CodeTypeMember[type.Members.Count];
			type.Members.CopyTo (members, 0);

#if NET_2_0
			if (!Options.VerbatimOrder)
#endif
 {
				int[] order = new int[members.Length];
				for (int n = 0; n < members.Length; n++)
					order[n] = Array.IndexOf (memberTypes, members[n].GetType ()) * members.Length + n;

				Array.Sort (order, members);
			}

			// WARNING: if anything is missing in the foreach loop and you add it, add the type in
			// its corresponding place in CodeTypeMemberComparer class (below)

			CodeTypeDeclaration subtype = null;
			foreach (CodeTypeMember member in members) {
				CodeTypeMember prevMember = this.currentMember;
				this.currentMember = member;

				if (prevMember != null && subtype == null) {
					if (prevMember.LinePragma != null)
						GenerateLinePragmaEnd (prevMember.LinePragma);
#if NET_2_0
					if (prevMember.EndDirectives.Count > 0)
						GenerateDirectives (prevMember.EndDirectives);
#endif
				}

				if (options.BlankLinesBetweenMembers)
					output.WriteLine ();

				subtype = member as CodeTypeDeclaration;
				if (subtype != null) {
					GenerateType (subtype);
					this.currentType = type;
					continue;
				}

#if NET_2_0
				if (currentMember.StartDirectives.Count > 0)
					GenerateDirectives (currentMember.StartDirectives);
#endif
				foreach (CodeCommentStatement statement in member.Comments)
					GenerateCommentStatement (statement);

				if (member.LinePragma != null)
					GenerateLinePragmaStart (member.LinePragma);

				try {
					member.Accept (visitor);
				} catch (NotImplementedException) {
					throw new ArgumentException ("Element type " + member.GetType () + " is not supported.");
				}
			}

			// Hack because of previous continue usage
			if (currentMember != null && !(currentMember is CodeTypeDeclaration)) {
				if (currentMember.LinePragma != null)
					GenerateLinePragmaEnd (currentMember.LinePragma);
#if NET_2_0
				if (currentMember.EndDirectives.Count > 0)
					GenerateDirectives (currentMember.EndDirectives);
#endif
			}

			this.currentType = type;
			GenerateTypeEnd (type);

			if (type.LinePragma != null)
				GenerateLinePragmaEnd (type.LinePragma);

#if NET_2_0
			if (type.EndDirectives.Count > 0)
				GenerateDirectives (type.EndDirectives);
#endif
		}

		protected abstract string GetTypeOutput (CodeTypeReference type);

		string ICodeGenerator.GetTypeOutput (CodeTypeReference type)
		{
			return GetTypeOutput (type);
		}

		protected abstract bool IsValidIdentifier (string value);

		bool ICodeGenerator.IsValidIdentifier (string value)
		{
			return IsValidIdentifier (value);
		}

		public static bool IsValidLanguageIndependentIdentifier (string value)
		{
			if (value == null)
				return false;
			if (value.Equals (string.Empty))
				return false;

			switch (char.GetUnicodeCategory (value[0])) {
			case UnicodeCategory.LetterNumber:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.ConnectorPunctuation:
				break;
			default:
				return false;
			}

			for (int x = 1; x < value.Length; ++x) {
				switch (char.GetUnicodeCategory (value[x])) {
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.ConnectorPunctuation:
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.Format:
					break;
				default:
					return false;
				}
			}

			return true;
		}

		protected abstract bool Supports (GeneratorSupport supports);

		bool ICodeGenerator.Supports (GeneratorSupport value)
		{
			return Supports (value);
		}

		protected virtual void ValidateIdentifier (string value)
		{
			if (!(IsValidIdentifier (value)))
				throw new ArgumentException ("Identifier is invalid", "value");
		}

		[MonoTODO]
		public static void ValidateIdentifiers (CodeObject e)
		{
			throw new NotImplementedException();
		}

		void ICodeGenerator.ValidateIdentifier (string value)
		{
			ValidateIdentifier (value);
		}

		// The position in the array determines the order in which those
		// kind of CodeTypeMembers are generated. Less is more ;-)
		static Type [] memberTypes = {	typeof (CodeMemberField),
						typeof (CodeSnippetTypeMember),
						typeof (CodeTypeConstructor),
						typeof (CodeConstructor),
						typeof (CodeMemberProperty),
						typeof (CodeMemberEvent),
						typeof (CodeMemberMethod),
						typeof (CodeTypeDeclaration),
						typeof (CodeEntryPointMethod)
					};

#if NET_2_0
		protected virtual void GenerateDirectives (CodeDirectiveCollection directives)
		{
		}
#endif

		internal class Visitor : ICodeDomVisitor {
			CodeGenerator g;

			public Visitor (CodeGenerator generator)
			{
				this.g = generator;
			}

			// CodeExpression
				
			public void Visit (CodeArgumentReferenceExpression o)
			{
				g.GenerateArgumentReferenceExpression (o);
			}
	
			public void Visit (CodeArrayCreateExpression o)
			{
				g.GenerateArrayCreateExpression (o);
			}
	
			public void Visit (CodeArrayIndexerExpression o)
			{
				g.GenerateArrayIndexerExpression (o);
			}
	
			public void Visit (CodeBaseReferenceExpression o)
			{
				g.GenerateBaseReferenceExpression (o);
			}
	
			public void Visit (CodeBinaryOperatorExpression o)
			{
				g.GenerateBinaryOperatorExpression (o);
			}
	
			public void Visit (CodeCastExpression o)
			{
				g.GenerateCastExpression (o);
			}
	
#if NET_2_0
			public void Visit (CodeDefaultValueExpression o)
			{
				g.GenerateDefaultValueExpression (o);
			}
#endif
	
			public void Visit (CodeDelegateCreateExpression o)
			{
				g.GenerateDelegateCreateExpression (o);
			}
	
			public void Visit (CodeDelegateInvokeExpression o)
			{
				g.GenerateDelegateInvokeExpression (o);
			}
	
			public void Visit (CodeDirectionExpression o)
			{
				g.GenerateDirectionExpression (o);
			}
	
			public void Visit (CodeEventReferenceExpression o)
			{
				g.GenerateEventReferenceExpression (o);
			}
	
			public void Visit (CodeFieldReferenceExpression o)
			{
				g.GenerateFieldReferenceExpression (o);
			}
	
			public void Visit (CodeIndexerExpression o)
			{
				g.GenerateIndexerExpression (o);
			}
	
			public void Visit (CodeMethodInvokeExpression o)
			{
				g.GenerateMethodInvokeExpression (o);
			}
	
			public void Visit (CodeMethodReferenceExpression o)
			{
				g.GenerateMethodReferenceExpression (o);
			}
	
			public void Visit (CodeObjectCreateExpression o)
			{
				g.GenerateObjectCreateExpression (o);
			}
	
			public void Visit (CodeParameterDeclarationExpression o)
			{
				g.GenerateParameterDeclarationExpression (o);
			}
	
			public void Visit (CodePrimitiveExpression o)
			{
				g.GeneratePrimitiveExpression (o);
			}
	
			public void Visit (CodePropertyReferenceExpression o)
			{
				g.GeneratePropertyReferenceExpression (o);
			}
	
			public void Visit (CodePropertySetValueReferenceExpression o)
			{
				g.GeneratePropertySetValueReferenceExpression (o);
			}
	
			public void Visit (CodeSnippetExpression o)
			{
				g.GenerateSnippetExpression (o);
			}
	
			public void Visit (CodeThisReferenceExpression o)
			{
				g.GenerateThisReferenceExpression (o);
			}
	
			public void Visit (CodeTypeOfExpression o)
			{
				g.GenerateTypeOfExpression (o);
			}
	
			public void Visit (CodeTypeReferenceExpression o)
			{
				g.GenerateTypeReferenceExpression (o);
			}
			
			public void Visit (CodeVariableReferenceExpression o)
			{
				g.GenerateVariableReferenceExpression (o);
			}
			
			// CodeStatement

			public void Visit (CodeAssignStatement o)
			{
				g.GenerateAssignStatement (o);
			}

			public void Visit (CodeAttachEventStatement o)
			{
				g.GenerateAttachEventStatement (o);
			}

			public void Visit (CodeCommentStatement o)
			{
				g.GenerateCommentStatement (o);
			}

			public void Visit (CodeConditionStatement o)
			{
				g.GenerateConditionStatement (o);
			}

			public void Visit (CodeExpressionStatement o)
			{
				g.GenerateExpressionStatement (o);
			}

			public void Visit (CodeGotoStatement o)
			{
				g.GenerateGotoStatement (o);
			}

			public void Visit (CodeIterationStatement o)
			{
				g.GenerateIterationStatement (o);
			}

			public void Visit (CodeLabeledStatement o)
			{
				g.GenerateLabeledStatement (o);
			}

			public void Visit (CodeMethodReturnStatement o)
			{
				g.GenerateMethodReturnStatement (o);
			}

			public void Visit (CodeRemoveEventStatement o)
			{
				g.GenerateRemoveEventStatement (o);
			}

			public void Visit (CodeThrowExceptionStatement o)
			{
				g.GenerateThrowExceptionStatement (o);
			}

			public void Visit (CodeTryCatchFinallyStatement o)
			{
				g.GenerateTryCatchFinallyStatement (o);
			}

			public void Visit (CodeVariableDeclarationStatement o)
			{
				g.GenerateVariableDeclarationStatement (o);
			}
		
			// CodeTypeMember
			
			public void Visit (CodeConstructor o)
			{
				g.GenerateConstructor (o, g.CurrentClass);
			}
			
			public void Visit (CodeEntryPointMethod o)
			{
				g.GenerateEntryPointMethod (o, g.CurrentClass);
			}
	
			public void Visit (CodeMemberEvent o)
			{
				g.GenerateEvent (o, g.CurrentClass);
			}
	
			public void Visit (CodeMemberField o)
			{
				g.GenerateField (o);
			}
			
			public void Visit (CodeMemberMethod o)
			{
				g.GenerateMethod (o, g.CurrentClass);
			}
	
			public void Visit (CodeMemberProperty o)
			{
				g.GenerateProperty (o, g.CurrentClass);		
			}
	
			public void Visit (CodeSnippetTypeMember o)
			{
				g.GenerateSnippetMember (o);
			}
	
			public void Visit (CodeTypeConstructor o)
			{
				g.GenerateTypeConstructor (o);
			}
		}
	}
}
