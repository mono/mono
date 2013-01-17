//
// Mono.Data.CustomDataClassGenerator
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// API notes are the bottom of the source.
//
// This class is standalone testable (even under MS.NET) when compiled with
// -d:DATACLASS_GENERATOR_STANDALONE .
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.CodeDom;
using System.Globalization;
using System.Text;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Reflection;

// only for Driver
using Microsoft.CSharp;
using System.IO;

namespace System.Data
{

#if DATACLASS_GENERATOR_STANDALONE
	public class Driver
	{
		public static void Main (string [] args)
		{
			try {
				if (args.Length < 1) {
					Console.WriteLine ("mono dsgentest.exe filename");
					return;
				}

				DataSet ds = new DataSet ();
				ds.ReadXml (args [0]);
				ICodeGenerator gen = new CSharpCodeProvider ().CreateGenerator ();

				CodeNamespace cns = new CodeNamespace ("MyNamespace");
				TextWriter tw = new StreamWriter (Path.ChangeExtension (args [0], ".ms.cs"), false, Encoding.Default);
				TypedDataSetGenerator.Generate (ds, cns, gen);
				gen.GenerateCodeFromNamespace (cns, tw, null);
				tw.Close ();

				cns = new CodeNamespace ("MyNamespace");
				tw = new StreamWriter (Path.ChangeExtension (args [0], ".mono.cs"), false, Encoding.Default);
				CustomDataClassGenerator.CreateDataSetClasses (ds, cns, gen, null);
				gen.GenerateCodeFromNamespace (cns, tw, null);
				tw.Close ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
#endif

#if DATACLASS_GENERATOR_STANDALONE
	public class CustomDataClassGenerator
#else
	internal class CustomDataClassGenerator
#endif
	{
		public static void CreateDataSetClasses (DataSet ds,
			CodeNamespace cns, ICodeGenerator gen,
			ClassGeneratorOptions options)
		{
			new Generator (ds, cns, gen, options).Run ();
		}

#if NET_2_0
		public static void CreateDataSetClasses (DataSet ds,
			CodeNamespace cns, CodeDomProvider codeProvider,
			ClassGeneratorOptions options)
		{
			new Generator (ds, cns, codeProvider, options).Run ();
		}

		public static void CreateDataSetClasses (DataSet ds, 
		                                         CodeCompileUnit cunit, 
		                                         CodeNamespace cns, 
		                                         CodeDomProvider codeProvider, 
		                                         ClassGeneratorOptions options)
		{
			new Generator (ds, cunit, cns, codeProvider, options).Run ();
		}
#endif
		public static string MakeSafeName (string name, ICodeGenerator codeGen)
		{
			if (name == null || codeGen == null)
				throw new NullReferenceException ();

			name = codeGen.CreateValidIdentifier (name);

			return MakeSafeNameInternal (name);
		}
		
#if NET_2_0
		public static string MakeSafeName (string name, CodeDomProvider provider)
		{
			if (name == null || provider == null)
				throw new NullReferenceException ();

			name = provider.CreateValidIdentifier (name);

			return MakeSafeNameInternal (name);
		}
#endif
		
		public static string MakeSafeNameInternal (string name)
		{
			if (name.Length == 0)
				return "_";

			StringBuilder sb = null;
			if (!Char.IsLetter (name, 0) && name [0] != '_') {
				sb = new StringBuilder ();
				sb.Append ('_');
			}

			int start = 0;
			for (int i = 0; i < name.Length; i++) {
				if (!Char.IsLetterOrDigit (name, i)) {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (name, start, i - start);
					sb.Append ('_');
					start = i + 1;
				}
			}

			if (sb != null) {
				sb.Append (name, start, name.Length - start);
				return sb.ToString ();
			}
			else
				return name;
		}
	}

#if DATACLASS_GENERATOR_STANDALONE
	public delegate string CodeNamingMethod (string source, ICodeGenerator gen);
#else
	internal delegate string CodeNamingMethod (string source, ICodeGenerator gen);
#endif

#if DATACLASS_GENERATOR_STANDALONE
	public delegate string CodeDomNamingMethod (string source, CodeDomProvider provider);
#else
	internal delegate string CodeDomNamingMethod (string source, CodeDomProvider provider);
#endif	
	
#if DATACLASS_GENERATOR_STANDALONE
	public class ClassICodeGeneratorOptions : ClassGeneratorOptions
#else
	internal class ClassICodeGeneratorOptions : ClassGeneratorOptions
#endif
	{
		ICodeGenerator gen;
		
		public CodeNamingMethod CreateDataSetName;
		public CodeNamingMethod CreateTableTypeName;
		public CodeNamingMethod CreateTableMemberName;
		public CodeNamingMethod CreateTableColumnName;
		public CodeNamingMethod CreateColumnName;
		public CodeNamingMethod CreateRowName;
		public CodeNamingMethod CreateRelationName;
		public CodeNamingMethod CreateTableDelegateName;
		public CodeNamingMethod CreateEventArgsName;
		public CodeNamingMethod CreateTableAdapterNSName;
		public CodeNamingMethod CreateTableAdapterName;

		public ClassICodeGeneratorOptions (ICodeGenerator codeGen)
		{
			this.gen = codeGen;
		}
		
		internal override string DataSetName (string source)
		{
			if (CreateDataSetName != null)
				return CreateDataSetName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal override string TableTypeName (string source)
		{
			if (CreateTableTypeName != null)
				return CreateTableTypeName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "DataTable";
		}

		internal override string TableMemberName (string source)
		{
			if (CreateTableMemberName != null)
				return CreateTableMemberName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal override string TableColName (string source)
		{
			if (CreateTableColumnName != null)
				return CreateTableColumnName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal override string TableDelegateName (string source)
		{
			if (CreateTableDelegateName != null)
				return CreateTableDelegateName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "RowChangedEventHandler";
		}

		internal override string EventArgsName (string source)
		{
			if (CreateEventArgsName != null)
				return CreateEventArgsName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "RowChangedEventArgs";
		}

		internal override string ColumnName (string source)
		{
			if (CreateColumnName != null)
				return CreateColumnName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal override string RowName (string source)
		{
			if (CreateRowName != null)
				return CreateRowName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "Row";
		}

		internal override string RelationName (string source)
		{
			if (CreateRelationName != null)
				return CreateRelationName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "Relation";
		}

		internal override string TableAdapterNSName (string source)
		{
			if (CreateTableAdapterNSName != null)
				return CreateTableAdapterNSName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "TableAdapters";
		}
		
		internal override string TableAdapterName (string source)
		{
			if (CreateTableAdapterName != null)
				return CreateTableAdapterName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}
	}
	
#if NET_2_0
#if DATACLASS_GENERATOR_STANDALONE
	public class ClassCodeDomProviderOptions : ClassGeneratorOptions
#else
	internal class ClassCodeDomProviderOptions : ClassGeneratorOptions
#endif
	{
		CodeDomProvider provider;
		
		public CodeDomNamingMethod CreateDataSetName;
		public CodeDomNamingMethod CreateTableTypeName;
		public CodeDomNamingMethod CreateTableMemberName;
		public CodeDomNamingMethod CreateTableColumnName;
		public CodeDomNamingMethod CreateColumnName;
		public CodeDomNamingMethod CreateRowName;
		public CodeDomNamingMethod CreateRelationName;
		public CodeDomNamingMethod CreateTableDelegateName;
		public CodeDomNamingMethod CreateEventArgsName;			
		public CodeDomNamingMethod CreateTableAdapterNSName;
		public CodeDomNamingMethod CreateTableAdapterName;
		
		public ClassCodeDomProviderOptions (CodeDomProvider codeProvider)
		{
			this.provider = codeProvider;
		}
		
		internal override string DataSetName (string source)
		{
			if (CreateDataSetName != null)
				return CreateDataSetName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider);
		}

		internal override string TableTypeName (string source)
		{
			if (CreateTableTypeName != null)
				return CreateTableTypeName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "DataTable";
		}

		internal override string TableMemberName (string source)
		{
			if (CreateTableMemberName != null)
				return CreateTableMemberName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider);
		}

		internal override string TableColName (string source)
		{
			if (CreateTableColumnName != null)
				return CreateTableColumnName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider);
		}

		internal override string TableDelegateName (string source)
		{
			if (CreateTableDelegateName != null)
				return CreateTableDelegateName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "RowChangedEventHandler";
		}

		internal override string EventArgsName (string source)
		{
			if (CreateEventArgsName != null)
				return CreateEventArgsName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "RowChangedEventArgs";
		}

		internal override string ColumnName (string source)
		{
			if (CreateColumnName != null)
				return CreateColumnName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider);
		}

		internal override string RowName (string source)
		{
			if (CreateRowName != null)
				return CreateRowName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "Row";
		}

		internal override string RelationName (string source)
		{
			if (CreateRelationName != null)
				return CreateRelationName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "Relation";
		}

		internal override string TableAdapterNSName (string source)
		{
			if (CreateTableAdapterNSName != null)
				return CreateTableAdapterNSName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider) + "TableAdapters";
		}

		internal override string TableAdapterName (string source)
		{
			if (CreateTableAdapterName != null)
				return CreateTableAdapterName (source, provider);
			else
				return CustomDataClassGenerator.MakeSafeName (source, provider);
		}		
	}
#endif  // NET_2_0
	
#if DATACLASS_GENERATOR_STANDALONE
	public abstract class ClassGeneratorOptions
#else
	internal abstract class ClassGeneratorOptions
#endif
	{
		public bool MakeClassesInsideDataSet = true; // default = MS compatible
		
		internal abstract string DataSetName (string source);
		internal abstract string TableTypeName (string source);
		internal abstract string TableMemberName (string source);
		internal abstract string TableColName (string source);
		internal abstract string TableDelegateName (string source);
		internal abstract string EventArgsName (string source);
		internal abstract string ColumnName (string source);
		internal abstract string RowName (string source);
		internal abstract string RelationName (string source);
		internal abstract string TableAdapterNSName (string source);
		internal abstract string TableAdapterName (string source);
	}

	internal class Generator
	{
//		static ClassGeneratorOptions DefaultOptions = new ClassGeneratorOptions ();

		DataSet ds;
		CodeNamespace cns;
		ClassGeneratorOptions opts;
		CodeCompileUnit cunit;

		CodeTypeDeclaration dsType;

		public Generator (DataSet ds, CodeNamespace cns, ICodeGenerator codeGen, ClassGeneratorOptions options)
		{
			this.ds = ds;
			this.cns = cns;
			this.opts = options;
			this.cunit = null;
			if (opts == null)
				opts = new ClassICodeGeneratorOptions (codeGen);
		}
#if NET_2_0
		public Generator (DataSet ds, CodeNamespace cns, CodeDomProvider codeProvider, 
		                  ClassGeneratorOptions options)
		{
			this.ds = ds;
			this.cns = cns;
			this.opts = options;
			this.cunit = null;
			if (opts == null)
				opts = new ClassCodeDomProviderOptions (codeProvider);			
		}

		public Generator (DataSet ds, CodeCompileUnit cunit, CodeNamespace cns, 
		                  CodeDomProvider codeProvider, ClassGeneratorOptions options)
		{
			this.ds = ds;
			this.cns = cns;
			this.opts = options;
			this.cunit = cunit;
			if (opts == null)
				opts = new ClassCodeDomProviderOptions (codeProvider);
		}
#endif
		public void Run ()
		{
			// using decls
			cns.Imports.Add (new CodeNamespaceImport ("System"));
			cns.Imports.Add (new CodeNamespaceImport ("System.Collections"));
			cns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
			cns.Imports.Add (new CodeNamespaceImport ("System.Data"));
			cns.Imports.Add (new CodeNamespaceImport ("System.Runtime.Serialization"));
			cns.Imports.Add (new CodeNamespaceImport ("System.Xml"));


			CodeTypeDeclaration dsType = GenerateDataSetType ();
			cns.Types.Add (dsType);

			foreach (DataTable dt in ds.Tables) {
				// 1. table types ([foo]DataTable)
				// 2. row types ([foo]Row)
				// 3. delegates ([foo]RowChangedEventHandler)
				// 4. eventargs ([foo]RowChangeEventArgs)

				CodeTypeDeclaration dtType = GenerateDataTableType (dt);

				CodeTypeDeclaration dtRow = GenerateDataRowType (dt);

				CodeTypeDelegate dtDelegate = new CodeTypeDelegate (opts.TableDelegateName (dt.TableName));
				dtDelegate.Parameters.Add (Param (typeof (object), "o"));
				dtDelegate.Parameters.Add (Param (opts.EventArgsName (dt.TableName), "e"));

				CodeTypeDeclaration dtEventType = GenerateEventType (dt);

				// Add types to either DataSet or CodeNamespace
				if (opts.MakeClassesInsideDataSet) {
					dsType.Members.Add (dtType);
					dsType.Members.Add (dtRow);
					dsType.Members.Add (dtDelegate);
					dsType.Members.Add (dtEventType);
				}
				else {
					cns.Types.Add (dtType);
					cns.Types.Add (dtRow);
					cns.Types.Add (dtDelegate);
					cns.Types.Add (dtEventType);
				}
			}

#if NET_2_0
			if (cunit == null)
				return;
			
			TableAdapterSchemaInfo adapterInfo = ds.TableAdapterSchemaData;
			if (adapterInfo != null) {
				// #325464 debugging
				//Console.WriteLine (opts.TableAdapterNSName(opts.DataSetName (ds.DataSetName)));
				CodeNamespace cnsTA = new CodeNamespace (opts.TableAdapterNSName(opts.DataSetName (ds.DataSetName)));
				CodeTypeDeclaration dtAdapter = GenerateTableAdapterType (adapterInfo);
				cnsTA.Types.Add (dtAdapter);
				cunit.Namespaces.Add (cnsTA);
			}
#endif
		}
		
		private CodeThisReferenceExpression This ()
		{
			return new CodeThisReferenceExpression ();
		}

		private CodeBaseReferenceExpression Base ()
		{
			return new CodeBaseReferenceExpression ();
		}

		private CodePrimitiveExpression Const (object value)
		{
			return new CodePrimitiveExpression (value);
		}

		private CodeTypeReference TypeRef (Type t)
		{
			return new CodeTypeReference (t);
		}

		private CodeTypeReference TypeRef (string name)
		{
			return new CodeTypeReference (name);
		}

		private CodeTypeReference TypeRefArray (Type t, int dimension)
		{
			return new CodeTypeReference (TypeRef (t), dimension);
		}

		private CodeTypeReference TypeRefArray (string name, int dimension)
		{
			return new CodeTypeReference (TypeRef (name), dimension);
		}
		
		private CodeParameterDeclarationExpression Param (string t, string name)
		{
			return new CodeParameterDeclarationExpression (t, name);
		}

		private CodeParameterDeclarationExpression Param (Type t, string name)
		{
			return new CodeParameterDeclarationExpression (t, name);
		}

		private CodeParameterDeclarationExpression Param (CodeTypeReference t, string name)
		{
			return new CodeParameterDeclarationExpression (t, name);
		}

		private CodeArgumentReferenceExpression ParamRef (string name)
		{
			return new CodeArgumentReferenceExpression (name);
		}

		private CodeCastExpression Cast (string t, CodeExpression exp)
		{
			return new CodeCastExpression (t, exp);
		}

		private CodeCastExpression Cast (Type t, CodeExpression exp)
		{
			return new CodeCastExpression (t, exp);
		}

		private CodeCastExpression Cast (CodeTypeReference t, CodeExpression exp)
		{
			return new CodeCastExpression (t, exp);
		}

		private CodeExpression New (Type t, params CodeExpression [] parameters)
		{
			return new CodeObjectCreateExpression (t, parameters);
		}

		private CodeExpression New (string t, params CodeExpression [] parameters)
		{
			return new CodeObjectCreateExpression (TypeRef (t), parameters);
		}

		private CodeExpression NewArray (Type t, params CodeExpression [] parameters)
		{
			return new CodeArrayCreateExpression (t, parameters);
		}

		private CodeExpression NewArray (Type t, int size )
		{
			return new CodeArrayCreateExpression (t, size);
		}

		private CodeVariableReferenceExpression Local (string name)
		{
			return new CodeVariableReferenceExpression (name);
		}

		private CodeFieldReferenceExpression FieldRef (string name)
		{
			return new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), name);
		}

