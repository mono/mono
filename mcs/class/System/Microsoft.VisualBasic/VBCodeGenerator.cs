//
// Microsoft.VisualBasic.VBCodeGenerator.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   (partially based on CSharpCodeGenerator)
//   Jochen Wezel (jwezel@compumaster.de)
//
// (C) 2003 Andreas Nahr
// (C) 2003 Jochen Wezel (http://www.compumaster.de)
//
// Modifications:
// 2003-11-06 JW: some corrections regarding missing spaces in generated code (e. g. "Property ")
// 2003-11-06 JW: QuoteSnippetString implemented
// 2003-11-08 JW: automatically add Microsoft.VisualBasic
// 2003-11-12 JW: some corrections to allow correct compilation
// 2003-11-28 JW: implementing code differences into current build of this file
// 2003-12-10 JW: added "String." for the ChrW method because mbas doesn't support it without the String currently / TODO: remove it ASAP!

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
using System.Text;
using System.Text.RegularExpressions;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Microsoft.VisualBasic
{
	internal class VBCodeGenerator : CodeGenerator
	{
		private string[] Keywords = new string[] {
			"AddHandler", "AddressOf", "Alias", "And",
			"AndAlso", "Ansi", "As", "Assembly",
			"Auto", "Boolean", "ByRef", "Byte", 
			"ByVal", "Call", "Case", "Catch", 
			"CBool", "CByte", "CChar", "CDate", 
			"CDec", "CDbl", "Char", "CInt", 
			"Class", "CLng", "CObj", "Const", 
			"CShort", "CSng", "CStr", "CType", 
			"Date", "Decimal", "Declare", "Default", 
			"Delegate", "Dim", "DirectCast", "Do", 
			"Double", "Each", "Else", "ElseIf", 
			"End", "Enum", "Erase", "Error", 
			"Event", "Exit", "False", "Finally", 
			"For", "Friend", "Function", "Get", 
			"GetType", "GoSub", "GoTo", "Handles", 
			"If", "Implements", "Imports", "In", 
			"Inherits", "Integer", "Interface", "Is", 
			"Let", "Lib", "Like", "Long", 
			"Loop", "Me", "Mod", "Module", 
			"MustInherit", "MustOverride", "MyBase", "MyClass", 
			"Namespace", "New", "Next", "Not", 
			"Nothing", "NotInheritable", "NotOverridable", "Object", 
			"On", "Option", "Optional", "Or", 
			"OrElse", "Overloads", "Overridable", "Overrides", 
			"ParamArray", "Preserve", "Private", "Property", 
			"Protected", "Public", "RaiseEvent", "ReadOnly", 
			"ReDim", "REM", "RemoveHandler", "Resume", 
			"Return", "Select", "Set", "Shadows", 
			"Shared", "Short", "Single", "Static", 
			"Step", "Stop", "String", "Structure", 
			"Sub", "SyncLock", "Then", "Throw", 
			"To", "True", "Try", "TypeOf", 
			"Unicode", "Until", "Variant", "When", 
			"While", "With", "WithEvents", "WriteOnly", 
			"Xor" 
		};

		public VBCodeGenerator()
		{
		}

		protected override string NullToken {
			get {
				return "Nothing";
			}
		}

		protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression expression)
		{
			TextWriter output = Output;

			output.Write ("New ");

			CodeExpressionCollection initializers = expression.Initializers;
			CodeTypeReference createType = expression.CreateType;

			if (initializers.Count > 0) {

				OutputType (createType);
				
				output.WriteLine (" {");
				++Indent;
				OutputExpressionList (initializers, true);
				--Indent;
				output.Write ("}");

			} 
			else {
				CodeTypeReference arrayType = createType.ArrayElementType;
				while (arrayType != null) 
				{
					createType = arrayType;
					arrayType = arrayType.ArrayElementType;
				}

				OutputType (createType);

				output.Write ('(');

				CodeExpression size = expression.SizeExpression;
				if (size != null)
					GenerateExpression (size);
				else
					output.Write (expression.Size);

				output.Write (')');
			}
		}

		protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression expression)
		{
			Output.Write ("MyBase");
		}

		protected override void GenerateCastExpression (CodeCastExpression expression)
		{
			TextWriter output = Output;
			// ENHANCE: Use a DirectCast if it is known that expression.Expression is no Value-Type
			output.Write ("CType(");
			GenerateExpression (expression.Expression);
			output.Write (", ");
			OutputType (expression.TargetType);
			output.Write (")");
		}

		private bool AsBool(object datavalue)
		{
			return datavalue != null && datavalue is bool && (bool)datavalue;
		}
		
		private string OnOff(bool datavalue)
		{
			return datavalue?"On":"Off";
		}

		protected override void GenerateCompileUnitStart (CodeCompileUnit compileUnit)
		{
			GenerateComment (new CodeComment ("------------------------------------------------------------------------------"));
			GenerateComment (new CodeComment (" <autogenerated>"));
			GenerateComment (new CodeComment ("     This code was generated by a tool."));
			GenerateComment (new CodeComment ("     Mono Runtime Version: " + System.Environment.Version));
			GenerateComment (new CodeComment (""));
			GenerateComment (new CodeComment ("     Changes to this file may cause incorrect behavior and will be lost if "));
			GenerateComment (new CodeComment ("     the code is regenerated."));
			GenerateComment (new CodeComment (" </autogenerated>"));
			GenerateComment (new CodeComment ("------------------------------------------------------------------------------"));
			Output.WriteLine ();
			Output.WriteLine("Option Explicit {0}",OnOff(AsBool(compileUnit.UserData["RequireVariableDeclaration"])));
			Output.WriteLine("Option Strict {0}",OnOff(!AsBool(compileUnit.UserData["AllowLateBound"])));
			Output.WriteLine ();				
		}

		protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression expression)
		{
			TextWriter output = Output;

			output.Write ("AddressOf ");

			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			output.Write (expression.MethodName);
		}

		protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			Output.Write (expression.FieldName);
		}
		
		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression expression)
		{
			Output.Write (expression.ParameterName);
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression expression)
		{
			Output.Write (expression.VariableName);
		}
		
		protected override void GenerateIndexerExpression (CodeIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);
			output.Write ('(');
			OutputExpressionList (expression.Indices);
			output.Write (')');
		}
		
		protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);
			output.Write (".Item(");
			OutputExpressionList (expression.Indices);
			output.Write (')');
		}
		
		protected override void GenerateSnippetExpression (CodeSnippetExpression expression)
		{
			Output.Write (expression.Value);
		}
		
		protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression expression)
		{
			TextWriter output = Output;

			GenerateMethodReferenceExpression (expression.Method);

			output.Write ('(');
			OutputExpressionList (expression.Parameters);
			output.Write (')');
		}

		protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression expression)
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('.');
			Output.Write (expression.MethodName);
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression expression)
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('.');
			Output.Write (expression.EventName);
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression expression)
		{
			Output.Write ("RaiseEvent ");
			GenerateExpression (expression.TargetObject);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.WriteLine (')');
		}
		
		protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression expression)
		{
			Output.Write( "New " );
			OutputType (expression.CreateType);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
		}

		protected override void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			OutputTypeNamePair (e.Type, e.Name);
		}

		protected override void GeneratePrimitiveExpression (CodePrimitiveExpression e)
		{
			TextWriter output = Output;

			if (e.Value == null) {
				output.Write (NullToken);
				return;
			}

			Type type = e.Value.GetType ();
			if (type == typeof (bool)) {
				if ((bool)e.Value)
					output.Write ("True");
				else
					output.Write ("False");
			} 
			else if (type == typeof (char)) {
				output.Write ("\"" + e.Value.ToString () + "\"c");
			} 
			else if (type == typeof (string)) {
				output.Write (QuoteSnippetString ((string) e.Value));
			} 
			else if (type == typeof (byte) || type == typeof (sbyte) || type == typeof (short) ||
				type == typeof (int) || type == typeof (long) || type == typeof (float) ||
				type == typeof (double) || type == typeof (decimal)) {
				output.Write (e.Value.ToString ());
			} 
			else {
				throw new ArgumentException ("Value type (" + type + ") is not a primitive type");
			}
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression expression)
		{
			GenerateMemberReferenceExpression (expression.TargetObject, expression.PropertyName);
		}

		protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression expression)
		{
			Output.Write ("Value");	
		}

		protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression expression)
		{
			Output.Write ("Me");
		}

		protected override void GenerateExpressionStatement (CodeExpressionStatement statement)
		{
			GenerateExpression (statement.Expression);
			Output.WriteLine (); //start new line
		}

		protected override void GenerateIterationStatement (CodeIterationStatement statement)
		{
			TextWriter output = Output;

			GenerateStatement (statement.InitStatement);
			output.Write ("Do While ");
			GenerateExpression (statement.TestExpression);
			output.WriteLine ();
			GenerateStatements (statement.Statements);
			GenerateStatement (statement.IncrementStatement);
			output.WriteLine ("Loop");
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement statement)
		{
			Output.Write ("Throw ");
			GenerateExpression (statement.ToThrow);
		}

		protected override void GenerateComment (CodeComment comment)
		{
			TextWriter output = Output;

			if (comment.DocComment)
				output.Write ("''' ");
			else
				output.Write ("' ");

			output.WriteLine (comment.Text);
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement statement)
		{
			TextWriter output = Output;

			output.Write ("Return ");
			GenerateExpression (statement.Expression);
			output.WriteLine ();
		}

		protected override void GenerateConditionStatement (CodeConditionStatement statement)
		{
			TextWriter output = Output;
			output.Write ("If (");

			GenerateExpression (statement.Condition);

			output.WriteLine (") Then");
			++Indent;
			GenerateStatements (statement.TrueStatements);
			--Indent;

			CodeStatementCollection falses = statement.FalseStatements;
			if (falses.Count > 0) {
				output.WriteLine ("Else");
				++Indent;
				GenerateStatements (falses);
				--Indent;
			}
			else {
				if (Options.ElseOnClosing)
					output.WriteLine ("Else");
			}
			output.WriteLine ("End If");
		}

		protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement statement)
		{
			TextWriter output = Output;

			output.WriteLine ("Try");
			++Indent;
			GenerateStatements (statement.TryStatements);
			--Indent;
			output.WriteLine ();
			
			foreach (CodeCatchClause clause in statement.CatchClauses) {
				output.Write ("Catch ");
				OutputTypeNamePair (clause.CatchExceptionType, clause.LocalName);
				output.WriteLine ();
				++Indent;
				GenerateStatements (clause.Statements);
				--Indent;
				output.WriteLine ();
			}

			CodeStatementCollection finallies = statement.FinallyStatements;
			if (finallies.Count > 0) {

				output.WriteLine ("Finally");
				++Indent;
				GenerateStatements (finallies);
				--Indent;
				output.WriteLine ();
			}

			if (Options.ElseOnClosing) {
				if (statement.CatchClauses.Count == 0)
					output.WriteLine ("Catch");
				if (statement.FinallyStatements.Count == 0)
					output.WriteLine ("Finally");
			}

			output.WriteLine("End Try");
		}

		protected override void GenerateAssignStatement (CodeAssignStatement statement)
		{			
			TextWriter output = Output;
			GenerateExpression (statement.Left);
			output.Write (" = ");
			GenerateExpression (statement.Right);
			output.WriteLine ();
		}

		protected override void GenerateAttachEventStatement (CodeAttachEventStatement statement)
		{
			TextWriter output = Output;

			Output.Write ("AddHandler ");
			GenerateEventReferenceExpression (statement.Event);
			Output.Write ( ", ");
			GenerateExpression (statement.Listener);
			output.WriteLine ();
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement statement)
		{
			TextWriter output = Output;

			Output.Write ("RemoveHandler ");
			GenerateEventReferenceExpression (statement.Event);
			Output.Write ( ", ");
			GenerateExpression (statement.Listener);
			output.WriteLine ();
		}

		protected override void GenerateGotoStatement (CodeGotoStatement statement)
		{
			TextWriter output = Output;

			output.Write ("Goto ");
			output.Write (statement.Label);
			output.WriteLine ();
		}
		
		protected override void GenerateLabeledStatement (CodeLabeledStatement statement)
		{
			TextWriter output = Output;

			output.Write (statement.Label + ":");
			GenerateStatement (statement.Statement);
		}

		protected override void GenerateTypeOfExpression (CodeTypeOfExpression e)
		{
			TextWriter output = Output;

			output.Write ("GetType(");
			OutputType (e.Type);
			output.Write (")");
		}

		protected override void GenerateVariableDeclarationStatement( CodeVariableDeclarationStatement statement )
		{
			TextWriter output = Output;

			output.Write ("Dim ");
			OutputTypeNamePair (statement.Type, statement.Name);

			CodeExpression initExpression = statement.InitExpression;
			if (initExpression != null) 
			{
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine();
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma linePragma)
		{
			Output.WriteLine ();
			Output.Write ("#ExternalSource(");
			Output.Write (linePragma.FileName);
			Output.Write (", ");
			Output.Write (linePragma.LineNumber);
			Output.WriteLine (")");
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma linePragma)
		{
			Output.WriteLine ("#End ExternalSource");
		}

		protected override void GenerateEvent (CodeMemberEvent eventRef, CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			if (eventRef.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (eventRef.CustomAttributes);

			MemberAttributes attributes = eventRef.Attributes;

			OutputMemberAccessModifier (attributes);
			OutputMemberScopeModifier (attributes);

			output.Write ("Event ");
			OutputTypeNamePair (eventRef.Type, eventRef.Name);
			output.WriteLine ();
		}

		protected override void GenerateField (CodeMemberField field)
		{
			TextWriter output = Output;

			if (field.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (field.CustomAttributes);

			MemberAttributes attributes = field.Attributes;
			OutputMemberAccessModifier (attributes);
			OutputFieldScopeModifier (attributes);

			OutputTypeNamePair (field.Type, field.Name);

			CodeExpression initExpression = field.InitExpression;
			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine();
		}
		
		protected override void GenerateSnippetMember (CodeSnippetTypeMember member)
		{
			Output.Write (member.Text);
		}
		
		protected override void GenerateEntryPointMethod( CodeEntryPointMethod method, CodeTypeDeclaration declaration )
		{
			method.Name = "Main";
			GenerateMethod (method, declaration);
		}
		
		[MonoTODO ("partially implemented")]
		protected override void GenerateMethod (CodeMemberMethod method, CodeTypeDeclaration declaration)
		{
			bool isSub = method.ReturnType == null || method.ReturnType.BaseType == "System.Void";

			TextWriter output = Output;

			if (method.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (method.CustomAttributes);

			MemberAttributes attributes = method.Attributes;

			OutputMemberAccessModifier (attributes);
			OutputMemberScopeModifier (attributes);

			if (isSub)
				output.Write ("Sub ");
			else
				output.Write ("Function ");

			output.Write (method.Name);
			output.Write ('(');
			OutputParameters (method.Parameters);
			output.Write (')');

			if (!isSub) {
				output.Write (" As ");
				OutputType (method.ReturnType);
			}

			if (method.ImplementationTypes.Count > 0) {
				output.Write (" Implements ");
				foreach (CodeTypeReference type in method.ImplementationTypes)
				{
					OutputType (type);
					output.Write ('.');
					// TODO implementation incomplete

				}
			}

			// TODO private implementations

			if ((attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract)
				output.WriteLine ();
			else {
				output.WriteLine ();
				++Indent;
				GenerateStatements (method.Statements);
				--Indent;
				if (isSub)
					output.WriteLine ("End Sub");
				else
					output.WriteLine ("End Function");
			}
		}

		protected override void GenerateProperty (CodeMemberProperty property, CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			if (property.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (property.CustomAttributes);

			MemberAttributes attributes = property.Attributes;
			OutputMemberAccessModifier (attributes);
			OutputMemberScopeModifier (attributes);

			if (property.HasGet && (!property.HasSet))
				output.Write ("ReadOnly " );

			if (property.HasSet && (!property.HasGet))
				output.Write ("WriteOnly " );

			output.Write ("Property " );
			
			OutputTypeNamePair (property.Type, property.Name);
			output.WriteLine ();
			++Indent;

			if (property.HasGet) {
				output.WriteLine ("Get");
				++Indent;

				GenerateStatements (property.GetStatements);

				--Indent;
				output.WriteLine ("End Get");
			}
			
			if (property.HasSet) {
				output.Write ("Set (");
				OutputTypeNamePair (property.Type, "Value");
				output.WriteLine (")");
				++Indent;

				GenerateStatements (property.SetStatements);

				--Indent;
				output.WriteLine ("End Set");
			}

			--Indent;
			output.WriteLine ("End Property");
		}

		[MonoTODO ("not implemented")]
		protected override void GenerateConstructor (CodeConstructor constructor, CodeTypeDeclaration declaration)
		{
			if (constructor.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (constructor.CustomAttributes);
			OutputMemberAccessModifier (constructor.Attributes);
			Output.Write ("Sub New(");
			OutputParameters (constructor.Parameters);
			Output.WriteLine (")");
			// Handle BaseConstructorArgs, ChainedConstructorArgs, ImplementationTypes
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ("End Sub");
		}
		
		protected override void GenerateTypeConstructor (CodeTypeConstructor constructor)
		{
			Output.WriteLine ("Shared Sub New()");
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ("End Sub");
		}

		[MonoTODO ("not implemented")]
		protected override void GenerateTypeStart (CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			if (declaration.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (declaration.CustomAttributes);
			TypeAttributes attributes = declaration.TypeAttributes;
			OutputTypeAttributes (attributes,
				declaration.IsStruct,
				declaration.IsEnum);

			output.WriteLine (declaration.Name);

			++Indent;
			
			IEnumerator enumerator = declaration.BaseTypes.GetEnumerator();
			if (enumerator.MoveNext()) 
			{
				CodeTypeReference type = (CodeTypeReference)enumerator.Current;
			
				if (type != null)
				{
					output.Write ("Inherits ");
					OutputType (type);
					output.WriteLine ();
				}
				
				while (enumerator.MoveNext()) 
				{
					type = (CodeTypeReference)enumerator.Current;
				
					if (type != null)
					{
						output.Write ("Implements ");
						OutputType (type);
						output.WriteLine ();
					}
				}
			}			
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration declaration)
		{
			string output = string.Empty;

			--Indent;
			if (declaration.IsStruct)
				output = "End Structure";
			if (declaration.IsInterface)
				output = "End Interface";
			if (declaration.IsEnum)
				output = "End Enum";
			if (declaration.IsClass)
				output = "End Class";

			Output.WriteLine (output);
		}

		protected override void GenerateNamespace(CodeNamespace ns)
		{
			GenerateCommentStatements (ns.Comments);
			
			bool Imports2MSVBFound;
			Imports2MSVBFound = false;
			if (null != ns.Imports) 
			{
				foreach (CodeNamespaceImport import in ns.Imports)
				{
					if (string.Compare (import.Namespace, "Microsoft.VisualBasic", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
						Imports2MSVBFound = true;
				}
			}
			// add standard import to Microsoft.VisualBasic if missing
			if (Imports2MSVBFound == false)
				Output.WriteLine ("Imports Microsoft.VisualBasic");
			// add regular imports
			GenerateNamespaceImports (ns);

			TextWriter output = Output;
			output.WriteLine(); 
			GenerateNamespaceStart (ns); 
			GenerateTypes (ns);
			GenerateNamespaceEnd (ns);
		}


		protected override void GenerateNamespaceStart (CodeNamespace ns)
		{
			TextWriter output = Output;
			
			string name = ns.Name;
			if (name != null && name != string.Empty) {
				output.Write ("Namespace ");
				output.WriteLine (name);
				++Indent;
			}
		}

		protected override void GenerateNamespaceEnd (CodeNamespace ns)
		{
			string name = ns.Name;
			if (name != null && name != string.Empty) {
				--Indent;
				Output.WriteLine ("End Namespace");
			}
		}

		protected override void GenerateNamespaceImport (CodeNamespaceImport import)
		{
			TextWriter output = Output;

			output.Write ("Imports ");
			output.Write (import.Namespace);
			output.WriteLine ();
		}
		
		protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes)
		{
			Output.Write ('<');
		}
		
		protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes)
		{
			Output.WriteLine ("> _");
		}

		protected override void OutputDirection (FieldDirection direction)
		{
			switch (direction) {
			case FieldDirection.In:
				//there is no "In"
				break;
			case FieldDirection.Out:
				Output.Write ("ByVal ");
				break;
			case FieldDirection.Ref:
				Output.Write ("ByRef ");
				break;
			}
		}

		protected override void OutputFieldScopeModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				Output.Write ("New ");

			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Static:
				Output.Write ("Shared ");
				break;
			case MemberAttributes.Const:
				Output.Write ("Const ");
				break;
			}
		}

		protected override void OutputMemberAccessModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.AccessMask) {
			case MemberAttributes.Assembly:
				Output.Write ("Friend ");
				break;
			case MemberAttributes.FamilyAndAssembly:
				Output.Write ("Friend "); 
				break;
			case MemberAttributes.Family:
				Output.Write ("Protected ");
				break;
			case MemberAttributes.FamilyOrAssembly:
				Output.Write ("Protected Friend ");
				break;
			case MemberAttributes.Private:
				Output.Write ("Private ");
				break;
			case MemberAttributes.Public:
				Output.Write ("Public ");
				break;
			}
		}

		protected override void OutputMemberScopeModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				Output.Write ("New ");

			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Abstract:
				Output.Write ("MustOverride ");
				break;
			case MemberAttributes.Final:
				//JW 2004-06-03: seems to be the "sealed" keyword in C# and the "NotOverridable" keyword in VB, but conflicts with ASP.NET generation
				//Output.Write ("NotOverridable ");
				break;
			case MemberAttributes.Static:
				Output.Write ("Shared ");
				break;
			case MemberAttributes.Override:
				Output.Write ("Overrides ");
				break;
			case MemberAttributes.Overloaded:
				// based on http://gendotnet.com/Code%20Gen%20Articles/codedom.htm
				Output.Write ("Overloads ");
                                MemberAttributes access_ovl = attributes & MemberAttributes.AccessMask;
                                if ( access_ovl == MemberAttributes.Public || 
                                        access_ovl == MemberAttributes.Family )
                                        Output.Write ("Overridable ");
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
				MemberAttributes access = attributes & MemberAttributes.AccessMask;
				if ( access == MemberAttributes.Public || 
					access == MemberAttributes.Family )
					Output.Write ("Overridable ");
				break;
			}
		}

		protected override void OutputOperator (CodeBinaryOperatorType op)
		{
			switch (op) {
			case CodeBinaryOperatorType.Add:
				Output.Write ("+");
				break;
			case CodeBinaryOperatorType.Subtract:
				Output.Write ("-");
				break;
			case CodeBinaryOperatorType.Multiply:
				Output.Write ("*");
				break;
			case CodeBinaryOperatorType.Divide:
				Output.Write ("/");
				break;
			case CodeBinaryOperatorType.Modulus:
				Output.Write ("Mod");
				break;
			case CodeBinaryOperatorType.Assign:
				Output.Write ("=");
				break;
			case CodeBinaryOperatorType.IdentityInequality:
				Output.Write ("<>");
				break;
			case CodeBinaryOperatorType.IdentityEquality:
				Output.Write ("Is");
				break;
			case CodeBinaryOperatorType.ValueEquality:
				Output.Write ("=");
				break;
			case CodeBinaryOperatorType.BitwiseOr:
				Output.Write ("Or");
				break;
			case CodeBinaryOperatorType.BitwiseAnd:
				Output.Write ("And");
				break;
			case CodeBinaryOperatorType.BooleanOr:
				Output.Write ("OrElse");
				break;
			case CodeBinaryOperatorType.BooleanAnd:
				Output.Write ("AndAlso");
				break;
			case CodeBinaryOperatorType.LessThan:
				Output.Write ("<");
				break;
			case CodeBinaryOperatorType.LessThanOrEqual:
				Output.Write ("<=");
				break;
			case CodeBinaryOperatorType.GreaterThan:
				Output.Write (">");
				break;
			case CodeBinaryOperatorType.GreaterThanOrEqual:
				Output.Write (">=");
				break;
			}
		}

		protected override void OutputTypeAttributes (TypeAttributes attributes, bool isStruct, bool isEnum)
		{
			TextWriter output = Output;

			switch (attributes & TypeAttributes.VisibilityMask) {
			case TypeAttributes.NotPublic:
				// Does this mean friend access?
				output.Write ("Friend ");
				break; 

			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				output.Write ("Public ");
				break;

			case TypeAttributes.NestedPrivate:
				output.Write ("Private ");
				break;
			case TypeAttributes.NestedAssembly:
				output.Write ("Friend ");
				break;
			case TypeAttributes.NestedFamily:
				output.Write ("Protected ");
				break;
			case TypeAttributes.NestedFamORAssem:
				output.Write ("Protected Friend ");
				break;
			case TypeAttributes.NestedFamANDAssem:
				output.Write ("Friend ");
				break;
			}

			if (isStruct)
				output.Write ("Structure ");

			else if (isEnum)
				output.Write ("Enumeration ");

			else {
				if ((attributes & TypeAttributes.Interface) != 0) 
					output.Write ("Interface ");

				else {
					if ((attributes & TypeAttributes.Sealed) != 0)
						output.Write ("NotInheritable ");

					if ((attributes & TypeAttributes.Abstract) != 0)
						output.Write ("MustInherit ");
					
					output.Write ("Class ");
				}
			}
		}

		protected override void OutputTypeNamePair (CodeTypeReference typeRef, String name)
		{
			Output.Write (name + " As " + GetTypeOutput (typeRef));
		}

		protected override void OutputType (CodeTypeReference type)
		{
			Output.Write (GetTypeOutput (type));
		}

		protected override string QuoteSnippetString (string value)
		{
			StringBuilder mySBuilder = new StringBuilder(value.Length);
			mySBuilder.Append ("\"");
			bool inQuotes = true;
			for (int MyCounter = 0; MyCounter < value.Length; MyCounter++)
			{
				if (value[MyCounter] == 34) //quotation mark
				{
					if (!inQuotes)
					{
						mySBuilder.Append ("&\"");
						inQuotes = true;
					}
					mySBuilder.Append (value[MyCounter]);
					mySBuilder.Append (value[MyCounter]);
				}
				else if (value[MyCounter] >= 32) //standard ansi/unicode characters
				{
					if (!inQuotes)
					{
						mySBuilder.Append ("&\"");
						inQuotes = true;
					}
					mySBuilder.Append (value[MyCounter]);
				}
				else //special chars, e.g. line break
				{
					if (inQuotes)
					{ 
						mySBuilder.Append ("\"");
						inQuotes = false;
					}
					mySBuilder.Append ("&Microsoft.VisualBasic.ChrW(");
					mySBuilder.Append ((int)value[MyCounter]); 
					mySBuilder.Append (")");
				}			
			}
			if (inQuotes)
				mySBuilder.Append ("\"");
			return mySBuilder.ToString();
		}

		private void GenerateDeclaration (CodeTypeReference type, string name, CodeExpression initExpression)
		{
			TextWriter output = Output;

			OutputTypeNamePair (type, name);

			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine ();
		}
		
		private void GenerateMemberReferenceExpression (CodeExpression targetObject, string memberName)
		{
			GenerateExpression (targetObject);
			Output.Write ('.');
			Output.Write (memberName);
		}
			
		/* 
		 * ICodeGenerator
		 */

		protected override string CreateEscapedIdentifier (string value)
		{
			for (int x = 0; x < Keywords.Length; x++)
				if (value.ToLower().Equals (Keywords[x].ToLower()))
					return "[" + value + "]";
			return value;
		}

		protected override string CreateValidIdentifier (string value)
		{
			for (int x = 0; x < Keywords.Length; x++)
				if (value.ToLower().Equals (Keywords[x].ToLower()))
					return "_" + value;
			return value;
		}

		protected override string GetTypeOutput (CodeTypeReference type)
		{
			string output;
			CodeTypeReference arrayType;

			arrayType = type.ArrayElementType;
			if (arrayType != null)
				output = GetTypeOutput (arrayType);
			else { 
				switch (type.BaseType) {

				case "System.Decimal":
					output = "Decimal";
					break;
				case "System.Double":
					output = "Double";
					break;
				case "System.Single":
					output = "Single";
					break;
				
				case "System.Byte":
					output = "Byte";
					break;
				case "System.SByte":
					output = "SByte";
					break;
				case "System.Int32":
					output = "Integer";
					break;
				case "System.UInt32":
					output = "UInt32";
					break;
				case "System.Int64":
					output = "Long";
					break;
				case "System.UInt64":
					output = "UInt64";
					break;
				case "System.Int16":
					output = "Short";
					break;
				case "System.UInt16":
					output = "UInt16";
					break;

				case "System.Boolean":
					output = "Boolean";
					break;
				
				case "System.Char":
					output = "Char";
					break;

				case "System.String":
					output = "String";
					break;
				case "System.Object":
					output = "Object";
					break;

				case "System.Void":
					output = "Nothing";
					break;

				default:
					output = type.BaseType;
					break;
				}
			}

			int rank = type.ArrayRank;
			if (rank > 0) {
				output += "(";
				for (--rank; rank > 0; --rank)
					output += ",";
				output += ")";
			}

			return output;
		}

		protected override bool IsValidIdentifier (string identifier)
		{
			for (int x = 0; x < Keywords.Length; x++)
				if (identifier.ToLower().Equals (Keywords[x].ToLower()))
					return false;
			return true;
		}

		protected override bool Supports (GeneratorSupport supports)
		{
			return true;
		}
	}
}
