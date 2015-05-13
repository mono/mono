//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using Microsoft.CSharp;
#if JSCRIPT
using Microsoft.JScript;
#endif
using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Management
{
	internal class ManagementClassGenerator
	{
		private const int DMTF_DATETIME_STR_LENGTH = 25;

		private const int IDS_COMMENT_SHOULDSERIALIZE = 0;

		private const int IDS_COMMENT_ISPROPNULL = 1;

		private const int IDS_COMMENT_RESETPROP = 2;

		private const int IDS_COMMENT_ATTRIBPROP = 3;

		private const int IDS_COMMENT_DATECONVFUNC = 4;

		private const int IDS_COMMENT_GETINSTANCES = 5;

		private const int IDS_COMMENT_CLASSBEGIN = 6;

		private const int IDS_COMMENT_PRIV_AUTOCOMMIT = 7;

		private const int IDS_COMMENT_CONSTRUCTORS = 8;

		private const int IDS_COMMENT_ORIG_NAMESPACE = 9;

		private const int IDS_COMMENT_CLASSNAME = 10;

		private const int IDS_COMMENT_SYSOBJECT = 11;

		private const int IDS_COMMENT_LATEBOUNDOBJ = 12;

		private const int IDS_COMMENT_MGMTSCOPE = 13;

		private const int IDS_COMMENT_AUTOCOMMITPROP = 14;

		private const int IDS_COMMENT_MGMTPATH = 15;

		private const int IDS_COMMENT_PROP_TYPECONVERTER = 16;

		private const int IDS_COMMENT_SYSPROPCLASS = 17;

		private const int IDS_COMMENT_ENUMIMPL = 18;

		private const int IDS_COMMENT_LATEBOUNDPROP = 19;

		private const int IDS_COMMENTS_CREATEDCLASS = 20;

		private const int IDS_COMMENT_EMBEDDEDOBJ = 21;

		private const int IDS_COMMENT_CURRENTOBJ = 22;

		private const int IDS_COMMENT_FLAGFOREMBEDDED = 23;

		private string VSVERSION;

		private string OriginalServer;

		private string OriginalNamespace;

		private string OriginalClassName;

		private string OriginalPath;

		private bool bSingletonClass;

		private bool bUnsignedSupported;

		private string NETNamespace;

		private string arrConvFuncName;

		private string enumType;

		private bool bDateConversionFunctionsAdded;

		private bool bTimeSpanConversionFunctionsAdded;

		private ManagementClass classobj;

		private CodeDomProvider cp;

		private TextWriter tw;

		private string genFileName;

		private CodeTypeDeclaration cc;

		private CodeTypeDeclaration ccc;

		private CodeTypeDeclaration ecc;

		private CodeTypeDeclaration EnumObj;

		private CodeNamespace cn;

		private CodeMemberProperty cmp;

		private CodeConstructor cctor;

		private CodeMemberField cf;

		private CodeObjectCreateExpression coce;

		private CodeParameterDeclarationExpression cpde;

		private CodeIndexerExpression cie;

		private CodeMemberField cmf;

		private CodeMemberMethod cmm;

		private CodePropertyReferenceExpression cpre;

		private CodeMethodInvokeExpression cmie;

		private CodeExpressionStatement cmis;

		private CodeConditionStatement cis;

		private CodeBinaryOperatorExpression cboe;

		private CodeIterationStatement cfls;

		private CodeAttributeArgument caa;

		private CodeAttributeDeclaration cad;

		private ArrayList arrKeyType;

		private ArrayList arrKeys;

		private ArrayList BitMap;

		private ArrayList BitValues;

		private ArrayList ValueMap;

		private ArrayList Values;

		private SortedList PublicProperties;

		private SortedList PublicMethods;

		private SortedList PublicNamesUsed;

		private SortedList PrivateNamesUsed;

		private ArrayList CommentsString;

		private bool bHasEmbeddedProperties;

		public string GeneratedFileName
		{
			get
			{
				return this.genFileName;
			}
		}

		public string GeneratedTypeName
		{
			get
			{
				return string.Concat(this.PrivateNamesUsed["GeneratedNamespace"].ToString(), ".", this.PrivateNamesUsed["GeneratedClassName"].ToString());
			}
		}

		public ManagementClassGenerator()
		{
			this.VSVERSION = "8.0.0.0";
			this.OriginalServer = string.Empty;
			this.OriginalNamespace = string.Empty;
			this.OriginalClassName = string.Empty;
			this.OriginalPath = string.Empty;
			this.bUnsignedSupported = true;
			this.NETNamespace = string.Empty;
			this.arrConvFuncName = string.Empty;
			this.enumType = string.Empty;
			this.genFileName = string.Empty;
			this.arrKeyType = new ArrayList(5);
			this.arrKeys = new ArrayList(5);
			this.BitMap = new ArrayList(5);
			this.BitValues = new ArrayList(5);
			this.ValueMap = new ArrayList(5);
			this.Values = new ArrayList(5);
			this.PublicProperties = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PublicMethods = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PublicNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PrivateNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.CommentsString = new ArrayList(5);
		}

		public ManagementClassGenerator(ManagementClass cls)
		{
			this.VSVERSION = "8.0.0.0";
			this.OriginalServer = string.Empty;
			this.OriginalNamespace = string.Empty;
			this.OriginalClassName = string.Empty;
			this.OriginalPath = string.Empty;
			this.bUnsignedSupported = true;
			this.NETNamespace = string.Empty;
			this.arrConvFuncName = string.Empty;
			this.enumType = string.Empty;
			this.genFileName = string.Empty;
			this.arrKeyType = new ArrayList(5);
			this.arrKeys = new ArrayList(5);
			this.BitMap = new ArrayList(5);
			this.BitValues = new ArrayList(5);
			this.ValueMap = new ArrayList(5);
			this.Values = new ArrayList(5);
			this.PublicProperties = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PublicMethods = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PublicNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.PrivateNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
			this.CommentsString = new ArrayList(5);
			this.classobj = cls;
		}

		private void AddClassComments(CodeTypeDeclaration cc)
		{
			cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_SHOULDSERIALIZE")));
			cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_ISPROPNULL")));
			cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_RESETPROP")));
			cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_ATTRIBPROP")));
		}

		private void AddCommentsForEmbeddedProperties()
		{
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT1")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT2")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT3")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT4")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT5")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT6")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT7")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP1")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP2")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP3")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP4")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP5")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP6")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP7")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP8")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP9")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_VB_CODESAMP10")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDDED_COMMENT8")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP1")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP2")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP3")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP4")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP5")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP6")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP7")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP8")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP9")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP10")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP11")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP12")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP13")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP14")));
			this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("EMBEDED_CS_CODESAMP15")));
		}

		private void AddGetStatementsForEnumArray(CodeIndexerExpression ciProp, CodeMemberProperty cmProp)
		{
			string str = "arrEnumVals";
			string str1 = "enumToRet";
			string str2 = "counter";
			string baseType = cmProp.Type.BaseType;
			cmProp.GetStatements.Add(new CodeVariableDeclarationStatement("System.Array", str, new CodeCastExpression(new CodeTypeReference("System.Array"), ciProp)));
			cmProp.GetStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(baseType, 1), str1, new CodeArrayCreateExpression(new CodeTypeReference(baseType), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "Length"))));
			this.cfls = new CodeIterationStatement();
			cmProp.GetStatements.Add(new CodeVariableDeclarationStatement("System.Int32", str2, new CodePrimitiveExpression((object)0)));
			this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodePrimitiveExpression((object)0));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str2);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.LessThan;
			codeBinaryOperatorExpression.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "Length");
			this.cfls.TestExpression = codeBinaryOperatorExpression;
			this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str2), CodeBinaryOperatorType.Add, new CodePrimitiveExpression((object)1)));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = "GetValue";
			codeMethodInvokeExpression.Method.TargetObject = new CodeVariableReferenceExpression(str);
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str2));
			CodeMethodInvokeExpression codeTypeReferenceExpression = new CodeMethodInvokeExpression();
			codeTypeReferenceExpression.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
			codeTypeReferenceExpression.Parameters.Add(codeMethodInvokeExpression);
			codeTypeReferenceExpression.Method.MethodName = this.arrConvFuncName;
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[1];
			codeVariableReferenceExpression[0] = new CodeVariableReferenceExpression(str2);
			this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str1), codeVariableReferenceExpression), new CodeCastExpression(new CodeTypeReference(baseType), codeTypeReferenceExpression)));
			cmProp.GetStatements.Add(this.cfls);
			cmProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str1)));
		}

		private void AddPropertySet(CodeIndexerExpression prop, bool bArray, CodeStatementCollection statColl, string strType, CodeVariableReferenceExpression varValue)
		{
			if (varValue == null)
			{
				varValue = new CodeVariableReferenceExpression("value");
			}
			if (bArray)
			{
				string str = "len";
				string str1 = "iCounter";
				string str2 = "arrProp";
				CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
				codeBinaryOperatorExpression.Left = varValue;
				codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
				codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
				codeConditionStatement.Condition = codeBinaryOperatorExpression;
				CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), varValue), "Length");
				codeConditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str, codePropertyReferenceExpression));
				CodeTypeReference codeTypeReference = new CodeTypeReference(new CodeTypeReference("System.String"), 1);
				codeConditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(codeTypeReference, str2, new CodeArrayCreateExpression(new CodeTypeReference("System.String"), new CodeVariableReferenceExpression(str))));
				this.cfls = new CodeIterationStatement();
				this.cfls.InitStatement = new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str1, new CodePrimitiveExpression((object)0));
				codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
				codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str1);
				codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.LessThan;
				codeBinaryOperatorExpression.Right = new CodeVariableReferenceExpression(str);
				this.cfls.TestExpression = codeBinaryOperatorExpression;
				this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.Add, new CodePrimitiveExpression((object)1)));
				CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
				codeMethodInvokeExpression.Method.MethodName = "GetValue";
				codeMethodInvokeExpression.Method.TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), varValue);
				codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
				CodeExpression[] codeVariableReferenceExpression = new CodeExpression[1];
				codeVariableReferenceExpression[0] = new CodeVariableReferenceExpression(str1);
				this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str2), codeVariableReferenceExpression), this.ConvertPropertyToString(strType, codeMethodInvokeExpression)));
				codeConditionStatement.TrueStatements.Add(this.cfls);
				codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(prop, new CodeVariableReferenceExpression(str2)));
				codeConditionStatement.FalseStatements.Add(new CodeAssignStatement(prop, new CodePrimitiveExpression(null)));
				statColl.Add(codeConditionStatement);
				return;
			}
			else
			{
				statColl.Add(new CodeAssignStatement(prop, this.ConvertPropertyToString(strType, varValue)));
				return;
			}
		}

		private void AddToDateTimeFunction()
		{
			string str = "dmtfDate";
			string str1 = "year";
			string str2 = "month";
			string str3 = "day";
			string str4 = "hour";
			string str5 = "minute";
			string str6 = "second";
			string str7 = "ticks";
			string str8 = "dmtf";
			string str9 = "tempString";
			string str10 = "datetime";
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["ToDateTimeMethod"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Static;
			codeMemberMethod.ReturnType = new CodeTypeReference("System.DateTime");
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), str));
			codeMemberMethod.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_TODATETIME")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), "initializer", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("initializer");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str1, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Year")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Month")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str3, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Day")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str4, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Hour")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str5, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Minute")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str6, new CodePropertyReferenceExpression(codeVariableReferenceExpression, "Second")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str7, new CodePrimitiveExpression((object)0)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str8, new CodeVariableReferenceExpression(str)));
			CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), str10, codeFieldReferenceExpression));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str9, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str8);
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
			codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString());
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str8), "Length");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.ValueEquality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str8), "Length");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)25);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			CodeTryCatchFinallyStatement codeTryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "****", str9, str8, str1, 0, 4);
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "**", str9, str8, str2, 4, 2);
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "**", str9, str8, str3, 6, 2);
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "**", str9, str8, str4, 8, 2);
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "**", str9, str8, str5, 10, 2);
			ManagementClassGenerator.DateTimeConversionFunctionHelper(codeTryCatchFinallyStatement.TryStatements, "**", str9, str8, str6, 12, 2);
			CodeMethodReferenceExpression codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "Substring");
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)15));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)6));
			codeTryCatchFinallyStatement.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str9), codeMethodInvokeExpression));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePrimitiveExpression("******");
			codeBinaryOperatorExpression.Right = new CodeVariableReferenceExpression(str9);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			CodeMethodReferenceExpression codeMethodReferenceExpression1 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int64"), "Parse");
			CodeMethodInvokeExpression codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression1.Method = codeMethodReferenceExpression1;
			codeMethodInvokeExpression1.Parameters.Add(new CodeVariableReferenceExpression(str9));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)0x3e8);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.Divide;
			CodeCastExpression codeCastExpression = new CodeCastExpression("System.Int64", codeBinaryOperatorExpression);
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeMethodInvokeExpression1;
			codeBinaryOperatorExpression1.Right = codeCastExpression;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.Multiply;
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str7), codeBinaryOperatorExpression1));
			codeTryCatchFinallyStatement.TryStatements.Add(codeConditionStatement);
			CodeBinaryOperatorExpression codePrimitiveExpression = new CodeBinaryOperatorExpression();
			codePrimitiveExpression.Left = new CodeVariableReferenceExpression(str1);
			codePrimitiveExpression.Right = new CodePrimitiveExpression((object)0);
			codePrimitiveExpression.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeVariableReferenceExpression1 = new CodeBinaryOperatorExpression();
			codeVariableReferenceExpression1.Left = new CodeVariableReferenceExpression(str2);
			codeVariableReferenceExpression1.Right = new CodePrimitiveExpression((object)0);
			codeVariableReferenceExpression1.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codePrimitiveExpression1 = new CodeBinaryOperatorExpression();
			codePrimitiveExpression1.Left = new CodeVariableReferenceExpression(str3);
			codePrimitiveExpression1.Right = new CodePrimitiveExpression((object)0);
			codePrimitiveExpression1.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression2.Left = new CodeVariableReferenceExpression(str4);
			codeBinaryOperatorExpression2.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression2.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeVariableReferenceExpression2 = new CodeBinaryOperatorExpression();
			codeVariableReferenceExpression2.Left = new CodeVariableReferenceExpression(str5);
			codeVariableReferenceExpression2.Right = new CodePrimitiveExpression((object)0);
			codeVariableReferenceExpression2.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codePrimitiveExpression2 = new CodeBinaryOperatorExpression();
			codePrimitiveExpression2.Left = new CodeVariableReferenceExpression(str6);
			codePrimitiveExpression2.Right = new CodePrimitiveExpression((object)0);
			codePrimitiveExpression2.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression3 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression3.Left = new CodeVariableReferenceExpression(str7);
			codeBinaryOperatorExpression3.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression3.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression4 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression4.Left = codePrimitiveExpression;
			codeBinaryOperatorExpression4.Right = codeVariableReferenceExpression1;
			codeBinaryOperatorExpression4.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression5 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression5.Left = codeBinaryOperatorExpression4;
			codeBinaryOperatorExpression5.Right = codePrimitiveExpression1;
			codeBinaryOperatorExpression5.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression6 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression6.Left = codeBinaryOperatorExpression5;
			codeBinaryOperatorExpression6.Right = codeBinaryOperatorExpression2;
			codeBinaryOperatorExpression6.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression7 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression7.Left = codeBinaryOperatorExpression6;
			codeBinaryOperatorExpression7.Right = codeVariableReferenceExpression2;
			codeBinaryOperatorExpression7.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression8 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression8.Left = codeBinaryOperatorExpression7;
			codeBinaryOperatorExpression8.Right = codeVariableReferenceExpression2;
			codeBinaryOperatorExpression8.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression9 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression9.Left = codeBinaryOperatorExpression8;
			codeBinaryOperatorExpression9.Right = codePrimitiveExpression2;
			codeBinaryOperatorExpression9.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression10 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression10.Left = codeBinaryOperatorExpression9;
			codeBinaryOperatorExpression10.Right = codeBinaryOperatorExpression3;
			codeBinaryOperatorExpression10.Operator = CodeBinaryOperatorType.BooleanOr;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression10;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeTryCatchFinallyStatement.TryStatements.Add(codeConditionStatement);
			string str11 = "e";
			CodeCatchClause codeCatchClause = new CodeCatchClause(str11);
			CodeObjectCreateExpression codeTypeReference = new CodeObjectCreateExpression();
			codeTypeReference.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString());
			codeTypeReference.Parameters.Add(new CodePrimitiveExpression(null));
			codeTypeReference.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str11), "Message"));
			codeCatchClause.Statements.Add(new CodeThrowExceptionStatement(codeTypeReference));
			codeTryCatchFinallyStatement.CatchClauses.Add(codeCatchClause);
			codeMemberMethod.Statements.Add(codeTryCatchFinallyStatement);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference("System.DateTime");
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str4));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str5));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str6));
			this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), this.coce));
			CodeMethodReferenceExpression codeMethodReferenceExpression2 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str10), "AddTicks");
			CodeMethodInvokeExpression codeMethodInvokeExpression2 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression2.Method = codeMethodReferenceExpression2;
			codeMethodInvokeExpression2.Parameters.Add(new CodeVariableReferenceExpression(str7));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), codeMethodInvokeExpression2));
			codeMethodReferenceExpression1 = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.TimeZone"), "CurrentTimeZone"), "GetUtcOffset");
			codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression1.Method = codeMethodReferenceExpression1;
			codeMethodInvokeExpression1.Parameters.Add(new CodeVariableReferenceExpression(str10));
			string str12 = "tickOffset";
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str12, codeMethodInvokeExpression1));
			string str13 = "UTCOffset";
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str13, new CodePrimitiveExpression((object)0)));
			string str14 = "OffsetToBeAdjusted";
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str14, new CodePrimitiveExpression((object)0)));
			string str15 = "OffsetMins";
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str12), "Ticks");
			codeBinaryOperatorExpression.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute");
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.Divide;
			codeCastExpression = new CodeCastExpression("System.Int64", codeBinaryOperatorExpression);
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str15, codeCastExpression));
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "Substring");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)22));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)3));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str9), codeMethodInvokeExpression));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str9);
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression("******");
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "Substring");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)21));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)4));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str9), codeMethodInvokeExpression));
			CodeTryCatchFinallyStatement codeTryCatchFinallyStatement1 = new CodeTryCatchFinallyStatement();
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str9));
			codeTryCatchFinallyStatement1.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str13), codeMethodInvokeExpression));
			codeTryCatchFinallyStatement1.CatchClauses.Add(codeCatchClause);
			codeConditionStatement.TrueStatements.Add(codeTryCatchFinallyStatement1);
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str15);
			codeBinaryOperatorExpression.Right = new CodeVariableReferenceExpression(str13);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.Subtract;
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str14), new CodeCastExpression(new CodeTypeReference("System.Int32"), codeBinaryOperatorExpression)));
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str10), "AddMinutes");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodeCastExpression("System.Double", new CodeVariableReferenceExpression(str14)));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), codeMethodInvokeExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str10)));
			this.cc.Members.Add(codeMemberMethod);
		}

		private void AddToDMTFDateTimeFunction()
		{
			string str = "utcString";
			string str1 = "date";
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["ToDMTFDateTimeMethod"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Static;
			codeMemberMethod.ReturnType = new CodeTypeReference("System.String");
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.DateTime"), str1));
			codeMemberMethod.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_TODMTFDATETIME")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
			CodeMethodReferenceExpression codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.TimeZone"), "CurrentTimeZone"), "GetUtcOffset");
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
			string str2 = "tickOffset";
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str2, codeMethodInvokeExpression));
			string str3 = "OffsetMins";
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks");
			this.cboe.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute");
			this.cboe.Operator = CodeBinaryOperatorType.Divide;
			CodeCastExpression codeCastExpression = new CodeCastExpression("System.Int64", this.cboe);
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str3, codeCastExpression));
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Math"), "Abs");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = codeMethodInvokeExpression;
			this.cboe.Right = new CodePrimitiveExpression((object)0x3e7);
			this.cboe.Operator = CodeBinaryOperatorType.GreaterThan;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = this.cboe;
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str1), "ToUniversalTime");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), codeMethodInvokeExpression));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), new CodePrimitiveExpression("+000")));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.GreaterThanOrEqual;
			CodeConditionStatement codeConditionStatement1 = new CodeConditionStatement();
			codeConditionStatement1.Condition = codeBinaryOperatorExpression;
			CodeBinaryOperatorExpression codePropertyReferenceExpression = new CodeBinaryOperatorExpression();
			codePropertyReferenceExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks");
			codePropertyReferenceExpression.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute");
			codePropertyReferenceExpression.Operator = CodeBinaryOperatorType.Divide;
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), codePropertyReferenceExpression), "ToString");
			CodeMethodInvokeExpression codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression1.Method = new CodeMethodReferenceExpression(codeMethodInvokeExpression, "PadLeft");
			codeMethodInvokeExpression1.Parameters.Add(new CodePrimitiveExpression((object)3));
			codeMethodInvokeExpression1.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeConditionStatement1.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), ManagementClassGenerator.GenerateConcatStrings(new CodePrimitiveExpression("+"), codeMethodInvokeExpression1)));
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str3)), "ToString");
			codeConditionStatement1.FalseStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), "strTemp", codeMethodInvokeExpression));
			codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression1.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("strTemp"), "Substring");
			codeMethodInvokeExpression1.Parameters.Add(new CodePrimitiveExpression((object)1));
			codeMethodInvokeExpression1.Parameters.Add(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("strTemp"), "Length"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression((object)1)));
			CodeMethodInvokeExpression codeMethodReferenceExpression1 = new CodeMethodInvokeExpression();
			codeMethodReferenceExpression1.Method = new CodeMethodReferenceExpression(codeMethodInvokeExpression1, "PadLeft");
			codeMethodReferenceExpression1.Parameters.Add(new CodePrimitiveExpression((object)3));
			codeMethodReferenceExpression1.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeConditionStatement1.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), ManagementClassGenerator.GenerateConcatStrings(new CodePrimitiveExpression("-"), codeMethodReferenceExpression1)));
			codeConditionStatement.FalseStatements.Add(codeConditionStatement1);
			codeMemberMethod.Statements.Add(codeConditionStatement);
			string str4 = "dmtfDateTime";
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Year")), "ToString");
			codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression1.Method = new CodeMethodReferenceExpression(codeMethodInvokeExpression, "PadLeft");
			codeMethodInvokeExpression1.Parameters.Add(new CodePrimitiveExpression((object)4));
			codeMethodInvokeExpression1.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str4, codeMethodInvokeExpression1));
			this.ToDMTFDateHelper("Month", codeMemberMethod, "System.Int32 ");
			this.ToDMTFDateHelper("Day", codeMemberMethod, "System.Int32 ");
			this.ToDMTFDateHelper("Hour", codeMemberMethod, "System.Int32 ");
			this.ToDMTFDateHelper("Minute", codeMemberMethod, "System.Int32 ");
			this.ToDMTFDateHelper("Second", codeMemberMethod, "System.Int32 ");
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str4), ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str4), new CodePrimitiveExpression("."))));
			string str5 = "dtTemp";
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference("System.DateTime");
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Year"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Month"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Day"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Hour"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Minute"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Second"));
			this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), str5, this.coce));
			string str6 = "microsec";
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Ticks");
			this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str5), "Ticks");
			this.cboe.Operator = CodeBinaryOperatorType.Subtract;
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = this.cboe;
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)0x3e8);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.Multiply;
			codePropertyReferenceExpression = new CodeBinaryOperatorExpression();
			codePropertyReferenceExpression.Left = codeBinaryOperatorExpression;
			codePropertyReferenceExpression.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond");
			codePropertyReferenceExpression.Operator = CodeBinaryOperatorType.Divide;
			codeCastExpression = new CodeCastExpression("System.Int64", codePropertyReferenceExpression);
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str6, codeCastExpression));
			string str7 = "strMicrosec";
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str6)), "ToString");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str7, codeMethodInvokeExpression));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str7), "Length");
			this.cboe.Right = new CodePrimitiveExpression((object)6);
			this.cboe.Operator = CodeBinaryOperatorType.GreaterThan;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = this.cboe;
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str7), "Substring");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)0));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)6));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str7), codeMethodInvokeExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str7), "PadLeft");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)6));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str4), codeMethodInvokeExpression);
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str4), codeMethodInvokeExpression1));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str4), ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str4), new CodeVariableReferenceExpression(str))));
			codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str4)));
			this.cc.Members.Add(codeMemberMethod);
		}

		private void AddToDMTFTimeIntervalFunction()
		{
			string str = "dmtftimespan";
			string str1 = "timespan";
			string str2 = "tsTemp";
			string str3 = "microsec";
			string str4 = "strMicroSec";
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["ToDMTFTimeIntervalMethod"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Static;
			codeMemberMethod.ReturnType = new CodeTypeReference("System.String");
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.TimeSpan"), str1));
			codeMemberMethod.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_TODMTFTIMEINTERVAL")));
			CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Days");
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), codePropertyReferenceExpression), "ToString");
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)8));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str, codeMethodInvokeExpression));
			CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
			codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString());
			CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "MaxValue");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), "maxTimeSpan", codeFieldReferenceExpression));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Days");
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.GreaterThan;
			codeBinaryOperatorExpression.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("maxTimeSpan"), "Days");
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			CodeFieldReferenceExpression codeFieldReferenceExpression1 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "MinValue");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), "minTimeSpan", codeFieldReferenceExpression1));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Days");
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.LessThan;
			codeBinaryOperatorExpression1.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("minTimeSpan"), "Days");
			CodeConditionStatement codeConditionStatement1 = new CodeConditionStatement();
			codeConditionStatement1.Condition = codeBinaryOperatorExpression1;
			codeConditionStatement1.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement1);
			codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Hours");
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), codePropertyReferenceExpression), "ToString");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)2));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			CodeMethodInvokeExpression codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression);
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Minutes");
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), codePropertyReferenceExpression), "ToString");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)2));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression);
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Seconds");
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), codePropertyReferenceExpression), "ToString");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)2));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression);
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), new CodePrimitiveExpression("."));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Days"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Hours"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Minutes"));
			this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Seconds"));
			this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str2, this.coce));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Ticks");
			codeBinaryOperatorExpression.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks");
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.Subtract;
			CodeBinaryOperatorExpression codePrimitiveExpression = new CodeBinaryOperatorExpression();
			codePrimitiveExpression.Left = codeBinaryOperatorExpression;
			codePrimitiveExpression.Right = new CodePrimitiveExpression((object)0x3e8);
			codePrimitiveExpression.Operator = CodeBinaryOperatorType.Multiply;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression2.Left = codePrimitiveExpression;
			codeBinaryOperatorExpression2.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond");
			codeBinaryOperatorExpression2.Operator = CodeBinaryOperatorType.Divide;
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str3, new CodeCastExpression("System.Int64", codeBinaryOperatorExpression2)));
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str3)), "ToString");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str4, this.cmie));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str4), "Length");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)6);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.GreaterThan;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str4), "Substring");
			this.cmie.Parameters.Add(new CodePrimitiveExpression((object)0));
			this.cmie.Parameters.Add(new CodePrimitiveExpression((object)6));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str4), this.cmie));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str4), "PadLeft");
			this.cmie.Parameters.Add(new CodePrimitiveExpression((object)6));
			this.cmie.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), this.cmie);
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), new CodePrimitiveExpression(":000"));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression1));
			codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str)));
			this.cc.Members.Add(codeMemberMethod);
		}

		private void AddToTimeSpanFunction()
		{
			string str = "dmtfTimespan";
			string str1 = "days";
			string str2 = "hours";
			string str3 = "minutes";
			string str4 = "seconds";
			string str5 = "ticks";
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["ToTimeSpanMethod"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Static;
			codeMemberMethod.ReturnType = new CodeTypeReference("System.TimeSpan");
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), str));
			codeMemberMethod.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_TOTIMESPAN")));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str1, new CodePrimitiveExpression((object)0)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePrimitiveExpression((object)0)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str3, new CodePrimitiveExpression((object)0)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str4, new CodePrimitiveExpression((object)0)));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str5, new CodePrimitiveExpression((object)0)));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str);
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
			codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString());
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "Length");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.ValueEquality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "Length");
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)25);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str), "Substring");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)21));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)4));
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = codeMethodInvokeExpression;
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(":000");
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			CodeTryCatchFinallyStatement codeTryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
			string str6 = "tempString";
			codeTryCatchFinallyStatement.TryStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str6, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
			ManagementClassGenerator.ToTimeSpanHelper(0, 8, str1, codeTryCatchFinallyStatement.TryStatements);
			ManagementClassGenerator.ToTimeSpanHelper(8, 2, str2, codeTryCatchFinallyStatement.TryStatements);
			ManagementClassGenerator.ToTimeSpanHelper(10, 2, str3, codeTryCatchFinallyStatement.TryStatements);
			ManagementClassGenerator.ToTimeSpanHelper(12, 2, str4, codeTryCatchFinallyStatement.TryStatements);
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str), "Substring");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)15));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)6));
			codeTryCatchFinallyStatement.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str6), codeMethodInvokeExpression));
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int64"), "Parse");
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str6));
			codeTryCatchFinallyStatement.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str5), new CodeBinaryOperatorExpression(codeMethodInvokeExpression, CodeBinaryOperatorType.Multiply, new CodeCastExpression("System.Int64", new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond"), CodeBinaryOperatorType.Divide, new CodePrimitiveExpression((object)0x3e8))))));
			CodeBinaryOperatorExpression codeVariableReferenceExpression = new CodeBinaryOperatorExpression();
			codeVariableReferenceExpression.Left = new CodeVariableReferenceExpression(str1);
			codeVariableReferenceExpression.Right = new CodePrimitiveExpression((object)0);
			codeVariableReferenceExpression.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codePrimitiveExpression = new CodeBinaryOperatorExpression();
			codePrimitiveExpression.Left = new CodeVariableReferenceExpression(str2);
			codePrimitiveExpression.Right = new CodePrimitiveExpression((object)0);
			codePrimitiveExpression.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = new CodeVariableReferenceExpression(str3);
			codeBinaryOperatorExpression1.Right = new CodePrimitiveExpression((object)0);
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeVariableReferenceExpression1 = new CodeBinaryOperatorExpression();
			codeVariableReferenceExpression1.Left = new CodeVariableReferenceExpression(str4);
			codeVariableReferenceExpression1.Right = new CodePrimitiveExpression((object)0);
			codeVariableReferenceExpression1.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codePrimitiveExpression1 = new CodeBinaryOperatorExpression();
			codePrimitiveExpression1.Left = new CodeVariableReferenceExpression(str5);
			codePrimitiveExpression1.Right = new CodePrimitiveExpression((object)0);
			codePrimitiveExpression1.Operator = CodeBinaryOperatorType.LessThan;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression2.Left = codeVariableReferenceExpression;
			codeBinaryOperatorExpression2.Right = codePrimitiveExpression;
			codeBinaryOperatorExpression2.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression3 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression3.Left = codeBinaryOperatorExpression2;
			codeBinaryOperatorExpression3.Right = codeBinaryOperatorExpression1;
			codeBinaryOperatorExpression3.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression4 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression4.Left = codeBinaryOperatorExpression3;
			codeBinaryOperatorExpression4.Right = codeVariableReferenceExpression1;
			codeBinaryOperatorExpression4.Operator = CodeBinaryOperatorType.BooleanOr;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression5 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression5.Left = codeBinaryOperatorExpression4;
			codeBinaryOperatorExpression5.Right = codePrimitiveExpression1;
			codeBinaryOperatorExpression5.Operator = CodeBinaryOperatorType.BooleanOr;
			codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression5;
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(codeObjectCreateExpression));
			string str7 = "e";
			CodeCatchClause codeCatchClause = new CodeCatchClause(str7);
			CodeObjectCreateExpression codeTypeReference = new CodeObjectCreateExpression();
			codeTypeReference.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString());
			codeTypeReference.Parameters.Add(new CodePrimitiveExpression(null));
			codeTypeReference.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str7), "Message"));
			codeCatchClause.Statements.Add(new CodeThrowExceptionStatement(codeTypeReference));
			codeTryCatchFinallyStatement.CatchClauses.Add(codeCatchClause);
			codeMemberMethod.Statements.Add(codeTryCatchFinallyStatement);
			string str8 = "timespan";
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str4));
			this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str8, this.coce));
			string str9 = "tsTemp";
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "FromTicks");
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str5));
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str9, codeMethodInvokeExpression));
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "Add");
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str9));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str8), codeMethodInvokeExpression));
			codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str8)));
			this.cc.Members.Add(codeMemberMethod);
		}

		private void CheckIfClassIsProperlyInitialized()
		{
			if (this.classobj == null)
			{
				if (this.OriginalNamespace == null || this.OriginalNamespace != null && this.OriginalNamespace.Length == 0)
				{
					throw new ArgumentOutOfRangeException(ManagementClassGenerator.GetString("NAMESPACE_NOTINIT_EXCEPT"));
				}
				else
				{
					if (this.OriginalClassName == null || this.OriginalClassName != null && this.OriginalClassName.Length == 0)
					{
						throw new ArgumentOutOfRangeException(ManagementClassGenerator.GetString("CLASSNAME_NOTINIT_EXCEPT"));
					}
				}
			}
		}

		private static int ConvertBitMapValueToInt32(string bitMap)
		{
			int num;
			string empty = "0x";
			if (bitMap.StartsWith(empty, StringComparison.Ordinal) || bitMap.StartsWith(empty.ToUpper(CultureInfo.InvariantCulture), StringComparison.Ordinal))
			{
				empty = string.Empty;
				char[] charArray = bitMap.ToCharArray();
				int length = bitMap.Length;
				for (int i = 2; i < length; i++)
				{
					empty = string.Concat(empty, charArray[i]);
				}
				num = Convert.ToInt32(empty, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int)));
			}
			else
			{
				num = Convert.ToInt32(bitMap, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int)));
			}
			return num;
		}

		private CodeTypeReference ConvertCIMType(CimType cType, bool isArray)
		{
			string str = null;
			CimType cimType = cType;
			switch (cimType)
			{
				case CimType.SInt16:
				{
					str = "System.Int16";
					break;
				}
				case CimType.SInt32:
				{
					str = "System.Int32";
					break;
				}
				case CimType.Real32:
				{
					str = "System.Single";
					break;
				}
				case CimType.Real64:
				{
					str = "System.Double";
					break;
				}
				case CimType.SInt16 | CimType.Real32:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64:
				/*case 9:*/
				case CimType.SInt16 | CimType.String:
				case CimType.Real32 | CimType.String:
				case CimType.Object:
				case CimType.SInt16 | CimType.Real32 | CimType.String:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
				{
					str = this.PublicNamesUsed["BaseObjClass"].ToString();
					break;
				}
				case CimType.String:
				{
					str = "System.String";
					break;
				}
				case CimType.Boolean:
				{
					str = "System.Boolean";
					break;
				}
				case CimType.SInt8:
				{
					str = "System.SByte";
					break;
				}
				case CimType.UInt8:
				{
					str = "System.Byte";
					break;
				}
				case CimType.UInt16:
				{
					if (this.bUnsignedSupported)
					{
						str = "System.UInt16";
						break;
					}
					else
					{
						str = "System.Int16";
						break;
					}
				}
				case CimType.UInt32:
				{
					if (this.bUnsignedSupported)
					{
						str = "System.UInt32";
						break;
					}
					else
					{
						str = "System.Int32";
						break;
					}
				}
				case CimType.SInt64:
				{
					str = "System.Int64";
					break;
				}
				case CimType.UInt64:
				{
					if (this.bUnsignedSupported)
					{
						str = "System.UInt64";
						break;
					}
					else
					{
						str = "System.Int64";
						break;
					}
				}
				default:
				{
					switch (cimType)
					{
						case CimType.DateTime:
						{
							str = "System.DateTime";
							break;
						}
						case CimType.Reference:
						{
							str = this.PublicNamesUsed["PathClass"].ToString();
							break;
						}
						case CimType.Char16:
						{
							str = "System.Char";
							break;
						}
						default:
						{
							str = this.PublicNamesUsed["BaseObjClass"].ToString();
							break;
						}
					}
					break;
				}
			}
			if (!isArray)
			{
				return new CodeTypeReference(str);
			}
			else
			{
				return new CodeTypeReference(str, 1);
			}
		}

		private CodeExpression ConvertPropertyToString(string strType, CodeExpression beginingExpression)
		{
			string str = strType;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "System.DateTime")
				{
					CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
					codeMethodInvokeExpression.Parameters.Add(new CodeCastExpression(new CodeTypeReference("System.DateTime"), beginingExpression));
					codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["ToDMTFDateTimeMethod"].ToString();
					return codeMethodInvokeExpression;
				}
				else
				{
					if (str1 == "System.TimeSpan")
					{
						CodeMethodInvokeExpression codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
						codeMethodInvokeExpression1.Parameters.Add(new CodeCastExpression(new CodeTypeReference("System.TimeSpan"), beginingExpression));
						codeMethodInvokeExpression1.Method.MethodName = this.PrivateNamesUsed["ToDMTFTimeIntervalMethod"].ToString();
						return codeMethodInvokeExpression1;
					}
					else
					{
						if (str1 == "System.Management.ManagementPath")
						{
							return new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), beginingExpression), this.PublicNamesUsed["PathProperty"].ToString());
						}
					}
				}
			}
			return null;
		}

		private static string ConvertToNumericValueAndAddToArray(CimType cimType, string numericValue, ArrayList arrayToAdd, out string enumType)
		{
			string empty = string.Empty;
			enumType = string.Empty;
			CimType cimType1 = cimType;
			switch (cimType1)
			{
				case CimType.SInt16:
				case CimType.SInt32:
				{
					arrayToAdd.Add(Convert.ToInt32(numericValue, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
					empty = "ToInt32";
					enumType = "System.Int32";
					break;
				}
				default:
				{
					if (cimType1 == CimType.SInt8 || cimType1 == CimType.UInt8 || cimType1 == CimType.UInt16)
					{
						arrayToAdd.Add(Convert.ToInt32(numericValue, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
						empty = "ToInt32";
						enumType = "System.Int32";
						break;
					}
					else if (cimType1 == CimType.UInt32)
					{
						arrayToAdd.Add(Convert.ToInt32(numericValue, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
						empty = "ToInt32";
						enumType = "System.Int32";
						break;
					}
					break;
				}
			}
			return empty;
		}

		private static string ConvertValuesToName(string str)
		{
			string empty = string.Empty;
			string str1 = "_";
			string empty1 = string.Empty;
			bool flag = true;
			if (str.Length != 0)
			{
				char[] charArray = str.ToCharArray();
				if (!char.IsLetter(charArray[0]))
				{
					empty = "Val_";
					empty1 = "l";
				}
				for (int i = 0; i < str.Length; i++)
				{
					flag = true;
					if (char.IsLetterOrDigit(charArray[i]))
					{
						empty1 = new string(charArray[i], 1);
					}
					else
					{
						if (empty1 != str1)
						{
							empty1 = str1;
						}
						else
						{
							flag = false;
						}
					}
					if (flag)
					{
						empty = string.Concat(empty, empty1);
					}
				}
				return empty;
			}
			else
			{
				return string.Copy("");
			}
		}

		private CodeExpression CreateObjectForProperty(string strType, CodeExpression param)
		{
			string str = strType;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "System.DateTime")
				{
					if (param != null)
					{
						this.cmie = new CodeMethodInvokeExpression();
						this.cmie.Parameters.Add(param);
						this.cmie.Method.MethodName = this.PrivateNamesUsed["ToDateTimeMethod"].ToString();
						return this.cmie;
					}
					else
					{
						return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue");
					}
				}
				else
				{
					if (str1 == "System.TimeSpan")
					{
						if (param != null)
						{
							this.cmie = new CodeMethodInvokeExpression();
							this.cmie.Parameters.Add(param);
							this.cmie.Method.MethodName = this.PrivateNamesUsed["ToTimeSpanMethod"].ToString();
							return this.cmie;
						}
						else
						{
							this.coce = new CodeObjectCreateExpression();
							this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
							this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
							this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
							this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
							this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
							this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
							return this.coce;
						}
					}
					else
					{
						if (str1 == "System.Management.ManagementPath")
						{
							this.coce = new CodeObjectCreateExpression();
							this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
							this.coce.Parameters.Add(param);
							return this.coce;
						}
					}
				}
			}
			return null;
		}

		private static void DateTimeConversionFunctionHelper(CodeStatementCollection cmmdt, string toCompare, string tempVarName, string dmtfVarName, string toAssign, int SubStringParam1, int SubStringParam2)
		{
			CodeMethodReferenceExpression codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(dmtfVarName), "Substring");
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)SubStringParam1));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)SubStringParam2));
			cmmdt.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(tempVarName), codeMethodInvokeExpression));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePrimitiveExpression(toCompare);
			codeBinaryOperatorExpression.Right = new CodeVariableReferenceExpression(tempVarName);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			codeMethodReferenceExpression = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse");
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = codeMethodReferenceExpression;
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(tempVarName));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(toAssign), codeMethodInvokeExpression));
			cmmdt.Add(codeConditionStatement);
		}

		private void GenarateConstructorWithLateBound()
		{
			string str = "theObject";
			string str1 = "SystemProperties";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
			this.cpde.Name = str;
			this.cctor.Parameters.Add(this.cpde);
			this.InitPrivateMemberVariables(this.cctor);
			this.cis = new CodeConditionStatement();
			this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), str1);
			CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
			codePrimitiveExpression[0] = new CodePrimitiveExpression("__CLASS");
			this.cie = new CodeIndexerExpression(this.cpre, codePrimitiveExpression);
			this.cpre = new CodePropertyReferenceExpression(this.cie, "Value");
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = this.cmie;
			this.cboe.Right = new CodePrimitiveExpression((object)(true));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), new CodeVariableReferenceExpression(str)));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString())));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
			this.coce.Parameters.Add(new CodePrimitiveExpression(ManagementClassGenerator.GetString("CLASSNOT_FOUND_EXCEPT")));
			this.cis.FalseStatements.Add(new CodeThrowExceptionStatement(this.coce));
			this.cctor.Statements.Add(this.cis);
			this.cc.Members.Add(this.cctor);
		}

		private void GenarateConstructorWithLateBoundForEmbedded()
		{
			string str = "theObject";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
			this.cpde.Name = str;
			this.cctor.Parameters.Add(this.cpde);
			this.InitPrivateMemberVariables(this.cctor);
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = this.cmie;
			this.cboe.Right = new CodePrimitiveExpression((object)(true));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis = new CodeConditionStatement();
			this.cis.Condition = this.cboe;
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EmbeddedObject"].ToString()), new CodeVariableReferenceExpression(str)));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["EmbeddedObject"].ToString())));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), new CodePrimitiveExpression((object)(true))));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
			this.coce.Parameters.Add(new CodePrimitiveExpression(ManagementClassGenerator.GetString("CLASSNOT_FOUND_EXCEPT")));
			this.cis.FalseStatements.Add(new CodeThrowExceptionStatement(this.coce));
			this.cctor.Statements.Add(this.cis);
			this.cc.Members.Add(this.cctor);
		}

		private bool GenerateAndWriteCode(CodeLanguage lang)
		{
			if (this.InitializeCodeGenerator(lang))
			{
				this.InitializeCodeTypeDeclaration(lang);
				this.GetCodeTypeDeclarationForClass(true);
				this.cc.Name = this.cp.CreateValidIdentifier(this.cc.Name);
				this.cn.Types.Add(this.cc);
				try
				{
					this.cp.GenerateCodeFromNamespace(this.cn, this.tw, new CodeGeneratorOptions());
				}
				finally
				{
					this.tw.Close();
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		private void GenerateClassNameProperty()
		{
			string str = "strRet";
			this.cmp = new CodeMemberProperty();
			this.cmp.Name = this.PublicNamesUsed["ClassNameProperty"].ToString();
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Type = new CodeTypeReference("System.String");
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmp.CustomAttributes.Add(this.cad);
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "DesignerSerializationVisibility";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes.Add(this.cad);
			this.cmp.GetStatements.Add(new CodeVariableDeclarationStatement("System.String", str, new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString())));
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString());
			this.cboe.Right = new CodePrimitiveExpression(null);
			this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
			this.cis.Condition = this.cboe;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), this.PublicNamesUsed["ClassPathProperty"].ToString());
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.cis.TrueStatements.Add(codeConditionStatement);
			CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
			codePrimitiveExpression[0] = new CodePrimitiveExpression("__CLASS");
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), new CodeCastExpression(new CodeTypeReference("System.String"), new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), codePrimitiveExpression))));
			CodeConditionStatement codeConditionStatement1 = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeVariableReferenceExpression = new CodeBinaryOperatorExpression();
			codeVariableReferenceExpression.Left = new CodeVariableReferenceExpression(str);
			codeVariableReferenceExpression.Right = new CodePrimitiveExpression(null);
			codeVariableReferenceExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodeBinaryOperatorExpression codeFieldReferenceExpression = new CodeBinaryOperatorExpression();
			codeFieldReferenceExpression.Left = new CodeVariableReferenceExpression(str);
			codeFieldReferenceExpression.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty");
			codeFieldReferenceExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeVariableReferenceExpression;
			codeBinaryOperatorExpression1.Right = codeFieldReferenceExpression;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.BooleanOr;
			codeConditionStatement1.Condition = codeBinaryOperatorExpression1;
			codeConditionStatement1.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString())));
			codeConditionStatement.TrueStatements.Add(codeConditionStatement1);
			this.cmp.GetStatements.Add(this.cis);
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str)));
			this.cc.Members.Add(this.cmp);
		}

		public CodeTypeDeclaration GenerateCode(bool includeSystemProperties, bool systemPropertyClass)
		{
			CodeTypeDeclaration codeTypeDeclarationForClass;
			if (!systemPropertyClass)
			{
				this.CheckIfClassIsProperlyInitialized();
				this.InitializeCodeGeneration();
				codeTypeDeclarationForClass = this.GetCodeTypeDeclarationForClass(includeSystemProperties);
			}
			else
			{
				this.InitilializePublicPrivateMembers();
				codeTypeDeclarationForClass = this.GenerateSystemPropertiesClass();
			}
			return codeTypeDeclarationForClass;
		}

		public bool GenerateCode(CodeLanguage lang, string filePath, string netNamespace)
		{
			if (filePath != null)
			{
				if (filePath.Length != 0)
				{
					this.NETNamespace = netNamespace;
					this.CheckIfClassIsProperlyInitialized();
					this.InitializeCodeGeneration();
					this.tw = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8);
					return this.GenerateAndWriteCode(lang);
				}
				else
				{
					throw new ArgumentOutOfRangeException(ManagementClassGenerator.GetString("EMPTY_FILEPATH_EXCEPT"));
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException(ManagementClassGenerator.GetString("NULLFILEPATH_EXCEPT"));
			}
		}

		private void GenerateCodeForRefAndDateTimeTypes(CodeIndexerExpression prop, bool bArray, CodeStatementCollection statColl, string strType, CodeVariableReferenceExpression varToAssign, bool bIsValueProprequired)
		{
			CodeExpression codeCastExpression;
			CodePropertyReferenceExpression codePropertyReferenceExpression;
			if (bArray)
			{
				string str = "len";
				string str1 = "iCounter";
				string str2 = "arrToRet";
				CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
				codeBinaryOperatorExpression.Left = prop;
				codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
				codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
				codeConditionStatement.Condition = codeBinaryOperatorExpression;
				if (!bIsValueProprequired)
				{
					codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), prop), "Length");
				}
				else
				{
					codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), new CodePropertyReferenceExpression(prop, "Value")), "Length");
				}
				codeConditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str, codePropertyReferenceExpression));
				CodeTypeReference codeTypeReference = new CodeTypeReference(new CodeTypeReference(strType), 1);
				codeConditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(codeTypeReference, str2, new CodeArrayCreateExpression(new CodeTypeReference(strType), new CodeVariableReferenceExpression(str))));
				this.cfls = new CodeIterationStatement();
				this.cfls.InitStatement = new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str1, new CodePrimitiveExpression((object)0));
				codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
				codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str1);
				codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.LessThan;
				codeBinaryOperatorExpression.Right = new CodeVariableReferenceExpression(str);
				this.cfls.TestExpression = codeBinaryOperatorExpression;
				this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.Add, new CodePrimitiveExpression((object)1)));
				CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
				codeMethodInvokeExpression.Method.MethodName = "GetValue";
				if (!bIsValueProprequired)
				{
					codeMethodInvokeExpression.Method.TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), prop);
				}
				else
				{
					codeMethodInvokeExpression.Method.TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), new CodePropertyReferenceExpression(prop, "Value"));
				}
				codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
				CodeMethodInvokeExpression codeMethodInvokeExpression1 = new CodeMethodInvokeExpression();
				codeMethodInvokeExpression1.Method.MethodName = "ToString";
				codeMethodInvokeExpression1.Method.TargetObject = codeMethodInvokeExpression;
				CodeExpression[] codeVariableReferenceExpression = new CodeExpression[1];
				codeVariableReferenceExpression[0] = new CodeVariableReferenceExpression(str1);
				this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str2), codeVariableReferenceExpression), this.CreateObjectForProperty(strType, codeMethodInvokeExpression1)));
				codeConditionStatement.TrueStatements.Add(this.cfls);
				if (varToAssign != null)
				{
					statColl.Add(new CodeAssignStatement(varToAssign, new CodePrimitiveExpression(null)));
					codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(varToAssign, new CodeVariableReferenceExpression(str2)));
					statColl.Add(codeConditionStatement);
					return;
				}
				else
				{
					codeConditionStatement.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str2)));
					statColl.Add(codeConditionStatement);
					statColl.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
					return;
				}
			}
			else
			{
				CodeConditionStatement codeConditionStatement1 = new CodeConditionStatement();
				CodeBinaryOperatorExpression codePrimitiveExpression = new CodeBinaryOperatorExpression();
				codePrimitiveExpression.Left = prop;
				codePrimitiveExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
				codePrimitiveExpression.Right = new CodePrimitiveExpression(null);
				codeConditionStatement1.Condition = codePrimitiveExpression;
				if (string.Compare(strType, this.PublicNamesUsed["PathClass"].ToString(), StringComparison.OrdinalIgnoreCase) != 0)
				{
					statColl.Add(codeConditionStatement1);
					if (!bIsValueProprequired)
					{
						codeCastExpression = new CodeCastExpression(new CodeTypeReference("System.String"), prop);
					}
					else
					{
						codeCastExpression = new CodeCastExpression(new CodeTypeReference("System.String"), new CodePropertyReferenceExpression(prop, "Value"));
					}
					if (varToAssign != null)
					{
						codeConditionStatement1.TrueStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, codeCastExpression)));
						codeConditionStatement1.FalseStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, null)));
						return;
					}
					else
					{
						codeConditionStatement1.TrueStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, codeCastExpression)));
						codeConditionStatement1.FalseStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, null)));
						return;
					}
				}
				else
				{
					CodeMethodReferenceExpression codeMethodReferenceExpression = new CodeMethodReferenceExpression();
					codeMethodReferenceExpression.MethodName = "ToString";
					codeMethodReferenceExpression.TargetObject = prop;
					this.cmie = new CodeMethodInvokeExpression();
					this.cmie.Method = codeMethodReferenceExpression;
					if (varToAssign != null)
					{
						statColl.Add(new CodeAssignStatement(varToAssign, new CodePrimitiveExpression(null)));
						codeConditionStatement1.TrueStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, this.cmie)));
						statColl.Add(codeConditionStatement1);
						return;
					}
					else
					{
						codeConditionStatement1.TrueStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, this.cmie)));
						statColl.Add(codeConditionStatement1);
						statColl.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
						return;
					}
				}
			}
		}

		private void GenerateCollectionClass()
		{
			string str = "ManagementObjectCollection";
			string str1 = "privColObj";
			string str2 = "objCollection";
			this.ccc = new CodeTypeDeclaration(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.ccc.BaseTypes.Add("System.Object");
			this.ccc.BaseTypes.Add("ICollection");
			this.ccc.TypeAttributes = TypeAttributes.NestedPublic;
			this.cf = new CodeMemberField();
			this.cf.Name = str1;
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cf.Type = new CodeTypeReference(str);
			this.ccc.Members.Add(this.cf);
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Name = str2;
			this.cpde.Type = new CodeTypeReference(str);
			this.cctor.Parameters.Add(this.cpde);
			this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodeVariableReferenceExpression(str2)));
			this.ccc.Members.Add(this.cctor);
			this.cmp = new CodeMemberProperty();
			this.cmp.Type = new CodeTypeReference("System.Int32");
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Name = "Count";
			this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Count")));
			this.ccc.Members.Add(this.cmp);
			this.cmp = new CodeMemberProperty();
			this.cmp.Type = new CodeTypeReference("System.Boolean");
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Name = "IsSynchronized";
			this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "IsSynchronized")));
			this.ccc.Members.Add(this.cmp);
			this.cmp = new CodeMemberProperty();
			this.cmp.Type = new CodeTypeReference("System.Object");
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Name = "SyncRoot";
			this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeThisReferenceExpression()));
			this.ccc.Members.Add(this.cmp);
			string str3 = "array";
			string str4 = "index";
			string str5 = "nCtr";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "CopyTo";
			this.cmm.ImplementationTypes.Add("System.Collections.ICollection");
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Name = str3;
			this.cpde.Type = new CodeTypeReference("System.Array");
			this.cmm.Parameters.Add(this.cpde);
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Name = str4;
			this.cpde.Type = new CodeTypeReference("System.Int32");
			this.cmm.Parameters.Add(this.cpde);
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str1), "CopyTo", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str4));
			this.cmm.Statements.Add(new CodeExpressionStatement(this.cmie));
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.Int32", str5));
			this.cfls = new CodeIterationStatement();
			this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str5), new CodePrimitiveExpression((object)0));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(str5);
			this.cboe.Operator = CodeBinaryOperatorType.LessThan;
			this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Length");
			this.cfls.TestExpression = this.cboe;
			this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str5), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str5), CodeBinaryOperatorType.Add, new CodePrimitiveExpression((object)1)));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str3), "SetValue", new CodeExpression[0]);
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[1];
			codeVariableReferenceExpression[0] = new CodeVariableReferenceExpression(str5);
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str3), "GetValue", codeVariableReferenceExpression);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.coce.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString()), codeMethodInvokeExpression));
			this.cmie.Parameters.Add(this.coce);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str5));
			this.cfls.Statements.Add(new CodeExpressionStatement(this.cmie));
			this.cmm.Statements.Add(this.cfls);
			this.ccc.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetEnumerator";
			this.cmm.ImplementationTypes.Add("System.Collections.IEnumerable");
			this.cmm.ReturnType = new CodeTypeReference("System.Collections.IEnumerator");
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["EnumeratorClass"].ToString());
			this.coce.Parameters.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str1), "GetEnumerator", new CodeExpression[0]));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
			this.ccc.Members.Add(this.cmm);
			this.GenerateEnumeratorClass();
			this.ccc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_ENUMIMPL")));
			this.cc.Members.Add(this.ccc);
		}

		private void GenerateCommitMethod()
		{
			this.cmm = new CodeMemberMethod();
			this.cmm.Name = this.PublicNamesUsed["CommitMethod"].ToString();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmm.CustomAttributes.Add(this.cad);
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
			this.cmie.Method.MethodName = "Put";
			this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
			this.cmm.Statements.Add(this.cis);
			this.cc.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Name = this.PublicNamesUsed["CommitMethod"].ToString();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression();
			codeParameterDeclarationExpression.Type = new CodeTypeReference(this.PublicNamesUsed["PutOptions"].ToString());
			codeParameterDeclarationExpression.Name = this.PrivateNamesUsed["putOptions"].ToString();
			this.cmm.Parameters.Add(codeParameterDeclarationExpression);
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmm.CustomAttributes.Add(this.cad);
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
			this.cmie.Method.MethodName = "Put";
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["putOptions"].ToString()));
			this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
			this.cmm.Statements.Add(this.cis);
			this.cc.Members.Add(this.cmm);
		}

		private static CodeMethodInvokeExpression GenerateConcatStrings(CodeExpression ce1, CodeExpression ce2)
		{
			CodeExpression[] codeExpressionArray = new CodeExpression[2];
			codeExpressionArray[0] = ce1;
			codeExpressionArray[1] = ce2;
			CodeExpression[] codeExpressionArray1 = codeExpressionArray;
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Concat", codeExpressionArray1);
			return codeMethodInvokeExpression;
		}

		private void GenerateConstructorWithKeys()
		{
			if (this.arrKeyType.Count > 0)
			{
				this.cctor = new CodeConstructor();
				this.cctor.Attributes = MemberAttributes.Public;
				CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
				codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
				codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
				for (int i = 0; i < this.arrKeys.Count; i++)
				{
					this.cpde = new CodeParameterDeclarationExpression();
					this.cpde.Type = new CodeTypeReference(((CodeTypeReference)this.arrKeyType[i]).BaseType);
					this.cpde.Name = string.Concat("key", this.arrKeys[i].ToString());
					this.cctor.Parameters.Add(this.cpde);
				}
				if (this.cctor.Parameters.Count == 1 && this.cctor.Parameters[0].Type.BaseType == (new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString())).BaseType)
				{
					this.cpde = new CodeParameterDeclarationExpression();
					this.cpde.Type = new CodeTypeReference("System.Object");
					this.cpde.Name = "dummyParam";
					this.cctor.Parameters.Add(this.cpde);
					this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("dummyParam"), new CodePrimitiveExpression(null)));
				}
				codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
				this.cmie = new CodeMethodInvokeExpression();
				this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
				this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
				for (int j = 0; j < this.arrKeys.Count; j++)
				{
					this.cmie.Parameters.Add(new CodeVariableReferenceExpression(string.Concat("key", this.arrKeys[j])));
				}
				this.coce = new CodeObjectCreateExpression();
				this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
				this.coce.Parameters.Add(this.cmie);
				codeMethodInvokeExpression.Parameters.Add(this.coce);
				codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
				this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
				this.cc.Members.Add(this.cctor);
			}
		}

		private void GenerateConstructorWithOptions()
		{
			string str = "getOptions";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.coce.Parameters.Add(this.cmie);
			codeMethodInvokeExpression.Parameters.Add(this.coce);
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithPath()
		{
			string str = "path";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.cpde.Name = str;
			this.cctor.Parameters.Add(this.cpde);
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithPathOptions()
		{
			string str = "path";
			string str1 = "getOptions";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str));
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str1));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithScope()
		{
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.coce.Parameters.Add(this.cmie);
			codeMethodInvokeExpression.Parameters.Add(this.coce);
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithScopeKeys()
		{
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			if (this.arrKeyType.Count > 0)
			{
				for (int i = 0; i < this.arrKeys.Count; i++)
				{
					this.cpde = new CodeParameterDeclarationExpression();
					this.cpde.Type = new CodeTypeReference(((CodeTypeReference)this.arrKeyType[i]).BaseType);
					this.cpde.Name = string.Concat("key", this.arrKeys[i].ToString());
					this.cctor.Parameters.Add(this.cpde);
				}
				if (this.cctor.Parameters.Count == 2 && this.cctor.Parameters[1].Type.BaseType == (new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString())).BaseType)
				{
					this.cpde = new CodeParameterDeclarationExpression();
					this.cpde.Type = new CodeTypeReference("System.Object");
					this.cpde.Name = "dummyParam";
					this.cctor.Parameters.Add(this.cpde);
					this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("dummyParam"), new CodePrimitiveExpression(null)));
				}
				codeMethodInvokeExpression.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString())));
				this.cmie = new CodeMethodInvokeExpression();
				this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
				this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
				for (int j = 0; j < this.arrKeys.Count; j++)
				{
					this.cmie.Parameters.Add(new CodeVariableReferenceExpression(string.Concat("key", this.arrKeys[j])));
				}
				this.coce = new CodeObjectCreateExpression();
				this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
				this.coce.Parameters.Add(this.cmie);
				codeMethodInvokeExpression.Parameters.Add(this.coce);
				codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
				this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
				this.cc.Members.Add(this.cctor);
			}
		}

		private void GenerateConstructorWithScopeOptions()
		{
			string str = "getOptions";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.coce.Parameters.Add(this.cmie);
			codeMethodInvokeExpression.Parameters.Add(this.coce);
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithScopePath()
		{
			string str = "path";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructorWithScopePathOptions()
		{
			string str = "path";
			string str1 = "getOptions";
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str));
			this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str1));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
		}

		private void GenerateConstructPath()
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression;
			object obj;
			object obj1;
			this.cmm = new CodeMemberMethod();
			this.cmm.Name = this.PublicNamesUsed["ConstructPathFunction"].ToString();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cmm.ReturnType = new CodeTypeReference("System.String");
			for (int i = 0; i < this.arrKeys.Count; i++)
			{
				string baseType = ((CodeTypeReference)this.arrKeyType[i]).BaseType;
				this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(baseType, string.Concat("key", this.arrKeys[i].ToString())));
			}
			string str = string.Concat(this.OriginalNamespace, ":", this.OriginalClassName);
			if (!this.bSingletonClass)
			{
				string str1 = "strPath";
				this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.String", str1, new CodePrimitiveExpression(str)));
				for (int j = 0; j < this.arrKeys.Count; j++)
				{
					if (((CodeTypeReference)this.arrKeyType[j]).BaseType != "System.String")
					{
						this.cmie = new CodeMethodInvokeExpression();
						this.cmie.Method.TargetObject = new CodeCastExpression(new CodeTypeReference(string.Concat(((CodeTypeReference)this.arrKeyType[j]).BaseType, " ")), new CodeVariableReferenceExpression(string.Concat("key", this.arrKeys[j])));
						this.cmie.Method.MethodName = "ToString";
						if (j == 0)
						{
							obj = string.Concat(".", this.arrKeys[j], "=");
						}
						else
						{
							obj = string.Concat(",", this.arrKeys[j], "=");
						}
						CodeMethodInvokeExpression codeMethodInvokeExpression1 = ManagementClassGenerator.GenerateConcatStrings(new CodePrimitiveExpression(obj), this.cmie);
						codeMethodInvokeExpression = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str1), codeMethodInvokeExpression1);
					}
					else
					{
						CodeMethodInvokeExpression codeMethodInvokeExpression2 = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(string.Concat("key", this.arrKeys[j])), new CodePrimitiveExpression("\""));
						CodeMethodInvokeExpression codeMethodInvokeExpression3 = ManagementClassGenerator.GenerateConcatStrings(new CodePrimitiveExpression("\""), codeMethodInvokeExpression2);
						if (j == 0)
						{
							obj1 = string.Concat(".", this.arrKeys[j], "=");
						}
						else
						{
							obj1 = string.Concat(",", this.arrKeys[j], "=");
						}
						CodeMethodInvokeExpression codeMethodInvokeExpression4 = ManagementClassGenerator.GenerateConcatStrings(new CodePrimitiveExpression(obj1), codeMethodInvokeExpression3);
						codeMethodInvokeExpression = ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str1), codeMethodInvokeExpression4);
					}
					this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), codeMethodInvokeExpression));
				}
				this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str1)));
			}
			else
			{
				str = string.Concat(str, "=@");
				this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(str)));
			}
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateCreateInstance()
		{
			string str = "tmpMgmtClass";
			this.cmm = new CodeMemberMethod();
			string str1 = "mgmtScope";
			string str2 = "mgmtPath";
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["CreateInst"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmm.CustomAttributes.Add(this.cad);
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), str1, new CodePrimitiveExpression(null)));
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString());
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), this.coce));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Path"), "NamespacePath"), new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationWmiNamespace"].ToString())));
			codeConditionStatement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
			this.cmm.Statements.Add(codeConditionStatement);
			CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
			codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			codeObjectCreateExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString()));
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str2, codeObjectCreateExpression));
			CodeObjectCreateExpression codeTypeReference = new CodeObjectCreateExpression();
			codeTypeReference.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
			codeTypeReference.Parameters.Add(new CodeVariableReferenceExpression(str1));
			codeTypeReference.Parameters.Add(new CodeVariableReferenceExpression(str2));
			codeTypeReference.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(this.PublicNamesUsed["ManagementClass"].ToString(), str, codeTypeReference));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = "CreateInstance";
			codeMethodInvokeExpression.Method.TargetObject = new CodeVariableReferenceExpression(str);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.coce.Parameters.Add(codeMethodInvokeExpression);
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateDateTimeConversionFunction()
		{
			this.AddToDateTimeFunction();
			this.AddToDMTFDateTimeFunction();
		}

		private void GenerateDefaultConstructor()
		{
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMethodInvokeExpression.Method.TargetObject = new CodeThisReferenceExpression();
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			if (!this.bSingletonClass)
			{
				codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			}
			else
			{
				this.cmie = new CodeMethodInvokeExpression();
				this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
				this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
				this.coce = new CodeObjectCreateExpression();
				this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
				this.coce.Parameters.Add(this.cmie);
				codeMethodInvokeExpression.Parameters.Add(this.coce);
			}
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression(null));
			this.cctor.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
			this.cc.Members.Add(this.cctor);
			this.cctor.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_CONSTRUCTORS")));
		}

		private void GenerateDeleteInstance()
		{
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["DeleteInst"].ToString();
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmm.CustomAttributes.Add(this.cad);
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = "Delete";
			codeMethodInvokeExpression.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
			this.cmm.Statements.Add(codeMethodInvokeExpression);
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateEnumeratorClass()
		{
			string str = "privObjEnum";
			string str1 = "ManagementObjectEnumerator";
			string str2 = "ManagementObjectCollection";
			string str3 = "objEnum";
			this.ecc = new CodeTypeDeclaration(this.PrivateNamesUsed["EnumeratorClass"].ToString());
			this.ecc.TypeAttributes = TypeAttributes.NestedPublic;
			this.ecc.BaseTypes.Add("System.Object");
			this.ecc.BaseTypes.Add("System.Collections.IEnumerator");
			this.cf = new CodeMemberField();
			this.cf.Name = str;
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cf.Type = new CodeTypeReference(string.Concat(str2, ".", str1));
			this.ecc.Members.Add(this.cf);
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Name = str3;
			this.cpde.Type = new CodeTypeReference(string.Concat(str2, ".", str1));
			this.cctor.Parameters.Add(this.cpde);
			this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), new CodeVariableReferenceExpression(str3)));
			this.ecc.Members.Add(this.cctor);
			this.cmp = new CodeMemberProperty();
			this.cmp.Type = new CodeTypeReference("System.Object");
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Name = "Current";
			this.cmp.ImplementationTypes.Add("System.Collections.IEnumerator");
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.coce.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString()), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "Current")));
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(this.coce));
			this.ecc.Members.Add(this.cmp);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "MoveNext";
			this.cmm.ImplementationTypes.Add("System.Collections.IEnumerator");
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str), "MoveNext", new CodeExpression[0]);
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.ecc.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "Reset";
			this.cmm.ImplementationTypes.Add("System.Collections.IEnumerator");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str), "Reset", new CodeExpression[0]);
			this.cmm.Statements.Add(new CodeExpressionStatement(this.cmie));
			this.ecc.Members.Add(this.cmm);
			this.ccc.Members.Add(this.ecc);
		}

		private void GenerateGetInstancesWithCondition()
		{
			string str = "condition";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", str));
			this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithNoParameters()
		{
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.MethodName = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
			this.cmm.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_GETINSTANCES")));
		}

		private void GenerateGetInstancesWithProperties()
		{
			string str = "selectedProperties";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", str));
			this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithScope()
		{
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString()), this.PrivateNamesUsed["EnumParam"].ToString()));
			string str = "clsObject";
			string str1 = "pathObj";
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString());
			this.cboe.Right = new CodePrimitiveExpression(null);
			this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
			this.cis.Condition = this.cboe;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString());
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), this.coce));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), "Path"), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
			codeConditionStatement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
			this.cis.TrueStatements.Add(codeConditionStatement);
			this.cmm.Statements.Add(this.cis);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str1, this.coce));
			this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "ClassName"), new CodePrimitiveExpression(this.OriginalClassName)));
			this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.coce.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString()), str, this.coce));
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString());
			this.cboe.Right = new CodePrimitiveExpression(null);
			this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
			this.cis.Condition = this.cboe;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString());
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), this.coce));
			this.cis.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), "EnsureLocatable"), new CodePrimitiveExpression((object)(true))));
			this.cmm.Statements.Add(this.cis);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str), "GetInstances");
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()));
			this.coce.Parameters.Add(this.cmie);
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithScopeCondition()
		{
			string str = "condition";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), str));
			this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithScopeProperties()
		{
			string str = "selectedProperties";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(this.PublicNamesUsed["ScopeClass"].ToString(), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", str));
			this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithScopeWhereProperties()
		{
			string str = "condition";
			string str1 = "selectedProperties";
			string str2 = "ObjectSearcher";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", str));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", str1));
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString());
			this.cboe.Right = new CodePrimitiveExpression(null);
			this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
			this.cis.Condition = this.cboe;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString());
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), this.coce));
			codeConditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), "Path"), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
			codeConditionStatement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
			this.cis.TrueStatements.Add(codeConditionStatement);
			this.cmm.Statements.Add(this.cis);
			CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
			codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryClass"].ToString());
			codeObjectCreateExpression.Parameters.Add(new CodePrimitiveExpression(this.OriginalClassName));
			codeObjectCreateExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			codeObjectCreateExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ObjectSearcherClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.coce.Parameters.Add(codeObjectCreateExpression);
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(this.PublicNamesUsed["ObjectSearcherClass"].ToString(), str2, this.coce));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString());
			this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString()), this.PrivateNamesUsed["EnumParam"].ToString(), this.coce));
			this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), "EnsureLocatable"), new CodePrimitiveExpression((object)(true))));
			this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Options"), new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString())));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.coce.Parameters.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str2), "Get", new CodeExpression[0]));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateGetInstancesWithWhereProperties()
		{
			string str = "selectedProperties";
			string str1 = "condition";
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
			this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", str));
			this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateIfClassvalidFunction()
		{
			this.GenerateIfClassvalidFuncWithAllParams();
			string str = "theObj";
			string str1 = "count";
			string str2 = "parentClasses";
			this.cmm = new CodeMemberMethod();
			this.cmm.Name = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), str));
			CodeExpression[] codeCastExpression = new CodeExpression[4];
			CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
			codePrimitiveExpression[0] = new CodePrimitiveExpression("__CLASS");
			codeCastExpression[0] = new CodeCastExpression(new CodeTypeReference("System.String"), new CodeIndexerExpression(new CodeVariableReferenceExpression(str), codePrimitiveExpression));
			codeCastExpression[1] = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString());
			codeCastExpression[2] = new CodePrimitiveExpression((object)(true));
			codeCastExpression[3] = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture");
			CodeExpression[] codeExpressionArray = codeCastExpression;
			this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", codeExpressionArray);
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = this.cmie;
			this.cboe.Right = new CodePrimitiveExpression((object)0);
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str);
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeBinaryOperatorExpression;
			codeBinaryOperatorExpression1.Right = this.cboe;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cis = new CodeConditionStatement();
			this.cis.Condition = codeBinaryOperatorExpression1;
			this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(true))));
			CodeExpression[] codePrimitiveExpression1 = new CodeExpression[1];
			codePrimitiveExpression1[0] = new CodePrimitiveExpression("__DERIVATION");
			CodeExpression codeExpression = new CodeCastExpression(new CodeTypeReference("System.Array"), new CodeIndexerExpression(new CodeVariableReferenceExpression(str), codePrimitiveExpression1));
			this.cis.FalseStatements.Add(new CodeVariableDeclarationStatement("System.Array", str2, codeExpression));
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(str2);
			this.cboe.Right = new CodePrimitiveExpression(null);
			this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement.Condition = this.cboe;
			this.cfls = new CodeIterationStatement();
			codeConditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement("System.Int32", str1, new CodePrimitiveExpression((object)0)));
			this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodePrimitiveExpression((object)0));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(str1);
			this.cboe.Operator = CodeBinaryOperatorType.LessThan;
			this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Length");
			this.cfls.TestExpression = this.cboe;
			this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str1), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.Add, new CodePrimitiveExpression((object)1)));
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = "GetValue";
			codeMethodInvokeExpression.Method.TargetObject = new CodeVariableReferenceExpression(str2);
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str1));
			CodeExpression[] codePropertyReferenceExpression = new CodeExpression[4];
			codePropertyReferenceExpression[0] = new CodeCastExpression(new CodeTypeReference("System.String"), codeMethodInvokeExpression);
			codePropertyReferenceExpression[1] = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString());
			codePropertyReferenceExpression[2] = new CodePrimitiveExpression((object)(true));
			codePropertyReferenceExpression[3] = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture");
			CodeExpression[] codeExpressionArray1 = codePropertyReferenceExpression;
			CodeMethodInvokeExpression codeMethodInvokeExpression1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", codeExpressionArray1);
			CodeConditionStatement codeConditionStatement1 = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = codeMethodInvokeExpression1;
			this.cboe.Right = new CodePrimitiveExpression((object)0);
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			codeConditionStatement1.Condition = this.cboe;
			codeConditionStatement1.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(true))));
			codeConditionStatement.TrueStatements.Add(this.cfls);
			this.cfls.Statements.Add(codeConditionStatement1);
			this.cis.FalseStatements.Add(codeConditionStatement);
			this.cmm.Statements.Add(this.cis);
			this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(false))));
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateIfClassvalidFuncWithAllParams()
		{
			string str = "path";
			string str1 = "OptionsParam";
			this.cmm = new CodeMemberMethod();
			this.cmm.Name = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str1));
			CodeExpression[] codePropertyReferenceExpression = new CodeExpression[4];
			codePropertyReferenceExpression[0] = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), "ClassName");
			codePropertyReferenceExpression[1] = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString());
			codePropertyReferenceExpression[2] = new CodePrimitiveExpression((object)(true));
			codePropertyReferenceExpression[3] = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture");
			CodeExpression[] codeExpressionArray = codePropertyReferenceExpression;
			this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", codeExpressionArray);
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = this.cmie;
			this.cboe.Right = new CodePrimitiveExpression((object)0);
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str);
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(null);
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeBinaryOperatorExpression;
			codeBinaryOperatorExpression1.Right = this.cboe;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cis = new CodeConditionStatement();
			this.cis.Condition = codeBinaryOperatorExpression1;
			this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(true))));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str1));
			CodeMethodReferenceExpression codeMethodReferenceExpression = new CodeMethodReferenceExpression();
			codeMethodReferenceExpression.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			CodeExpression[] codeExpressionArray1 = new CodeExpression[1];
			codeExpressionArray1[0] = this.coce;
			this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(codeMethodReferenceExpression, codeExpressionArray1)));
			this.cmm.Statements.Add(this.cis);
			this.cc.Members.Add(this.cmm);
		}

		private void GenerateInitializeObject()
		{
			string str = "path";
			string str1 = "getOptions";
			bool flag = true;
			try
			{
				this.classobj.Qualifiers["priveleges"].ToString();
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (managementException.ErrorCode != ManagementStatus.NotFound)
				{
					throw;
				}
				else
				{
					flag = false;
				}
			}
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["InitialObjectFunc"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str1));
			this.InitPrivateMemberVariables(codeMemberMethod);
			this.cis = new CodeConditionStatement();
			this.cis.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = this.cmie;
			this.cboe.Right = new CodePrimitiveExpression((object)(true));
			this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement.Condition = this.cboe;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
			this.coce.Parameters.Add(new CodePrimitiveExpression(ManagementClassGenerator.GetString("CLASSNOT_FOUND_EXCEPT")));
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(this.coce));
			this.cis.TrueStatements.Add(codeConditionStatement);
			codeMemberMethod.Statements.Add(this.cis);
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str));
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(str1));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), this.coce));
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
			this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString())));
			this.cc.Members.Add(codeMemberMethod);
			if (flag)
			{
				this.cpre = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), this.PublicNamesUsed["ScopeProperty"].ToString()), "Options"), "EnablePrivileges");
				this.cctor.Statements.Add(new CodeAssignStatement(this.cpre, new CodePrimitiveExpression((object)(true))));
			}
		}

		private void GenerateMethods()
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression;
			string str;
			string str1 = "inParams";
			string str2 = "outParams";
			string str3 = "classObj";
			bool flag = false;
			bool flag1 = false;
			CodePropertyReferenceExpression codePropertyReferenceExpression = null;
			CimType type = CimType.SInt8;
			CodeTypeReference returnType = null;
			bool isArray = false;
			bool dateTimeType = false;
			ArrayList arrayLists = new ArrayList(5);
			ArrayList arrayLists1 = new ArrayList(5);
			ArrayList arrayLists2 = new ArrayList(5);
			for (int i = 0; i < this.PublicMethods.Count; i++)
			{
				flag = false;
				MethodData item = this.classobj.Methods[this.PublicMethods.GetKey(i).ToString()];
				string str4 = this.PrivateNamesUsed["LateBoundObject"].ToString();
				if (item.OutParameters != null && item.OutParameters.Properties != null)
				{
					foreach (PropertyData property in item.OutParameters.Properties)
					{
						arrayLists.Add(property.Name);
					}
				}
				this.cmm = new CodeMemberMethod();
				this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
				this.cmm.Name = this.PublicMethods[item.Name].ToString();
				foreach (QualifierData qualifier in item.Qualifiers)
				{
					if (string.Compare(qualifier.Name, "static", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare(qualifier.Name, "privileges", StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						flag1 = true;
					}
					else
					{
						CodeMemberMethod attributes = this.cmm;
						attributes.Attributes = attributes.Attributes | MemberAttributes.Static;
						flag = true;
						break;
					}
				}
				this.cis = new CodeConditionStatement();
				this.cboe = new CodeBinaryOperatorExpression();
				if (!flag)
				{
					this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
					this.cboe.Right = new CodePrimitiveExpression((object)(false));
				}
				else
				{
					this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.Boolean", "IsMethodStatic", new CodePrimitiveExpression((object)flag)));
					this.cboe.Left = new CodeVariableReferenceExpression("IsMethodStatic");
					this.cboe.Right = new CodePrimitiveExpression((object)(true));
				}
				this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
				this.cis.Condition = this.cboe;
				bool flag2 = true;
				this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), str1, new CodePrimitiveExpression(null)));
				if (flag)
				{
					string str5 = "mgmtPath";
					CodeObjectCreateExpression codeObjectCreateExpression = new CodeObjectCreateExpression();
					codeObjectCreateExpression.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
					codeObjectCreateExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString()));
					this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str5, codeObjectCreateExpression));
					CodeObjectCreateExpression codeTypeReference = new CodeObjectCreateExpression();
					codeTypeReference.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
					codeTypeReference.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()));
					codeTypeReference.Parameters.Add(new CodeVariableReferenceExpression(str5));
					codeTypeReference.Parameters.Add(new CodePrimitiveExpression(null));
					this.coce = new CodeObjectCreateExpression();
					this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
					this.coce.Parameters.Add(codeTypeReference);
					this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString()), str3, codeTypeReference));
					str4 = str3;
				}
				if (flag1)
				{
					if (flag)
					{
						str = str3;
					}
					else
					{
						str = this.PrivateNamesUsed["LateBoundObject"].ToString();
					}
					codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str), this.PublicNamesUsed["ScopeProperty"].ToString()), "Options"), "EnablePrivileges");
					this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement("System.Boolean", this.PrivateNamesUsed["Privileges"].ToString(), codePropertyReferenceExpression));
					this.cis.TrueStatements.Add(new CodeAssignStatement(codePropertyReferenceExpression, new CodePrimitiveExpression((object)(true))));
				}
				if (item.InParameters != null && item.InParameters.Properties != null)
				{
					foreach (PropertyData propertyDatum in item.InParameters.Properties)
					{
						dateTimeType = false;
						if (flag2)
						{
							CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
							codePrimitiveExpression[0] = new CodePrimitiveExpression(item.Name);
							this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str4), "GetMethodParameters", codePrimitiveExpression);
							this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str1), this.cmie));
							flag2 = false;
						}
						this.cpde = new CodeParameterDeclarationExpression();
						this.cpde.Name = propertyDatum.Name;
						this.cpde.Type = this.ConvertCIMType(propertyDatum.Type, propertyDatum.IsArray);
						this.cpde.Direction = FieldDirection.In;
						if (propertyDatum.Type == CimType.DateTime)
						{
							CodeTypeReference type1 = this.cpde.Type;
							dateTimeType = this.GetDateTimeType(propertyDatum, ref type1);
							this.cpde.Type = type1;
						}
						for (int j = 0; j < arrayLists.Count; j++)
						{
							if (string.Compare(propertyDatum.Name, arrayLists[j].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
							{
								this.cpde.Direction = FieldDirection.Ref;
								arrayLists1.Add(propertyDatum.Name);
								arrayLists2.Add(this.cpde.Type);
							}
						}
						this.cmm.Parameters.Add(this.cpde);
						CodeExpression[] codeExpressionArray = new CodeExpression[1];
						codeExpressionArray[0] = new CodePrimitiveExpression(propertyDatum.Name);
						this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(str1), codeExpressionArray);
						if (propertyDatum.Type != CimType.Reference)
						{
							if (propertyDatum.Type != CimType.DateTime)
							{
								if (this.cpde.Type.ArrayRank != 0)
								{
									this.cis.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodeCastExpression(this.cpde.Type, new CodeVariableReferenceExpression(this.cpde.Name))));
								}
								else
								{
									this.cis.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodeCastExpression(new CodeTypeReference(string.Concat(this.cpde.Type.BaseType, " ")), new CodeVariableReferenceExpression(this.cpde.Name))));
								}
							}
							else
							{
								if (!dateTimeType)
								{
									this.AddPropertySet(this.cie, propertyDatum.IsArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression(this.cpde.Name));
								}
								else
								{
									this.AddPropertySet(this.cie, propertyDatum.IsArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression(this.cpde.Name));
								}
							}
						}
						else
						{
							this.AddPropertySet(this.cie, propertyDatum.IsArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression(this.cpde.Name));
						}
					}
				}
				arrayLists.Clear();
				bool flag3 = false;
				flag2 = true;
				bool flag4 = false;
				if (item.OutParameters != null && item.OutParameters.Properties != null)
				{
					foreach (PropertyData property1 in item.OutParameters.Properties)
					{
						dateTimeType = false;
						if (flag2)
						{
							this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str4), "InvokeMethod", new CodeExpression[0]);
							this.cmie.Parameters.Add(new CodePrimitiveExpression(item.Name));
							this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
							this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
							this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), str2, this.cmie));
							flag2 = false;
							flag4 = true;
						}
						bool flag5 = false;
						for (int k = 0; k < arrayLists1.Count; k++)
						{
							if (string.Compare(property1.Name, arrayLists1[k].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
							{
								flag5 = true;
							}
						}
						if (flag5)
						{
							continue;
						}
						if (string.Compare(property1.Name, "ReturnValue", StringComparison.OrdinalIgnoreCase) != 0)
						{
							this.cpde = new CodeParameterDeclarationExpression();
							this.cpde.Name = property1.Name;
							this.cpde.Type = this.ConvertCIMType(property1.Type, property1.IsArray);
							this.cpde.Direction = FieldDirection.Out;
							this.cmm.Parameters.Add(this.cpde);
							if (property1.Type == CimType.DateTime)
							{
								CodeTypeReference codeTypeReference1 = this.cpde.Type;
								dateTimeType = this.GetDateTimeType(property1, ref codeTypeReference1);
								this.cpde.Type = codeTypeReference1;
							}
							this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
							CodeExpression[] codePrimitiveExpression1 = new CodeExpression[1];
							codePrimitiveExpression1[0] = new CodePrimitiveExpression(property1.Name);
							this.cie = new CodeIndexerExpression(this.cpre, codePrimitiveExpression1);
							if (property1.Type != CimType.Reference)
							{
								if (property1.Type != CimType.DateTime)
								{
									if (property1.IsArray || property1.Type == CimType.Object)
									{
										this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), new CodeCastExpression(this.ConvertCIMType(property1.Type, property1.IsArray), new CodePropertyReferenceExpression(this.cie, "Value"))));
									}
									else
									{
										codeMethodInvokeExpression = new CodeMethodInvokeExpression();
										codeMethodInvokeExpression.Parameters.Add(new CodePropertyReferenceExpression(this.cie, "Value"));
										codeMethodInvokeExpression.Method.MethodName = this.GetConversionFunction(property1.Type);
										codeMethodInvokeExpression.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
										this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), codeMethodInvokeExpression));
									}
								}
								else
								{
									if (!dateTimeType)
									{
										this.GenerateCodeForRefAndDateTimeTypes(this.cie, property1.IsArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression(property1.Name), true);
									}
									else
									{
										this.GenerateCodeForRefAndDateTimeTypes(this.cie, property1.IsArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression(property1.Name), true);
									}
								}
							}
							else
							{
								this.GenerateCodeForRefAndDateTimeTypes(this.cie, property1.IsArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression(property1.Name), true);
							}
							if (property1.Type != CimType.DateTime || property1.IsArray)
							{
								if (!ManagementClassGenerator.IsPropertyValueType(property1.Type) || property1.IsArray)
								{
									this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), new CodePrimitiveExpression(null)));
								}
								else
								{
									codeMethodInvokeExpression = new CodeMethodInvokeExpression();
									codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)0));
									codeMethodInvokeExpression.Method.MethodName = this.GetConversionFunction(property1.Type);
									codeMethodInvokeExpression.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
									this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), codeMethodInvokeExpression));
								}
							}
							else
							{
								if (!dateTimeType)
								{
									this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
								}
								else
								{
									this.coce = new CodeObjectCreateExpression();
									this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
									this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property1.Name), this.coce));
								}
							}
						}
						else
						{
							this.cmm.ReturnType = this.ConvertCIMType(property1.Type, property1.IsArray);
							flag3 = true;
							type = property1.Type;
							if (property1.Type == CimType.DateTime)
							{
								CodeTypeReference returnType1 = this.cmm.ReturnType;
								this.GetDateTimeType(property1, ref returnType1);
								this.cmm.ReturnType = returnType1;
							}
							returnType = this.cmm.ReturnType;
							isArray = property1.IsArray;
						}
					}
				}
				if (!flag4)
				{
					this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str4), "InvokeMethod", new CodeExpression[0]);
					this.cmie.Parameters.Add(new CodePrimitiveExpression(item.Name));
					this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
					this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
					this.cmis = new CodeExpressionStatement(this.cmie);
					this.cis.TrueStatements.Add(this.cmis);
				}
				int num = 0;
				while (num < arrayLists1.Count)
				{
					this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
					CodeExpression[] codeExpressionArray1 = new CodeExpression[1];
					codeExpressionArray1[0] = new CodePrimitiveExpression(arrayLists1[num].ToString());
					this.cie = new CodeIndexerExpression(this.cpre, codeExpressionArray1);
					this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(arrayLists1[num].ToString()), new CodeCastExpression((CodeTypeReference)arrayLists2[num], new CodePropertyReferenceExpression(this.cie, "Value"))));
					num++;
				}
				arrayLists1.Clear();
				if (flag1)
				{
					this.cis.TrueStatements.Add(new CodeAssignStatement(codePropertyReferenceExpression, new CodeVariableReferenceExpression(this.PrivateNamesUsed["Privileges"].ToString())));
				}
				if (flag3)
				{
					CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement(returnType, "retVar");
					this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
					CodeExpression[] codePrimitiveExpression2 = new CodeExpression[1];
					codePrimitiveExpression2[0] = new CodePrimitiveExpression("ReturnValue");
					this.cie = new CodeIndexerExpression(this.cpre, codePrimitiveExpression2);
					if (returnType.BaseType != (new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString())).BaseType)
					{
						if (returnType.BaseType != "System.DateTime")
						{
							if (returnType.BaseType != "System.TimeSpan")
							{
								if (returnType.ArrayRank != 0 || !(returnType.BaseType != (new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString())).BaseType))
								{
									this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(returnType, new CodePropertyReferenceExpression(this.cie, "Value"))));
									this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
								}
								else
								{
									this.cmie = new CodeMethodInvokeExpression();
									this.cmie.Parameters.Add(new CodePropertyReferenceExpression(this.cie, "Value"));
									this.cmie.Method.MethodName = this.GetConversionFunction(type);
									this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
									this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cmie));
									this.cmie = new CodeMethodInvokeExpression();
									this.cmie.Parameters.Add(new CodePrimitiveExpression((object)0));
									this.cmie.Method.MethodName = this.GetConversionFunction(type);
									this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
									this.cis.FalseStatements.Add(new CodeMethodReturnStatement(this.cmie));
								}
							}
							else
							{
								this.cmm.Statements.Add(codeVariableDeclarationStatement);
								this.coce = new CodeObjectCreateExpression();
								this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
								this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
								this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
								this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
								this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
								this.coce.Parameters.Add(new CodePrimitiveExpression((object)0));
								this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), this.coce));
								this.GenerateCodeForRefAndDateTimeTypes(this.cie, isArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression("retVar"), true);
								this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
								this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
							}
						}
						else
						{
							this.cmm.Statements.Add(codeVariableDeclarationStatement);
							this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
							this.GenerateCodeForRefAndDateTimeTypes(this.cie, isArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression("retVar"), true);
							this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
							this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
						}
					}
					else
					{
						this.cmm.Statements.Add(codeVariableDeclarationStatement);
						this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), new CodePrimitiveExpression(null)));
						this.GenerateCodeForRefAndDateTimeTypes(this.cie, isArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression("retVar"), true);
						this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
						this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
					}
				}
				this.cmm.Statements.Add(this.cis);
				this.cc.Members.Add(this.cmm);
			}
		}

		private void GenerateMethodToInitializeVariables()
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = this.PrivateNamesUsed["initVariable"].ToString();
			codeMemberMethod.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), new CodePrimitiveExpression((object)(true))));
			codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), new CodePrimitiveExpression((object)(false))));
			this.cc.Members.Add(codeMemberMethod);
		}

		private void GeneratePathProperty()
		{
			this.cmp = new CodeMemberProperty();
			this.cmp.Name = this.PublicNamesUsed["PathProperty"].ToString();
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Type = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmp.CustomAttributes.Add(this.cad);
			this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), "Path");
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cpre));
			this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
			this.cmp.GetStatements.Add(this.cis);
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			this.cmie = new CodeMethodInvokeExpression();
			this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression("value"));
			this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = this.cmie;
			codeBinaryOperatorExpression.Right = new CodePrimitiveExpression((object)(true));
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
			codeConditionStatement.Condition = codeBinaryOperatorExpression;
			this.coce = new CodeObjectCreateExpression();
			this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
			this.coce.Parameters.Add(new CodePrimitiveExpression(ManagementClassGenerator.GetString("CLASSNOT_FOUND_EXCEPT")));
			codeConditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(this.coce));
			this.cis.TrueStatements.Add(codeConditionStatement);
			this.cis.TrueStatements.Add(new CodeAssignStatement(this.cpre, new CodeSnippetExpression("value")));
			this.cmp.SetStatements.Add(this.cis);
			this.cc.Members.Add(this.cmp);
			this.cmp.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_MGMTPATH")));
		}

		private void GeneratePrivateMember(string memberName, string MemberType, string Comment)
		{
			this.GeneratePrivateMember(memberName, MemberType, null, false, Comment);
		}

		private void GeneratePrivateMember(string memberName, string MemberType, CodeExpression initExpression, bool isStatic, string Comment)
		{
			this.cf = new CodeMemberField();
			this.cf.Name = memberName;
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			if (isStatic)
			{
				this.cf.Attributes = this.cf.Attributes | MemberAttributes.Static;
			}
			this.cf.Type = new CodeTypeReference(MemberType);
			if (initExpression != null && isStatic)
			{
				this.cf.InitExpression = initExpression;
			}
			this.cc.Members.Add(this.cf);
			if (Comment != null && Comment.Length != 0)
			{
				this.cf.Comments.Add(new CodeCommentStatement(Comment));
			}
		}

		private void GenerateProperties()
		{
			bool flag = this.IsDynamicClass();
			CodeMemberMethod codeMemberMethod = null;
			bool dateTimeType = false;
			for (int i = 0; i < this.PublicProperties.Count; i++)
			{
				dateTimeType = false;
				PropertyData item = this.classobj.Properties[this.PublicProperties.GetKey(i).ToString()];
				bool flag1 = true;
				bool flag2 = true;
				bool flag3 = false;
				this.cmp = new CodeMemberProperty();
				this.cmp.Name = this.PublicProperties[item.Name].ToString();
				this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
				this.cmp.Type = this.ConvertCIMType(item.Type, item.IsArray);
				if (item.Type == CimType.DateTime)
				{
					CodeTypeReference type = this.cmp.Type;
					dateTimeType = this.GetDateTimeType(item, ref type);
					this.cmp.Type = type;
				}
				if (this.cmp.Type.ArrayRank == 0 && this.cmp.Type.BaseType == (new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString())).BaseType || this.cmp.Type.ArrayRank > 0 && this.cmp.Type.ArrayElementType.BaseType == (new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString())).BaseType)
				{
					this.bHasEmbeddedProperties = true;
				}
				string str = string.Concat("Is", this.PublicProperties[item.Name].ToString(), "Null");
				CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
				codeMemberProperty.Name = str;
				codeMemberProperty.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
				codeMemberProperty.Type = new CodeTypeReference("System.Boolean");
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodePrimitiveExpression((object)(true));
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "Browsable";
				this.cad.Arguments.Add(this.caa);
				this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
				this.cmp.CustomAttributes.Add(this.cad);
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodePrimitiveExpression((object)(false));
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "Browsable";
				this.cad.Arguments.Add(this.caa);
				codeMemberProperty.CustomAttributes = new CodeAttributeDeclarationCollection();
				codeMemberProperty.CustomAttributes.Add(this.cad);
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "DesignerSerializationVisibility";
				this.cad.Arguments.Add(this.caa);
				this.cmp.CustomAttributes.Add(this.cad);
				codeMemberProperty.CustomAttributes.Add(this.cad);
				CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
				codePrimitiveExpression[0] = new CodePrimitiveExpression(item.Name);
				this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), codePrimitiveExpression);
				bool flag4 = false;
				string str1 = this.ProcessPropertyQualifiers(item, ref flag1, ref flag2, ref flag3, flag, out flag4);
				if (flag1 || flag2)
				{
					if (str1.Length != 0)
					{
						this.caa = new CodeAttributeArgument();
						this.caa.Value = new CodePrimitiveExpression(str1);
						this.cad = new CodeAttributeDeclaration();
						this.cad.Name = "Description";
						this.cad.Arguments.Add(this.caa);
						this.cmp.CustomAttributes.Add(this.cad);
					}
					bool flag5 = this.GeneratePropertyHelperEnums(item, this.PublicProperties[item.Name].ToString(), flag4);
					if (flag1)
					{
						if (ManagementClassGenerator.IsPropertyValueType(item.Type) && !item.IsArray)
						{
							this.cis = new CodeConditionStatement();
							this.cis.Condition = new CodeBinaryOperatorExpression(this.cie, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
							this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(true))));
							this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(false))));
							codeMemberProperty.GetStatements.Add(this.cis);
							this.cc.Members.Add(codeMemberProperty);
							this.caa = new CodeAttributeArgument();
							this.caa.Value = new CodeTypeOfExpression(this.PrivateNamesUsed["ConverterClass"].ToString());
							this.cad = new CodeAttributeDeclaration();
							this.cad.Name = this.PublicNamesUsed["TypeConverter"].ToString();
							this.cad.Arguments.Add(this.caa);
							this.cmp.CustomAttributes.Add(this.cad);
							if (item.Type != CimType.DateTime)
							{
								this.cis = new CodeConditionStatement();
								this.cis.Condition = new CodeBinaryOperatorExpression(this.cie, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
								if (!flag5)
								{
									this.cmie = new CodeMethodInvokeExpression();
									this.cmie.Parameters.Add(new CodePrimitiveExpression((object)item.NullEnumValue));
									this.cmie.Method.MethodName = this.GetConversionFunction(item.Type);
									this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
									if (!item.IsArray)
									{
										this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cmie));
									}
									else
									{
										CodeExpression[] codeExpressionArray = new CodeExpression[1];
										codeExpressionArray[0] = this.cmie;
										CodeExpression[] codeExpressionArray1 = codeExpressionArray;
										this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(this.cmp.Type, codeExpressionArray1)));
									}
								}
								else
								{
									if (!item.IsArray)
									{
										this.cmie = new CodeMethodInvokeExpression();
										this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
										this.cmie.Parameters.Add(new CodePrimitiveExpression((object)item.NullEnumValue));
										this.cmie.Method.MethodName = this.arrConvFuncName;
										this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cmie)));
									}
									else
									{
										this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
									}
								}
								this.cmp.GetStatements.Add(this.cis);
							}
							this.cmm = new CodeMemberMethod();
							this.cmm.Name = string.Concat("ShouldSerialize", this.PublicProperties[item.Name].ToString());
							this.cmm.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
							this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
							CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
							codeConditionStatement.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), str), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(false)));
							codeConditionStatement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(true))));
							this.cmm.Statements.Add(codeConditionStatement);
							this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((object)(false))));
							this.cc.Members.Add(this.cmm);
						}
						if (item.Type != CimType.Reference)
						{
							if (item.Type != CimType.DateTime)
							{
								if (!flag5)
								{
									this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cie)));
								}
								else
								{
									if (!item.IsArray)
									{
										this.cmie = new CodeMethodInvokeExpression();
										this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
										this.cmie.Parameters.Add(this.cie);
										this.cmie.Method.MethodName = this.arrConvFuncName;
										this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cmie)));
									}
									else
									{
										this.AddGetStatementsForEnumArray(this.cie, this.cmp);
									}
								}
							}
							else
							{
								if (!dateTimeType)
								{
									this.GenerateCodeForRefAndDateTimeTypes(this.cie, item.IsArray, this.cmp.GetStatements, "System.DateTime", null, false);
								}
								else
								{
									this.GenerateCodeForRefAndDateTimeTypes(this.cie, item.IsArray, this.cmp.GetStatements, "System.TimeSpan", null, false);
								}
							}
						}
						else
						{
							this.GenerateCodeForRefAndDateTimeTypes(this.cie, item.IsArray, this.cmp.GetStatements, this.PublicNamesUsed["PathClass"].ToString(), null, false);
						}
					}
					if (flag2)
					{
						if (flag4)
						{
							codeMemberMethod = new CodeMemberMethod();
							codeMemberMethod.Name = string.Concat("Reset", this.PublicProperties[item.Name].ToString());
							codeMemberMethod.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
							codeMemberMethod.Statements.Add(new CodeAssignStatement(this.cie, new CodePrimitiveExpression(null)));
						}
						if (item.Type != CimType.Reference)
						{
							if (item.Type != CimType.DateTime)
							{
								if (!flag5 || !flag4)
								{
									this.cmp.SetStatements.Add(new CodeAssignStatement(this.cie, new CodeSnippetExpression("value")));
								}
								else
								{
									CodeConditionStatement codeBinaryOperatorExpression = new CodeConditionStatement();
									if (!item.IsArray)
									{
										codeBinaryOperatorExpression.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(string.Concat(this.PublicProperties[item.Name].ToString(), "Values"))), "NULL_ENUM_VALUE"), CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("value"));
									}
									else
									{
										CodeExpression[] codePrimitiveExpression1 = new CodeExpression[1];
										codePrimitiveExpression1[0] = new CodePrimitiveExpression((object)0);
										codeBinaryOperatorExpression.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(string.Concat(this.PublicProperties[item.Name].ToString(), "Values"))), "NULL_ENUM_VALUE"), CodeBinaryOperatorType.ValueEquality, new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("value"), codePrimitiveExpression1));
									}
									codeBinaryOperatorExpression.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodePrimitiveExpression(null)));
									codeBinaryOperatorExpression.FalseStatements.Add(new CodeAssignStatement(this.cie, new CodeSnippetExpression("value")));
									this.cmp.SetStatements.Add(codeBinaryOperatorExpression);
								}
							}
							else
							{
								if (!dateTimeType)
								{
									this.AddPropertySet(this.cie, item.IsArray, this.cmp.SetStatements, "System.DateTime", null);
								}
								else
								{
									this.AddPropertySet(this.cie, item.IsArray, this.cmp.SetStatements, "System.TimeSpan", null);
								}
							}
						}
						else
						{
							this.AddPropertySet(this.cie, item.IsArray, this.cmp.SetStatements, this.PublicNamesUsed["PathClass"].ToString(), null);
						}
						this.cmie = new CodeMethodInvokeExpression();
						this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
						this.cmie.Method.MethodName = "Put";
						this.cboe = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(true)));
						CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(false)));
						CodeBinaryOperatorExpression codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression();
						codeBinaryOperatorExpression2.Right = this.cboe;
						codeBinaryOperatorExpression2.Left = codeBinaryOperatorExpression1;
						codeBinaryOperatorExpression2.Operator = CodeBinaryOperatorType.BooleanAnd;
						this.cis = new CodeConditionStatement();
						this.cis.Condition = codeBinaryOperatorExpression2;
						this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
						this.cmp.SetStatements.Add(this.cis);
						if (flag4)
						{
							codeMemberMethod.Statements.Add(this.cis);
						}
					}
					this.cc.Members.Add(this.cmp);
					if (flag4 & flag2)
					{
						this.cc.Members.Add(codeMemberMethod);
					}
				}
			}
			this.GenerateCommitMethod();
		}

		private bool GeneratePropertyHelperEnums(PropertyData prop, string strPropertyName, bool bNullable)
		{
			bool flag = false;
			bool flag1 = false;
			string str = this.ResolveCollision(string.Concat(strPropertyName, "Values"), true);
			if (this.Values.Count > 0 && (this.ValueMap.Count == 0 || this.ValueMap.Count == this.Values.Count))
			{
				if (this.ValueMap.Count == 0)
				{
					flag1 = true;
				}
				this.EnumObj = new CodeTypeDeclaration(str);
				if (!prop.IsArray)
				{
					this.cmp.Type = new CodeTypeReference(str);
				}
				else
				{
					this.cmp.Type = new CodeTypeReference(str, 1);
				}
				this.EnumObj.IsEnum = true;
				this.EnumObj.TypeAttributes = TypeAttributes.Public;
				long num = (long)0;
				for (int i = 0; i < this.Values.Count; i++)
				{
					this.cmf = new CodeMemberField();
					this.cmf.Name = this.Values[i].ToString();
					if (this.ValueMap.Count <= 0)
					{
						this.cmf.InitExpression = new CodePrimitiveExpression((object)i);
						if ((long)i > num)
						{
							num = (long)i;
						}
					}
					else
					{
						this.cmf.InitExpression = new CodePrimitiveExpression(this.ValueMap[i]);
						long num1 = Convert.ToInt64(this.ValueMap[i], (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(ulong)));
						if (num1 > num)
						{
							num = num1;
						}
						if (!flag1 && Convert.ToInt64(this.ValueMap[i], (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(ulong))) == (long)0)
						{
							flag1 = true;
						}
					}
					this.EnumObj.Members.Add(this.cmf);
				}
				if (!bNullable || flag1)
				{
					if (!bNullable || !flag1)
					{
						if (!bNullable && !flag1)
						{
							this.cmf = new CodeMemberField();
							this.cmf.Name = "INVALID_ENUM_VALUE";
							this.cmf.InitExpression = new CodePrimitiveExpression((object)0);
							this.EnumObj.Members.Add(this.cmf);
							prop.NullEnumValue = (long)0;
						}
					}
					else
					{
						this.cmf = new CodeMemberField();
						this.cmf.Name = "NULL_ENUM_VALUE";
						this.cmf.InitExpression = new CodePrimitiveExpression((object)((int)(num + (long)1)));
						this.EnumObj.Members.Add(this.cmf);
						prop.NullEnumValue = (long)((int)(num + (long)1));
					}
				}
				else
				{
					this.cmf = new CodeMemberField();
					this.cmf.Name = "NULL_ENUM_VALUE";
					this.cmf.InitExpression = new CodePrimitiveExpression((object)0);
					this.EnumObj.Members.Add(this.cmf);
					prop.NullEnumValue = (long)0;
				}
				this.cc.Members.Add(this.EnumObj);
				flag = true;
			}
			this.Values.Clear();
			this.ValueMap.Clear();
			flag1 = false;
			if (this.BitValues.Count > 0 && (this.BitMap.Count == 0 || this.BitMap.Count == this.BitValues.Count))
			{
				if (this.BitMap.Count == 0)
				{
					flag1 = true;
				}
				this.EnumObj = new CodeTypeDeclaration(str);
				if (!prop.IsArray)
				{
					this.cmp.Type = new CodeTypeReference(str);
				}
				else
				{
					this.cmp.Type = new CodeTypeReference(str, 1);
				}
				this.EnumObj.IsEnum = true;
				this.EnumObj.TypeAttributes = TypeAttributes.Public;
				int num2 = 1;
				long num3 = (long)0;
				for (int j = 0; j < this.BitValues.Count; j++)
				{
					this.cmf = new CodeMemberField();
					this.cmf.Name = this.BitValues[j].ToString();
					if (this.BitMap.Count <= 0)
					{
						this.cmf.InitExpression = new CodePrimitiveExpression((object)num2);
						if ((long)num2 > num3)
						{
							num3 = (long)num2;
						}
						num2 = num2 << 1;
					}
					else
					{
						this.cmf.InitExpression = new CodePrimitiveExpression(this.BitMap[j]);
						long num4 = Convert.ToInt64(this.BitMap[j], (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(ulong)));
						if (num4 > num3)
						{
							num3 = num4;
						}
					}
					if (!flag1 && Convert.ToInt64(this.BitMap[j], (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(ulong))) == (long)0)
					{
						flag1 = true;
					}
					this.EnumObj.Members.Add(this.cmf);
				}
				if (!bNullable || flag1)
				{
					if (!bNullable || !flag1)
					{
						if (!bNullable && !flag1)
						{
							this.cmf = new CodeMemberField();
							this.cmf.Name = "INVALID_ENUM_VALUE";
							this.cmf.InitExpression = new CodePrimitiveExpression((object)0);
							this.EnumObj.Members.Add(this.cmf);
							prop.NullEnumValue = (long)0;
						}
					}
					else
					{
						this.cmf = new CodeMemberField();
						this.cmf.Name = "NULL_ENUM_VALUE";
						if (this.BitValues.Count <= 30)
						{
							num3 = num3 << 1;
						}
						else
						{
							num3 = num3 + (long)1;
						}
						this.cmf.InitExpression = new CodePrimitiveExpression((object)((int)num3));
						this.EnumObj.Members.Add(this.cmf);
						prop.NullEnumValue = (long)((int)num3);
					}
				}
				else
				{
					this.cmf = new CodeMemberField();
					this.cmf.Name = "NULL_ENUM_VALUE";
					this.cmf.InitExpression = new CodePrimitiveExpression((object)0);
					this.EnumObj.Members.Add(this.cmf);
					prop.NullEnumValue = (long)0;
				}
				this.cc.Members.Add(this.EnumObj);
				flag = true;
			}
			this.BitValues.Clear();
			this.BitMap.Clear();
			return flag;
		}

		private void GeneratePublicProperty(string propName, string propType, CodeExpression Value, bool isBrowsable, string Comment, bool isStatic)
		{
			this.cmp = new CodeMemberProperty();
			this.cmp.Name = propName;
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Type = new CodeTypeReference(propType);
			if (isStatic)
			{
				this.cmp.Attributes = this.cmp.Attributes | MemberAttributes.Static;
			}
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)isBrowsable);
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmp.CustomAttributes.Add(this.cad);
			if (ManagementClassGenerator.IsDesignerSerializationVisibilityToBeSet(propName))
			{
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "DesignerSerializationVisibility";
				this.cad.Arguments.Add(this.caa);
				this.cmp.CustomAttributes.Add(this.cad);
			}
			this.cmp.GetStatements.Add(new CodeMethodReturnStatement(Value));
			this.cmp.SetStatements.Add(new CodeAssignStatement(Value, new CodeSnippetExpression("value")));
			this.cc.Members.Add(this.cmp);
			if (Comment != null && Comment.Length != 0)
			{
				this.cmp.Comments.Add(new CodeCommentStatement(Comment));
			}
		}

		private void GeneratePublicReadOnlyProperty(string propName, string propType, object propValue, bool isLiteral, bool isBrowsable, string Comment)
		{
			this.cmp = new CodeMemberProperty();
			this.cmp.Name = propName;
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Type = new CodeTypeReference(propType);
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)isBrowsable);
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmp.CustomAttributes.Add(this.cad);
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "DesignerSerializationVisibility";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes.Add(this.cad);
			if (!isLiteral)
			{
				this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(propValue)));
			}
			else
			{
				this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(propValue.ToString())));
			}
			this.cc.Members.Add(this.cmp);
			if (Comment != null && Comment.Length != 0)
			{
				this.cmp.Comments.Add(new CodeCommentStatement(Comment));
			}
		}

		private void GenerateScopeProperty()
		{
			this.cmp = new CodeMemberProperty();
			this.cmp.Name = this.PublicNamesUsed["ScopeProperty"].ToString();
			this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmp.Type = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodePrimitiveExpression((object)(true));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = "Browsable";
			this.cad.Arguments.Add(this.caa);
			this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
			this.cmp.CustomAttributes.Add(this.cad);
			if (ManagementClassGenerator.IsDesignerSerializationVisibilityToBeSet(this.PublicNamesUsed["ScopeProperty"].ToString()))
			{
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "DesignerSerializationVisibility";
				this.cad.Arguments.Add(this.caa);
				this.cmp.CustomAttributes.Add(this.cad);
			}
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			CodeExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), "Scope");
			this.cis.TrueStatements.Add(new CodeMethodReturnStatement(codePropertyReferenceExpression));
			this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
			this.cmp.GetStatements.Add(this.cis);
			this.cis = new CodeConditionStatement();
			this.cboe = new CodeBinaryOperatorExpression();
			this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
			this.cboe.Right = new CodePrimitiveExpression((object)(false));
			this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
			this.cis.Condition = this.cboe;
			this.cis.TrueStatements.Add(new CodeAssignStatement(codePropertyReferenceExpression, new CodeSnippetExpression("value")));
			this.cmp.SetStatements.Add(this.cis);
			this.cc.Members.Add(this.cmp);
			this.cmp.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_MGMTSCOPE")));
		}

		private CodeTypeDeclaration GenerateSystemPropertiesClass()
		{
			CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
			codeTypeDeclaration.TypeAttributes = TypeAttributes.NestedPublic;
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
			this.cpde.Name = "ManagedObject";
			this.cctor.Parameters.Add(this.cpde);
			this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), new CodeVariableReferenceExpression("ManagedObject")));
			codeTypeDeclaration.Members.Add(this.cctor);
			this.caa = new CodeAttributeArgument();
			this.caa.Value = new CodeTypeOfExpression(typeof(ExpandableObjectConverter));
			this.cad = new CodeAttributeDeclaration();
			this.cad.Name = this.PublicNamesUsed["TypeConverter"].ToString();
			this.cad.Arguments.Add(this.caa);
			codeTypeDeclaration.CustomAttributes.Add(this.cad);
			int num = 0;
			foreach (PropertyData systemProperty in this.classobj.SystemProperties)
			{
				this.cmp = new CodeMemberProperty();
				this.caa = new CodeAttributeArgument();
				this.caa.Value = new CodePrimitiveExpression((object)(true));
				this.cad = new CodeAttributeDeclaration();
				this.cad.Name = "Browsable";
				this.cad.Arguments.Add(this.caa);
				this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
				this.cmp.CustomAttributes.Add(this.cad);
				char[] charArray = systemProperty.Name.ToCharArray();
				num = 0;
				while (num < (int)charArray.Length && !char.IsLetterOrDigit(charArray[num]))
				{
					num++;
				}
				if (num == (int)charArray.Length)
				{
					num = 0;
				}
				char[] chrArray = new char[(int)charArray.Length - num];
				for (int i = num; i < (int)charArray.Length; i++)
				{
					chrArray[i - num] = charArray[i];
				}
				this.cmp.Name = (new string(chrArray)).ToUpper(CultureInfo.InvariantCulture);
				this.cmp.Attributes = MemberAttributes.Final | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
				this.cmp.Type = this.ConvertCIMType(systemProperty.Type, systemProperty.IsArray);
				CodeExpression[] codePrimitiveExpression = new CodeExpression[1];
				codePrimitiveExpression[0] = new CodePrimitiveExpression(systemProperty.Name);
				this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), codePrimitiveExpression);
				this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cie)));
				codeTypeDeclaration.Members.Add(this.cmp);
			}
			this.cf = new CodeMemberField();
			this.cf.Name = this.PrivateNamesUsed["LateBoundObject"].ToString();
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
			codeTypeDeclaration.Members.Add(this.cf);
			codeTypeDeclaration.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_SYSPROPCLASS")));
			return codeTypeDeclaration;
		}

		private void GenerateTimeSpanConversionFunction()
		{
			this.AddToTimeSpanFunction();
			this.AddToDMTFTimeIntervalFunction();
		}

		private CodeTypeDeclaration GenerateTypeConverterClass()
		{
			string str = "System.ComponentModel.ITypeDescriptorContext";
			string str1 = "context";
			string str2 = "destinationType";
			string str3 = "value";
			string str4 = "System.Globalization.CultureInfo";
			string str5 = "culture";
			string str6 = "System.Collections.IDictionary";
			string str7 = "dictionary";
			string str8 = "PropertyDescriptorCollection";
			string str9 = "attributeVar";
			string str10 = "inBaseType";
			string str11 = "baseConverter";
			string str12 = "baseType";
			string str13 = "TypeDescriptor";
			string str14 = "srcType";
			CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(this.PrivateNamesUsed["ConverterClass"].ToString());
			codeTypeDeclaration.BaseTypes.Add(this.PublicNamesUsed["TypeConverter"].ToString());
			this.cf = new CodeMemberField();
			this.cf.Name = str11;
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["TypeConverter"].ToString());
			codeTypeDeclaration.Members.Add(this.cf);
			this.cf = new CodeMemberField();
			this.cf.Name = str12;
			this.cf.Attributes = MemberAttributes.Final | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["Type"].ToString());
			codeTypeDeclaration.Members.Add(this.cf);
			this.cctor = new CodeConstructor();
			this.cctor.Attributes = MemberAttributes.Public;
			this.cpde = new CodeParameterDeclarationExpression();
			this.cpde.Name = str10;
			this.cpde.Type = new CodeTypeReference("System.Type");
			this.cctor.Parameters.Add(this.cpde);
			this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(str13), "GetConverter", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str10));
			this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str11), this.cmie));
			this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str12), new CodeVariableReferenceExpression(str10)));
			codeTypeDeclaration.Members.Add(this.cctor);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "CanConvertFrom";
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str14));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "CanConvertFrom", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str14));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "CanConvertTo";
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str2));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "CanConvertTo", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str2));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "ConvertFrom";
			this.cmm.ReturnType = new CodeTypeReference("System.Object");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str4, str5));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str3));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "ConvertFrom", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str5));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.ReturnType = new CodeTypeReference("System.Object");
			this.cmm.Name = "CreateInstance";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str6, str7));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "CreateInstance", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str7));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetCreateInstanceSupported";
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetCreateInstanceSupported", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetProperties";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str3));
			CodeTypeReference codeTypeReference = new CodeTypeReference(new CodeTypeReference("System.Attribute"), 1);
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(codeTypeReference, str9));
			this.cmm.ReturnType = new CodeTypeReference(str8);
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetProperties", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str9));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetPropertiesSupported";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetPropertiesSupported", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetStandardValues";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.ReturnType = new CodeTypeReference("System.ComponentModel.TypeConverter.StandardValuesCollection");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetStandardValues", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetStandardValuesExclusive";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetStandardValuesExclusive", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "GetStandardValuesSupported";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "GetStandardValuesSupported", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
			codeTypeDeclaration.Members.Add(this.cmm);
			this.cmm = new CodeMemberMethod();
			this.cmm.Attributes = MemberAttributes.Override | MemberAttributes.Overloaded | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			this.cmm.Name = "ConvertTo";
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str, str1));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str4, str5));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str3));
			this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str2));
			this.cmm.ReturnType = new CodeTypeReference("System.Object");
			this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str11), "ConvertTo", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str1));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str5));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
			this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str2));
			CodeMethodReturnStatement codeMethodReturnStatement = new CodeMethodReturnStatement(this.cmie);
			this.cis = new CodeConditionStatement();
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str12), "BaseType");
			codeBinaryOperatorExpression.Right = new CodeTypeOfExpression(typeof(Enum));
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			this.cis.Condition = codeBinaryOperatorExpression;
			CodeBinaryOperatorExpression codeMethodInvokeExpression = new CodeBinaryOperatorExpression();
			codeMethodInvokeExpression.Left = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("value"), "GetType", new CodeExpression[0]);
			codeMethodInvokeExpression.Right = new CodeVariableReferenceExpression("destinationType");
			codeMethodInvokeExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodeStatement[] codeStatementArray = new CodeStatement[1];
			codeStatementArray[0] = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("value"));
			this.cis.TrueStatements.Add(new CodeConditionStatement(codeMethodInvokeExpression, codeStatementArray));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("value"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression3 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression3.Left = codeBinaryOperatorExpression1;
			codeBinaryOperatorExpression3.Right = codeBinaryOperatorExpression2;
			codeBinaryOperatorExpression3.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Instance"));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression4 = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(false)));
			CodeBinaryOperatorExpression codeBinaryOperatorExpression5 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression5.Left = codeBinaryOperatorExpression3;
			codeBinaryOperatorExpression5.Right = codeBinaryOperatorExpression4;
			codeBinaryOperatorExpression5.Operator = CodeBinaryOperatorType.BooleanAnd;
			CodeStatement[] codeMethodReturnStatement1 = new CodeStatement[1];
			codeMethodReturnStatement1[0] = new CodeMethodReturnStatement(new CodeSnippetExpression(" \"NULL_ENUM_VALUE\" "));
			this.cis.TrueStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression5, codeMethodReturnStatement1));
			this.cis.TrueStatements.Add(codeMethodReturnStatement);
			this.cmm.Statements.Add(this.cis);
			this.cis = new CodeConditionStatement();
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression.Left = new CodeVariableReferenceExpression(str12);
			codeBinaryOperatorExpression.Right = new CodeTypeOfExpression(this.PublicNamesUsed["Boolean"].ToString());
			codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			codeMethodInvokeExpression = new CodeBinaryOperatorExpression();
			codeMethodInvokeExpression.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str12), "BaseType");
			codeMethodInvokeExpression.Right = new CodeTypeOfExpression(this.PublicNamesUsed["ValueType"].ToString());
			codeMethodInvokeExpression.Operator = CodeBinaryOperatorType.IdentityEquality;
			codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeBinaryOperatorExpression;
			codeBinaryOperatorExpression1.Right = codeMethodInvokeExpression;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cis.Condition = codeBinaryOperatorExpression1;
			codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("value"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			codeBinaryOperatorExpression2 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
			codeBinaryOperatorExpression3 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression3.Left = codeBinaryOperatorExpression1;
			codeBinaryOperatorExpression3.Right = codeBinaryOperatorExpression2;
			codeBinaryOperatorExpression3.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Instance"));
			codeBinaryOperatorExpression4 = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(false)));
			codeBinaryOperatorExpression5 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression5.Left = codeBinaryOperatorExpression3;
			codeBinaryOperatorExpression5.Right = codeBinaryOperatorExpression4;
			codeBinaryOperatorExpression5.Operator = CodeBinaryOperatorType.BooleanAnd;
			CodeStatement[] codeStatementArray1 = new CodeStatement[1];
			codeStatementArray1[0] = new CodeMethodReturnStatement(new CodePrimitiveExpression(""));
			this.cis.TrueStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression5, codeStatementArray1));
			this.cis.TrueStatements.Add(codeMethodReturnStatement);
			this.cmm.Statements.Add(this.cis);
			this.cis = new CodeConditionStatement();
			codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str1), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
			this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
			this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), "Instance"));
			codeMethodInvokeExpression = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression((object)(false)));
			codeBinaryOperatorExpression1 = new CodeBinaryOperatorExpression();
			codeBinaryOperatorExpression1.Left = codeBinaryOperatorExpression;
			codeBinaryOperatorExpression1.Right = codeMethodInvokeExpression;
			codeBinaryOperatorExpression1.Operator = CodeBinaryOperatorType.BooleanAnd;
			this.cis.Condition = codeBinaryOperatorExpression1;
			this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression("")));
			this.cmm.Statements.Add(this.cis);
			this.cmm.Statements.Add(codeMethodReturnStatement);
			codeTypeDeclaration.Members.Add(this.cmm);
			codeTypeDeclaration.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_PROPTYPECONVERTER")));
			return codeTypeDeclaration;
		}

		private CodeTypeDeclaration GetCodeTypeDeclarationForClass(bool bIncludeSystemClassinClassDef)
		{
			this.cc = new CodeTypeDeclaration(this.PrivateNamesUsed["GeneratedClassName"].ToString());
			this.cc.BaseTypes.Add(new CodeTypeReference(this.PrivateNamesUsed["ComponentClass"].ToString()));
			this.AddClassComments(this.cc);
			this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["NamespaceProperty"].ToString(), "System.String", this.OriginalNamespace, false, true, ManagementClassGenerator.GetString("COMMENT_ORIGNAMESPACE"));
			this.GeneratePrivateMember(this.PrivateNamesUsed["CreationWmiNamespace"].ToString(), "System.String", new CodePrimitiveExpression(this.OriginalNamespace), true, ManagementClassGenerator.GetString("COMMENT_CREATEDWMINAMESPACE"));
			this.GenerateClassNameProperty();
			this.GeneratePrivateMember(this.PrivateNamesUsed["CreationClassName"].ToString(), "System.String", new CodePrimitiveExpression(this.OriginalClassName), true, ManagementClassGenerator.GetString("COMMENT_CREATEDCLASS"));
			this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["SystemPropertiesProperty"].ToString(), this.PublicNamesUsed["SystemPropertiesClass"].ToString(), this.PrivateNamesUsed["SystemPropertiesObject"].ToString(), true, true, ManagementClassGenerator.GetString("COMMENT_SYSOBJECT"));
			this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["LateBoundObjectProperty"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), this.PrivateNamesUsed["CurrentObject"].ToString(), true, false, ManagementClassGenerator.GetString("COMMENT_LATEBOUNDPROP"));
			this.GenerateScopeProperty();
			this.GeneratePublicProperty(this.PublicNamesUsed["AutoCommitProperty"].ToString(), "System.Boolean", new CodeSnippetExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), false, ManagementClassGenerator.GetString("COMMENT_AUTOCOMMITPROP"), false);
			this.GeneratePathProperty();
			this.GeneratePrivateMember(this.PrivateNamesUsed["statMgmtScope"].ToString(), this.PublicNamesUsed["ScopeClass"].ToString(), new CodePrimitiveExpression(null), true, ManagementClassGenerator.GetString("COMMENT_STATICMANAGEMENTSCOPE"));
			this.GeneratePublicProperty(this.PrivateNamesUsed["staticScope"].ToString(), this.PublicNamesUsed["ScopeClass"].ToString(), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()), true, ManagementClassGenerator.GetString("COMMENT_STATICSCOPEPROPERTY"), true);
			this.GenerateIfClassvalidFunction();
			this.GenerateProperties();
			this.GenerateMethodToInitializeVariables();
			this.GenerateConstructPath();
			this.GenerateDefaultConstructor();
			this.GenerateInitializeObject();
			if (!this.bSingletonClass)
			{
				this.GenerateConstructorWithKeys();
				this.GenerateConstructorWithScopeKeys();
				this.GenerateConstructorWithPathOptions();
				this.GenerateConstructorWithScopePath();
				this.GenerateGetInstancesWithNoParameters();
				this.GenerateGetInstancesWithCondition();
				this.GenerateGetInstancesWithProperties();
				this.GenerateGetInstancesWithWhereProperties();
				this.GenerateGetInstancesWithScope();
				this.GenerateGetInstancesWithScopeCondition();
				this.GenerateGetInstancesWithScopeProperties();
				this.GenerateGetInstancesWithScopeWhereProperties();
				this.GenerateCollectionClass();
			}
			else
			{
				this.GenerateConstructorWithScope();
				this.GenerateConstructorWithOptions();
				this.GenerateConstructorWithScopeOptions();
			}
			this.GenerateConstructorWithPath();
			this.GenerateConstructorWithScopePathOptions();
			this.GenarateConstructorWithLateBound();
			this.GenarateConstructorWithLateBoundForEmbedded();
			this.GenerateCreateInstance();
			this.GenerateDeleteInstance();
			this.GenerateMethods();
			this.GeneratePrivateMember(this.PrivateNamesUsed["SystemPropertiesObject"].ToString(), this.PublicNamesUsed["SystemPropertiesClass"].ToString(), null);
			this.GeneratePrivateMember(this.PrivateNamesUsed["LateBoundObject"].ToString(), this.PublicNamesUsed["LateBoundClass"].ToString(), ManagementClassGenerator.GetString("COMMENT_LATEBOUNDOBJ"));
			this.GeneratePrivateMember(this.PrivateNamesUsed["AutoCommitProperty"].ToString(), "System.Boolean", new CodePrimitiveExpression((object)(true)), false, ManagementClassGenerator.GetString("COMMENT_PRIVAUTOCOMMIT"));
			this.GeneratePrivateMember(this.PrivateNamesUsed["EmbeddedObject"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), ManagementClassGenerator.GetString("COMMENT_EMBEDDEDOBJ"));
			this.GeneratePrivateMember(this.PrivateNamesUsed["CurrentObject"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), ManagementClassGenerator.GetString("COMMENT_CURRENTOBJ"));
			this.GeneratePrivateMember(this.PrivateNamesUsed["IsEmbedded"].ToString(), "System.Boolean", new CodePrimitiveExpression((object)(false)), false, ManagementClassGenerator.GetString("COMMENT_FLAGFOREMBEDDED"));
			this.cc.Members.Add(this.GenerateTypeConverterClass());
			if (bIncludeSystemClassinClassDef)
			{
				this.cc.Members.Add(this.GenerateSystemPropertiesClass());
			}
			if (this.bHasEmbeddedProperties)
			{
				this.AddCommentsForEmbeddedProperties();
			}
			this.cc.Comments.Add(new CodeCommentStatement(string.Concat(ManagementClassGenerator.GetString("COMMENT_CLASSBEGIN"), this.OriginalClassName)));
			return this.cc;
		}

		private string GetConversionFunction(CimType cimType)
		{
			string empty = string.Empty;
			CimType cimType1 = cimType;
			switch (cimType1)
			{
				case CimType.SInt16:
				{
					empty = "ToInt16";
					return empty;
				}
				case CimType.SInt32:
				{
					empty = "ToInt32";
					return empty;
				}
				case CimType.Real32:
				{
					empty = "ToSingle";
					return empty;
				}
				case CimType.Real64:
				{
					empty = "ToDouble";
					return empty;
				}
				case CimType.SInt16 | CimType.Real32:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64:
				/*case 9:*/
				case CimType.SInt16 | CimType.String:
				case CimType.Real32 | CimType.String:
				case CimType.Object:
				case CimType.SInt16 | CimType.Real32 | CimType.String:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
				{
					return empty;
				}
				case CimType.String:
				{
					empty = "ToString";
					return empty;
				}
				case CimType.Boolean:
				{
					empty = "ToBoolean";
					return empty;
				}
				case CimType.SInt8:
				{
					empty = "ToSByte";
					return empty;
				}
				case CimType.UInt8:
				{
					empty = "ToByte";
					return empty;
				}
				case CimType.UInt16:
				{
					if (this.bUnsignedSupported)
					{
						empty = "ToUInt16";
						return empty;
					}
					else
					{
						empty = "ToInt16";
						return empty;
					}
				}
				case CimType.UInt32:
				{
					if (this.bUnsignedSupported)
					{
						empty = "ToUInt32";
						return empty;
					}
					else
					{
						empty = "ToInt32";
						return empty;
					}
				}
				case CimType.SInt64:
				{
					empty = "ToInt64";
					return empty;
				}
				case CimType.UInt64:
				{
					if (this.bUnsignedSupported)
					{
						empty = "ToUInt64";
						return empty;
					}
					else
					{
						empty = "ToInt64";
						return empty;
					}
				}
				default:
				{
					if (cimType1 == CimType.Char16)
					{
						empty = "ToChar";
						return empty;
					}
					else
					{
						return empty;
					}
				}
			}
		}

		private bool GetDateTimeType(PropertyData prop, ref CodeTypeReference codeType)
		{
			bool flag = false;
			codeType = null;
			if (!prop.IsArray)
			{
				codeType = new CodeTypeReference("System.DateTime");
			}
			else
			{
				codeType = new CodeTypeReference("System.DateTime", 1);
			}
			try
			{
				if (string.Compare(prop.Qualifiers["SubType"].Value.ToString(), "interval", StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
					if (!prop.IsArray)
					{
						codeType = new CodeTypeReference("System.TimeSpan");
					}
					else
					{
						codeType = new CodeTypeReference("System.TimeSpan", 1);
					}
				}
			}
			catch (ManagementException managementException)
			{
			}
			if (!flag)
			{
				if (!this.bDateConversionFunctionsAdded)
				{
					this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_DATECONVFUNC")));
					this.bDateConversionFunctionsAdded = true;
					this.GenerateDateTimeConversionFunction();
				}
			}
			else
			{
				if (!this.bTimeSpanConversionFunctionsAdded)
				{
					this.cc.Comments.Add(new CodeCommentStatement(ManagementClassGenerator.GetString("COMMENT_TIMESPANCONVFUNC")));
					this.bTimeSpanConversionFunctionsAdded = true;
					this.GenerateTimeSpanConversionFunction();
				}
			}
			return flag;
		}

		private static string GetString(string strToGet)
		{
			return RC.GetString(strToGet);
		}

		private void GetUnsignedSupport(CodeLanguage Language)
		{
			CodeLanguage language = Language;
			switch (language)
			{
				case CodeLanguage.CSharp:
				{
					this.bUnsignedSupported = true;
					return;
				}
				#if JSCRIPT
				case CodeLanguage.JScript:
				#endif
				case CodeLanguage.VB:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void InitializeClassObject()
		{
			ManagementPath managementPath;
			if (this.classobj != null)
			{
				ManagementPath path = this.classobj.Path;
				this.OriginalServer = path.Server;
				this.OriginalClassName = path.ClassName;
				this.OriginalNamespace = path.NamespacePath;
				char[] charArray = this.OriginalNamespace.ToCharArray();
				if ((int)charArray.Length >= 2 && charArray[0] == '\\' && charArray[1] == '\\')
				{
					bool flag = false;
					int length = this.OriginalNamespace.Length;
					this.OriginalNamespace = string.Empty;
					for (int i = 2; i < length; i++)
					{
						if (!flag)
						{
							if (charArray[i] == '\\')
							{
								flag = true;
							}
						}
						else
						{
							this.OriginalNamespace = string.Concat(this.OriginalNamespace, charArray[i]);
						}
					}
				}
			}
			else
			{
				if (this.OriginalPath.Length == 0)
				{
					managementPath = new ManagementPath();
					if (this.OriginalServer.Length != 0)
					{
						managementPath.Server = this.OriginalServer;
					}
					managementPath.ClassName = this.OriginalClassName;
					managementPath.NamespacePath = this.OriginalNamespace;
				}
				else
				{
					managementPath = new ManagementPath(this.OriginalPath);
				}
				this.classobj = new ManagementClass(managementPath);
			}
			try
			{
				this.classobj.Get();
			}
			catch (ManagementException managementException)
			{
				throw;
			}
			this.bSingletonClass = false;
			foreach (QualifierData qualifier in this.classobj.Qualifiers)
			{
				if (string.Compare(qualifier.Name, "singleton", StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				this.bSingletonClass = true;
				break;
			}
		}

		private void InitializeCodeGeneration()
		{
			this.InitializeClassObject();
			this.InitilializePublicPrivateMembers();
			this.ProcessNamespaceAndClassName();
			this.ProcessNamingCollisions();
		}

		private bool InitializeCodeGenerator(CodeLanguage lang)
		{
			Assembly assembly;
			Type type;
			AssemblyName name;
			AssemblyName assemblyName;
			string str = "";
			bool flag = true;
			try
			{
				CodeLanguage codeLanguage = lang;
				switch (codeLanguage)
				{
					case CodeLanguage.CSharp:
					{
						str = "C#.";
						this.cp = new CSharpCodeProvider();
						break;
					}
#if JSCRIPT
					case CodeLanguage.JScript:
					{
						str = "JScript.NET.";
						this.cp = new JScriptCodeProvider();
						break;
					}
#endif
					case CodeLanguage.VB:
					{
						str = "Visual Basic.";
						this.cp = new VBCodeProvider();
						break;
					}
					case CodeLanguage.VJSharp:
					{
						str = "Visual J#.";
						flag = false;
						name = Assembly.GetExecutingAssembly().GetName();
						assemblyName = new AssemblyName();
						assemblyName.CultureInfo = new CultureInfo("");
						assemblyName.Name = "VJSharpCodeProvider";
						assemblyName.SetPublicKey(name.GetPublicKey());
						assemblyName.Version = name.Version;
						assembly = Assembly.Load(assemblyName);
						if (assembly == null)
						{
							break;
						}
						type = assembly.GetType("Microsoft.VJSharp.VJSharpCodeProvider");
						if (type == null)
						{
							break;
						}
						this.cp = (CodeDomProvider)Activator.CreateInstance(type);
						flag = true;
						break;
					}
					case CodeLanguage.Mcpp:
					{
						str = "Managed C++.";
						flag = false;
						name = Assembly.GetExecutingAssembly().GetName();
						assemblyName = new AssemblyName();
						assemblyName.CultureInfo = new CultureInfo("");
						assemblyName.SetPublicKey(name.GetPublicKey());
						assemblyName.Name = "CppCodeProvider";
						assemblyName.Version = new Version(this.VSVERSION);
						assembly = Assembly.Load(assemblyName);
						if (assembly == null)
						{
							break;
						}
						type = assembly.GetType("Microsoft.VisualC.CppCodeProvider");
						if (type == null)
						{
							break;
						}
						this.cp = (CodeDomProvider)Activator.CreateInstance(type);
						flag = true;
						break;
					}
				}
			}
			catch
			{
				throw new ArgumentOutOfRangeException(string.Format(ManagementClassGenerator.GetString("UNABLE_TOCREATE_GEN_EXCEPT"), str));
			}
			if (!flag)
			{
				throw new ArgumentOutOfRangeException(string.Format(ManagementClassGenerator.GetString("UNABLE_TOCREATE_GEN_EXCEPT"), str));
			}
			else
			{
				this.GetUnsignedSupport(lang);
				return true;
			}
		}

		private void InitializeCodeTypeDeclaration(CodeLanguage lang)
		{
			this.cn = new CodeNamespace(this.PrivateNamesUsed["GeneratedNamespace"].ToString());
			this.cn.Imports.Add(new CodeNamespaceImport("System"));
			this.cn.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
			this.cn.Imports.Add(new CodeNamespaceImport("System.Management"));
			this.cn.Imports.Add(new CodeNamespaceImport("System.Collections"));
			this.cn.Imports.Add(new CodeNamespaceImport("System.Globalization"));
			if (lang == CodeLanguage.VB)
			{
				this.cn.Imports.Add(new CodeNamespaceImport("Microsoft.VisualBasic"));
			}
		}

		private void InitilializePublicPrivateMembers()
		{
			this.PublicNamesUsed.Add("SystemPropertiesProperty", "SystemProperties");
			this.PublicNamesUsed.Add("LateBoundObjectProperty", "LateBoundObject");
			this.PublicNamesUsed.Add("NamespaceProperty", "OriginatingNamespace");
			this.PublicNamesUsed.Add("ClassNameProperty", "ManagementClassName");
			this.PublicNamesUsed.Add("ScopeProperty", "Scope");
			this.PublicNamesUsed.Add("PathProperty", "Path");
			this.PublicNamesUsed.Add("SystemPropertiesClass", "ManagementSystemProperties");
			this.PublicNamesUsed.Add("LateBoundClass", "System.Management.ManagementObject");
			this.PublicNamesUsed.Add("PathClass", "System.Management.ManagementPath");
			this.PublicNamesUsed.Add("ScopeClass", "System.Management.ManagementScope");
			this.PublicNamesUsed.Add("QueryOptionsClass", "System.Management.EnumerationOptions");
			this.PublicNamesUsed.Add("GetOptionsClass", "System.Management.ObjectGetOptions");
			this.PublicNamesUsed.Add("ArgumentExceptionClass", "System.ArgumentException");
			this.PublicNamesUsed.Add("QueryClass", "SelectQuery");
			this.PublicNamesUsed.Add("ObjectSearcherClass", "System.Management.ManagementObjectSearcher");
			this.PublicNamesUsed.Add("FilterFunction", "GetInstances");
			this.PublicNamesUsed.Add("ConstructPathFunction", "ConstructPath");
			this.PublicNamesUsed.Add("TypeConverter", "TypeConverter");
			this.PublicNamesUsed.Add("AutoCommitProperty", "AutoCommit");
			this.PublicNamesUsed.Add("CommitMethod", "CommitObject");
			this.PublicNamesUsed.Add("ManagementClass", "System.Management.ManagementClass");
			this.PublicNamesUsed.Add("NotSupportedExceptClass", "System.NotSupportedException");
			this.PublicNamesUsed.Add("BaseObjClass", "System.Management.ManagementBaseObject");
			this.PublicNamesUsed.Add("OptionsProp", "Options");
			this.PublicNamesUsed.Add("ClassPathProperty", "ClassPath");
			this.PublicNamesUsed.Add("CreateInst", "CreateInstance");
			this.PublicNamesUsed.Add("DeleteInst", "Delete");
			this.PublicNamesUsed.Add("SystemNameSpace", "System");
			this.PublicNamesUsed.Add("ArgumentOutOfRangeException", "System.ArgumentOutOfRangeException");
			this.PublicNamesUsed.Add("System", "System");
			this.PublicNamesUsed.Add("Other", "Other");
			this.PublicNamesUsed.Add("Unknown", "Unknown");
			this.PublicNamesUsed.Add("PutOptions", "System.Management.PutOptions");
			this.PublicNamesUsed.Add("Type", "System.Type");
			this.PublicNamesUsed.Add("Boolean", "System.Boolean");
			this.PublicNamesUsed.Add("ValueType", "System.ValueType");
			this.PublicNamesUsed.Add("Events1", "Events");
			this.PublicNamesUsed.Add("Component1", "Component");
			this.PrivateNamesUsed.Add("SystemPropertiesObject", "PrivateSystemProperties");
			this.PrivateNamesUsed.Add("LateBoundObject", "PrivateLateBoundObject");
			this.PrivateNamesUsed.Add("AutoCommitProperty", "AutoCommitProp");
			this.PrivateNamesUsed.Add("Privileges", "EnablePrivileges");
			this.PrivateNamesUsed.Add("ComponentClass", "System.ComponentModel.Component");
			this.PrivateNamesUsed.Add("ScopeParam", "mgmtScope");
			this.PrivateNamesUsed.Add("NullRefExcep", "System.NullReferenceException");
			this.PrivateNamesUsed.Add("ConverterClass", "WMIValueTypeConverter");
			this.PrivateNamesUsed.Add("EnumParam", "enumOptions");
			this.PrivateNamesUsed.Add("CreationClassName", "CreatedClassName");
			this.PrivateNamesUsed.Add("CreationWmiNamespace", "CreatedWmiNamespace");
			this.PrivateNamesUsed.Add("ClassNameCheckFunc", "CheckIfProperClass");
			this.PrivateNamesUsed.Add("EmbeddedObject", "embeddedObj");
			this.PrivateNamesUsed.Add("CurrentObject", "curObj");
			this.PrivateNamesUsed.Add("IsEmbedded", "isEmbedded");
			this.PrivateNamesUsed.Add("ToDateTimeMethod", "ToDateTime");
			this.PrivateNamesUsed.Add("ToDMTFDateTimeMethod", "ToDmtfDateTime");
			this.PrivateNamesUsed.Add("ToDMTFTimeIntervalMethod", "ToDmtfTimeInterval");
			this.PrivateNamesUsed.Add("ToTimeSpanMethod", "ToTimeSpan");
			this.PrivateNamesUsed.Add("SetMgmtScope", "SetStaticManagementScope");
			this.PrivateNamesUsed.Add("statMgmtScope", "statMgmtScope");
			this.PrivateNamesUsed.Add("staticScope", "StaticScope");
			this.PrivateNamesUsed.Add("initVariable", "Initialize");
			this.PrivateNamesUsed.Add("putOptions", "putOptions");
			this.PrivateNamesUsed.Add("InitialObjectFunc", "InitializeObject");
		}

		private void InitPrivateMemberVariables(CodeMemberMethod cmMethod)
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method.MethodName = this.PrivateNamesUsed["initVariable"].ToString();
			cmMethod.Statements.Add(codeMethodInvokeExpression);
		}

		private int IsContainedIn(string strToFind, ref SortedList sortedList)
		{
			int num = -1;
			int num1 = 0;
			while (num1 < sortedList.Count)
			{
				if (string.Compare(sortedList.GetByIndex(num1).ToString(), strToFind, StringComparison.OrdinalIgnoreCase) != 0)
				{
					num1++;
				}
				else
				{
					num = num1;
					break;
				}
			}
			return num;
		}

		private static bool IsContainedInArray(string strToFind, ArrayList arrToSearch)
		{
			int num = 0;
			while (num < arrToSearch.Count)
			{
				if (string.Compare(arrToSearch[num].ToString(), strToFind, StringComparison.OrdinalIgnoreCase) != 0)
				{
					num++;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsDesignerSerializationVisibilityToBeSet(string propName)
		{
			if (string.Compare(propName, "Path", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool IsDynamicClass()
		{
			bool flag = false;
			try
			{
				flag = Convert.ToBoolean(this.classobj.Qualifiers["dynamic"].Value, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(bool)));
			}
			catch (ManagementException managementException)
			{
			}
			return flag;
		}

		private static bool IsPropertyValueType(CimType cType)
		{
			bool flag = true;
			CimType cimType = cType;
			if (cimType == CimType.String || cimType == CimType.Object || cimType == CimType.Reference)
			{
				flag = false;
			}
			return flag;
		}

		private static bool isTypeInt(CimType cType)
		{
			bool flag;
			CimType cimType = cType;
			switch (cimType)
			{
				case CimType.SInt16:
				case CimType.SInt32:
				case CimType.SInt8:
				case CimType.UInt8:
				case CimType.UInt16:
				case CimType.UInt32:
				{
					flag = true;
					break;
				}
				case CimType.Real32:
				case CimType.Real64:
				case CimType.SInt16 | CimType.Real32:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64:
				case CimType.String:
				/* case 9: */
				case CimType.SInt16 | CimType.String:
				case CimType.Boolean:
				case CimType.Real32 | CimType.String:
				case CimType.Object:
				case CimType.SInt16 | CimType.Real32 | CimType.String:
				case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
				case CimType.SInt64:
				case CimType.UInt64:
				{
					flag = false;
					break;
				}
				default:
				{
					if (cimType == CimType.DateTime || cimType == CimType.Reference || cimType == CimType.Char16)
					{
						flag = false;
						break;
					}
					flag = false;
					break;
				}
			}
			return flag;
		}

		private void ProcessNamespaceAndClassName()
		{
			string originalClassName;
			string nETNamespace;
			if (this.NETNamespace.Length != 0)
			{
				nETNamespace = this.NETNamespace;
			}
			else
			{
				nETNamespace = this.OriginalNamespace;
				nETNamespace = nETNamespace.Replace('\\', '.');
				nETNamespace = nETNamespace.ToUpper(CultureInfo.InvariantCulture);
			}
			if (this.OriginalClassName.IndexOf('\u005F') <= 0)
			{
				originalClassName = this.OriginalClassName;
			}
			else
			{
				originalClassName = this.OriginalClassName.Substring(0, this.OriginalClassName.IndexOf('\u005F'));
				if (this.NETNamespace.Length == 0)
				{
					nETNamespace = string.Concat(nETNamespace, ".");
					nETNamespace = string.Concat(nETNamespace, originalClassName);
				}
				originalClassName = this.OriginalClassName.Substring(this.OriginalClassName.IndexOf('\u005F') + 1);
			}
			if (!char.IsLetter(originalClassName[0]))
			{
				originalClassName = string.Concat("C", originalClassName);
			}
			originalClassName = this.ResolveCollision(originalClassName, true);
			if (Type.GetType(string.Concat("System.", originalClassName)) != null || Type.GetType(string.Concat("System.ComponentModel.", originalClassName)) != null || Type.GetType(string.Concat("System.Management.", originalClassName)) != null || Type.GetType(string.Concat("System.Collections.", originalClassName)) != null || Type.GetType(string.Concat("System.Globalization.", originalClassName)) != null)
			{
				this.PublicNamesUsed.Add(originalClassName, originalClassName);
				originalClassName = this.ResolveCollision(originalClassName, true);
			}
			this.PrivateNamesUsed.Add("GeneratedClassName", originalClassName);
			this.PrivateNamesUsed.Add("GeneratedNamespace", nETNamespace);
		}

		private void ProcessNamingCollisions()
		{
			int num;
			if (this.classobj.Properties != null)
			{
				foreach (PropertyData property in this.classobj.Properties)
				{
					this.PublicProperties.Add(property.Name, property.Name);
				}
			}
			if (this.classobj.Methods != null)
			{
				foreach (MethodData method in this.classobj.Methods)
				{
					this.PublicMethods.Add(method.Name, method.Name);
				}
			}
			foreach (string value in this.PublicNamesUsed.Values)
			{
				num = this.IsContainedIn(value, ref this.PublicProperties);
				if (num == -1)
				{
					num = this.IsContainedIn(value, ref this.PublicMethods);
					if (num == -1)
					{
						continue;
					}
					this.PublicMethods.SetByIndex(num, this.ResolveCollision(value, false));
				}
				else
				{
					this.PublicProperties.SetByIndex(num, this.ResolveCollision(value, false));
				}
			}
			foreach (string str in this.PublicProperties.Values)
			{
				num = this.IsContainedIn(str, ref this.PrivateNamesUsed);
				if (num == -1)
				{
					continue;
				}
				this.PrivateNamesUsed.SetByIndex(num, this.ResolveCollision(str, false));
			}
			foreach (string value1 in this.PublicMethods.Values)
			{
				num = this.IsContainedIn(value1, ref this.PrivateNamesUsed);
				if (num == -1)
				{
					continue;
				}
				this.PrivateNamesUsed.SetByIndex(num, this.ResolveCollision(value1, false));
			}
			foreach (string str1 in this.PublicProperties.Values)
			{
				num = this.IsContainedIn(str1, ref this.PublicMethods);
				if (num == -1)
				{
					continue;
				}
				this.PublicMethods.SetByIndex(num, this.ResolveCollision(str1, false));
			}
			string str2 = string.Concat(this.PrivateNamesUsed["GeneratedClassName"].ToString(), "Collection");
			this.PrivateNamesUsed.Add("CollectionClass", this.ResolveCollision(str2, true));
			str2 = string.Concat(this.PrivateNamesUsed["GeneratedClassName"].ToString(), "Enumerator");
			this.PrivateNamesUsed.Add("EnumeratorClass", this.ResolveCollision(str2, true));
		}

		private string ProcessPropertyQualifiers(PropertyData prop, ref bool bRead, ref bool bWrite, ref bool bStatic, bool bDynamicClass, out bool nullable)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			nullable = true;
			bRead = true;
			bWrite = false;
			this.arrConvFuncName = "ToInt32";
			this.enumType = "System.Int32";
			string empty = string.Empty;
			foreach (QualifierData qualifier in prop.Qualifiers)
			{
				if (string.Compare(qualifier.Name, "description", StringComparison.OrdinalIgnoreCase) != 0)
				{
					if (string.Compare(qualifier.Name, "Not_Null", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare(qualifier.Name, "key", StringComparison.OrdinalIgnoreCase) != 0)
						{
							if (string.Compare(qualifier.Name, "static", StringComparison.OrdinalIgnoreCase) != 0)
							{
								if (string.Compare(qualifier.Name, "read", StringComparison.OrdinalIgnoreCase) != 0)
								{
									if (string.Compare(qualifier.Name, "write", StringComparison.OrdinalIgnoreCase) != 0)
									{
										if (string.Compare(qualifier.Name, "ValueMap", StringComparison.OrdinalIgnoreCase) != 0 || flag2)
										{
											if (string.Compare(qualifier.Name, "Values", StringComparison.OrdinalIgnoreCase) != 0 || flag2)
											{
												if (string.Compare(qualifier.Name, "BitMap", StringComparison.OrdinalIgnoreCase) != 0 || flag2)
												{
													if (string.Compare(qualifier.Name, "BitValues", StringComparison.OrdinalIgnoreCase) != 0 || flag2)
													{
														continue;
													}
													try
													{
														this.BitValues.Clear();
														if (ManagementClassGenerator.isTypeInt(prop.Type) && qualifier.Value != null)
														{
															ArrayList arrayLists = new ArrayList(5);
															string[] value = (string[])qualifier.Value;
															int num = 0;
															while (num < (int)value.Length)
															{
																if (value[num].Length != 0)
																{
																	string name = ManagementClassGenerator.ConvertValuesToName(value[num]);
																	arrayLists.Add(name);
																	num++;
																}
																else
																{
																	this.BitValues.Clear();
																	flag2 = true;
																	break;
																}
															}
															this.ResolveEnumNameValues(arrayLists, ref this.BitValues);
														}
													}
													catch (InvalidCastException invalidCastException)
													{
														this.BitValues.Clear();
													}
												}
												else
												{
													try
													{
														this.BitMap.Clear();
														if (ManagementClassGenerator.isTypeInt(prop.Type) && qualifier.Value != null)
														{
															string[] strArrays = (string[])qualifier.Value;
															for (int i = 0; i < (int)strArrays.Length; i++)
															{
																this.BitMap.Add(ManagementClassGenerator.ConvertBitMapValueToInt32(strArrays[i]));
															}
														}
													}
													catch (FormatException formatException)
													{
														this.BitMap.Clear();
														flag2 = true;
													}
													catch (InvalidCastException invalidCastException1)
													{
														this.BitMap.Clear();
													}
												}
											}
											else
											{
												try
												{
													this.Values.Clear();
													if (ManagementClassGenerator.isTypeInt(prop.Type) && qualifier.Value != null)
													{
														ArrayList arrayLists1 = new ArrayList(5);
														string[] value1 = (string[])qualifier.Value;
														int num1 = 0;
														while (num1 < (int)value1.Length)
														{
															if (value1[num1].Length != 0)
															{
																string str = ManagementClassGenerator.ConvertValuesToName(value1[num1]);
																arrayLists1.Add(str);
																num1++;
															}
															else
															{
																this.Values.Clear();
																flag2 = true;
																break;
															}
														}
														this.ResolveEnumNameValues(arrayLists1, ref this.Values);
													}
												}
												catch (InvalidCastException invalidCastException2)
												{
													this.Values.Clear();
												}
											}
										}
										else
										{
											try
											{
												this.ValueMap.Clear();
												if (ManagementClassGenerator.isTypeInt(prop.Type) && qualifier.Value != null)
												{
													string[] strArrays1 = (string[])qualifier.Value;
													for (int j = 0; j < (int)strArrays1.Length; j++)
													{
														try
														{
															this.arrConvFuncName = ManagementClassGenerator.ConvertToNumericValueAndAddToArray(prop.Type, strArrays1[j], this.ValueMap, out this.enumType);
														}
														catch (OverflowException overflowException)
														{
														}
													}
												}
											}
											catch (FormatException formatException1)
											{
												flag2 = true;
												this.ValueMap.Clear();
											}
											catch (InvalidCastException invalidCastException3)
											{
												this.ValueMap.Clear();
											}
										}
									}
									else
									{
										flag = true;
										if (!(bool)qualifier.Value)
										{
											flag1 = false;
										}
										else
										{
											flag1 = true;
										}
									}
								}
								else
								{
									if ((bool)qualifier.Value)
									{
										bRead = true;
									}
									else
									{
										bRead = false;
									}
								}
							}
							else
							{
								bStatic = true;
								CodeMemberProperty attributes = this.cmp;
								attributes.Attributes = attributes.Attributes | MemberAttributes.Static;
							}
						}
						else
						{
							this.arrKeyType.Add(this.cmp.Type);
							this.arrKeys.Add(prop.Name);
							nullable = false;
							break;
						}
					}
					else
					{
						nullable = false;
					}
				}
				else
				{
					empty = qualifier.Value.ToString();
				}
			}
			if (!bDynamicClass && !flag || !bDynamicClass && flag && flag1 || bDynamicClass && flag && flag1)
			{
				bWrite = true;
			}
			return empty;
		}

		private string ResolveCollision(string inString, bool bCheckthisFirst)
		{
			string str = inString;
			bool flag = true;
			int num = -1;
			string str1 = "";
			if (!bCheckthisFirst)
			{
				num++;
				str = string.Concat(str, str1, num.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
			}
			while (flag)
			{
				if (this.IsContainedIn(str, ref this.PublicProperties) != -1 || this.IsContainedIn(str, ref this.PublicMethods) != -1 || this.IsContainedIn(str, ref this.PublicNamesUsed) != -1 || this.IsContainedIn(str, ref this.PrivateNamesUsed) != -1)
				{
					try
					{
						num++;
					}
					catch (OverflowException overflowException)
					{
						str1 = string.Concat(str1, "_");
						num = 0;
					}
					str = string.Concat(inString, str1, num.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
				}
				else
				{
					flag = false;
					break;
				}
			}
			if (str.Length > 0)
			{
				string upper = str.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture);
				str = string.Concat(upper, str.Substring(1, str.Length - 1));
			}
			return str;
		}

		private void ResolveEnumNameValues(ArrayList arrIn, ref ArrayList arrayOut)
		{
			arrayOut.Clear();
			int num = 0;
			IFormatProvider format = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
			for (int i = 0; i < arrIn.Count; i++)
			{
				string str = arrIn[i].ToString();
				str = this.ResolveCollision(str, true);
				if (ManagementClassGenerator.IsContainedInArray(str, arrayOut))
				{
					num = 0;
					for (str = string.Concat(arrIn[i].ToString(), num.ToString(format)); ManagementClassGenerator.IsContainedInArray(str, arrayOut); str = string.Concat(arrIn[i].ToString(), num.ToString(format)))
					{
						num++;
					}
				}
				arrayOut.Add(str);
			}
		}

		private void ToDMTFDateHelper(string dateTimeMember, CodeMemberMethod cmmdt, string strType)
		{
			string str = "dmtfDateTime";
			string str1 = "date";
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference(strType), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str1), dateTimeMember)), "ToString");
			CodeMethodInvokeExpression codeMethodReferenceExpression = new CodeMethodInvokeExpression();
			codeMethodReferenceExpression.Method = new CodeMethodReferenceExpression(codeMethodInvokeExpression, "PadLeft");
			codeMethodReferenceExpression.Parameters.Add(new CodePrimitiveExpression((object)2));
			codeMethodReferenceExpression.Parameters.Add(new CodePrimitiveExpression((object)((char)48)));
			ManagementClassGenerator.GenerateConcatStrings(codeMethodInvokeExpression, codeMethodReferenceExpression);
			cmmdt.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), ManagementClassGenerator.GenerateConcatStrings(new CodeVariableReferenceExpression(str), codeMethodReferenceExpression)));
		}

		private static void ToTimeSpanHelper(int start, int numOfCharacters, string strVarToAssign, CodeStatementCollection statCol)
		{
			string str = "tempString";
			string str1 = "dmtfTimespan";
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str1), "Substring");
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)start));
			codeMethodInvokeExpression.Parameters.Add(new CodePrimitiveExpression((object)numOfCharacters));
			statCol.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str), codeMethodInvokeExpression));
			codeMethodInvokeExpression = new CodeMethodInvokeExpression();
			codeMethodInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse");
			codeMethodInvokeExpression.Parameters.Add(new CodeVariableReferenceExpression(str));
			statCol.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(strVarToAssign), codeMethodInvokeExpression));
		}
	}
}