		private CodeFieldReferenceExpression FieldRef (CodeExpression exp, string name)
		{
			return new CodeFieldReferenceExpression (exp, name);
		}

		private CodePropertyReferenceExpression PropRef (string name)
		{
			return new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), name);
		}

		private CodePropertyReferenceExpression PropRef (CodeExpression target, string name)
		{
			return new CodePropertyReferenceExpression (target, name);
		}

		private CodeIndexerExpression IndexerRef (CodeExpression target, CodeExpression parameters)
		{
			return new CodeIndexerExpression (target, parameters);
		}

		private CodeIndexerExpression IndexerRef (CodeExpression param)
		{
			return new CodeIndexerExpression (new CodeThisReferenceExpression (), param);
		}

		private CodeEventReferenceExpression EventRef (string name)
		{
			return new CodeEventReferenceExpression (new CodeThisReferenceExpression (), name);
		}

		private CodeEventReferenceExpression EventRef (CodeExpression target, string name)
		{
			return new CodeEventReferenceExpression (target, name);
		}

		private CodeMethodInvokeExpression MethodInvoke (string name, params CodeExpression [] parameters)
		{
			return new CodeMethodInvokeExpression (new CodeThisReferenceExpression (), name, parameters);
		}

		private CodeMethodInvokeExpression MethodInvoke (CodeExpression target, string name, params CodeExpression [] parameters)
		{
			return new CodeMethodInvokeExpression (target, name, parameters);
		}

		private CodeBinaryOperatorExpression EqualsValue (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.ValueEquality, exp2);
		}

		// note that this is "Identity" equality comparison
		private CodeBinaryOperatorExpression Equals (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.IdentityEquality, exp2);
		}

		private CodeBinaryOperatorExpression Inequals (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.IdentityInequality, exp2);
		}

		private CodeBinaryOperatorExpression GreaterThan (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.GreaterThan, exp2);
		}

		private CodeBinaryOperatorExpression LessThan (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.LessThan, exp2);
		}

		private CodeBinaryOperatorExpression Compute (CodeExpression exp1, CodeExpression exp2, CodeBinaryOperatorType ops)
		{
			if (ops >= CodeBinaryOperatorType.Add && ops < CodeBinaryOperatorType.Assign)
				return new CodeBinaryOperatorExpression (exp1, ops, exp2);
			else
				return null;
		}

		private CodeBinaryOperatorExpression BitOps (CodeExpression exp1, CodeExpression exp2, CodeBinaryOperatorType ops)
		{
			if (ops >= CodeBinaryOperatorType.BitwiseOr && ops <= CodeBinaryOperatorType.BitwiseAnd)
				return new CodeBinaryOperatorExpression (exp1, ops, exp2);
			else
				return null;
		}
		
		private CodeBinaryOperatorExpression BooleanOps (CodeExpression exp1, CodeExpression exp2, CodeBinaryOperatorType ops)
		{
			if (ops >= CodeBinaryOperatorType.BooleanOr && ops <= CodeBinaryOperatorType.BooleanAnd)
				return new CodeBinaryOperatorExpression (exp1, ops, exp2);
			else
				return null;
		}

		private CodeTypeReferenceExpression TypeRefExp (Type t)
		{
			return new CodeTypeReferenceExpression (t);
		}

		private CodeTypeOfExpression TypeOfRef (string name)
		{
			return new CodeTypeOfExpression (TypeRef (name));
		}

		private CodeExpressionStatement Eval (CodeExpression exp)
		{
			return new CodeExpressionStatement (exp);
		}

		private CodeAssignStatement Let (CodeExpression exp, CodeExpression value)
		{
			return new CodeAssignStatement (exp, value);
		}

		private CodeMethodReturnStatement Return (CodeExpression exp)
		{
			return new CodeMethodReturnStatement (exp);
		}

		private CodeVariableDeclarationStatement VarDecl (Type t,
			string name, CodeExpression init)
		{
			return new CodeVariableDeclarationStatement (t, name, init);
		}

		private CodeVariableDeclarationStatement VarDecl (string t,
			string name, CodeExpression init)
		{
			return new CodeVariableDeclarationStatement (t, name, init);
		}

		private CodeCommentStatement Comment (string comment)
		{
			return new CodeCommentStatement (comment);
		}

		private CodeThrowExceptionStatement Throw (CodeExpression exp)
		{
			return new CodeThrowExceptionStatement (exp);
		}

