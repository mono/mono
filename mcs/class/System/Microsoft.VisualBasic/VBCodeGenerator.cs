//
// Microsoft.VisualBasic.VBCodeGenerator.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   (partially based on CSharpCodeGenerator)
//   Jochen Wezel (jwezel@compumaster.de)
//   Frederik Carlier (frederik.carlier@carlier-online.be)
//   Rolf Bjarne Kvinge (RKvinge@novell.com)
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
// 2007-04-13 FC: Added support for the IdentityInequality operator when comparing against Nothing

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
using System.Globalization;
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
		private string [] Keywords = new string [] {
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
			"GetType", "Global", "GoSub", "GoTo", "Handles", 
			"If", "Implements", "Imports", "In", 
			"Inherits", "Integer", "Interface", "Is", 
			"Let", "Lib", "Like", "Long", 
			"Loop", "Me", "Mod", "Module", 
			"MustInherit", "MustOverride", "MyBase", "MyClass", 
			"Namespace", "New", "Next", "Not", 
			"Nothing", "NotInheritable", "NotOverridable", "Object", 
			"On", "Option", "Optional", "Or", 
			"OrElse", "Overloads", "Overridable", "Overrides", 
			"ParamArray", "Partial", "Preserve", "Private", "Property", 
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

		protected override void ContinueOnNewLine (string st)
		{
			Output.Write (st);
			Output.WriteLine (" _");
		}

		protected override void GenerateBinaryOperatorExpression (CodeBinaryOperatorExpression e)
		{
			// We need to special case for comparisons against null;
			// in Visual Basic the "Not (Expr) Is Nothing" construct is used
			
			bool null_comparison = false;
			bool reverse = false;
			if (e.Operator == CodeBinaryOperatorType.IdentityInequality) {
				CodePrimitiveExpression nothing;
				nothing = e.Left as CodePrimitiveExpression;
				if (nothing == null) {
					nothing = e.Right as CodePrimitiveExpression;
				} else {
					reverse = true;
				}
				null_comparison = nothing != null && nothing.Value == null;
			}

			if (null_comparison) {
				TextWriter output = Output;

				output.Write ("(Not (");
				GenerateExpression (reverse ? e.Right : e.Left);
				output.Write (") Is ");
				GenerateExpression (reverse ? e.Left : e.Right);
				output.Write (')');
			} else {
				base.GenerateBinaryOperatorExpression (e);
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
				
				output.Write ("() {");
				++Indent;
				OutputExpressionList (initializers);
				--Indent;
				output.Write ("}");
			} else {
				CodeTypeReference arrayType = createType.ArrayElementType;
				while (arrayType != null) {
					createType = arrayType;
					arrayType = arrayType.ArrayElementType;
				}

				OutputType (createType);

				output.Write ("((");

				CodeExpression size = expression.SizeExpression;
				if (size != null)
					GenerateExpression (size);
				else
					output.Write (expression.Size);

				output.Write (") - 1) {}");
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

		private bool AsBool (object datavalue)
		{
			return datavalue != null && datavalue is bool && (bool) datavalue;
		}
		
		private string OnOff (bool datavalue)
		{
			return datavalue ? "On" : "Off";
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
			if (AsBool (compileUnit.UserData ["AllowLateBound"])) {
				Output.WriteLine("Option Explicit {0}", OnOff (AsBool (compileUnit.UserData ["RequireVariableDeclaration"])));
				Output.WriteLine("Option Strict Off");
			} else {
				Output.WriteLine("Option Explicit On"); // Strict On implies Explicit On
				Output.WriteLine("Option Strict On");
			}
			Output.WriteLine ();
		}

		protected override void GenerateCompileUnit (CodeCompileUnit compileUnit)
		{
			GenerateCompileUnitStart (compileUnit);

			OutputAttributes (compileUnit.AssemblyCustomAttributes,
				"Assembly: ", LineHandling.NewLine);

			GenerateNamespaces (compileUnit);

			GenerateCompileUnitEnd (compileUnit);
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
			Output.Write (CreateEscapedIdentifier (expression.FieldName));
		}
		
		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression expression)
		{
			Output.Write (CreateEscapedIdentifier (expression.ParameterName));
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression expression)
		{
			Output.Write (CreateEscapedIdentifier (expression.VariableName));
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
			output.Write ("(");
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
			if (expression.TargetObject != null) {
				GenerateExpression (expression.TargetObject);
				Output.Write ('.');
			}
			Output.Write (CreateEscapedIdentifier (expression.MethodName));
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression expression)
		{
			if (expression.TargetObject != null) {
				GenerateExpression (expression.TargetObject);
				Output.Write ('.');
				if (expression.TargetObject is CodeThisReferenceExpression) {
					// We're actually creating a reference to a compiler-generated field here...
					Output.Write (expression.EventName + "Event");
				} else {
					Output.Write (CreateEscapedIdentifier (expression.EventName));
				}
			} else {
				Output.Write (CreateEscapedIdentifier (expression.EventName + "Event"));
			}
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression expression)
		{
			CodeEventReferenceExpression ev = expression.TargetObject as CodeEventReferenceExpression;
			
			if (ev != null) {
				Output.Write ("RaiseEvent ");
				if (ev.TargetObject != null && !(ev.TargetObject is CodeThisReferenceExpression)) {
					GenerateExpression (ev.TargetObject);
					Output.Write (".");
				}
				Output.Write (ev.EventName);
			} else if (expression.TargetObject != null) {
				GenerateExpression (expression.TargetObject);
			}
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
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
			OutputAttributes (e.CustomAttributes, null, LineHandling.InLine);
			OutputDirection (e.Direction);
			OutputTypeNamePair (e.Type, e.Name);
		}

		protected override void GeneratePrimitiveExpression (CodePrimitiveExpression e)
		{
			if (e.Value is char) {
				char c = (char) e.Value;
				int ch = (int) c;
				Output.Write("Global.Microsoft.VisualBasic.ChrW(" + ch.ToString(CultureInfo.InvariantCulture) + ")");
			} else if (e.Value is ushort) {
				ushort uc = (ushort) e.Value;
				Output.Write (uc.ToString(CultureInfo.InvariantCulture));
				Output.Write ("US");
			} else if (e.Value is uint) {
				uint ui = (uint) e.Value;
				Output.Write (ui.ToString(CultureInfo.InvariantCulture));
				Output.Write ("UI");
			} else if (e.Value is ulong) {
				ulong ul = (ulong) e.Value;
				Output.Write (ul.ToString(CultureInfo.InvariantCulture));
				Output.Write ("UL");
			} else if (e.Value is sbyte) {
				sbyte sb = (sbyte) e.Value;
				Output.Write ("CSByte(");
				Output.Write (sb.ToString(CultureInfo.InvariantCulture));
				Output.Write (')');
			} else {
				base.GeneratePrimitiveExpression(e);
			}
		}

		protected override void GenerateSingleFloatValue (float s)
		{
			base.GenerateSingleFloatValue (s);
			base.Output.Write ('!');
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression expression)
		{
			if (expression.TargetObject != null) {
				GenerateMemberReferenceExpression (expression.TargetObject, expression.PropertyName);
			} else {
				Output.Write (CreateEscapedIdentifier (expression.PropertyName));
			}
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
			Indent++;
			GenerateStatements (statement.Statements);
			GenerateStatement (statement.IncrementStatement);
			Indent--;
			output.WriteLine ("Loop");
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement statement)
		{
			Output.Write ("Throw");
			if (statement.ToThrow != null) {
				Output.Write (' ');
				GenerateExpression (statement.ToThrow);
			}
			Output.WriteLine ();
		}

		protected override void GenerateComment (CodeComment comment)
		{
			TextWriter output = Output;
			string commentChars = null;

			if (comment.DocComment) {
				commentChars = "'''";
			} else {
				commentChars = "'";
			}
	
			output.Write (commentChars);
			string text = comment.Text;

			for (int i = 0; i < text.Length; i++) {
				output.Write (text [i]);
				if (text[i] == '\r') {
					if (i < (text.Length - 1) && text [i + 1] == '\n') {
						continue;
					}
					output.Write (commentChars);
				} else if (text [i] == '\n') {
					output.Write (commentChars);
				}
			}

			output.WriteLine ();
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement statement)
		{
			TextWriter output = Output;

			if (statement.Expression != null) {
				output.Write ("Return ");
				GenerateExpression (statement.Expression);
				output.WriteLine ();
			} else {
				output.WriteLine ("Return");
			}
		}

		protected override void GenerateConditionStatement (CodeConditionStatement statement)
		{
			TextWriter output = Output;
			output.Write ("If ");

			GenerateExpression (statement.Condition);

			output.WriteLine (" Then");
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

			output.WriteLine ("Try ");
			++Indent;
			GenerateStatements (statement.TryStatements);
			--Indent;
			
			foreach (CodeCatchClause clause in statement.CatchClauses) {
				output.Write ("Catch ");
				OutputTypeNamePair (clause.CatchExceptionType, clause.LocalName);
				output.WriteLine ();
				++Indent;
				GenerateStatements (clause.Statements);
				--Indent;
			}

			CodeStatementCollection finallies = statement.FinallyStatements;
			if (finallies.Count > 0) {
				output.WriteLine ("Finally");
				++Indent;
				GenerateStatements (finallies);
				--Indent;
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
			if (statement.Event.TargetObject != null) {
				GenerateEventReferenceExpression (statement.Event);
			} else {
				Output.Write (CreateEscapedIdentifier (statement.Event.EventName));
			}
			Output.Write ( ", ");
			GenerateExpression (statement.Listener);
			output.WriteLine ();
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement statement)
		{
			TextWriter output = Output;

			Output.Write ("RemoveHandler ");
			if (statement.Event.TargetObject != null) {
				GenerateEventReferenceExpression (statement.Event);
			} else {
				Output.Write (CreateEscapedIdentifier (statement.Event.EventName));
			}
			Output.Write ( ", ");
			GenerateExpression (statement.Listener);
			output.WriteLine ();
		}

		protected override void GenerateGotoStatement (CodeGotoStatement statement)
		{
			TextWriter output = Output;

			output.Write ("goto ");
			output.Write (statement.Label);
			output.WriteLine ();
		}
		
		protected override void GenerateLabeledStatement (CodeLabeledStatement statement)
		{
			TextWriter output = Output;

			Indent--;
			output.WriteLine (statement.Label + ":");
			Indent++;
			if (statement.Statement != null) {
				GenerateStatement (statement.Statement);
			}
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
			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine();
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma linePragma)
		{
			Output.WriteLine ();
			Output.Write ("#ExternalSource(\"");
			Output.Write (linePragma.FileName);
			Output.Write ("\",");
			Output.Write (linePragma.LineNumber);
			Output.WriteLine (")");
			Output.WriteLine ("");
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma linePragma)
		{
			Output.WriteLine ("#End ExternalSource");
		}

		protected override void GenerateEvent (CodeMemberEvent eventRef, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum)
				return;

			TextWriter output = Output;

			OutputAttributes (eventRef.CustomAttributes, null,
				LineHandling.ContinueLine);

			OutputMemberAccessModifier (eventRef.Attributes);

			output.Write ("Event ");
			OutputTypeNamePair (eventRef.Type, GetEventName(eventRef));

			if (eventRef.ImplementationTypes.Count > 0) {
				OutputImplementationTypes (eventRef.ImplementationTypes, eventRef.Name);
			} else if (eventRef.PrivateImplementationType != null) {
				output.Write (" Implements ");
				OutputType (eventRef.PrivateImplementationType);
				output.Write ('.');
				output.Write (eventRef.Name);
			}

			output.WriteLine ();
		}

		protected override void GenerateField (CodeMemberField field)
		{
			if (IsCurrentDelegate || IsCurrentInterface)
				return;

			TextWriter output = Output;

			OutputAttributes (field.CustomAttributes, null, 
				LineHandling.ContinueLine);

			if (IsCurrentEnum) {
				output.Write (field.Name);
			} else {
				MemberAttributes attributes = field.Attributes;
				OutputMemberAccessModifier (attributes);
				OutputVTableModifier (attributes);
				OutputFieldScopeModifier (attributes);
				OutputTypeNamePair (field.Type, field.Name);
			}

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
		
		protected override void GenerateEntryPointMethod (CodeEntryPointMethod method, CodeTypeDeclaration declaration)
		{
			OutputAttributes (method.CustomAttributes, null,
				LineHandling.ContinueLine);

			Output.WriteLine ("Public Shared Sub Main()");
			Indent++;
			GenerateStatements (method.Statements);
			Indent--;
			Output.WriteLine ("End Sub");
		}
		
		[MonoTODO ("partially implemented")]
		protected override void GenerateMethod (CodeMemberMethod method, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum)
				return;

			bool isSub = method.ReturnType.BaseType == typeof(void).FullName;

			TextWriter output = Output;

			OutputAttributes (method.CustomAttributes, null, 
				LineHandling.ContinueLine);

			MemberAttributes attributes = method.Attributes;

			if (!IsCurrentInterface) {
				if (method.PrivateImplementationType == null) {
					OutputMemberAccessModifier (attributes);
					if (IsOverloaded (method, declaration)) {
						output.Write ("Overloads ");
					}
				}
				OutputVTableModifier (attributes);
				OutputMemberScopeModifier (attributes);
			} else {
				OutputVTableModifier (attributes);
			}

			if (isSub)
				output.Write ("Sub ");
			else
				output.Write ("Function ");

			output.Write (GetMethodName(method));
			OutputTypeParameters (method.TypeParameters);
			output.Write ('(');
			OutputParameters (method.Parameters);
			output.Write (')');

			if (!isSub) {
				output.Write (" As ");
				OutputAttributes (method.ReturnTypeCustomAttributes, null,
					LineHandling.InLine);
				OutputType (method.ReturnType);
			}

			if (method.ImplementationTypes.Count > 0) {
				OutputImplementationTypes (method.ImplementationTypes, method.Name);
			} else if (method.PrivateImplementationType != null) {
				output.Write (" Implements ");
				OutputType (method.PrivateImplementationType);
				output.Write ('.');
				output.Write (method.Name);
			}

			output.WriteLine ();
			if (!IsCurrentInterface) {
				if ((attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract) {
					++Indent;
					GenerateStatements (method.Statements);
					--Indent;
					if (isSub)
						output.WriteLine ("End Sub");
					else
						output.WriteLine ("End Function");
				}
			}
		}

		protected override void GenerateProperty (CodeMemberProperty property, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum)
				return;

			TextWriter output = Output;

			OutputAttributes (property.CustomAttributes, null, 
				LineHandling.ContinueLine);

			MemberAttributes attributes = property.Attributes;

			if (!IsCurrentInterface) {
				if (property.PrivateImplementationType == null) {
					OutputMemberAccessModifier (attributes);
					if (IsOverloaded (property, declaration)) {
						output.Write ("Overloads ");
					}
				}
				OutputVTableModifier (attributes);
				OutputMemberScopeModifier (attributes);
			} else {
				OutputVTableModifier (attributes);
			}

			// mark property as default property if we're dealing with an indexer
			if (string.Compare (GetPropertyName(property), "Item", true, CultureInfo.InvariantCulture) == 0 && property.Parameters.Count > 0) {
				output.Write ("Default ");
			}

			if (property.HasGet && (!property.HasSet))
				output.Write ("ReadOnly " );

			if (property.HasSet && (!property.HasGet))
				output.Write ("WriteOnly " );

			output.Write ("Property ");
			Output.Write (GetPropertyName (property));
			// in .NET 2.0, always output parantheses (whether or not there 
			// are any parameters to output
			Output.Write ('(');
			OutputParameters (property.Parameters);
			Output.Write (')');
			Output.Write (" As ");
			Output.Write (GetTypeOutput(property.Type));

			if (property.ImplementationTypes.Count > 0) {
				OutputImplementationTypes (property.ImplementationTypes, property.Name);
			} else if (property.PrivateImplementationType != null) {
				output.Write (" Implements ");
				OutputType (property.PrivateImplementationType);
				output.Write ('.');
				output.Write (property.Name);
			}

			output.WriteLine ();

			if (!IsCurrentInterface) {
				++Indent;
				if (property.HasGet) {
					output.WriteLine ("Get");
					if (!IsAbstract (property.Attributes)) {
						++Indent;
						GenerateStatements (property.GetStatements);
						--Indent;
						output.WriteLine ("End Get");
					}
				}

				if (property.HasSet) {
					output.WriteLine ("Set");
					if (!IsAbstract (property.Attributes)) {
						++Indent;
						GenerateStatements (property.SetStatements);
						--Indent;
						output.WriteLine ("End Set");
					}
				}

				--Indent;
				output.WriteLine ("End Property");
			}
		}

		protected override void GenerateConstructor (CodeConstructor constructor, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum || IsCurrentInterface)
				return;

			OutputAttributes (constructor.CustomAttributes, null,
				LineHandling.ContinueLine);
			OutputMemberAccessModifier (constructor.Attributes);
			Output.Write ("Sub New(");
			OutputParameters (constructor.Parameters);
			Output.WriteLine (")");
			Indent++;
			// check if ctor passes args on to other ctor in class
			CodeExpressionCollection ctorArgs = constructor.ChainedConstructorArgs;
			if (ctorArgs.Count > 0) {
				Output.Write ("Me.New(");
				OutputExpressionList (ctorArgs);
				Output.WriteLine (")");
			} else {
				// check if ctor passes args on to ctor in base class
				ctorArgs = constructor.BaseConstructorArgs;
				if (ctorArgs.Count > 0) {
					Output.Write ("MyBase.New(");
					OutputExpressionList (ctorArgs);
					Output.WriteLine (")");
				} else if (IsCurrentClass) {
					// call default base ctor
					Output.WriteLine ("MyBase.New");
				}
			}
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ("End Sub");
		}
		
		protected override void GenerateTypeConstructor (CodeTypeConstructor constructor)
		{
			if (IsCurrentDelegate || IsCurrentEnum || IsCurrentInterface)
				return;

			OutputAttributes (constructor.CustomAttributes, null,
				LineHandling.ContinueLine);

			Output.WriteLine ("Shared Sub New()");
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ("End Sub");
		}

		[MonoTODO ("partially implemented")]
		protected override void GenerateTypeStart (CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			OutputAttributes (declaration.CustomAttributes, null, 
				LineHandling.ContinueLine);

			TypeAttributes attributes = declaration.TypeAttributes;

			if (IsCurrentDelegate) {
				CodeTypeDelegate delegateDecl = (CodeTypeDelegate) declaration;

				if ((attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public) {
					output.Write ("Public ");
				}

				bool isSub = delegateDecl.ReturnType.BaseType == typeof (void).FullName;
				if (isSub) {
					output.Write ("Delegate Sub ");
				} else {
					output.Write ("Delegate Function ");
				}

				output.Write (CreateEscapedIdentifier (delegateDecl.Name));
				OutputTypeParameters (delegateDecl.TypeParameters);
				output.Write ("(");
				OutputParameters (delegateDecl.Parameters);
				Output.Write (")");
				if (!isSub) {
					Output.Write (" As ");
					OutputType (delegateDecl.ReturnType);
				}
				Output.WriteLine ("");
			} else {
				OutputTypeAttributes (declaration);
				output.Write (CreateEscapedIdentifier (declaration.Name));
				OutputTypeParameters (declaration.TypeParameters);

				if (IsCurrentEnum) {
					if (declaration.BaseTypes.Count > 0) {
						output.Write (" As ");
						OutputType (declaration.BaseTypes[0]);
					}
					output.WriteLine ();
					++Indent;
				} else {
					++Indent;

					bool firstInherits = true;
					bool firstImplements = true;

					for (int i = 0; i < declaration.BaseTypes.Count; i++) {
						// a struct can only implement interfaces
						// an interface can only inherit from other interface

						CodeTypeReference typeRef = declaration.BaseTypes[i];
						
						if (firstInherits && !declaration.IsStruct && !typeRef.IsInterface) {
							output.WriteLine ();
							output.Write ("Inherits ");
							firstInherits = false;
						} else if (!declaration.IsInterface && firstImplements) {
							output.WriteLine ();
							output.Write ("Implements ");
							firstImplements = false;
						} else {
							output.Write (", ");
						}
						OutputType (typeRef);
					}
					output.WriteLine ();
				}
			}
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate) {
				return;
			}
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
			GenerateNamespaceImports (ns);
			Output.WriteLine ();
			GenerateCommentStatements (ns.Comments);
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
			Output.Write (">");
		}

		private void OutputAttributes (CodeAttributeDeclarationCollection attributes, string prefix, LineHandling lineHandling) {
			if (attributes.Count == 0) {
				return;
			}

			GenerateAttributeDeclarationsStart (attributes);

			IEnumerator enumerator = attributes.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				CodeAttributeDeclaration att = (CodeAttributeDeclaration) enumerator.Current;
				if (prefix != null) {
					Output.Write (prefix);
				}
				OutputAttributeDeclaration (att);

				while (enumerator.MoveNext ()) {
					Output.Write (", ");
					if (lineHandling != LineHandling.InLine) {
						ContinueOnNewLine ("");
						Output.Write (" ");
					}
					att = (CodeAttributeDeclaration) enumerator.Current;
					if (prefix != null) {
						Output.Write (prefix);
					}
					OutputAttributeDeclaration (att);
				}
			}
			GenerateAttributeDeclarationsEnd (attributes);
			Output.Write (" ");

			switch (lineHandling) {
				case LineHandling.ContinueLine:
					ContinueOnNewLine ("");
					break;
				case LineHandling.NewLine:
					Output.WriteLine ();
					break;
			}
		}

		protected override void OutputAttributeArgument (CodeAttributeArgument argument)
		{
			string name = argument.Name;
			if (name != null && name.Length > 0) {
				Output.Write (name);
				Output.Write (":=");
			}
			GenerateExpression (argument.Value);
		}

		private void OutputAttributeDeclaration (CodeAttributeDeclaration attribute)
		{
			Output.Write (attribute.Name.Replace ('+', '.'));
			Output.Write ('(');
			IEnumerator enumerator = attribute.Arguments.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				CodeAttributeArgument argument = (CodeAttributeArgument) enumerator.Current;
				OutputAttributeArgument (argument);

				while (enumerator.MoveNext ()) {
					Output.Write (", ");
					argument = (CodeAttributeArgument) enumerator.Current;
					OutputAttributeArgument (argument);
				}
			}
			Output.Write (')');
		}

		protected override void OutputDirection (FieldDirection direction)
		{
			switch (direction) {
			case FieldDirection.In:
				Output.Write ("ByVal ");
				break;
			case FieldDirection.Out:
			case FieldDirection.Ref:
				Output.Write ("ByRef ");
				break;
			}
		}

		protected override void OutputFieldScopeModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Static:
				Output.Write ("Shared ");
				break;
			case MemberAttributes.Const:
				Output.Write ("Const ");
				break;
			}
		}

		private void OutputImplementationTypes (CodeTypeReferenceCollection implementationTypes, string member)
		{
			IEnumerator enumerator = implementationTypes.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				Output.Write (" Implements ");

				CodeTypeReference typeReference = (CodeTypeReference) enumerator.Current;
				OutputType (typeReference);
				Output.Write ('.');
				OutputIdentifier (member);

				while (enumerator.MoveNext ()) {
					Output.Write (" , ");
					typeReference = (CodeTypeReference) enumerator.Current;
					OutputType (typeReference);
					Output.Write ('.');
					OutputIdentifier (member);
				}
			}
		}

		protected override void OutputMemberAccessModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.AccessMask) {
			case MemberAttributes.Assembly:
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

		private void OutputVTableModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				Output.Write ("Shadows ");
		}

		protected override void OutputMemberScopeModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Abstract:
				Output.Write ("MustOverride ");
				break;
			case MemberAttributes.Final:
				// do nothing
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
				if (access_ovl == MemberAttributes.Public || access_ovl == MemberAttributes.Family)
					Output.Write ("Overridable ");
				break;
			default:
				//
				// FUNNY! if the scope value is
				// rubbish (0 or >Const), and access
				// is public, protected make it
				// "virtual".
				//
				// i'm not sure whether this is 100%
				// correct, but it seems to be MS
				// behavior.
				//
				// On MS.NET 2.0, internal properties
				// are also marked "virtual".
				//
				MemberAttributes access = attributes & MemberAttributes.AccessMask;
				if (access == MemberAttributes.Public || 
					access == MemberAttributes.Family || access == MemberAttributes.Assembly)
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

		private void OutputTypeAttributes (CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;
			TypeAttributes attributes = declaration.TypeAttributes;

			if (declaration.IsPartial)
				output.Write ("Partial ");
			
			switch (attributes & TypeAttributes.VisibilityMask) {
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				output.Write ("Public ");
				break;
			case TypeAttributes.NestedPrivate:
				output.Write ("Private ");
				break;
			case TypeAttributes.NotPublic:
			case TypeAttributes.NestedFamANDAssem:
			case TypeAttributes.NestedAssembly:
				output.Write ("Friend ");
				break; 
			case TypeAttributes.NestedFamily:
				output.Write ("Protected ");
				break;
			case TypeAttributes.NestedFamORAssem:
				output.Write ("Protected Friend ");
				break;
			}

			if (declaration.IsStruct) {
				output.Write ("Structure ");
			} else if (declaration.IsEnum) {
				output.Write ("Enum ");
			} else {
				if ((attributes & TypeAttributes.Interface) != 0) {
					output.Write ("Interface ");
				} else {
					if ((attributes & TypeAttributes.Sealed) != 0)
						output.Write ("NotInheritable ");

					if ((attributes & TypeAttributes.Abstract) != 0)
						output.Write ("MustInherit ");

					output.Write ("Class ");
				}
			}
		}

		void OutputTypeParameters (CodeTypeParameterCollection parameters)
		{
			int count = parameters.Count;
			if (count == 0)
				return;

			Output.Write ("(Of ");
			for (int i = 0; i < count; ++i) {
				if (i > 0)
					Output.Write (", ");
				CodeTypeParameter p = parameters [i];
				Output.Write (p.Name);
				OutputTypeParameterConstraints (p);
			}
			Output.Write (')');
		}

		void OutputTypeParameterConstraints (CodeTypeParameter parameter)
		{
			int constraint_count = parameter.Constraints.Count +
				(parameter.HasConstructorConstraint ? 1 : 0);

			if (constraint_count == 0)
				return;

			Output.Write (" As ");

			if (constraint_count > 1)
				Output.Write (" {");

			for (int i = 0; i < parameter.Constraints.Count; i++) {
				if (i > 0)
					Output.Write (", ");
				OutputType (parameter.Constraints [i]);
			}

			if (parameter.HasConstructorConstraint) {
				if (constraint_count > 1)
					Output.Write (", ");
				Output.Write ("New");
			}

			if (constraint_count > 1)
				Output.Write ("}");
		}

		protected override void OutputTypeNamePair (CodeTypeReference typeRef, String name)
		{
			if (name.Length == 0)
				name = "__exception";
			Output.Write (CreateEscapedIdentifier(name) + " As " + GetTypeOutput (typeRef));
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
			for (int MyCounter = 0; MyCounter < value.Length; MyCounter++) {
				if (value[MyCounter] == 34) //quotation mark
				{
					if (!inQuotes) {
						mySBuilder.Append ("&\"");
						inQuotes = true;
					}
					mySBuilder.Append (value[MyCounter]);
					mySBuilder.Append (value[MyCounter]);
				}
				else if (value[MyCounter] >= 32) //standard ansi/unicode characters
				{
					if (!inQuotes) {
						mySBuilder.Append ("&\"");
						inQuotes = true;
					}
					mySBuilder.Append (value[MyCounter]);
				}
				else //special chars, e.g. line break
				{
					if (inQuotes) {
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
				case "System.DateTime":
					output = "Date";
					break;
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
				case "System.Int32":
					output = "Integer";
					break;
				case "System.Int64":
					output = "Long";
					break;
				case "System.Int16":
					output = "Short";
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
				case "System.SByte":
					output = "SByte";
					break;
				case "System.UInt16":
					output = "UShort";
					break;
				case "System.UInt32":
					output = "UInteger";
					break;
				case "System.UInt64":
					output = "ULong";
					break;
				default:
					output = type.BaseType.Replace('+', '.');
					output = CreateEscapedIdentifier (output);
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

		private bool IsOverloaded (CodeMemberProperty property, CodeTypeDeclaration type)
		{
			if ((property.Attributes & MemberAttributes.Overloaded) == MemberAttributes.Overloaded) {
				return true;
			}

			foreach (CodeTypeMember member in type.Members) {
				CodeMemberProperty p = member as CodeMemberProperty;
				if (p == null) {
					// member is not a property
					continue;
				}

				if (p != property && p.Name == property.Name && p.PrivateImplementationType == null)
					return true;
			}
			return false;
		}

		private bool IsOverloaded (CodeMemberMethod method, CodeTypeDeclaration type)
		{
			if ((method.Attributes & MemberAttributes.Overloaded) == MemberAttributes.Overloaded) {
				return true;
			}

			foreach (CodeTypeMember member in type.Members) {
				CodeMemberMethod m = member as CodeMemberMethod;
				if (m == null) {
					// member is not a method
					continue;
				}

				if (!(m is CodeTypeConstructor) && !(m is CodeConstructor) && m != method && m.Name == method.Name && m.PrivateImplementationType == null)
					return true;
			}
			return false;
		}

		private string GetEventName (CodeMemberEvent evt)
		{
			if (evt.PrivateImplementationType == null)
				return evt.Name;

			string baseType = evt.PrivateImplementationType.BaseType.Replace ('.', '_');
			return baseType + "_" + evt.Name;
		}

		private string GetMethodName (CodeMemberMethod method)
		{
			if (method.PrivateImplementationType == null)
				return method.Name;

			string baseType = method.PrivateImplementationType.BaseType.Replace ('.', '_');
			return baseType + "_" + method.Name;
		}

		private string GetPropertyName (CodeMemberProperty property)
		{
			if (property.PrivateImplementationType == null)
				return property.Name;

			string baseType = property.PrivateImplementationType.BaseType.Replace ('.', '_');
			return baseType + "_" + property.Name;
		}

		static bool IsAbstract (MemberAttributes attributes)
		{
			return (attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract;
		}

		private enum LineHandling
		{
			InLine,
			ContinueLine,
			NewLine
		}
	}
}
