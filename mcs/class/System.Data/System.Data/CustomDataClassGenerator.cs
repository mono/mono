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
using System.Collections;
using System.CodeDom;
using System.Globalization;
using System.Text;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;

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

		public static string MakeSafeName (string name, ICodeGenerator codeGen)
		{
			if (name == null || codeGen == null)
				throw new NullReferenceException ();

			name = codeGen.CreateValidIdentifier (name);

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
	public class ClassGeneratorOptions
#else
	internal class ClassGeneratorOptions
#endif
	{
		public bool MakeClassesInsideDataSet = true; // default = MS compatible

		public CodeNamingMethod CreateDataSetName;
		public CodeNamingMethod CreateTableTypeName;
		public CodeNamingMethod CreateTableMemberName;
		public CodeNamingMethod CreateTableColumnName;
		public CodeNamingMethod CreateColumnName;
		public CodeNamingMethod CreateRowName;
		public CodeNamingMethod CreateRelationName;
		public CodeNamingMethod CreateTableDelegateName;
		public CodeNamingMethod CreateEventArgsName;

		internal string DataSetName (string source, ICodeGenerator gen)
		{
			if (CreateDataSetName != null)
				return CreateDataSetName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal string TableTypeName (string source, ICodeGenerator gen)
		{
			if (CreateTableTypeName != null)
				return CreateTableTypeName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "DataTable";
		}

		internal string TableMemberName (string source, ICodeGenerator gen)
		{
			if (CreateTableMemberName != null)
				return CreateTableMemberName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal string TableColName (string source, ICodeGenerator gen)
		{
			if (CreateTableColumnName != null)
				return CreateTableColumnName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal string TableDelegateName (string source, ICodeGenerator gen)
		{
			if (CreateTableDelegateName != null)
				return CreateTableDelegateName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "RowChangedEventHandler";
		}

		internal string EventArgsName (string source, ICodeGenerator gen)
		{
			if (CreateEventArgsName != null)
				return CreateEventArgsName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "RowChangedEventArgs";
		}

		internal string ColumnName (string source, ICodeGenerator gen)
		{
			if (CreateColumnName != null)
				return CreateColumnName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen);
		}

		internal string RowName (string source, ICodeGenerator gen)
		{
			if (CreateRowName != null)
				return CreateRowName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "Row";
		}

		internal string RelationName (string source, ICodeGenerator gen)
		{
			if (CreateRelationName != null)
				return CreateRelationName (source, gen);
			else
				return CustomDataClassGenerator.MakeSafeName (source, gen) + "Relation";
		}
	}

	internal class Generator
	{
		static ClassGeneratorOptions DefaultOptions = new ClassGeneratorOptions ();

		DataSet ds;
		CodeNamespace cns;
		ClassGeneratorOptions opts;
		ICodeGenerator gen;

		CodeTypeDeclaration dsType;

		public Generator (DataSet ds, CodeNamespace cns, ICodeGenerator gen, ClassGeneratorOptions options)
		{
			this.ds = ds;
			this.cns = cns;
			this.gen = gen;
			this.opts = opts;

			if (opts == null)
				opts = DefaultOptions;
		}

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

				CodeTypeDelegate dtDelegate = new CodeTypeDelegate (opts.TableDelegateName (dt.TableName, gen));
				dtDelegate.Parameters.Add (Param (typeof (object), "o"));
				dtDelegate.Parameters.Add (Param (opts.EventArgsName (dt.TableName, gen), "e"));

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

		// note that this is "Identity" equality comparison
		private CodeBinaryOperatorExpression Equals (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.IdentityEquality, exp2);
		}

		private CodeBinaryOperatorExpression Inequals (CodeExpression exp1, CodeExpression exp2)
		{
			return new CodeBinaryOperatorExpression (exp1, CodeBinaryOperatorType.IdentityInequality, exp2);
		}

		private CodeTypeReferenceExpression TypeRefExp (Type t)
		{
			return new CodeTypeReferenceExpression (t);
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
			dsType = new CodeTypeDeclaration (opts.DataSetName (ds.DataSetName, gen));
			dsType.BaseTypes.Add (TypeRef (typeof (DataSet)));

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

			// GetSchemaSerializable()
			dsType.Members.Add (CreateDataSetGetSchemaSerializable ());
*/
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
					New (typeof (CollectionChangeEventHandler), FieldRef ("SchemaChanged")));
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

		// Code:
		// protected override bool ShouldSerializeRelations ()
		// {
		//   return true; // it should be false
		// }
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

		// Code:
		// protected override void ReadXmlSerializable()
		// {
		//   // TODO: implement
		//   throw new NotImplementedException ();
		// }
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
				string tableFieldName = "__table" + opts.TableMemberName (dt.TableName, gen);
				string tableTypeName = opts.TableTypeName (dt.TableName, gen);
				m.Statements.Add (Let (FieldRef (tableFieldName), New (tableTypeName)));
				m.Statements.Add (Eval (MethodInvoke (PropRef ("Tables"), "Add", FieldRef (tableFieldName))));
			}

			bool fkcExists = false;
			bool ucExists = false;
			// First the UniqueConstraints
			foreach (DataTable dt in ds.Tables) {
				string tname = "__table" + opts.TableMemberName (dt.TableName, gen);
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
				string tname = "__table" + opts.TableMemberName (dt.TableName, gen);
				foreach (Constraint c in dt.Constraints) {
					ForeignKeyConstraint fkc = c as ForeignKeyConstraint;
					if (fkc != null) {
						if (!fkcExists) {
							m.Statements.Add (VarDecl (typeof (ForeignKeyConstraint), "fkc", null));
							fkcExists = true;
						}
						string rtname = "__table" + opts.TableMemberName (fkc.RelatedTable.TableName, gen);
						CreateForeignKeyStatements (m, fkc, tname, rtname);
					}
				}
			}
			// What if other cases? dunno. Just ignore ;-)
			foreach (DataRelation rel in ds.Relations) {
				string relName = opts.RelationName (rel.RelationName, gen);
				ArrayList pcols = new ArrayList ();
				foreach (DataColumn pcol in rel.ParentColumns)
					pcols.Add (IndexerRef (PropRef (FieldRef ("__table" + opts.TableMemberName (rel.ParentTable.TableName, gen)), "Columns"), Const (pcol.ColumnName)));

				ArrayList ccols = new ArrayList ();
				foreach (DataColumn ccol in rel.ChildColumns)
					ccols.Add (IndexerRef (PropRef (FieldRef ("__table" + opts.TableMemberName (rel.ChildTable.TableName, gen)), "Columns"), Const (ccol.ColumnName)));

				// relation field
				string fieldName = "__relation" + relName;
				m.Statements.Add (Let (FieldRef (fieldName), New (typeof (DataRelation),
					Const (rel.RelationName),
					NewArray (typeof (DataColumn), pcols.ToArray (typeof (CodeExpression)) as CodeExpression []),
					NewArray (typeof (DataColumn), ccols.ToArray (typeof (CodeExpression)) as CodeExpression []),
					Const (false)
					)));
				m.Statements.Add (Let (PropRef (FieldRef (fieldName), "Nested"), Const (rel.Nested)));
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
				m.Statements.Add (Eval (MethodInvoke (FieldRef ("__table" + opts.TableMemberName (dt.TableName, gen)), "InitializeFields")));

			foreach (DataRelation rel in ds.Relations)
				m.Statements.Add (Let (FieldRef ("__relation" + opts.RelationName (rel.RelationName, gen)), IndexerRef (PropRef ("Relations"), Const (rel.RelationName))));

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
					Equals (
						PropRef (ParamRef ("e"), "Action"),
						FieldRef (TypeRefExp (typeof (CollectionChangeAction)), "Remove")),
					new CodeStatement [] { Eval (MethodInvoke ("InitializeFields")) },
					new CodeStatement [] {}));
			return m;
		}

		private void CreateDataSetTableMembers (CodeTypeDeclaration dsType, DataTable table)
		{
			string tableTypeName = opts.TableTypeName (table.TableName, gen);
			string tableVarName = opts.TableMemberName (table.TableName, gen);

			CodeMemberField privTable = new CodeMemberField ();
			privTable.Type = TypeRef (tableTypeName);
			privTable.Name = "__table" + tableVarName;
			dsType.Members.Add (privTable);

			CodeMemberProperty pubTable = new CodeMemberProperty ();
			pubTable.Type = TypeRef (tableTypeName);
			pubTable.Attributes = MemberAttributes.Public;
			pubTable.Name = tableVarName;
			pubTable.HasSet = false;
			// Code: return __table[foo];
			pubTable.GetStatements.Add (Return (FieldRef ("__table" + tableVarName)));

			dsType.Members.Add (pubTable);

		}

		private void CreateDataSetRelationMembers (CodeTypeDeclaration dsType, DataRelation relation)
		{
			string relName = opts.RelationName (relation.RelationName, gen);
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
			t.Name = opts.TableTypeName (dt.TableName, gen);
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
			c.Parameters.Add (Param (typeof (DataTable), "table"));
			c.BaseConstructorArgs.Add (PropRef (ParamRef ("table"), "TableName"));
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
			foreach (DataColumn col in dt.Columns)
				m.Statements.Add (Let (FieldRef ("__column" + opts.TableColName (col.ColumnName, gen)), IndexerRef (PropRef ("Columns"), Const (col.ColumnName))));
			return m;
		}

		private CodeMemberMethod CreateTableClone (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Clone";
			m.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (DataTable));
			string typeName = opts.TableTypeName (dt.TableName, gen);
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
			return m;
		}

		private CodeMemberMethod CreateTableCreateInstance (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "CreateInstance";
			m.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			m.ReturnType = TypeRef (typeof (DataTable));
			m.Statements.Add (Return (New (opts.TableTypeName (dt.TableName, gen))));
			return m;
		}

		private CodeMemberField CreateTableColumnField (DataTable dt, DataColumn col)
		{
			CodeMemberField f = new CodeMemberField ();
			f.Name = "__column" + opts.ColumnName (col.ColumnName, gen);
			f.Type = TypeRef (typeof (DataColumn));
			return f;
		}

		private CodeMemberProperty CreateTableColumnProperty (DataTable dt, DataColumn col)
		{
			string name = opts.ColumnName (col.ColumnName, gen);
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
			string rowName = opts.RowName (dt.TableName, gen);
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
			string rowType = opts.RowName (dt.TableName, gen);
			m.Name = "Add" + rowType;
			m.Attributes = MemberAttributes.Public;
			m.Parameters.Add (Param (TypeRef (rowType), "row"));
			m.Statements.Add (Eval (MethodInvoke (PropRef ("Rows"), "Add", ParamRef ("row"))));
			return m;
		}

		private CodeMemberMethod CreateTableAddRow2 (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName, gen);
			m.Name = "Add" + rowType;
			m.ReturnType = TypeRef (rowType);
			m.Attributes = MemberAttributes.Public;

			m.Statements.Add (VarDecl (rowType, "row", MethodInvoke ("New" + rowType)));

			foreach (DataColumn col in dt.Columns) {
				if (col.ColumnMapping == MappingType.Hidden) {
					foreach (DataRelation r in dt.DataSet.Relations) {
						if (r.ChildTable == dt) {
							// parameter
							string paramType = opts.RowName (r.ParentTable.TableName, gen);
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
					string paramName = opts.ColumnName (col.ColumnName, gen);
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
			string rowType = opts.RowName (dt.TableName, gen);
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
			m.Statements.Add (Return (New (opts.RowName (dt.TableName, gen), ParamRef ("builder"))));
			return m;
		}

		private CodeMemberMethod CreateTableRemoveRow (DataTable dt)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			string rowType = opts.RowName (dt.TableName, gen);
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
			m.Statements.Add (Return (new CodeTypeOfExpression (opts.RowName (dt.TableName, gen))));
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
			string eventName = opts.TableMemberName (dt.TableName, gen) + "Row" + type;
			CodeStatement trueStmt = Eval (
				MethodInvoke (
					eventName,
					This (),
					New (
						opts.EventArgsName (dt.TableName, gen),
						Cast (opts.RowName (dt.TableName, gen), PropRef (ParamRef ("e"), "Row")),
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
			cme.Name = opts.TableMemberName (dt.TableName, gen) + nameSuffix;
			cme.Type = TypeRef (opts.TableDelegateName (dt.TableName, gen));
			return cme;
		}

#endregion



#region Row class

		public CodeTypeDeclaration GenerateDataRowType (DataTable dt)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.RowName (dt.TableName, gen);
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
			c.Statements.Add (Let (FieldRef ("table"), Cast (
				opts.TableTypeName (dt.TableName, gen),
				PropRef ("Table"))));
			return c;
		}

		private CodeMemberField CreateRowTableField (DataTable dt)
		{
			CodeMemberField f = new CodeMemberField ();
			f.Name = "table";
			f.Type = TypeRef (opts.TableTypeName (dt.TableName, gen));
			return f;
		}

		private CodeMemberProperty CreateRowColumnProperty (DataTable dt, DataColumn col)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = opts.ColumnName (col.ColumnName, gen);
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
					(PropRef ("table"), 
					opts.TableColName (col.ColumnName, gen) + "Column"))));
			p.GetStatements.Add (new CodeConditionStatement (
				Equals (
					Local ("ret"),
					PropRef (TypeRefExp (typeof (DBNull)), "Value")),
				new CodeStatement [] {
					Throw (New (typeof (StrongTypingException), Const ("Cannot get strong typed value since it is DB null."), Const (null))) },
				new CodeStatement [] {
					Return (Cast (col.DataType, Local ("ret"))) }));

			p.SetStatements.Add (Let (IndexerRef (PropRef (PropRef ("table"), opts.TableColName (col.ColumnName, gen) + "Column")), new CodePropertySetValueReferenceExpression ()));

			return p;
		}

		private CodeMemberMethod CreateRowColumnIsNull (DataTable dt, DataColumn col)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Is" + opts.ColumnName (col.ColumnName, gen) + "Null";
			m.Attributes = MemberAttributes.Public;
			m.ReturnType = TypeRef (typeof (bool));
			m.Statements.Add (Return (MethodInvoke (
				"IsNull",
				// table[foo].[bar]Column
				PropRef (
					PropRef ("table"), 
					opts.TableColName (col.ColumnName, gen) + "Column"))));
			return m;
		}

		private CodeMemberMethod CreateRowColumnSetNull (DataTable dt, DataColumn col)
		{
			CodeMemberMethod m = new CodeMemberMethod ();
			m.Name = "Set" + opts.ColumnName (col.ColumnName, gen) + "Null";
			m.Attributes = MemberAttributes.Public;
			m.Statements.Add (Let (IndexerRef (
				PropRef (
					PropRef ("table"), 
					opts.TableColName (col.ColumnName, gen) + "Column")),
				PropRef (TypeRefExp (typeof (DBNull)), "Value")));

			return m;
		}

		private CodeMemberProperty CreateRowParentRowProperty (DataTable dt, DataRelation rel)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = opts.TableMemberName (rel.ParentTable.TableName, gen) + "Row";
			p.Attributes = MemberAttributes.Public;
			p.Type = TypeRef (opts.RowName (rel.ParentTable.TableName, gen));
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
			m.Name = "Get" + opts.TableMemberName (rel.ChildTable.TableName, gen) + "Rows";
			m.Attributes = MemberAttributes.Public;
			m.ReturnType = new CodeTypeReference (opts.RowName (rel.ChildTable.TableName, gen), 1);
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
		//    private [foo]Row row;
		//    private DataRowAction action;
		//    (.ctor())
		//    (Row)
		//    (Action)
		//  }
		private CodeTypeDeclaration GenerateEventType (DataTable dt)
		{
			CodeTypeDeclaration t = new CodeTypeDeclaration ();
			t.Name = opts.EventArgsName (dt.TableName, gen);
			t.BaseTypes.Add (TypeRef (typeof (EventArgs)));
			t.Attributes = MemberAttributes.Public;

			t.Members.Add (
				new CodeMemberField (
					TypeRef (opts.RowName (dt.TableName, gen)),
					"row"));
			t.Members.Add (
				new CodeMemberField (
					TypeRef (typeof (DataRowAction)), "action"));
			t.Members.Add (CreateEventCtor (dt));

			t.Members.Add (CreateEventRow (dt));

			t.Members.Add (CreateEventAction (dt));

			return t;
		}

		// Code:
		//  public [foo]RowChangeEventArgs ([foo]Row r, DataRowAction a)
		//  {
		//    row = r;
		//    action = a;
		//  }
		private CodeConstructor CreateEventCtor (DataTable dt)
		{
			CodeConstructor c = new CodeConstructor ();
			c.Attributes = MemberAttributes.Public;
			c.Parameters.Add (Param (TypeRef (opts.RowName (dt.TableName, gen)), "r"));
			c.Parameters.Add (Param (TypeRef (typeof (DataRowAction)), "a"));
			c.Statements.Add (Let (FieldRef ("row"), ParamRef ("r")));
			c.Statements.Add (Let (FieldRef ("action"), ParamRef ("a")));

			return c;
		}

		// Code:
		//  public [foo]Row Row {
		//   get { return row; }
		// }
		private CodeMemberProperty CreateEventRow (DataTable dt)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Row";
			p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			p.Type = TypeRef (opts.RowName (dt.TableName, gen));
			p.HasSet = false;
			p.GetStatements.Add (Return (FieldRef ("row")));
			return p;
		}

		// Code:
		//  public DataRowAction Action {
		//   get { return action; }
		// }
		private CodeMemberProperty CreateEventAction (DataTable dt)
		{
			CodeMemberProperty p = new CodeMemberProperty ();
			p.Name = "Action";
			p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			p.Type = TypeRef (typeof (DataRowAction));
			p.HasSet = false;
			p.GetStatements.Add (Return (FieldRef ("action")));
			return p;
		}

#endregion

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