#region DataSet class

		private CodeTypeDeclaration GenerateDataSetType ()
		{
			// Type
			dsType = new CodeTypeDeclaration (opts.DataSetName (ds.DataSetName));
			dsType.BaseTypes.Add (TypeRef (typeof (DataSet)));
			dsType.BaseTypes.Add (TypeRef (typeof (IXmlSerializable)));

			// .ctor()
			dsType.Members.Add (CreateDataSetDefaultCtor ());
			// runtime serialization .ctor()
			dsType.Members.Add (CreateDataSetSerializationCtor ());

			// Clone()
			dsType.Members.Add (CreateDataSetCloneMethod (dsType));

// FIXME: I keep these methods out of the generated source right now.
// It should be added after runtime serialization was implemented.
/*
			// ShouldSerializeTables()
			dsType.Members.Add (CreateDataSetShouldSerializeTables ());

			// ShouldSerializeRelations()
			dsType.Members.Add (CreateDataSetShouldSerializeRelations ());

			// ReadXmlSerializable()
			dsType.Members.Add (CreateDataSetReadXmlSerializable ());
*/

			// GetSchemaSerializable()
			dsType.Members.Add (CreateDataSetGetSchemaSerializable ());

			dsType.Members.Add (CreateDataSetGetSchema ());
			dsType.Members.Add (CreateDataSetInitializeClass ());
			dsType.Members.Add (CreateDataSetInitializeFields ());
			dsType.Members.Add (CreateDataSetSchemaChanged ());

			// table class and members
			foreach (DataTable table in ds.Tables)
				CreateDataSetTableMembers (dsType, table);
			// relation class and members
			foreach (DataRelation rel in ds.Relations)
				CreateDataSetRelationMembers (dsType, rel);

			return dsType;
		}

		// Code:
		// public Foo ()
		// {
		//   InitializeClass();
		//   CollectionChangeEventHandler handler = new CollectionChangeEventHandler (SchemaChanged);
		//   Tables.CollectionChanged += handler;
		//   Relations.CollectionChanged += handler;
		// }
		private CodeConstructor CreateDataSetDefaultCtor ()
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			// Code: InitializeClass().
			ctor.Statements.Add (Eval (MethodInvoke ("InitializeClass")));

			// Code: CollectionChangedEventHandler handler = new CollectionChangeEventHandler (SchemeChanged);
			CodeVariableDeclarationStatement stmt2 = 
				VarDecl (
					typeof (CollectionChangeEventHandler), 
					"handler", 
					New (
						typeof (CollectionChangeEventHandler), 
						new CodeDelegateCreateExpression (
							new CodeTypeReference (typeof (CollectionChangeEventHandler)),
							new CodeThisReferenceExpression (), 
							"SchemaChanged")));

			ctor.Statements.Add (stmt2);

			// Code: Tables.CollectionChanged += handler;
			ctor.Statements.Add (
				new CodeAttachEventStatement (
					EventRef (
						PropRef ("Tables"), 
						"CollectionChanged"),
					Local ("handler")));

			// Code: Relations.CollectionChanged += handler;
			ctor.Statements.Add (
				new CodeAttachEventStatement (
					EventRef (
						PropRef ("Relations"), 
						"CollectionChanged"), 
					Local ("handler")));

			return ctor;
		}

		// TODO: implement

		// Code:
		// protected Foo (SerializationInfo info, StreamingContext ctx)
		// {
		//   throw new NotImplementedException ();
		// }
		private CodeConstructor CreateDataSetSerializationCtor ()
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Family;
			ctor.Parameters.Add (Param (typeof (SerializationInfo), "info"));
			ctor.Parameters.Add (Param (typeof (StreamingContext), "ctx"));

			// Code: 
			//  // TODO: implement
			//  throw new NotImplementedException ();
			ctor.Statements.Add (Comment ("TODO: implement"));
			ctor.Statements.Add (Throw (New (typeof (NotImplementedException))));

			return ctor;
		}

		// Code:
		//  public override DataSet Clone()
		//  {
		//    [foo] set = ([foo]) base.Clone ();
		//    set.InitializeFields ();
		//    return set;
		//  }
		private CodeMemberMethod CreateDataSetCloneMethod (CodeTypeDeclaration dsType)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.ReturnType = TypeRef (typeof (DataSet));
			m.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			m.Name = "Clone";
			// Code: [foo] set = ([foo]) base.Clone ();
			CodeVariableReferenceExpression set = Local ("set");
			m.Statements.Add (VarDecl (
					dsType.Name,
					"set", 
					Cast (
						dsType.Name,
						MethodInvoke (Base (), "Clone"))));
			m.Statements.Add (Eval (MethodInvoke (set, "InitializeFields")));
			m.Statements.Add (Return (set));
			return m;
		}

		// Code:
		// protected override bool ShouldSerializeTables ()
		// {
		//   return true; // it should be false
		// }
		/*
		private CodeMemberMethod CreateDataSetShouldSerializeTables ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "ShouldSerializeTables";
			m.ReturnType = TypeRef (typeof (bool));
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			// FIXME: set "false" after serialization .ctor() implementation
			m.Statements.Add (Return (Const (true)));
			return m;
		}
		*/
		// Code:
		// protected override bool ShouldSerializeRelations ()
		// {
		//   return true; // it should be false
		// }
		
		/*
		private CodeMemberMethod CreateDataSetShouldSerializeRelations ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "ShouldSerializeRelations";
			m.ReturnType = TypeRef (typeof (bool));
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			// FIXME: set "false" after serialization .ctor() implementation
			m.Statements.Add (Return (Const (true)));
			return m;
		}
		*/

		// Code:
		// protected override void ReadXmlSerializable()
		// {
		//   // TODO: implement
		//   throw new NotImplementedException ();
		// }
		/*
		private CodeMemberMethod CreateDataSetReadXmlSerializable ()
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "ReadXmlSerializable";
			method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			method.Parameters.Add (Param (TypeRef (typeof (XmlReader)), "reader"));
			// TODO: implemnet
			method.Statements.Add (Comment ("TODO: implement"));
			// Hey, how can I specify the constructor to invoke chained ctor with an empty parameter list!?
			method.Statements.Add (Throw (New (typeof (NotImplementedException))));
			return method;
		}
		*/

		private CodeMemberMethod CreateDataSetGetSchema ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.PrivateImplementationType = TypeRef (typeof (IXmlSerializable));
			m.Name = "GetSchema";
			m.ReturnType = TypeRef (typeof (XmlSchema));
			m.Statements.Add (Return (MethodInvoke ("GetSchemaSerializable")));

			return m;
		}

		private CodeMemberMethod CreateDataSetGetSchemaSerializable ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Attributes = MemberAttributes.Family | 
				MemberAttributes.Override;
			m.Name = "GetSchemaSerializable";
			m.ReturnType = TypeRef (typeof (XmlSchema));

			m.Statements.Add (VarDecl (typeof (StringWriter), "sw",
				New (typeof (StringWriter))));
			m.Statements.Add (Eval (MethodInvoke ("WriteXmlSchema", Local ("sw"))));
			m.Statements.Add (Return (MethodInvoke (
				TypeRefExp (typeof (XmlSchema)),
				"Read",
				New (typeof (XmlTextReader),
					New (typeof (StringReader),
						MethodInvoke (Local ("sw"),
							"ToString"))),
				Const (null))));

			return m;
		}

		private CodeMemberMethod CreateDataSetInitializeClass ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitializeClass";
			m.Attributes = MemberAttributes.Assembly;

			// dataset properties
			m.Statements.Add (Let (PropRef ("DataSetName"), Const (ds.DataSetName)));
			m.Statements.Add (Let (PropRef ("Prefix"), Const (ds.Prefix)));
			m.Statements.Add (Let (PropRef ("Namespace"), Const (ds.Namespace)));
			m.Statements.Add (Let (PropRef ("Locale"), New (typeof (CultureInfo), Const (ds.Locale.Name))));
			m.Statements.Add (Let (PropRef ("CaseSensitive"), Const (ds.CaseSensitive)));
			m.Statements.Add (Let (PropRef ("EnforceConstraints"), Const (ds.EnforceConstraints)));

			// table
			foreach (DataTable dt in ds.Tables) {
				string tableFieldName = "__table" + opts.TableMemberName (dt.TableName);
				string tableTypeName = opts.TableTypeName (dt.TableName);
				m.Statements.Add (Let (FieldRef (tableFieldName), New (tableTypeName)));
				m.Statements.Add (Eval (MethodInvoke (PropRef ("Tables"), "Add", FieldRef (tableFieldName))));
			}

			bool fkcExists = false;
			bool ucExists = false;
			// First the UniqueConstraints
			foreach (DataTable dt in ds.Tables) {
				string tname = "__table" + opts.TableMemberName (dt.TableName);
				foreach (Constraint c in dt.Constraints) {
					UniqueConstraint uc = c as UniqueConstraint;
					if (uc != null) {
						if (!ucExists) {
							m.Statements.Add (VarDecl (typeof (UniqueConstraint), "uc", null));
							ucExists = true;
						}
						CreateUniqueKeyStatements (m, uc, tname);
					}
				}
			}
			// Then the ForeignKeyConstraints
			foreach (DataTable dt in ds.Tables) {
				string tname = "__table" + opts.TableMemberName (dt.TableName);
				foreach (Constraint c in dt.Constraints) {
					ForeignKeyConstraint fkc = c as ForeignKeyConstraint;
					if (fkc != null) {
						if (!fkcExists) {
							m.Statements.Add (VarDecl (typeof (ForeignKeyConstraint), "fkc", null));
							fkcExists = true;
						}
						string rtname = "__table" + opts.TableMemberName (fkc.RelatedTable.TableName);
						CreateForeignKeyStatements (m, fkc, tname, rtname);
					}
				}
			}
			// What if other cases? dunno. Just ignore ;-)
			foreach (DataRelation rel in ds.Relations) {
				string relName = opts.RelationName (rel.RelationName);
				ArrayList pcols = new ArrayList ();
				foreach (DataColumn pcol in rel.ParentColumns)
					pcols.Add (IndexerRef (PropRef (FieldRef ("__table" + opts.TableMemberName (rel.ParentTable.TableName)), "Columns"), Const (pcol.ColumnName)));

				ArrayList ccols = new ArrayList ();
				foreach (DataColumn ccol in rel.ChildColumns)
					ccols.Add (IndexerRef (PropRef (FieldRef ("__table" + opts.TableMemberName (rel.ChildTable.TableName)), "Columns"), Const (ccol.ColumnName)));

				// relation field
				string fieldName = "__relation" + relName;
				m.Statements.Add (Let (FieldRef (fieldName), New (typeof (DataRelation),
					Const (rel.RelationName),
					NewArray (typeof (DataColumn), pcols.ToArray (typeof (CodeExpression)) as CodeExpression []),
					NewArray (typeof (DataColumn), ccols.ToArray (typeof (CodeExpression)) as CodeExpression []),
					Const (false)
					)));
				m.Statements.Add (Let (PropRef (FieldRef (fieldName), "Nested"), Const (rel.Nested)));
				m.Statements.Add (MethodInvoke (PropRef ("Relations"), "Add", FieldRef (fieldName)));
			}

			return m;
		}

		private void CreateUniqueKeyStatements (CodeMemberMethod m, UniqueConstraint uc, string tableField)
		{
			ArrayList al = new ArrayList ();
			foreach (DataColumn col in uc.Columns)
				al.Add (IndexerRef (PropRef (FieldRef (tableField), "Columns"), Const (col.ColumnName)));

			m.Statements.Add (Let (Local ("uc"), New (
				typeof (UniqueConstraint),
				Const (uc.ConstraintName),
				NewArray (
					typeof (DataColumn),
					al.ToArray (typeof (CodeExpression)) as CodeExpression []),
				Const (uc.IsPrimaryKey))));
			m.Statements.Add (MethodInvoke (PropRef (FieldRef (tableField), "Constraints"), "Add", Local ("uc")));
		}

		private void CreateForeignKeyStatements (CodeMemberMethod m,ForeignKeyConstraint fkc, string tableField, string rtableField)
		{
			ArrayList pcols = new ArrayList ();
			foreach (DataColumn col in fkc.RelatedColumns)
				pcols.Add (IndexerRef (PropRef (FieldRef (rtableField), "Columns"), Const (col.ColumnName)));

			ArrayList ccols = new ArrayList ();
			foreach (DataColumn col in fkc.Columns)
				ccols.Add (IndexerRef (PropRef (FieldRef (tableField), "Columns"), Const (col.ColumnName)));

			m.Statements.Add (Let (Local ("fkc"), New (
				typeof (ForeignKeyConstraint),
				Const (fkc.ConstraintName),
				NewArray (
					typeof (DataColumn),
					pcols.ToArray (typeof (CodeExpression)) as CodeExpression []),
				NewArray (
					typeof (DataColumn),
					ccols.ToArray (typeof (CodeExpression)) as CodeExpression []))));

			m.Statements.Add (Let (
				PropRef (Local ("fkc"), "AcceptRejectRule"),
				FieldRef (TypeRefExp (typeof (AcceptRejectRule)), Enum.GetName (typeof (AcceptRejectRule), fkc.AcceptRejectRule))));
			m.Statements.Add (Let (
				PropRef (Local ("fkc"), "DeleteRule"),
				FieldRef (TypeRefExp (typeof (Rule)), Enum.GetName (typeof (Rule), fkc.DeleteRule))));
			m.Statements.Add (Let (
				PropRef (Local ("fkc"), "UpdateRule"),
				FieldRef (TypeRefExp (typeof (Rule)), Enum.GetName (typeof (Rule), fkc.UpdateRule))));

			m.Statements.Add (MethodInvoke (PropRef (FieldRef (tableField), "Constraints"), "Add", Local ("fkc")));
		}

		private CodeMemberMethod CreateDataSetInitializeFields ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Attributes = MemberAttributes.Assembly;
			m.Name = "InitializeFields";

			foreach (DataTable dt in ds.Tables)
				m.Statements.Add (Eval (MethodInvoke (FieldRef ("__table" + opts.TableMemberName (dt.TableName)), "InitializeFields")));

			foreach (DataRelation rel in ds.Relations)
				m.Statements.Add (Let (FieldRef ("__relation" + opts.RelationName (rel.RelationName)), IndexerRef (PropRef ("Relations"), Const (rel.RelationName))));

			return m;
		}

		private CodeMemberMethod CreateDataSetSchemaChanged ()
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "SchemaChanged";
			m.Parameters.Add (Param (typeof (object), "sender"));
			m.Parameters.Add (Param (typeof (CollectionChangeEventArgs), "e"));

			m.Statements.Add (
				new CodeConditionStatement (
					EqualsValue (
						PropRef (ParamRef ("e"), "Action"),
						FieldRef (TypeRefExp (typeof (CollectionChangeAction)), "Remove")),
					new CodeStatement [] { Eval (MethodInvoke ("InitializeFields")) },
					new CodeStatement [] {}));
			return m;
		}

		private void CreateDataSetTableMembers (CodeTypeDeclaration dsType, DataTable table)
		{
			string tableTypeName = opts.TableTypeName (table.TableName);
			string tableVarName = opts.TableMemberName (table.TableName);

			CodeMemberField privTable = new CodeMemberField ();
			privTable.Type = TypeRef (tableTypeName);
			privTable.Name = "__table" + tableVarName;
			dsType.Members.Add (privTable);

			CodeMemberProperty pubTable = new CodeMemberProperty ();
			pubTable.Type = TypeRef (tableTypeName);
			pubTable.Attributes = MemberAttributes.Public;
			pubTable.Name = tableVarName == table.TableName ? "_"+tableVarName : tableVarName;
			pubTable.HasSet = false;
			// Code: return __table[foo];
			pubTable.GetStatements.Add (Return (FieldRef ("__table" + tableVarName)));

			dsType.Members.Add (pubTable);

		}

		private void CreateDataSetRelationMembers (CodeTypeDeclaration dsType, DataRelation relation)
		{
			string relName = opts.RelationName (relation.RelationName);
			string fieldName = "__relation" + relName;

			CodeMemberField field = new CodeMemberField ();
			field.Type = TypeRef (typeof (DataRelation));
			field.Name = fieldName;
			dsType.Members.Add (field);

			// This is not supported in MS.NET
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = TypeRef (typeof (DataRelation));
			prop.Attributes = MemberAttributes.Public;
			prop.Name = relName;
			prop.HasSet = false;
			// Code: return __relation[foo_bar];
			prop.GetStatements.Add (Return (FieldRef (fieldName)));
			dsType.Members.Add (prop);
		}

#endregion



#region DataTable class

		private CodeTypeDeclaration GenerateDataTableType (DataTable dt)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.TableTypeName (dt.TableName);
			t.BaseTypes.Add (TypeRef (typeof (DataTable)));
			t.BaseTypes.Add (TypeRef (typeof (IEnumerable)));

			t.Members.Add (CreateTableCtor1 (dt));
			t.Members.Add (CreateTableCtor2 (dt));

			t.Members.Add (CreateTableCount (dt));
			t.Members.Add (CreateTableIndexer (dt));

			t.Members.Add (CreateTableInitializeClass (dt));
			t.Members.Add (CreateTableInitializeFields (dt));

			t.Members.Add (CreateTableGetEnumerator (dt));
			t.Members.Add (CreateTableClone (dt));
			t.Members.Add (CreateTableCreateInstance (dt));

			t.Members.Add (CreateTableAddRow1 (dt));
			t.Members.Add (CreateTableAddRow2 (dt));
			t.Members.Add (CreateTableNewRow (dt));
			t.Members.Add (CreateTableNewRowFromBuilder (dt));
			t.Members.Add (CreateTableRemoveRow (dt));
			t.Members.Add (CreateTableGetRowType (dt));

			t.Members.Add (CreateTableEventStarter (dt, "Changing"));
			t.Members.Add (CreateTableEventStarter (dt, "Changed"));
			t.Members.Add (CreateTableEventStarter (dt, "Deleting"));
			t.Members.Add (CreateTableEventStarter (dt, "Deleted"));

			// events
			t.Members.Add (CreateTableEvent (dt, "RowChanging"));
			t.Members.Add (CreateTableEvent (dt, "RowChanged"));
			t.Members.Add (CreateTableEvent (dt, "RowDeleting"));
			t.Members.Add (CreateTableEvent (dt, "RowDeleted"));

			// column members
			foreach (DataColumn col in dt.Columns) {
				t.Members.Add (CreateTableColumnField (dt, col));
				t.Members.Add (CreateTableColumnProperty (dt, col));
			}

			return t;
		}

		// Code:
		//  internal [foo]DataTable () : base ("[foo]")
		//  {
		//    InitializeClass ();
		//  }
		private CodeConstructor CreateTableCtor1 (DataTable dt)
		{
			CodeConstructor c = new CodeConstructor ();
			c.Attributes = MemberAttributes.Assembly;
			c.BaseConstructorArgs.Add (Const (dt.TableName));
			c.Statements.Add (Eval (MethodInvoke ("InitializeClass")));
			c.Statements.Add (Eval (MethodInvoke ("InitializeFields")));
			return c;
		}

		// Code:
		//  internal [foo]DataTable (DataTable table) : base (table.TableName)
		//  {
		//    // TODO: implement
		//    throw new NotImplementedException ();
		//  }
		private CodeConstructor CreateTableCtor2 (DataTable dt)
		{
			CodeConstructor c = new CodeConstructor ();
			c.Attributes = MemberAttributes.Assembly;
			c.Parameters.Add (Param (typeof (DataTable), GetRowTableFieldName (dt)));
			c.BaseConstructorArgs.Add (PropRef (ParamRef (GetRowTableFieldName (dt)), "TableName"));
			// TODO: implement
			c.Statements.Add (Comment ("TODO: implement"));
			c.Statements.Add (Throw (New (typeof (NotImplementedException))));
			return c;
		}

		private CodeMemberMethod CreateTableInitializeClass (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitializeClass";
			foreach (DataColumn col in dt.Columns) {
				m.Statements.Add (Eval (MethodInvoke (
					PropRef ("Columns"),
					"Add",
					New (typeof (DataColumn),
						Const (col.ColumnName),
						new CodeTypeOfExpression (col.DataType)
						))));
			}
			return m;
		}

		private CodeMemberMethod CreateTableInitializeFields (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitializeFields";
			m.Attributes = MemberAttributes.Assembly;

			string colRef;
			foreach (DataColumn col in dt.Columns) {
				colRef = String.Format("__column{0}", opts.TableColName (col.ColumnName));

				m.Statements.Add (Let (FieldRef (colRef), IndexerRef (PropRef ("Columns"), Const (col.ColumnName))));
				if (!col.AllowDBNull)
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "AllowDBNull"), Const (col.AllowDBNull)));
				if (col.DefaultValue != null && col.DefaultValue.GetType() != typeof(System.DBNull))
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "DefaultValue"), Const (col.DefaultValue)));
				if (col.AutoIncrement)
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "AutoIncrement"), Const (col.AutoIncrement)));
				if (col.AutoIncrementSeed != 0)
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "AutoIncrementSeed"), Const (col.AutoIncrementSeed)));
				if (col.AutoIncrementStep != 1)
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "AutoIncrementStep"), Const (col.AutoIncrementStep)));
				if (col.ReadOnly)
					m.Statements.Add (Let (FieldRef (PropRef (colRef), "ReadOnly"), Const (col.ReadOnly)));
			}
			return m;
		}

		private CodeMemberMethod CreateTableClone (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Clone";
			m.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (DataTable));
			string typeName = opts.TableTypeName (dt.TableName);
			m.Statements.Add (
				VarDecl (typeName, "t", Cast (typeName, MethodInvoke (Base (), "Clone"))));
			m.Statements.Add (Eval (MethodInvoke (Local ("t"), "InitializeFields")));
			m.Statements.Add (Return (Local ("t")));
			return m;
		}

		private CodeMemberMethod CreateTableGetEnumerator (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "GetEnumerator";
			m.Attributes = MemberAttributes.Public;
			m.ReturnType = TypeRef (typeof (IEnumerator));
			m.Statements.Add (Return (MethodInvoke (PropRef ("Rows"), "GetEnumerator")));
			m.ImplementationTypes.Add (TypeRef (typeof (IEnumerable)));
			return m;
		}

		private CodeMemberMethod CreateTableCreateInstance (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "CreateInstance";
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (DataTable));
			m.Statements.Add (Return (New (opts.TableTypeName (dt.TableName))));
			return m;
		}

		private CodeMemberField CreateTableColumnField (DataTable dt, DataColumn col)
		{
			CodeMemberField f = new CodeMemberField ();
			f.Name = "__column" + opts.ColumnName (col.ColumnName);
			f.Type = TypeRef (typeof (DataColumn));
			return f;
		}

		private CodeMemberProperty CreateTableColumnProperty (DataTable dt, DataColumn col)
		{
			string name = opts.ColumnName (col.ColumnName);
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = name + "Column";
			p.Attributes = MemberAttributes.Assembly;
			p.Type = TypeRef (typeof (DataColumn));
			p.HasSet = false;
			p.GetStatements.Add (Return (FieldRef ("__column" + name)));
			return p;
		}

		private CodeMemberProperty CreateTableCount (DataTable dt)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Count";
			p.Attributes = MemberAttributes.Public;
			p.Type = TypeRef (typeof (int));
			p.HasSet = false;
			p.GetStatements.Add (Return (PropRef (PropRef ("Rows"), "Count")));
			return p;
		}

		private CodeMemberProperty CreateTableIndexer (DataTable dt)
		{
			string rowName = opts.RowName (dt.TableName);
			CodeMemberProperty ix = new CodeMemberProperty ();
			ix.Name = "Item"; // indexer
			ix.Attributes = MemberAttributes.Public;
			ix.Type = TypeRef (rowName);
			ix.Parameters.Add (Param (typeof (int), "i"));
			ix.HasSet = false;
			ix.GetStatements.Add (Return (Cast (rowName, IndexerRef (PropRef ("Rows"), ParamRef ("i")))));
			return ix;
		}

		private CodeMemberMethod CreateTableAddRow1 (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName);
			m.Name = "Add" + rowType;
			m.Attributes = MemberAttributes.Public;
			m.Parameters.Add (Param (TypeRef (rowType), "row"));
			m.Statements.Add (Eval (MethodInvoke (PropRef ("Rows"), "Add", ParamRef ("row"))));
			return m;
		}

		private CodeMemberMethod CreateTableAddRow2 (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName);
			m.Name = "Add" + rowType;
			m.ReturnType = TypeRef (rowType);
			m.Attributes = MemberAttributes.Public;

			m.Statements.Add (VarDecl (rowType, "row", MethodInvoke ("New" + rowType)));

			foreach (DataColumn col in dt.Columns) {
				if (col.ColumnMapping == MappingType.Hidden) {
					foreach (DataRelation r in dt.DataSet.Relations) {
						if (r.ChildTable == dt) {
							// parameter
							string paramType = opts.RowName (r.ParentTable.TableName);
							string paramName = paramType;
							m.Parameters.Add (Param (paramType, paramName));
							// CODE: SetParentRow (fooRow, DataSet.Relations ["foo_bar"]);
							m.Statements.Add (Eval (MethodInvoke (Local ("row"), "SetParentRow", ParamRef (paramName), IndexerRef (PropRef (PropRef ("DataSet"), "Relations"), Const (r.RelationName)))));
							break;
						}
					}
				}
				else {
					// parameter
					string paramName = opts.ColumnName (col.ColumnName);
					m.Parameters.Add (Param (col.DataType, paramName));
					// row ["foo"] = foo;
					m.Statements.Add (Let (IndexerRef (Local ("row"), Const (paramName)), ParamRef (paramName)));
				}
			}

			// Rows.Add (row);
			m.Statements.Add (MethodInvoke (PropRef ("Rows"), "Add", Local ("row")));
			m.Statements.Add (Return (Local ("row")));

			return m;
		}

		private CodeMemberMethod CreateTableNewRow (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName);
			m.Name = "New" + rowType;
			m.ReturnType = TypeRef (rowType);
			m.Attributes = MemberAttributes.Public;
			m.Statements.Add (Return (Cast (rowType, MethodInvoke ("NewRow"))));
			return m;
		}

		private CodeMemberMethod CreateTableNewRowFromBuilder (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "NewRowFromBuilder";
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (DataRow));
			m.Parameters.Add (Param (typeof (DataRowBuilder), "builder"));
			m.Statements.Add (Return (New (opts.RowName (dt.TableName), ParamRef ("builder"))));
			return m;
		}

		private CodeMemberMethod CreateTableRemoveRow (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName);
			m.Name = "Remove" + rowType;
			m.Attributes = MemberAttributes.Public;
			m.Parameters.Add (Param (TypeRef (rowType), "row"));
			m.Statements.Add (Eval (MethodInvoke (PropRef ("Rows"), "Remove", ParamRef ("row"))));
			return m;
		}

		private CodeMemberMethod CreateTableGetRowType (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "GetRowType";
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (Type));
			m.Statements.Add (Return (new CodeTypeOfExpression (opts.RowName (dt.TableName))));
			return m;
		}

		private CodeMemberMethod CreateTableEventStarter (DataTable dt, string type)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "OnRow" + type;
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			m.Parameters.Add (Param (typeof (DataRowChangeEventArgs), "e"));

			m.Statements.Add (Eval (MethodInvoke (
					Base (),
					m.Name,
					ParamRef ("e"))));
			string eventName = opts.TableMemberName (dt.TableName) + "Row" + type;
			CodeStatement trueStmt = Eval (
				new CodeDelegateInvokeExpression(
					new CodeEventReferenceExpression (This (), eventName),
					This (), 
					New (
						opts.EventArgsName (dt.TableName),
						Cast (opts.RowName (dt.TableName), PropRef (ParamRef ("e"), "Row")),
						PropRef (ParamRef ("e"), "Action"))));

			m.Statements.Add (
				new CodeConditionStatement (
					Inequals (EventRef (eventName), Const (null)),
					new CodeStatement [] {trueStmt},
					new CodeStatement [] {}));

			return m;
		}

		private CodeMemberEvent CreateTableEvent (DataTable dt, string nameSuffix)
		{
			CodeMemberEvent cme = new CodeMemberEvent ();
			cme.Attributes = MemberAttributes.Public;
			cme.Name = opts.TableMemberName (dt.TableName) + nameSuffix;
			cme.Type = TypeRef (opts.TableDelegateName (dt.TableName));
			return cme;
		}

#endregion



#region Row class

		public CodeTypeDeclaration GenerateDataRowType (DataTable dt)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.RowName (dt.TableName);
			t.BaseTypes.Add (TypeRef (typeof (DataRow)));

			t.Members.Add (CreateRowCtor (dt));

			t.Members.Add (CreateRowTableField (dt));

			foreach (DataColumn col in dt.Columns) {
				if (col.ColumnMapping != MappingType.Hidden) {
					t.Members.Add (CreateRowColumnProperty (dt, col));
					t.Members.Add (CreateRowColumnIsNull (dt, col));
					t.Members.Add (CreateRowColumnSetNull (dt, col));
				}
			}

			foreach (DataRelation rel in dt.ParentRelations)
				t.Members.Add (CreateRowParentRowProperty (dt, rel));
			foreach (DataRelation rel in dt.ChildRelations)
				t.Members.Add (CreateRowGetChildRows (dt, rel));

			return t;
		}

		private CodeConstructor CreateRowCtor (DataTable dt)
		{
			CodeConstructor c = new CodeConstructor ();
			c.Attributes = MemberAttributes.Assembly;
			c.Parameters.Add (Param (typeof (DataRowBuilder), "builder"));
			c.BaseConstructorArgs.Add (ParamRef ("builder"));
			c.Statements.Add (Let (FieldRef (GetRowTableFieldName (dt)), Cast (
				opts.TableTypeName (dt.TableName),
				PropRef ("Table"))));
			return c;
		}
		
		private string GetRowTableFieldName (DataTable dt)
		{
			return "table" + dt.TableName;
		}
		private CodeMemberField CreateRowTableField (DataTable dt)
		{
			CodeMemberField f = new CodeMemberField ();
			f.Name = GetRowTableFieldName (dt);
			f.Type = TypeRef (opts.TableTypeName (dt.TableName));
			return f;
		}

		private CodeMemberProperty CreateRowColumnProperty (DataTable dt, DataColumn col)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = opts.ColumnName (col.ColumnName);
			p.Type = TypeRef (col.DataType);
			p.Attributes = MemberAttributes.Public;

			// This part should be better than MS code output.
			// Code:
			//  object ret = this [col];
			//  if (ret == DBNull.Value)
			//    throw new StrongTypingException ()
			//  else
			//    return (type) ret;
			p.GetStatements.Add (VarDecl (typeof (object), "ret",
				IndexerRef (PropRef 
					(PropRef (GetRowTableFieldName (dt)), 
					opts.TableColName (col.ColumnName) + "Column"))));
			p.GetStatements.Add (new CodeConditionStatement (
				Equals (
					Local ("ret"),
					PropRef (TypeRefExp (typeof (DBNull)), "Value")),
				new CodeStatement [] {
					Throw (New (typeof (StrongTypingException), Const ("Cannot get strong typed value since it is DB null."), Const (null))) },
				new CodeStatement [] {
					Return (Cast (col.DataType, Local ("ret"))) }));

			p.SetStatements.Add (Let (IndexerRef (PropRef (PropRef (GetRowTableFieldName (dt)), opts.TableColName (col.ColumnName) + "Column")), new CodePropertySetValueReferenceExpression ()));

			return p;
		}

		private CodeMemberMethod CreateRowColumnIsNull (DataTable dt, DataColumn col)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Is" + opts.ColumnName (col.ColumnName) + "Null";
			m.Attributes = MemberAttributes.Public;
			m.ReturnType = TypeRef (typeof (bool));
			m.Statements.Add (Return (MethodInvoke (
				"IsNull",
				// table[foo].[bar]Column
				PropRef (
					PropRef (GetRowTableFieldName (dt)), 
					opts.TableColName (col.ColumnName) + "Column"))));
			return m;
		}

		private CodeMemberMethod CreateRowColumnSetNull (DataTable dt, DataColumn col)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Set" + opts.ColumnName (col.ColumnName) + "Null";
			m.Attributes = MemberAttributes.Public;
			m.Statements.Add (Let (IndexerRef (
				PropRef (
					PropRef (GetRowTableFieldName (dt)), 
					opts.TableColName (col.ColumnName) + "Column")),
				PropRef (TypeRefExp (typeof (DBNull)), "Value")));

			return m;
		}

		private CodeMemberProperty CreateRowParentRowProperty (DataTable dt, DataRelation rel)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = opts.TableMemberName (rel.ParentTable.TableName) + "Row" +
				(rel.ParentTable.TableName == rel.ChildTable.TableName ? "Parent" : String.Empty);
			p.Attributes = MemberAttributes.Public;
			p.Type = TypeRef (opts.RowName (rel.ParentTable.TableName));
			p.GetStatements.Add (Return (Cast (p.Type, MethodInvoke (
				"GetParentRow",
				IndexerRef (
					PropRef (
						PropRef (
							PropRef ("Table"),
							"DataSet"),
						"Relations"),
					Const (rel.RelationName))))));
			p.SetStatements.Add (Eval (MethodInvoke (
				"SetParentRow",
				new CodePropertySetValueReferenceExpression (),
				IndexerRef (
					PropRef (
						PropRef (
							PropRef ("Table"),
							"DataSet"),
						"Relations"),
					Const (rel.RelationName)))));

			return p;
		}

		private CodeMemberMethod CreateRowGetChildRows (DataTable dt, DataRelation rel)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Get" + opts.TableMemberName (rel.ChildTable.TableName) + "Rows";
			m.Attributes = MemberAttributes.Public;
			m.ReturnType = new CodeTypeReference (opts.RowName (rel.ChildTable.TableName), 1);
			m.Statements.Add (Return (Cast (m.ReturnType, MethodInvoke (
				"GetChildRows",
				IndexerRef (
					PropRef (
						PropRef (
							PropRef ("Table"),
							"DataSet"),
						"Relations"),
					Const (rel.RelationName))))));
			return m;
		}

#endregion


#region Event class

		// Code:
		//  public class [foo]ChangeEventArgs : EventArgs
		//  {
		//    private [foo]Row eventRow;
		//    private DataRowAction eventAction;
		//    (.ctor())
		//    (Row)
		//    (Action)
		//  }
		private CodeTypeDeclaration GenerateEventType (DataTable dt)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.EventArgsName (dt.TableName);
			t.BaseTypes.Add (TypeRef (typeof (EventArgs)));
			t.Attributes = MemberAttributes.Public;

			t.Members.Add (
				new CodeMemberField (
					TypeRef (opts.RowName (dt.TableName)),
					"eventRow"));
			t.Members.Add (
				new CodeMemberField (
					TypeRef (typeof (DataRowAction)), "eventAction"));
			t.Members.Add (CreateEventCtor (dt));

			t.Members.Add (CreateEventRow (dt));

			t.Members.Add (CreateEventAction (dt));

			return t;
		}

		// Code:
		//  public [foo]RowChangeEventArgs ([foo]Row r, DataRowAction a)
		//  {
		//    eventRow = r;
		//    eventAction = a;
		//  }
		private CodeConstructor CreateEventCtor (DataTable dt)
		{
			CodeConstructor c = new CodeConstructor ();
			c.Attributes = MemberAttributes.Public;
			c.Parameters.Add (Param (TypeRef (opts.RowName (dt.TableName)), "r"));
			c.Parameters.Add (Param (TypeRef (typeof (DataRowAction)), "a"));
			c.Statements.Add (Let (FieldRef ("eventRow"), ParamRef ("r")));
			c.Statements.Add (Let (FieldRef ("eventAction"), ParamRef ("a")));

			return c;
		}

		// Code:
		//  public [foo]Row Row {
		//   get { return eventRow; }
		// }
		private CodeMemberProperty CreateEventRow (DataTable dt)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Row";
			p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			p.Type = TypeRef (opts.RowName (dt.TableName));
			p.HasSet = false;
			p.GetStatements.Add (Return (FieldRef ("eventRow")));
			return p;
		}

		// Code:
		//  public DataRowAction Action {
		//   get { return eventAction; }
		// }
		private CodeMemberProperty CreateEventAction (DataTable dt)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Action";
			p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			p.Type = TypeRef (typeof (DataRowAction));
			p.HasSet = false;
			p.GetStatements.Add (Return (FieldRef ("eventAction")));
			return p;
		}

#endregion

#if NET_2_0
#region Table Adapter class
		
		private CodeTypeDeclaration GenerateTableAdapterType (TableAdapterSchemaInfo taInfo)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.TableAdapterName (taInfo.Name);
			t.BaseTypes.Add (TypeRef (taInfo.BaseClass));
		
			t.Members.Add (CreateTableAdapterDefaultCtor ());

			// table adapter fields/properties
			CreateDBAdapterFieldAndProperty (t, taInfo.Adapter);
			CreateDBConnectionFieldAndProperty (t, taInfo.Connection);
			
			DbCommand cmd = null;
			if (taInfo.Commands.Count > 0)
				cmd = ((DbCommandInfo)taInfo.Commands[0]).Command;
			else
				cmd = taInfo.Provider.CreateCommand ();
			CreateDBCommandCollectionFieldAndProperty (t, cmd);
			CreateAdapterClearBeforeFillFieldAndProperty (t);

			CreateAdapterInitializeMethod (t, taInfo);
			CreateConnectionInitializeMethod (t, taInfo);
			CreateCommandCollectionInitializeMethod (t, taInfo);

			CreateDbSourceMethods (t, taInfo);
			if (taInfo.ShortCommands)
				CreateShortCommandMethods (t, taInfo);
			
			return t;
		}
		
		private CodeConstructor CreateTableAdapterDefaultCtor ()
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Statements.Add (Let (PropRef ("ClearBeforeFill"), Const (true)));
			
			return ctor;
		}	

		private void CreateDBAdapterFieldAndProperty (CodeTypeDeclaration t, DbDataAdapter adapter)
		{
			CodeExpression expr;
			CodeStatement setStmt;
			CodeStatement stmt;
			CodeMemberField f = new CodeMemberField ();
			f.Name = "_adapter";
			f.Type = TypeRef (adapter.GetType ());
			t.Members.Add (f);
			
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Adapter";
			p.Attributes = MemberAttributes.Private;
			p.Type = f.Type;
			p.HasSet = false;

			expr = FieldRef ("_adapter");
			setStmt = Eval (MethodInvoke ("InitAdapter"));
			stmt = new CodeConditionStatement (Equals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt},
			                                   new CodeStatement [] {});
			p.GetStatements.Add (stmt);
			p.GetStatements.Add (Return (expr));
			t.Members.Add (p);
		}
		
		private void CreateDBConnectionFieldAndProperty (CodeTypeDeclaration t, DbConnection conn)
		{
			CodeExpression expr;
			CodeStatement setStmt;
			CodeStatement stmt;
			CodeMemberField f = new CodeMemberField ();
			f.Name = "_connection";
			f.Type = TypeRef (conn.GetType ());
			t.Members.Add (f);

			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Connection";
			p.Attributes = MemberAttributes.Assembly;
			p.Type = f.Type;

			expr = FieldRef ("_connection");
			setStmt = Eval (MethodInvoke ("InitConnection"));
			stmt = new CodeConditionStatement (Equals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt},
			                                   new CodeStatement [] {});
			p.GetStatements.Add (stmt);
			p.GetStatements.Add (Return (expr));
			p.SetStatements.Add (Let (expr, new CodePropertySetValueReferenceExpression()));

			// update connection in Insert/Delete/Update commands of adapter
						
			// insert command
			string cmdStr = "InsertCommand";
			string connStr = "Connection";
			setStmt = null;
			stmt = null;
			expr = null;

			expr = PropRef (PropRef ("Adapter"), cmdStr);
			setStmt = Let (PropRef (expr, connStr), new CodePropertySetValueReferenceExpression());
			stmt = new CodeConditionStatement (Inequals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt},
			                                   new CodeStatement [] {});			
			p.SetStatements.Add (stmt);

			// delete command
			setStmt = null;
			stmt = null;
			expr = null;
			
			cmdStr = "DeleteCommand";
			expr = PropRef (PropRef ("Adapter"), cmdStr);
			setStmt = Let (PropRef (expr, connStr), new CodePropertySetValueReferenceExpression());
			stmt = new CodeConditionStatement (Inequals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt}, new CodeStatement [] {});
			p.SetStatements.Add (stmt);

			// update command
			setStmt = null;
			stmt = null;

			cmdStr = "UpdateCommand";
			expr = PropRef (PropRef ("Adapter"), cmdStr);			
			setStmt = Let (PropRef (expr, connStr), new CodePropertySetValueReferenceExpression());
			stmt = new CodeConditionStatement (Inequals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt}, new CodeStatement [] {});
			p.SetStatements.Add (stmt);

			// iterate through command collection and update it
			setStmt = null;
			expr = null;
			stmt = null;
			setStmt = VarDecl (typeof (int), "i", Const (0));
			expr = LessThan (Local ("i"), PropRef (PropRef ("CommandCollection"), "Length"));
			stmt = Let (Local ("i"), Compute (Local ("i"), Const (1), CodeBinaryOperatorType.Add));
			
			// statements to execute in the loop
			CodeExpression expr1 = IndexerRef (PropRef ("CommandCollection"), Local ("i"));
			CodeStatement setStmt1 = Let (PropRef (expr1, "Connection"), new CodePropertySetValueReferenceExpression());
			CodeStatement stmt1 = new CodeConditionStatement (Inequals (expr1, Const (null)),
			                                   new CodeStatement [] {setStmt1}, new CodeStatement [] {});
			CodeIterationStatement forLoop = new CodeIterationStatement (setStmt, expr, stmt,
			                                                             new CodeStatement[] {stmt1});
			p.SetStatements.Add (forLoop);						
			t.Members.Add (p);
		}

		private void CreateDBCommandCollectionFieldAndProperty (CodeTypeDeclaration t, DbCommand cmd)
		{
			CodeExpression expr;
			CodeStatement setStmt;
			CodeStatement stmt;
			CodeMemberField f = new CodeMemberField ();
			f.Name = "_commandCollection";
			f.Type = TypeRefArray (cmd.GetType (), 1);
			t.Members.Add (f);
			
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "CommandCollection";
			p.Attributes = MemberAttributes.Family;
			p.Type = f.Type;
			p.HasSet = false;

			expr = FieldRef ("_commandCollection");
			setStmt = Eval (MethodInvoke ("InitCommandCollection"));
			stmt = new CodeConditionStatement (Equals (expr, Const (null)),
			                                   new CodeStatement [] {setStmt},
			                                   new CodeStatement [] {});
			p.GetStatements.Add (stmt);
			p.GetStatements.Add (Return (expr));
			t.Members.Add (p);
		}

		private void CreateAdapterClearBeforeFillFieldAndProperty (CodeTypeDeclaration t)
		{
			CodeMemberField f = new CodeMemberField ();
			f.Name = "_clearBeforeFill";
			f.Type = TypeRef (typeof (bool));
			t.Members.Add (f);
			
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "ClearBeforeFill";
			p.Attributes = MemberAttributes.Public;
			p.Type = f.Type;
			p.SetStatements.Add (Let (FieldRef ("_clearBeforeFill"), 
			                          new CodePropertySetValueReferenceExpression()));
			p.GetStatements.Add (Return (FieldRef ("_clearBeforeFill")));
			t.Members.Add (p);
		}

		private void CreateAdapterInitializeMethod (CodeTypeDeclaration t, TableAdapterSchemaInfo taInfo)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitAdapter";
			m.Attributes = MemberAttributes.Private;
			
			// code statements
			CodeExpression expr;
			CodeStatement stmt;
			
			// initialize adapter
			expr = FieldRef ("_adapter");
			stmt = Let (expr, New (taInfo.Adapter.GetType ()));
			m.Statements.Add (stmt);
			
			// populate tableMapping
			stmt = VarDecl (typeof (DataTableMapping), "tableMapping", null);
			m.Statements.Add (stmt);
			foreach (DataTableMapping tblMap in taInfo.Adapter.TableMappings) {
				expr = Local ("tableMapping");
				stmt = Let (expr, New (tblMap.GetType ()));
				m.Statements.Add (stmt);
				
				stmt = Let (PropRef (expr, "SourceTable"), Const (tblMap.SourceTable));
				m.Statements.Add (stmt);
				
				stmt = Let (PropRef (expr, "DataSetTable"), Const (tblMap.DataSetTable));
				m.Statements.Add (stmt);
				
				foreach (DataColumnMapping colMap in tblMap.ColumnMappings) { 
					stmt = Eval (MethodInvoke (PropRef (expr, "ColumnMappings"), "Add", 
					                           new CodeExpression [] {Const (colMap.SourceColumn), Const (colMap.DataSetColumn)}));
					m.Statements.Add (stmt);
				}
				expr = PropRef (FieldRef ("_adapter"), "TableMappings");
				stmt = Eval (MethodInvoke (expr, "Add", Local ("tableMapping")));
				m.Statements.Add (stmt);
			}
			// Generate code for adapter's deletecommand
			expr = PropRef (FieldRef ("_adapter"), "DeleteCommand");
			DbCommand cmd = taInfo.Adapter.DeleteCommand;
			AddDbCommandStatements (m, expr, cmd);

			// Generate code for adapter's insertcommand
			expr = PropRef (FieldRef ("_adapter"), "InsertCommand");
			cmd = taInfo.Adapter.InsertCommand;
			AddDbCommandStatements (m, expr, cmd);

			// Generate code for adapter's updatecommand
			expr = PropRef (FieldRef ("_adapter"), "UpdateCommand");
			cmd = taInfo.Adapter.UpdateCommand;
			AddDbCommandStatements (m, expr, cmd);

			t.Members.Add (m);
		}
		
		private void AddDbCommandStatements (CodeMemberMethod m, 
		                                     CodeExpression expr, 
		                                     DbCommand cmd)
		{
			if (cmd == null)
				return;
			
			CodeExpression expr1;
			CodeStatement stmt = Let (expr, New (cmd.GetType ()));
			m.Statements.Add (stmt);
			
			stmt = Let (PropRef (expr,"Connection"), PropRef ("Connection"));
			m.Statements.Add (stmt);
			stmt = Let (PropRef (expr, "CommandText"), Const (cmd.CommandText));
			m.Statements.Add (stmt);
			expr1 = PropRef (Local(typeof (CommandType).FullName), cmd.CommandType.ToString ());
			stmt = Let (PropRef (expr, "CommandType"), expr1);
			m.Statements.Add (stmt);
			
			expr1 = PropRef (expr, "Parameters");
			foreach (DbParameter param in cmd.Parameters) {
				AddDbParameterStatements (m, expr1, param);
			}
		}
		
		private void AddDbParameterStatements (CodeMemberMethod m, 
		                                       CodeExpression expr, 
		                                       DbParameter param)
		{
			object dbType = param.FrameworkDbType;
			string srcColumn = null;
			
			if (param.SourceColumn != String.Empty)
				srcColumn = param.SourceColumn;
			
			CodeExpression[] args = new CodeExpression[] {
				Const (param.ParameterName),
				PropRef (Local(dbType.GetType().FullName), dbType.ToString ()), 
				Const (param.Size),
				PropRef(Local(typeof (ParameterDirection).FullName), param.Direction.ToString ()),
				Const (param.IsNullable),
				Const (((IDbDataParameter)param).Precision),
				Const (((IDbDataParameter)param).Scale),
				Const (srcColumn),
				PropRef (Local (typeof (DataRowVersion).FullName), param.SourceVersion.ToString ()),
				/* Const (param.SourceColumnNullMapping), */ // TODO: Investigate with other providers
				Const (param.Value)
			};
			m.Statements.Add (Eval (MethodInvoke (expr, "Add", New (param.GetType (), args))));
		}
		private void CreateConnectionInitializeMethod (CodeTypeDeclaration t, 
		                                               TableAdapterSchemaInfo taInfo)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitConnection";
			m.Attributes = MemberAttributes.Private;
			
			// code statements
			CodeExpression expr, expr1;
			CodeStatement stmt;
			
			// initialize connection
			expr = FieldRef ("_connection");
			stmt = Let (expr, New (taInfo.Connection.GetType ()));
			m.Statements.Add (stmt);
			
			// assign connection string
			expr = PropRef (FieldRef ("_connection"), "ConnectionString");
			expr1 = IndexerRef (PropRef (Local (typeof (System.Configuration.ConfigurationManager).ToString()), "ConnectionStrings"), 
			                    Const (taInfo.ConnectionString));
			stmt = Let (expr, PropRef (expr1, "ConnectionString"));
			m.Statements.Add (stmt);
			
			t.Members.Add (m);
		}
		
		private void CreateCommandCollectionInitializeMethod (CodeTypeDeclaration t, 
		                                                      TableAdapterSchemaInfo taInfo)
		{
			//string tmp = null;
			Type type;
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "InitCommandCollection";
			m.Attributes = MemberAttributes.Private;
			
			// code statements
			CodeExpression expr, expr1;
			CodeStatement stmt;
			
			type = ((DbCommandInfo)taInfo.Commands[0]).Command.GetType();
			
			// initialize connection
			expr = FieldRef ("_commandCollection");
			stmt = Let (expr, NewArray (type, taInfo.Commands.Count));
			m.Statements.Add (stmt);
			
			// loop through the collection and generate the code
			for (int idx = 0; idx < taInfo.Commands.Count; idx++) {
				DbCommand cmd = ((DbCommandInfo)taInfo.Commands[idx]).Command;
				// Allocate
				expr1 = IndexerRef (expr, Const (idx));
				stmt = Let (expr1, New (type));
				m.Statements.Add (stmt);
				
				// Initialize cmd members
				stmt = Let (PropRef (expr1, "Connection"), PropRef ("Connection"));
				m.Statements.Add (stmt);
				stmt = Let (PropRef (expr1, "CommandText"), Const (cmd.CommandText));
				m.Statements.Add (stmt);
				stmt = Let (PropRef (expr1, "CommandType"), 
				            PropRef (Local(typeof (CommandType).FullName), cmd.CommandType.ToString ()));
				m.Statements.Add (stmt);
				expr1 = PropRef (expr1, "Parameters");
				foreach (DbParameter param in cmd.Parameters) {
					AddDbParameterStatements (m, expr1, param);
				}
			}		
			t.Members.Add (m);
		}
		
		private void CreateDbSourceMethods (CodeTypeDeclaration t, 
		                                    TableAdapterSchemaInfo taInfo)
		{
			string tmp = null;
			CodeMemberMethod m = null;
			CodeExpression expr, expr1, expr2;
			CodeStatement stmt;
			string tmpScalarVal = null;
			
			expr = PropRef (PropRef ("Adapter"), "SelectCommand");
			// loop through the collection and generate the code
			for (int idx = 0; idx < taInfo.Commands.Count; idx++) {
				DbCommandInfo cmdInfo = (DbCommandInfo)taInfo.Commands[idx];
				
				foreach (DbSourceMethodInfo mInfo in cmdInfo.Methods) {
					//Console.WriteLine ("Generating code for {0} method", mInfo.Name);
					
					// TODO: Add support for Fill methods
					if (mInfo.MethodType == GenerateMethodsType.Fill)
						continue;
					
					m = new CodeMemberMethod ();
					m.Name = mInfo.Name;
					
					stmt = Let (expr, IndexerRef (PropRef ("CommandCollection"), Const (idx)));
					m.Statements.Add (stmt);
					
					switch ((MemberAttributes) Enum.Parse (typeof (MemberAttributes), mInfo.Modifier)) {
						case MemberAttributes.Public:
							m.Attributes = MemberAttributes.Public;
							break;

						case MemberAttributes.Private:
							m.Attributes = MemberAttributes.Private;
							break;
						
						case MemberAttributes.Assembly:
							m.Attributes = MemberAttributes.Assembly;
							break;
						
						case MemberAttributes.Family:
							m.Attributes = MemberAttributes.Family;
							break;
					}
					//Console.WriteLine ("QueryType: {0}", mInfo.QueryType);
					QueryType qType = (QueryType) Enum.Parse (typeof (QueryType), mInfo.QueryType);
					switch (qType) {
						case QueryType.Scalar:
						case QueryType.NoData:
							// executes non query and returns status
							m.ReturnType = TypeRef (typeof (int));
							AddGeneratedMethodParametersAndStatements (m, expr, cmdInfo.Command);
						
							// store connection state
							tmp = typeof (System.Data.ConnectionState).FullName;
							expr1 = PropRef (Local ("command"), "Connection");
							expr2 = PropRef (PropRef (Local ("System"), "Data"), "ConnectionState");
							stmt = VarDecl (tmp, "previousConnectionState", PropRef (expr1, "State"));
							m.Statements.Add (stmt);
							
							// Open connection, if its not already
							CodeExpression expr3 = BitOps (PropRef (expr1, "State"), PropRef (expr2, "Open"),
						                                   CodeBinaryOperatorType.BitwiseAnd);
							stmt = new CodeConditionStatement (Inequals (expr3, PropRef (expr2, "Open")), 
							                                                  new CodeStatement[] {Eval (MethodInvoke (expr1, "Open", 
							                                         									new CodeExpression[] {}))},
							                                                  new CodeStatement[] {});
							m.Statements.Add (stmt);
						
							// declare return variable and execute query
							CodeTryCatchFinallyStatement try1 = new CodeTryCatchFinallyStatement ();
						
							if (qType == QueryType.NoData) {
								m.Statements.Add (VarDecl (typeof (int), "returnValue", null));
								expr3 = MethodInvoke (Local ("command"), "ExecuteNonQuery", new CodeExpression[] {});
							} else {
								tmpScalarVal = mInfo.ScalarCallRetval.Substring (0, mInfo.ScalarCallRetval.IndexOf (','));
								m.Statements.Add (VarDecl (TypeRef (tmpScalarVal).BaseType, "returnValue", null));
								expr3 = MethodInvoke (Local ("command"), "ExecuteScalar", new CodeExpression[] {});
							}
						
							// Execute query
							try1.TryStatements.Add (Let (Local ("returnValue"), expr3));
							
							// fill finally block
							stmt = new CodeConditionStatement (Equals (Local ("previousConnectionState"), PropRef (expr2, "Closed")), 
																new CodeStatement[] {Eval (MethodInvoke (expr1, "Close", 
							                                         									new CodeExpression[] {}))},
																new CodeStatement[] {});
							try1.FinallyStatements.Add (stmt);
							m.Statements.Add (try1);
						
							// return the value
							if (qType == QueryType.NoData) {
								m.Statements.Add (Return (Local ("returnValue")));
							} else {
								expr2 = Equals (Local ("returnValue"), Const (null));
								expr3 = Equals (MethodInvoke (Local ("returnValue"), "GetType", new CodeExpression[] {}), 
							                	TypeOfRef ("System.DBNull"));
								stmt = new CodeConditionStatement (BooleanOps (expr2, expr3, CodeBinaryOperatorType.BooleanOr), 
							                                   		new CodeStatement[] {Return (Const (null))},
																	new CodeStatement[] {Return (Cast (tmpScalarVal, Local ("returnValue")))});
								m.Statements.Add (stmt);
							}
						
							break;

						case QueryType.Rowset:
							// returns DataTable
							// TODO: Handle multiple DataTables
							tmp = opts.DataSetName (ds.DataSetName) + "." + opts.TableTypeName (ds.Tables[0].TableName);
							m.ReturnType = TypeRef (tmp);

							AddGeneratedMethodParametersAndStatements (m, expr, cmdInfo.Command);
							stmt = VarDecl (tmp, "dataTable", New (tmp));
							m.Statements.Add (stmt);

							// fill datatable
							expr = PropRef ("Adapter");
							stmt = Eval (MethodInvoke (expr, "Fill", Local ("dataTable")));
							m.Statements.Add (stmt);

							// return statement
							m.Statements.Add (Return (Local ("dataTable")));
							break;
					}
					t.Members.Add (m);
				}			
			}
		}
		
		private void AddGeneratedMethodParametersAndStatements (CodeMemberMethod m, 
		                                                        CodeExpression expr,
		                                                        DbCommand cmd)
		{
			CodeStatement stmt;
			CodeExpression expr1;
			int idx = 0;
			string tmp;

			foreach (DbParameter param in cmd.Parameters) {
				if (param.Direction != ParameterDirection.ReturnValue) {
					if (param.ParameterName[0] == '@')
						tmp = param.ParameterName.Substring (1);
					else
						tmp = param.ParameterName;
					if (param.SystemType != null)
						m.Parameters.Add (Param (TypeRef(param.SystemType), tmp));
					expr1 = IndexerRef (PropRef (expr, "Parameters"), Const (idx));
					stmt = Let (expr1, ParamRef (tmp));
					m.Statements.Add (stmt);
				}
				idx++;
			}
		}
		
		private void CreateShortCommandMethods (CodeTypeDeclaration t, TableAdapterSchemaInfo taInfo)
		{
			
		}
		
#endregion
#endif	//NET_2_0

	}
	
}


/* =========================================================


MonoDataSetGenerator API notes


** generator API:
	CreateDataSetClasses (
		DataSet ds,
		CodeNamespace cns,
		ICodeGenerator gen,
		GeneratorOptions options)

** classes:

*** Code naming method delegate

	public delegate string CodeNamingMethod (string sourceName);

	It is used in CodeGeneratorOptions (describled immediately below).



*** Generator Options

	public bool MakeClassesInsideDataSet
		indicates whether classes and delegates other than DataSet
		itself are "contained" in the DataSet class or not.

	public CodeNamingMethod CreateDataSetName;
	public CodeNamingMethod CreateTableTypeName;
	public CodeNamingMethod CreateTableMemberName;
	public CodeNamingMethod CreateColumnName;
	public CodeNamingMethod CreateRowName;
	public CodeNamingMethod CreateRelationName;
	public CodeNamingMethod CreateTableDelegateName;
	public CodeNamingMethod CreateEventArgsName;
		Custom methods each of that returns type or member name.

		By default, they are set as to useTypedDataSetGenerator.
		CreateIdName() with modifications listed as below:

		DataSetName: as is
		TableTypeName: "DataTable" suffix
		TableMemberName: as is
		ColumnName: as is
		RowName: "Row" suffix
		RelationName: (TBD; maybe had better have another delegate type)
		DelegateName: "RowChangedEventHandler" suffix
		EventArgsName: "RowChangedEventArgs" suffix

** Auto Generated classes

1. Custom DataSet class 

	class name = dataset name, encoded by options.CreateDataSetName().

*** .ctor

	public default .ctor()
		"initialize" class.
		set custom delegate on Tables.CollectionChanged
		set custom delegate on Relations.CollectionChanged

	runtime serialization .ctor()
		TBD

*** public members

	data tables: [foo]DataTable foo { return this.table[foo]; }

	Clone()
		init variables on new dataset.

*** protected members

	ShouldSerializeTables()
		returns false, while default DataSet returns true.
	ShouldSerializeRelations()
		returns false, while default DataSet returns true.

	ReadXmlSerializable() ... similar to runtime serialization
		TBD

	GetSchemaSerializable()
		Write its schema to temporary MemoryStream
		Read XML schema from the stream

*** internal members

	"init variables"
		set member fields (tables, relations)
		call members' "init variables"

	"init class"
		set DataSetName, Prefix, Namespace, Locale, CaseSensitive, EnforceConstraints
		for each table
			allocate table[foo] 
			Tables.Add() 
			create FKC: new FKC([rel], new DataColumn [] {table[foo].[keyColumnName]Column}, new DataColumn [] {table[child].[childColName]Column}
		fill Rule properties.
		allocate relation[rel] and fill Nested, then Relations.Add()

*** private members

	data tables: [foo]DataTable table[foo];

	data relations: DataRelation relation[rel];

	ShouldSerialize[foo]



2. Custom DataTable classes for each DataTable

	This class is created under the dataset.

*** internal members

	.ctor() : base("[foo]")
		initialize class

	.ctor(DataTable)
		wtf?

	DataColumn [bar]Column { return column[bar]; }

	"init variables"()
		fill each column fields

*** public members

	int Count { rowcount }

	this [int index] { row [i]; }

	event [foo]RowChangedEventHandler [foo]RowChanged
	event [foo]RowChangedEventHandler [foo]RowChanging
	event [foo]RowChangedEventHandler [foo]RowDeleted
	event [foo]RowChangedEventHandler [foo]RowDeleting

	void Add[foo]Row ([foo]Row row) { Rows.Add (row); }

	[foo]Row Add[foo]Row ([columnType] [columnName])
		create new [foo]row.
		set members
		Rows.Add ()
		// where
		//	non-relation-children are just created as column type
		//	relation-children are typeof fooRow[]

	GetEnumerator() { Rows.GetEnumerator (); }

	override DataTable Clone()
		"init variables"

	[foo]Row New[foo]Row()

	void Remove[foo]Row([foo]Row)

	//for each ChildRelations
	[bar]Row [] Get[foo_bar]Rows ()

*** protected members

	override DataTable CreateInstance() { return new }

	override DataRow NewRowFromBuilder(DataRowBuilder)

	override Type GetRowType()

	override void OnRowChanged(DataRowChangedEventArgs)
		base.()
		check this event [foo]RowChanged.

	override void OnRowChanging(DataRowChangedEventArgs)
	override void OnRowDeleted(DataRowChangedEventArgs)
	override void OnRowDeleting(DataRowChangedEventArgs)
	... as well

*** private members

	"initialize class"
		for each columns {
			column[bar] = new DataColumn (...);
			Columns.Add()
		}

	DataColumn [bar]Column

3. Custom DataRow classses

*** public members

	for simple members:

		[bar_type] [bar] {
			get { try { } catch { throw StrongTypingException(); }
			set { this [[foo]Table.[bar]Column] = value; }

		bool Is[bar]Null ()
			IsNull ([foo]Table.[bar]Column);

		void Set[bar]Null ()

	if the table is parent of some relations

		public [child]Row [] Get[child]Rows()

*** internal members

	.ctor(DataRowBuilder) : base.()
		table[foo] = Table;

*** private members

	[foo]DataTable table[foo]


4. Custom DataRowChangeEvent classes

*** private members

	[foo]Row eventRow
	DataRowAction eventAction

*** public members

	.ctor([foo]Row row, DataRowAction action)

	[foo]Row Row

	DataRowAction Action



5. public Table RowChangedEventHandler delegates

	[foo]RowChangedEventHandler(object, [foo]RowChangedEvent e)


======================================================== */
