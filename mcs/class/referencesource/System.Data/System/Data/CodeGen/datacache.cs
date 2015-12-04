//------------------------------------------------------------------------------
// <copyright file="datacache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Collections;
    using System.Globalization;

    [
    System.Security.Permissions.HostProtectionAttribute(SharedState=true, Synchronization=true),
    Obsolete("TypedDataSetGenerator class will be removed in a future release. Please use System.Data.Design.TypedDataSetGenerator in System.Design.dll.")
    ]
    public class TypedDataSetGenerator {
        private bool            useExtendedNaming;
        private ICodeGenerator  codeGen;
        private ArrayList       errorList;
        private ArrayList       conflictingTables;
        private Hashtable	lookupIdentifiers;

        public static void Generate(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen) {
            new TypedDataSetGenerator().GenerateCode(dataSet, codeNamespace, codeGen);
            CodeGenerator.ValidateIdentifiers(codeNamespace);
        }

        // given a variable name, this method will check to see if the 
        // name is a valid identifier name. if this is not the case, then
        // at the moment will replace all the blank space with underscores.
        public static string GenerateIdName(string name, ICodeGenerator codeGen) {
            if (codeGen.IsValidIdentifier(name)) {
                return name;
            }

            string ret = name.Replace(' ', '_');
            if (! codeGen.IsValidIdentifier(ret)) {
                ret = "_" + ret;
                UnicodeCategory unc;
                for (int i = 1; i < ret.Length; i++) {
                    unc = Char.GetUnicodeCategory(ret[i]);
                    if (
                        UnicodeCategory.UppercaseLetter      != unc &&  
                        UnicodeCategory.LowercaseLetter      != unc &&  
                        UnicodeCategory.TitlecaseLetter      != unc &&
                        UnicodeCategory.ModifierLetter       != unc &&
                        UnicodeCategory.OtherLetter          != unc &&
                        UnicodeCategory.LetterNumber         != unc &&
                        UnicodeCategory.NonSpacingMark       != unc &&
                        UnicodeCategory.SpacingCombiningMark != unc &&
                        UnicodeCategory.DecimalDigitNumber   != unc &&
                        UnicodeCategory.ConnectorPunctuation != unc 
                    ) {
                        ret = ret.Replace(ret[i], '_');
                    } // if
                } // for
            }

            return ret;
        }

        // -------------------- Implementation --------------------------

        internal CodeTypeDeclaration GenerateCode(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen) {
            this.useExtendedNaming = false;
            this.errorList         = new ArrayList();
            this.conflictingTables = new ArrayList();
            this.codeGen           = codeGen;

            CodeTypeDeclaration dataSetClass = CreateTypedDataSet(dataSet); {
                foreach(DataTable table in dataSet.Tables) {
                    dataSetClass.Members.Add(CreateTypedRowEventHandler(table));
                }
                foreach(DataTable table in dataSet.Tables) {
                    dataSetClass.Members.Add(CreateTypedTable(   table));
                    dataSetClass.Members.Add(CreateTypedRow(     table));
                    dataSetClass.Members.Add(CreateTypedRowEvent(table));
                }            
            
                if (errorList.Count > 0) {
                    throw new TypedDataSetGeneratorException(errorList);
                }
            }
            codeNamespace.Types.Add(dataSetClass);
            return dataSetClass;
        }

        private void InitLookupIdentifiers() {
            lookupIdentifiers = new Hashtable();

            System.Reflection.PropertyInfo[] props = typeof(DataRow).GetProperties();
            foreach(System.Reflection.PropertyInfo p in props) {
                lookupIdentifiers[p.Name] = '_' + p.Name;
            }
        }

        private string FixIdName(string inVarName) {
            if (lookupIdentifiers == null) {
                InitLookupIdentifiers();
            }
            string newName = (string)lookupIdentifiers[inVarName];
            if (newName == null) {
                newName = GenerateIdName(inVarName, this.codeGen);
                while (lookupIdentifiers.ContainsValue(newName)) {
                    newName = '_' + newName;
                }
                lookupIdentifiers[inVarName] = newName;
                if (! this.codeGen.IsValidIdentifier(newName)){
                    errorList.Add(Res.GetString(Res.CodeGen_InvalidIdentifier, newName));
                }
            }
            return newName;
        }

        private static bool isEmpty(string s) {
            return s == null || s.Length == 0;
        }

        // Name of a class for typed row
        private string RowClassName(DataTable table) {
            string className = (string) table.ExtendedProperties["typedName"];
            if(isEmpty(className)) {
                className = FixIdName(table.TableName) + "Row";
            }
            return className;
        }

        // Name of a class for typed row inherit from
        private string RowBaseClassName(DataTable table) {
            if(useExtendedNaming) {
                string className = (string) table.ExtendedProperties["typedBaseClass"];
                if(isEmpty(className)) {
                    className = (string) table.DataSet.ExtendedProperties["typedBaseClass"];
                    if(isEmpty(className)) {
                        className = "DataRow";
                    }
                }
                return className;
            }else {
                return "DataRow";
            }
        }

        // Name of a class for typed row
        private string RowConcreteClassName(DataTable table) {
            if(useExtendedNaming) {
                string className = (string) table.ExtendedProperties["typedConcreteClass"];
                if(isEmpty(className)) {
                    className = RowClassName(table);
                }
                return className;
            }else {
                return RowClassName(table);
            }
        }

        // Name of a class for typed table
        private string TableClassName(DataTable table) {
            string className = (string)table.ExtendedProperties["typedPlural"];
            if(isEmpty(className)) {
                className = (string)table.ExtendedProperties["typedName"];
                if(isEmpty(className)) {
                    // check for conflicts with same name different namespace
                    if ((table.DataSet.Tables.InternalIndexOf(table.TableName) == -3) && !conflictingTables.Contains(table.TableName)) {
                        conflictingTables.Add(table.TableName);
                        errorList.Add(Res.GetString(Res.CodeGen_DuplicateTableName, table.TableName));
                    }
                        
                    className = FixIdName(table.TableName);
                }
            }
            return className + "DataTable";
        }

        // Name of the property of typed dataset wich returns typed table:
        private string TablePropertyName(DataTable table) {
            string typedName = (string)table.ExtendedProperties["typedPlural"];
            if(isEmpty(typedName)) {
                typedName = (string)table.ExtendedProperties["typedName"];
                if(isEmpty(typedName)) {
                    typedName = FixIdName(table.TableName);
                }
                else
                    typedName = typedName + "Table";
            }
            return typedName;
        }

        // Name of the filed of typed dataset wich holds typed table
        private string TableFieldName(DataTable table) {
            return "table" + TablePropertyName(table);
        }

        private string RowColumnPropertyName(DataColumn column) {
                string typedName = (string) column.ExtendedProperties["typedName"];
                if(isEmpty(typedName)) {
                    typedName = FixIdName(column.ColumnName);
                }
                return typedName;
        }

        private string TableColumnFieldName(DataColumn column) {
            string columnName = RowColumnPropertyName(column);
            if (String.Compare("column", columnName, StringComparison.OrdinalIgnoreCase) != 0)
                return ("column" + columnName);
            return ("columnField" + columnName);
        }

        private string TableColumnPropertyName(DataColumn column) {
            return RowColumnPropertyName(column) + "Column";
        }

        private static int TablesConnectedness(DataTable parentTable, DataTable childTable) {
            int connectedness = 0;
            DataRelationCollection relations = childTable.ParentRelations;
            for (int i = 0; i < relations.Count; i++) {
                if (relations[i].ParentTable == parentTable) {
                    connectedness ++;
                }
            }
            return connectedness;
        }

        private string ChildPropertyName(DataRelation relation) {
            string typedName = (string) relation.ExtendedProperties["typedChildren"];
            if(isEmpty(typedName)) {
                string arrayName = (string)relation.ChildTable.ExtendedProperties["typedPlural"];
                if(isEmpty(arrayName)) {
                    arrayName = (string)relation.ChildTable.ExtendedProperties["typedName"];
                    if(isEmpty(arrayName)) {
                        typedName = "Get" + relation.ChildTable.TableName + "Rows";
                        if(1 < TablesConnectedness(relation.ParentTable, relation.ChildTable)) {
                            typedName +="By" + relation.RelationName;
                        }
                        return FixIdName(typedName);
                    }
                    arrayName += "Rows";
                }
                typedName = "Get" + arrayName;
            }
            return typedName;
        }

        private string ParentPropertyName(DataRelation relation) {
            string typedName = null;
            typedName = (string) relation.ExtendedProperties["typedParent"];
            if(isEmpty(typedName)) {
                typedName = RowClassName(relation.ParentTable);
                if(                                                // Complex case: 
                    relation.ChildTable == relation.ParentTable || //   Self join
                    relation.ChildColumnsReference.Length != 1              //   Multycolumn key
                ) {
                    typedName += "Parent";
                }
                if(1 < TablesConnectedness(relation.ParentTable, relation.ChildTable)) {
                    typedName +="By" + FixIdName(relation.RelationName);
                }
            }
            return typedName;
        }

        private string RelationFieldName(DataRelation relation) {
            return FixIdName("relation" + relation.RelationName);
        }
        
        private string GetTypeName(Type t) {
            return t.FullName;
        }

        private bool ChildRelationFollowable(DataRelation relation) {
            if (relation != null) {
                if (relation.ChildTable == relation.ParentTable) {
                    if (relation.ChildTable.Columns.Count == 1) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static CodeMemberMethod CreateOnRowEventMethod(string eventName, string rowClassName) {
            //\\ protected override void OnRow<eventName>(DataRowChangeEventArgs e) {
            //\\     base.OnRow<eventName>(e);
            //\\     if (((this.<RowClassName><eventName>) != (null))) {
            //\\         this.<RowClassName><eventName>(this, new <RowClassName><eventName>Event(((<eventName>)(e.Row)), e.Action));
            //\\     }
            //\\ }
            CodeMemberMethod onRowEvent = MethodDecl(typeof(void), "OnRow" + eventName, MemberAttributes.Family | MemberAttributes.Override); {
                onRowEvent.Parameters.Add(ParameterDecl(typeof(DataRowChangeEventArgs), "e"));
                onRowEvent.Statements.Add(MethodCall(Base(), "OnRow" + eventName, Argument("e")));
                onRowEvent.Statements.Add(If(IdNotEQ(Event(rowClassName + eventName), Primitive(null)),
                    Stm(DelegateCall(Event(rowClassName + eventName), 
                        New(rowClassName + "ChangeEvent", new CodeExpression[] { Cast(rowClassName, Property(Argument("e"), "Row")), Property(Argument("e"), "Action")})
                    ))
                ));
            }
            return onRowEvent;
        }// CreateOnRowEventMethod

        private CodeTypeDeclaration CreateTypedTable(DataTable table) {
            string stRowClassName = RowClassName(table);
            string stTblClassName = TableClassName(table);
            string stRowConcreateClassName = RowConcreteClassName(table);

            CodeTypeDeclaration dataTableClass = new CodeTypeDeclaration(stTblClassName);
            dataTableClass.BaseTypes.Add(typeof(DataTable));
            dataTableClass.BaseTypes.Add(typeof(System.Collections.IEnumerable));
            //dataTableClass.Attributes |= TypeAttributes.NestedPrivate;

            dataTableClass.CustomAttributes.Add(AttributeDecl("System.Serializable"));
            dataTableClass.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));

            for (int i = 0; i < table.Columns.Count; i++) {
                //\\ DataColumn column<ColumnName>;
                dataTableClass.Members.Add(FieldDecl(typeof(DataColumn), TableColumnFieldName(table.Columns[i])));
            }              

            dataTableClass.Members.Add(EventDecl(stRowClassName + "ChangeEventHandler", stRowClassName + "Changed" ));
            dataTableClass.Members.Add(EventDecl(stRowClassName + "ChangeEventHandler", stRowClassName + "Changing"));
            dataTableClass.Members.Add(EventDecl(stRowClassName + "ChangeEventHandler", stRowClassName + "Deleted" ));
            dataTableClass.Members.Add(EventDecl(stRowClassName + "ChangeEventHandler", stRowClassName + "Deleting"));

            //\\ internal <TableName>DataTableClass() : base("<TableName>") {
            //\\     this.InitClass();
            //\\ }
            CodeConstructor constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
                constructor.BaseConstructorArgs.Add(Str(table.TableName));
                constructor.Statements.Add(MethodCall(This(), "InitClass"));
            }
            dataTableClass.Members.Add(constructor);
            //\\ protected <TableName>DataTableClass("<info>,<context>") : base("<info>,<context>") {
            //\\    InitVars();
            //\\ }           
            constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Family;
                constructor.Parameters.Add(ParameterDecl(typeof(System.Runtime.Serialization.SerializationInfo), "info" ));
                constructor.Parameters.Add(ParameterDecl(typeof(System.Runtime.Serialization.StreamingContext), "context"));
                constructor.BaseConstructorArgs.AddRange(new CodeExpression[] {Argument("info"), Argument("context")});
                constructor.Statements.Add(MethodCall(This(), "InitVars"));
            }
            dataTableClass.Members.Add(constructor);

            //\\ internal <TableName>DataTableClass(DataTable table) : base(table.TableName) { // [....] : Assuming incoming table always associated with DataSet
            //\\ if (table.CaseSensitive != table.DataSet.CaseSensitive)
            //\\    this.CaseSensitive = table.CaseSensitive;
            //\\ if (table.Locale.ToString() != table.DataSet.Locale.ToString())
            //\\    this.Locale = table.Locale;
            //\\ if (table.Namespace != table.DataSet.Namespace)
            //\\    this.Namespace = table.Namespace;
            //\\ this.Prefix = table.Prefix;
            //\\ this.MinimumCapacity = table.MinimumCapacity;
            //\\ this.DisplayExpression = table.DisplayExpression;
            //\\ }
                            constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
                constructor.Parameters.Add(ParameterDecl(typeof(DataTable), "table"));
                constructor.BaseConstructorArgs.Add(Property(Argument("table"),"TableName"));
                constructor.Statements.Add(
                    If(IdNotEQ(Property(Argument("table"),"CaseSensitive"),Property(Property(Argument("table"),"DataSet"),"CaseSensitive")),
                        Assign(Property(This(),"CaseSensitive"),Property(Argument("table"),"CaseSensitive"))
                    )
                );
                constructor.Statements.Add(
                    If(IdNotEQ(MethodCall(Property(Argument("table"),"Locale"),"ToString"),MethodCall(Property(Property(Argument("table"),"DataSet"),"Locale"),"ToString")),
                        Assign(Property(This(),"Locale"),Property(Argument("table"),"Locale"))
                    )
                );
                constructor.Statements.Add(
                    If(IdNotEQ(Property(Argument("table"),"Namespace"),Property(Property(Argument("table"),"DataSet"),"Namespace")),
                        Assign(Property(This(),"Namespace"),Property(Argument("table"),"Namespace"))
                    )
                );
                constructor.Statements.Add(Assign(Property(This(), "Prefix"), Property(Argument("table"),"Prefix")));
                constructor.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Property(Argument("table"),"MinimumCapacity")));
                constructor.Statements.Add(Assign(Property(This(), "DisplayExpression"), Property(Argument("table"),"DisplayExpression")));
            }
            dataTableClass.Members.Add(constructor);

            //\\ public int Count {
            //\\     get { return this.Rows.Count; }
            //\\ }
            CodeMemberProperty countProp = PropertyDecl(typeof(System.Int32), "Count", MemberAttributes.Public | MemberAttributes.Final); {
                countProp.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable", Primitive(false)));
                countProp.GetStatements.Add(Return(Property(Property(This(), "Rows"), "Count")));
            }
            dataTableClass.Members.Add(countProp);

            for (int i = 0; i < table.Columns.Count; i++) {
                //\\ internal DataColumn NAMEColumn {
                //\\     get { return this.columnNAME; }
                //\\ }
                DataColumn column = table.Columns[i];
                CodeMemberProperty colProp = PropertyDecl(typeof(DataColumn), TableColumnPropertyName(column), MemberAttributes.Assembly | MemberAttributes.Final); {
                    colProp.GetStatements.Add(Return(Field(This(), TableColumnFieldName(column))));
                }
                dataTableClass.Members.Add(colProp);
            }

            //\\ public <RowClassName> this[int index] {
            //\\     return (<RowClassName>) this.Rows[index];
            //\\ }
            CodeMemberProperty thisIndex = PropertyDecl(stRowConcreateClassName, "Item", MemberAttributes.Public | MemberAttributes.Final); {
                thisIndex.Parameters.Add(ParameterDecl(typeof(Int32), "index"));
                thisIndex.GetStatements.Add(Return(Cast(stRowConcreateClassName, Indexer(Property(This(), "Rows"), Argument("index")))));
            }
            dataTableClass.Members.Add(thisIndex);

            //\\ public void Add<RowClassName>(<RowClassName>  row) {
            //\\     this.Rows.Add(row);
            //\\ }
            CodeMemberMethod addMethod = MethodDecl(typeof(void), "Add" + stRowClassName, MemberAttributes.Public | MemberAttributes.Final); {
                addMethod.Parameters.Add(ParameterDecl(stRowConcreateClassName, "row"));
                addMethod.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", Argument("row")));
            }
            dataTableClass.Members.Add(addMethod);

            //\\ public <RowClassName> Add<RowClassName>(<ColType> <ColName>[, <ColType> <ColName> ...]) {
            //\\     <RowClassName> row;
            //\\     row = ((COMPUTERRow)(this.NewRow()));
            //\\     row.ItemArray = new Object[] {NAME, VERSION, null};
            //\\     this.Rows.Add(row);
            //\\     return row;
            //\\ }
            ArrayList parameterColumnList = new ArrayList();
            for (int i = 0; i < table.Columns.Count; i++) {
                if (!table.Columns[i].AutoIncrement) {
                    parameterColumnList.Add(table.Columns[i]);
                }
            }

            CodeMemberMethod addByColName = MethodDecl(stRowConcreateClassName, "Add" + stRowClassName, MemberAttributes.Public | MemberAttributes.Final); {
                DataColumn[] index = new DataColumn[parameterColumnList.Count];
                parameterColumnList.CopyTo(index, 0);
                for (int i = 0; i < index.Length; i++) {
                    Type DataType = index[i].DataType;
                    DataRelation relation = index[i].FindParentRelation();
                    if (ChildRelationFollowable(relation)) {
                        string ParentTypedRowName = RowClassName(relation.ParentTable);
                        string argumentName = FixIdName("parent" + ParentTypedRowName + "By" + relation.RelationName);
                        addByColName.Parameters.Add(ParameterDecl(ParentTypedRowName, argumentName));
                    }
                    else {
                        addByColName.Parameters.Add(ParameterDecl(GetTypeName(DataType), RowColumnPropertyName(index[i])));
                    }
                }
                addByColName.Statements.Add(VariableDecl(stRowConcreateClassName, "row" + stRowClassName, Cast(stRowConcreateClassName, MethodCall(This(), "NewRow"))));
                CodeExpression varRow = Variable("row" + stRowClassName);

                CodeAssignStatement assignStmt = new CodeAssignStatement(); {
                    assignStmt.Left = Property(varRow, "ItemArray");
                    CodeArrayCreateExpression newArray = new CodeArrayCreateExpression();
                    newArray.CreateType = Type(typeof(object));
                    
                    index = new DataColumn[table.Columns.Count];
                    table.Columns.CopyTo(index, 0);
                    
                    for (int i = 0; i < index.Length; i++) {
                        if (index[i].AutoIncrement) {
                            newArray.Initializers.Add(Primitive(null));
                        }else {
                            DataRelation relation = index[i].FindParentRelation();
                            if (ChildRelationFollowable(relation)) {
                                string ParentTypedRowName = RowClassName(relation.ParentTable);
                                string argumentName = FixIdName("parent" + ParentTypedRowName + "By" + relation.RelationName);
                                newArray.Initializers.Add(Indexer(Argument(argumentName), Primitive(relation.ParentColumnsReference[0].Ordinal)));
                            }
                            else {
                                newArray.Initializers.Add(Argument(RowColumnPropertyName(index[i])));
                            }
                        }
                    }

                    assignStmt.Right = newArray;
                }
                addByColName.Statements.Add(assignStmt);

                addByColName.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", varRow));
                addByColName.Statements.Add(Return(varRow));
            }
            dataTableClass.Members.Add(addByColName);

            for (int j = 0; j < table.Constraints.Count; j++) {
                if (!(table.Constraints[j] is UniqueConstraint)) {
                    continue;
                }

                if (!(((UniqueConstraint)(table.Constraints[j])).IsPrimaryKey)) {
                    continue;
                }

                DataColumn[] index = ((UniqueConstraint)table.Constraints[j]).ColumnsReference;
                string FindByName = "FindBy";
                bool AllHidden = true;
                for (int i = 0; i < index.Length; i++) {                    
                    FindByName += RowColumnPropertyName(index[i]);
                    if(index[i].ColumnMapping != MappingType.Hidden) {
                        AllHidden = false;
                    }
                }

                if(AllHidden) {
                    continue; // We are not generating FindBy* methods for hidden columns
                }

                //\\ public <RowClassName> FindBy<ColName>[...](<ColType> <ColName>[, ...]) {
                //\\    return (<RowClassName>)(this.Rows.Find(new Object[] {<ColName>[, ...]}));
                //\\ }
                CodeMemberMethod findBy = MethodDecl(stRowClassName, FixIdName(FindByName), MemberAttributes.Public | MemberAttributes.Final); {
                    for (int i = 0; i < index.Length; i++) {
                        findBy.Parameters.Add(ParameterDecl(GetTypeName(index[i].DataType), RowColumnPropertyName(index[i])));
                    }

                    CodeArrayCreateExpression arrayCreate = new CodeArrayCreateExpression(typeof(object), index.Length);
                    for (int i = 0; i < index.Length; i++) {
                        arrayCreate.Initializers.Add(Argument(RowColumnPropertyName(index[i])));
                    }
                    findBy.Statements.Add(Return(Cast(stRowClassName, MethodCall(Property(This(), "Rows"), "Find", arrayCreate))));
                }
                dataTableClass.Members.Add(findBy);
            }

            //\\ public System.Collections.IEnumerator GetEnumerator() {
            //\\     return this.GetEnumerator();
            //\\ }
            CodeMemberMethod getEnumerator = MethodDecl(typeof(System.Collections.IEnumerator), "GetEnumerator", MemberAttributes.Public | MemberAttributes.Final); {
                getEnumerator.ImplementationTypes.Add(Type("System.Collections.IEnumerable"));
                getEnumerator.Statements.Add(Return(MethodCall(Property(This(), "Rows"), "GetEnumerator")));
            }
            dataTableClass.Members.Add(getEnumerator);

            //\\ public override DataTable Clone() {
            //\\     <TableClassName> cln = (<TableClassName)base.Clone();
            //\\     cln.InitVars();
            //\\     return cln;
            //\\ }
            CodeMemberMethod clone = MethodDecl(typeof(DataTable), "Clone", MemberAttributes.Public | MemberAttributes.Override); {
                clone.Statements.Add(VariableDecl(stTblClassName, "cln", Cast(stTblClassName, MethodCall(Base(), "Clone", new CodeExpression[] {}))));
                clone.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression [] {}));
                clone.Statements.Add(Return(Variable("cln")));
            }
            dataTableClass.Members.Add(clone);

            //\\ protected override DataTable CreateInstance() {
            //\\     return new <TableClassName>()
            //\\ }
            CodeMemberMethod createInstance = MethodDecl(typeof(DataTable), "CreateInstance", MemberAttributes.Family | MemberAttributes.Override); {
	         createInstance.Statements.Add(Return(New(stTblClassName, new CodeExpression[] {}))); 
	     }
            dataTableClass.Members.Add(createInstance);

            //\\ private void InitClass() ...
            CodeMemberMethod tableInitClass = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private); {

            //\\ public void InitVars() ...
            CodeMemberMethod tableInitVars = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final); {

                for (int i = 0; i < table.Columns.Count; i++) {
                    DataColumn column = table.Columns[i];
                    string ColumnName = TableColumnFieldName(column);
                    //\\ this.column<ColumnName>
                    CodeExpression codeField = Field(This(), ColumnName);

                    //\\ this.column<ColumnName> = new DataColumn("<ColumnName>", typeof(<ColumnType>), "", MappingType.Hidden);
                    tableInitClass.Statements.Add(Assign(codeField, 
                        New(typeof(DataColumn), 
                            new CodeExpression[] {
                                Str(column.ColumnName),
                                TypeOf(GetTypeName(column.DataType)),
                                Primitive(null),
                                Field(TypeExpr(typeof(MappingType)), 
                                    (column.ColumnMapping == MappingType.SimpleContent) ? "SimpleContent"      :
                                    (column.ColumnMapping == MappingType.Attribute    ) ? "Attribute" :
                                    (column.ColumnMapping == MappingType.Hidden       ) ? "Hidden"    :
                                    /*defult*/                                            "Element" 
                                )
                            }
                        )
                    ));
                    //\\ this.Columns.Add(this.column<ColumnName>);
                    tableInitClass.Statements.Add(MethodCall(Property(This(), "Columns"), "Add", Field(This(), ColumnName)));
                }

                for (int i = 0; i < table.Constraints.Count; i++) {
                    if (!(table.Constraints[i] is UniqueConstraint)) {
                        continue;
                    }
                    //\\ this.Constraints.Add = new UniqueConstraint(<constraintName>, new DataColumn[] {this.column<ColumnName> [, ...]});
                    UniqueConstraint uc = (UniqueConstraint)(table.Constraints[i]);
                    DataColumn[] columns = uc.ColumnsReference;
                    CodeExpression[] createArgs = new CodeExpression[columns.Length]; {
                        for (int j = 0; j < columns.Length; j++) {
                            createArgs[j] = Field(This(), TableColumnFieldName(columns[j]));
                        }
                    }
                    tableInitClass.Statements.Add(MethodCall(Property(This(), "Constraints"), "Add",
                        New(typeof(UniqueConstraint), 
                        new CodeExpression[] {
                                                Str(uc.ConstraintName),
                                                new CodeArrayCreateExpression(typeof(DataColumn), createArgs),
                                                Primitive(uc.IsPrimaryKey)
                                             }
                        )
                    ));
                }

                for (int i = 0; i < table.Columns.Count; i++) {
                    DataColumn column = table.Columns[i];
                    string ColumnName = TableColumnFieldName(column);
                    //\\ this.column<ColumnName>
                    CodeExpression codeField = Field(This(), ColumnName);

                    //\\ this.column<ColumnName> = this.Columns["<ColumnName>"];
                    tableInitVars.Statements.Add(Assign(codeField, Indexer(Property(This(),"Columns"),Str(column.ColumnName))));

                    if (column.AutoIncrement) {
                        //\\ this.column<ColumnName>.AutoIncrement = true;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "AutoIncrement"), Primitive(true)));
                    }
                    if (column.AutoIncrementSeed != 0) {
                        //\\ this.column<ColumnName>.AutoIncrementSeed = <column.AutoIncrementSeed>;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "AutoIncrementSeed"), Primitive(column.AutoIncrementSeed)));
                    }
                    if (column.AutoIncrementStep != 1) {
                        //\\ this.column<ColumnName>.AutoIncrementStep = <column.AutoIncrementStep>;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "AutoIncrementStep"), Primitive(column.AutoIncrementStep)));
                    }
                    if (!column.AllowDBNull) {
                        //\\ this.column<ColumnName>.AllowDBNull = false;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "AllowDBNull"), Primitive(false)));
                    }
                    if (column.ReadOnly) {
                        //\\ this.column<ColumnName>.ReadOnly = true;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "ReadOnly"), Primitive(true)));
                    }
                    if (column.Unique) {
                        //\\ this.column<ColumnName>.Unique = true;
                        tableInitClass.Statements.Add(Assign(Property(codeField, "Unique"), Primitive(true)));
                    }

                    if (!Common.ADP.IsEmpty(column.Prefix)) {
                        //\\ this.column<ColumnName>.Prefix = "<column.Prefix>";
                        tableInitClass.Statements.Add(Assign(Property(codeField, "Prefix"), Str(column.Prefix)));
                    }
                    if (column._columnUri != null) {
                        //\\ this.column<ColumnName>.Namespace = "<column.Namespace>";
                        tableInitClass.Statements.Add(Assign(Property(codeField, "Namespace"), Str(column.Namespace)));
                    }
                    if (column.Caption != column.ColumnName) {
                        //\\ this.column<ColumnName>.Caption = "<column.Caption>";
                        tableInitClass.Statements.Add(Assign(Property(codeField, "Caption"), Str(column.Caption)));
                    }
                    if (column.DefaultValue != DBNull.Value) {
                        //\\ this.column<ColumnName>.DefaultValue = "<column.DefaultValue>";
                        tableInitClass.Statements.Add(Assign(Property(codeField, "DefaultValue"), Primitive(column.DefaultValue)));
                    }
                    if (column.MaxLength != -1) {
                        //\\ this.column<ColumnName>.MaxLength = "<column.MaxLength>";
                        tableInitClass.Statements.Add(Assign(Property(codeField, "MaxLength"), Primitive(column.MaxLength)));
                    }
                }

                if (table.ShouldSerializeCaseSensitive()) {
                    //\\ this.CaseSensitive = <CaseSensitive>;
                    tableInitClass.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(table.CaseSensitive)));
                }
                if (table.ShouldSerializeLocale()) {
                    //\\ this.Locale = new System.Globalization.CultureInfo("<Locale>");
                    tableInitClass.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(System.Globalization.CultureInfo),new CodeExpression[] {Str(table.Locale.ToString())})));
                }
                if (!Common.ADP.IsEmpty(table.Prefix)) {
                    //\\ this.Prefix = "<Prefix>";
                    tableInitClass.Statements.Add(Assign(Property(This(), "Prefix"), Str(table.Prefix)));
                }
                if (table.tableNamespace != null) {
                    //\\ this.Namespace = <Namespace>;
                    tableInitClass.Statements.Add(Assign(Property(This(), "Namespace"), Str(table.Namespace)));
                }

                if (table.MinimumCapacity != 50) {
                    //\\ this.MinimumCapacity = <MinimumCapacity>;
                    tableInitClass.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Primitive(table.MinimumCapacity)));
                }
                if (table.displayExpression != null) {
                    //\\ this.DisplayExpression = "<DisplayExpression>";
                    tableInitClass.Statements.Add(Assign(Property(This(), "DisplayExpression"), Str(table.DisplayExpressionInternal)));
                }
            }
            dataTableClass.Members.Add(tableInitVars);
            }
            dataTableClass.Members.Add(tableInitClass);

            //\\ public <RowClassName> New<RowClassName>() {
            //\\     return (<RowClassName>) NewRow();
            //\\ }
            CodeMemberMethod newTableRow = MethodDecl(stRowConcreateClassName, "New" + stRowClassName, MemberAttributes.Public | MemberAttributes.Final); {
                newTableRow.Statements.Add(Return(Cast(stRowConcreateClassName, MethodCall(This(), "NewRow"))));
            }
            dataTableClass.Members.Add(newTableRow);

            //\\ protected override DataRow NewRowFromBuilder(DataRowBuilder builder) {
            //\\     return new<RowClassName>(builder);
            //\\ }
            CodeMemberMethod newRowFromBuilder = MethodDecl(typeof(DataRow), "NewRowFromBuilder", MemberAttributes.Family | MemberAttributes.Override); {
                newRowFromBuilder.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "builder"));
                newRowFromBuilder.Statements.Add(Return(New(stRowConcreateClassName, new CodeExpression[] {Argument("builder")})));
            }
            dataTableClass.Members.Add(newRowFromBuilder);

            //\\ protected override System.Type GetRowType() {
            //\\     return typeof(<RowConcreateClassName>);
            //\\ }        
            CodeMemberMethod getRowType = MethodDecl(typeof(System.Type), "GetRowType", MemberAttributes.Family | MemberAttributes.Override); {
                getRowType.Statements.Add(Return(TypeOf(stRowConcreateClassName)));
            }
            dataTableClass.Members.Add(getRowType);

            dataTableClass.Members.Add(CreateOnRowEventMethod("Changed" , stRowClassName));
            dataTableClass.Members.Add(CreateOnRowEventMethod("Changing", stRowClassName));
            dataTableClass.Members.Add(CreateOnRowEventMethod("Deleted" , stRowClassName));
            dataTableClass.Members.Add(CreateOnRowEventMethod("Deleting", stRowClassName));

            //\\ public void Remove<RowClassName>(<RowClassName> row) {
            //\\     this.Rows.Remove(row);
            //\\ }
            CodeMemberMethod removeMethod = MethodDecl(typeof(void), "Remove" + stRowClassName, MemberAttributes.Public | MemberAttributes.Final); {
                removeMethod.Parameters.Add(ParameterDecl(stRowConcreateClassName, "row"));
                removeMethod.Statements.Add(MethodCall(Property(This(), "Rows"), "Remove", Argument("row")));
            }
            dataTableClass.Members.Add(removeMethod);

            return dataTableClass;
        }// CreateTypedTable

        private CodeTypeDeclaration CreateTypedRow(DataTable table) {
            string stRowClassName = RowClassName(  table);
            string stTblClassName = TableClassName(table);
            string stTblFieldName = TableFieldName(table);
            bool   storageInitialized = false;

            CodeTypeDeclaration rowClass = new CodeTypeDeclaration();
            rowClass.Name = stRowClassName;
            
            string strTemp = RowBaseClassName(table);
            if (string.Compare(strTemp, "DataRow", StringComparison.Ordinal) == 0) {
                rowClass.BaseTypes.Add(typeof(DataRow));
            }
            else {
                rowClass.BaseTypes.Add(strTemp);
            }
            rowClass.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));

            //\\ <TableClassName> table<TableFieldName>;
            rowClass.Members.Add(FieldDecl(stTblClassName, stTblFieldName));

            CodeConstructor constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
                constructor.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "rb"));
                constructor.BaseConstructorArgs.Add(Argument("rb"));
                constructor.Statements.Add(Assign(Field(This(), stTblFieldName), Cast(stTblClassName, Property(This(),"Table"))));
            }
            rowClass.Members.Add(constructor);

            foreach(DataColumn col in table.Columns) {
                if(col.ColumnMapping != MappingType.Hidden) {
                    Type DataType = col.DataType;
                    string rowColumnName   = RowColumnPropertyName(  col);
                    string tableColumnName = TableColumnPropertyName(col);
                    //\\ public <ColumnType> <ColumnName> {
                    //\\     get {
                    //\\         try{
                    //\\             return ((<ColumnType>)(this[this.table<TableName>.<ColumnName>Column]));
                    //\\         }catch(InvalidCastException e) {
                    //\\             throw new StrongTypingException("StrongTyping_CananotAccessDBNull", e);
                    //\\         }
                    //\\     }
                    //\\or 
                    //\\     get {
                    //\\         if(Is<ColumnName>Null()){
                    //\\             return (<nullValue>);
                    //\\         }else {
                    //\\             return ((<ColumnType>)(this[this.table<TableName>.<ColumnName>Column]));
                    //\\         }
                    //\\     }
                    //\\or 
                    //\\     get {
                    //\\         if(Is<ColumnName>Null()){
                    //\\             return <ColumnName>_nullValue;
                    //\\         }else {
                    //\\             return ((<ColumnType>)(this[this.table<TableName>.<ColumnName>Column]));
                    //\\         }
                    //\\     }
                    //\\
                    //\\     set {this[this.table<TableName>.<ColumnName>Column] = value;}
                    //\\ }
                    //\\
                    //\\if required: 
                    //\\ private static <ColumnType> <ColumnName>_nullValue = ...;
		            CodeMemberProperty rowProp = PropertyDecl(DataType, rowColumnName, MemberAttributes.Public | MemberAttributes.Final); {
                        CodeStatement getStmnt = Return(Cast(GetTypeName(DataType), Indexer(This(), Property(Field(This(), stTblFieldName), tableColumnName))));
                        if(col.AllowDBNull) {
                            string nullValue = (string) col.ExtendedProperties["nullValue"];
                            if(nullValue == null || nullValue == "_throw") {
                                getStmnt = Try(getStmnt, 
                                    Catch(typeof(System.InvalidCastException), "e", Throw(typeof(System.Data.StrongTypingException), "StrongTyping_CananotAccessDBNull", "e"))
                                );
                            }else {
                                CodeExpression nullValueFieldInit = null; // in some cases we generate it
                                CodeExpression nullValueExpr;
                                if(nullValue == "_null") {
                                    if(col.DataType.IsSubclassOf(typeof(System.ValueType))) {
                                        errorList.Add(Res.GetString(Res.CodeGen_TypeCantBeNull, col.ColumnName, col.DataType.Name));
                                        continue; // with next column.
                                    }
                                    nullValueExpr = Primitive(null);
                                }else if(nullValue == "_empty") {
                                    if(col.DataType == typeof(string)) {
                                        nullValueExpr = Property(TypeExpr(col.DataType), "Empty");
                                    }else {
                                        nullValueExpr = Field(TypeExpr(stRowClassName), rowColumnName + "_nullValue");
                                        //\\ private static <ColumnType> <ColumnName>_nullValue = new <ColumnType>();
                                        /* check that object can be constructed with parameterless constructor */ {
                                            System.Reflection.ConstructorInfo ctor = col.DataType.GetConstructor(new Type[] {typeof(string)});
                                            if(ctor == null) {
                                                errorList.Add(Res.GetString(Res.CodeGen_NoCtor0, col.ColumnName, col.DataType.Name));
                                                continue; // with next column.
                                            }
                                            ctor.Invoke(new Object[] {}); // can throw here.
                                        }
                                        nullValueFieldInit = New(col.DataType, new CodeExpression[] {});
                                    }
                                }else {
                                    if(! storageInitialized) {                                    
                                        table.NewRow(); // by this we force DataTable create DataStorage for each column in a table.
                                        storageInitialized = true;
                                    }
                                    object nullValueObj = col.ConvertXmlToObject(nullValue); // the exception will be throw if nullValue can't be conwerted to col.DataType
                                    if(
                                        col.DataType == typeof(char)   || col.DataType == typeof(string) ||
                                        col.DataType == typeof(decimal)|| col.DataType == typeof(bool)   ||
                                        col.DataType == typeof(Single) || col.DataType == typeof(double) ||
                                        col.DataType == typeof(SByte)  || col.DataType == typeof(Byte)   || 
                                        col.DataType == typeof(Int16)  || col.DataType == typeof(UInt16) || 
                                        col.DataType == typeof(Int32)  || col.DataType == typeof(UInt32) || 
                                        col.DataType == typeof(Int64)  || col.DataType == typeof(UInt64)    
                                    ) { // types can be presented by literal. Realy this is language dependent :-(                                        
                                        nullValueExpr = Primitive(nullValueObj);
                                    }else {
                                        nullValueExpr = Field(TypeExpr(stRowClassName), rowColumnName + "_nullValue");
                                        //\\ private static <ColumnType> <ColumnName>_nullValue = new <ColumnType>("<nullValue>");
                                        if(col.DataType == typeof(Byte[])) {
                                            nullValueFieldInit = MethodCall(TypeExpr(typeof(System.Convert)), "FromBase64String", Primitive(nullValue));
                                        }else if(col.DataType == typeof(DateTime) || col.DataType == typeof(TimeSpan)) {
                                            nullValueFieldInit = MethodCall(TypeExpr(col.DataType), "Parse", Primitive(nullValueObj.ToString()));
                                        }else /*object*/ {
                                            /* check that type can be constructed from this string */ {
                                                System.Reflection.ConstructorInfo ctor = col.DataType.GetConstructor(new Type[] {typeof(string)});
                                                if(ctor == null) {
                                                    errorList.Add(Res.GetString(Res.CodeGen_NoCtor1, col.ColumnName, col.DataType.Name));
                                                    continue; // with next column.
                                                }
                                                ctor.Invoke(new Object[] {nullValue}); // can throw here.
                                            }
                                            nullValueFieldInit = New(col.DataType, new CodeExpression[] {Primitive(nullValue)});
                                        }
                                    }
                                }
                                getStmnt = If(MethodCall(This(), "Is" + rowColumnName + "Null"), 
                                    new CodeStatement[] {Return(nullValueExpr)},
                                    new CodeStatement[] {getStmnt}
                                );
                                if(nullValueFieldInit != null) {
                                    CodeMemberField nullValueField = FieldDecl(col.DataType, rowColumnName + "_nullValue"); {
                                        nullValueField.Attributes     = MemberAttributes.Static | MemberAttributes.Private;
                                        nullValueField.InitExpression = nullValueFieldInit;
                                    }
                                    rowClass.Members.Add(nullValueField);
                                }
                            }
                        }
                        rowProp.GetStatements.Add(getStmnt);
                        rowProp.SetStatements.Add(Assign(Indexer(This(), Property(Field(This(), stTblFieldName), tableColumnName)), Value()));
                    }
                    rowClass.Members.Add(rowProp);

                    if (col.AllowDBNull) {
                        //\\ public bool Is<ColumnName>Null() {
                        //\\     return this.IsNull(this.table<TableName>.<ColumnName>Column);
                        //\\ }
                        CodeMemberMethod isNull = MethodDecl(typeof(System.Boolean), "Is" + rowColumnName + "Null", MemberAttributes.Public | MemberAttributes.Final); {
                            isNull.Statements.Add(Return(MethodCall(This(), "IsNull", Property(Field(This(), stTblFieldName), tableColumnName))));
                        }
                        rowClass.Members.Add(isNull);

                        //\\ public void Set<ColumnName>Null() {
                        //\\     this[this.table<TableName>.<ColumnName>Column] = DBNull.Value;
                        //\\ }
                        CodeMemberMethod setNull = MethodDecl(typeof(void), "Set" + rowColumnName + "Null", MemberAttributes.Public | MemberAttributes.Final); {
                            setNull.Statements.Add(Assign(Indexer(This(), Property(Field(This(), stTblFieldName), tableColumnName)), Field(TypeExpr(typeof(Convert)), "DBNull")));
                        }
                        rowClass.Members.Add(setNull);
                    }
                }
            }

            DataRelationCollection ChildRelations = table.ChildRelations;
            for (int i = 0; i < ChildRelations.Count; i++) {
                //\\ public <rowConcreateClassName>[] Get<ChildTableName>Rows() {
                //\\     return (<rowConcreateClassName>[]) this.GetChildRows(this.Table.ChildRelations["<RelationName>"]); 
                //\\  }
                DataRelation relation = ChildRelations[i];
                string rowConcreateClassName = RowConcreteClassName(relation.ChildTable);

                CodeMemberMethod childArray = Method(Type(rowConcreateClassName, 1), ChildPropertyName(relation), MemberAttributes.Public | MemberAttributes.Final); {
                    childArray.Statements.Add(Return(Cast(Type(rowConcreateClassName, 1), MethodCall(This(), "GetChildRows", Indexer(Property(Property(This(), "Table"), "ChildRelations"), Str(relation.RelationName))))));
                }
                rowClass.Members.Add(childArray);
            }

            DataRelationCollection ParentRelations = table.ParentRelations;
            for (int i = 0; i < ParentRelations.Count; i++) {
                //\\ public <ParentRowClassName> <ParentRowClassName>Parent {
                //\\     get {
                //\\         return ((<ParentRowClassName>)(this.GetParentRow(this.Table.ParentRelations["<RelationName>"])));
                //\\     }
                //\\     set {
                //\\         this.SetParentRow(value, this.Table.ParentRelations["<RelationName>"]);
                //\\     }
                //\\ }
                DataRelation relation = ParentRelations[i];
                string ParentTypedRowName = RowClassName(relation.ParentTable);

                CodeMemberProperty anotherProp = PropertyDecl(ParentTypedRowName, ParentPropertyName(relation), MemberAttributes.Public | MemberAttributes.Final); {
                    anotherProp.GetStatements.Add(Return(Cast(ParentTypedRowName, MethodCall(This(), "GetParentRow", Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(relation.RelationName))))));
                    anotherProp.SetStatements.Add(MethodCall(This(), "SetParentRow", new CodeExpression[] {Value(), Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(relation.RelationName))}));
                }
                rowClass.Members.Add(anotherProp);
            }
            return rowClass;
        }// CreateTypedRow

        private CodeTypeDeclaration CreateTypedRowEvent(DataTable table) {
            string stRowClassName = RowClassName(  table);
            string stTblClassName = TableClassName(table);
            string stRowConcreateClassName = RowConcreteClassName(table);

            CodeTypeDeclaration rowClass = new CodeTypeDeclaration();
            rowClass.Name = stRowClassName + "ChangeEvent";
            rowClass.BaseTypes.Add(typeof(EventArgs));

            rowClass.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));

            //\\ private <RowConcreteClassName> eventRow;
            rowClass.Members.Add(FieldDecl(stRowConcreateClassName, "eventRow"));
            //\\ private DataRowAction eventAction;
            rowClass.Members.Add(FieldDecl(typeof(DataRowAction), "eventAction"));

            //\\ public <RowClassName>ChangeEvent(RowClassName row, DataRowAction action) {
            //\\     this.eventRow    = row;
            //\\     this.eventAction = action;
            //\\ }
            CodeConstructor constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                constructor.Parameters.Add(ParameterDecl(stRowConcreateClassName, "row"   ));
                constructor.Parameters.Add(ParameterDecl(typeof(DataRowAction),  "action"));
                constructor.Statements.Add(Assign(Field(This(), "eventRow"   ), Argument("row"   )));
                constructor.Statements.Add(Assign(Field(This(), "eventAction"), Argument("action")));
            }
            rowClass.Members.Add(constructor);

            //\\ public <RowClassName> COMPUTERRow {
            //\\     get { return this.eventRow; }
            //\\ }
            CodeMemberProperty rowProp = PropertyDecl(stRowConcreateClassName, "Row", MemberAttributes.Public | MemberAttributes.Final); {
                rowProp.GetStatements.Add(Return(Field(This(), "eventRow")));
            }
            rowClass.Members.Add(rowProp);

            //\\ public DataRowAction Action {
            //\\     get { return this.eventAction; }
            //\\ }
            rowProp = PropertyDecl(typeof(DataRowAction), "Action", MemberAttributes.Public | MemberAttributes.Final); {
                rowProp.GetStatements.Add(Return(Field(This(), "eventAction")));
            }
            rowClass.Members.Add(rowProp);
            return rowClass;
        }// CreateTypedRowEvent

        private CodeTypeDelegate CreateTypedRowEventHandler(DataTable table) {
            string stRowClassName = RowClassName(table);
            //\\ public delegate void <RowClassName>ChangeEventHandler(object sender, <RowClassName>ChangeEvent e);
            CodeTypeDelegate delegateClass = new CodeTypeDelegate(stRowClassName + "ChangeEventHandler"); {
                delegateClass.TypeAttributes |= System.Reflection.TypeAttributes.Public;
                delegateClass.Parameters.Add(ParameterDecl(typeof(object), "sender"));            
                delegateClass.Parameters.Add(ParameterDecl(stRowClassName + "ChangeEvent", "e"));
            }
            return delegateClass;
        }// CreateTypedRowEventHandler

        private CodeTypeDeclaration CreateTypedDataSet(DataSet dataSet) {
            string stDataSetClassName = FixIdName(dataSet.DataSetName);
            CodeTypeDeclaration dataSetClass = new CodeTypeDeclaration(stDataSetClassName);
            dataSetClass.BaseTypes.Add(typeof(DataSet));
            dataSetClass.CustomAttributes.Add(AttributeDecl("System.Serializable"));
            dataSetClass.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", Str("code")));
            dataSetClass.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
            dataSetClass.CustomAttributes.Add(AttributeDecl("System.ComponentModel.ToolboxItem", Primitive(true)));
            dataSetClass.CustomAttributes.Add(AttributeDecl(typeof(XmlSchemaProviderAttribute).FullName, Primitive("GetTypedDataSetSchema")));
            dataSetClass.CustomAttributes.Add(AttributeDecl(typeof(XmlRootAttribute).FullName, Primitive(stDataSetClassName)));

            for (int i = 0; i < dataSet.Tables.Count; i++) {
                dataSetClass.Members.Add(FieldDecl(TableClassName(dataSet.Tables[i]), TableFieldName(dataSet.Tables[i])));
            }

            for (int i = 0; i < dataSet.Relations.Count; i++) {
                //\\ DataRelation relation<RelationName>;
                dataSetClass.Members.Add(FieldDecl(typeof(DataRelation), RelationFieldName(dataSet.Relations[i])));
            }

            CodeConstructor constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Public;
                constructor.Statements.Add(MethodCall(This(), "BeginInit"));
                constructor.Statements.Add(MethodCall(This(), "InitClass"));
                constructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler),"schemaChangedHandler",
                                           new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)),This(),"SchemaChanged")));
                constructor.Statements.Add(new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Tables"),"CollectionChanged"),Variable("schemaChangedHandler")));
                constructor.Statements.Add(new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Relations"),"CollectionChanged"),Variable("schemaChangedHandler")));
                 constructor.Statements.Add(MethodCall(This(), "EndInit"));
            }
            dataSetClass.Members.Add(constructor);

            constructor = new CodeConstructor(); {
                constructor.Attributes = MemberAttributes.Family;
                constructor.Parameters.Add(ParameterDecl(typeof(System.Runtime.Serialization.SerializationInfo), "info"   ));
                constructor.Parameters.Add(ParameterDecl(typeof(System.Runtime.Serialization.StreamingContext), "context"));
                constructor.BaseConstructorArgs.AddRange(new CodeExpression[] {Argument("info"), Argument("context")});


                constructor.Statements.Add(
                    If(EQ(MethodCall(This(), "IsBinarySerialized", new CodeExpression[] {Argument("info"), Argument("context")}), Primitive(true)) ,
                        new CodeStatement[] { 
                            Stm(MethodCall(This(),"InitVars", Primitive(false))),
                            VariableDecl(typeof(CollectionChangeEventHandler),"schemaChangedHandler1",
                                           new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)),This(),"SchemaChanged")) ,
                            new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Tables"),"CollectionChanged"),Variable("schemaChangedHandler1")) ,
                            new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Relations"),"CollectionChanged"),Variable("schemaChangedHandler1")),
                            Return()})
                );

                constructor.Statements.Add(
                    VariableDecl(typeof(String), "strSchema",
                    Cast("System.String", MethodCall(Argument("info"), "GetValue", new CodeExpression[] {Str("XmlSchema"),TypeOf("System.String")})))
                );

                ArrayList schemaBody = new ArrayList();
                schemaBody.Add(VariableDecl(typeof(DataSet),"ds",New(typeof(DataSet),new CodeExpression[] {})));
                schemaBody.Add(Stm(MethodCall(Variable("ds"),"ReadXmlSchema",new CodeExpression[] {New(typeof(System.Xml.XmlTextReader),new CodeExpression[] {New("System.IO.StringReader",new CodeExpression[] {Variable("strSchema")})})})));
                for (int i = 0; i < dataSet.Tables.Count; i++) {
                    //\\ this.Tables.Add(new <TableClassName>("<TableName>"));
                    schemaBody.Add(
                        If(IdNotEQ(Indexer(Property(Variable("ds"),"Tables"),Str(dataSet.Tables[i].TableName)),Primitive(null)),
                            Stm(MethodCall(Property(This(), "Tables"), "Add", New(TableClassName(dataSet.Tables[i]), new CodeExpression[] {Indexer(Property(Variable("ds"),"Tables"),Str(dataSet.Tables[i].TableName))})))
                        )
                    );
                }
                schemaBody.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"),"DataSetName")));
                schemaBody.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"),"Prefix")));
                schemaBody.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"),"Namespace")));
                schemaBody.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"),"Locale")));
                schemaBody.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"),"CaseSensitive")));
                schemaBody.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"),"EnforceConstraints")));
                schemaBody.Add(Stm(MethodCall(This(),"Merge",new CodeExpression[] {Variable("ds"),Primitive(false),Field(TypeExpr(typeof(MissingSchemaAction)),"Add")})));
                schemaBody.Add(Stm(MethodCall(This(),"InitVars")));
                CodeStatement[] schemaBodyArray = new CodeStatement[schemaBody.Count];
                schemaBody.CopyTo(schemaBodyArray);
                constructor.Statements.Add(
                    If(IdNotEQ(Variable("strSchema"),Primitive(null)),
                        schemaBodyArray,
                        new CodeStatement[] {
                            Stm(MethodCall(This(), "BeginInit")),
                            Stm(MethodCall(This(), "InitClass")),
                            Stm(MethodCall(This(), "EndInit"))
                            
                        }
                    )
                );
                constructor.Statements.Add(MethodCall(This(), "GetSerializationData", new CodeExpression [] { Argument("info"), Argument("context") }));
                constructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler),"schemaChangedHandler",
                                           new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)),This(),"SchemaChanged")));
                constructor.Statements.Add(new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Tables"),"CollectionChanged"),Variable("schemaChangedHandler")));
                constructor.Statements.Add(new System.CodeDom.CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(),"Relations"),"CollectionChanged"),Variable("schemaChangedHandler")));
            }
            dataSetClass.Members.Add(constructor);

            //\\ public override DataSet Clone() {
            //\\     <DataSetClassName> cln = (<DataSetClassName)base.Clone();
            //\\     cln.InitVars();
            //\\     return cln;
            //\\ }
            CodeMemberMethod clone = MethodDecl(typeof(DataSet), "Clone", MemberAttributes.Public | MemberAttributes.Override); {
                clone.Statements.Add(VariableDecl(stDataSetClassName, "cln", Cast(stDataSetClassName, MethodCall(Base(), "Clone", new CodeExpression[] {}))));
                clone.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression [] {}));
                clone.Statements.Add(Return(Variable("cln")));
            }
            dataSetClass.Members.Add(clone);

            //\\ public void InitVars() 
            CodeMemberMethod initDataSetVarsMethod = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final); {
                initDataSetVarsMethod.Statements.Add(MethodCall(This(), "InitVars", new CodeExpression [] {Primitive(true)}));
            }
            dataSetClass.Members.Add(initDataSetVarsMethod);

            //\\ private void InitClass() 
            CodeMemberMethod initClassMethod = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private); {

            //\\ public void InitVars() 
            CodeMemberMethod initVarsMethod = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final); {
                initVarsMethod.Parameters.Add(ParameterDecl(typeof(Boolean), "initTable"));
                
                //\\ this.DataSetName = "<dataSet.DataSetName>"
                initClassMethod.Statements.Add(Assign(Property(This(), "DataSetName"), Str(dataSet.DataSetName)));
                //\\ this.Prefix   = "<dataSet.Prefix>"
                initClassMethod.Statements.Add(Assign(Property(This(), "Prefix"), Str(dataSet.Prefix)));
                //\\ this.Namespace   = "<dataSet.Namespace>"
                initClassMethod.Statements.Add(Assign(Property(This(), "Namespace"), Str(dataSet.Namespace)));
                //\\ this.Locale = new System.Globalization.CultureInfo("dataSet.<Locale>");
                initClassMethod.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(System.Globalization.CultureInfo),new CodeExpression[] {Str(dataSet.Locale.ToString())})));
                //\\ this.CaseSensitive = <dataSet.CaseSensitive>;
                initClassMethod.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(dataSet.CaseSensitive)));
                //\\ this.EnforceConstraints = <dataSet.EnforceConstraints>;
                initClassMethod.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Primitive(dataSet.EnforceConstraints)));

                for (int i = 0; i < dataSet.Tables.Count; i++) {
                    CodeExpression fieldTable = Field(This(), TableFieldName(dataSet.Tables[i]));
                    //\\ table<TableFieldName> = new <TableClassName>("<TableName>");
                    initClassMethod.Statements.Add(Assign(fieldTable, New(TableClassName(dataSet.Tables[i]), new CodeExpression[] {})));
                    //\\ this.Tables.Add(this.table<TableFieldName>);
                    initClassMethod.Statements.Add(MethodCall(Property(This(), "Tables"), "Add", fieldTable));

                    //\\ this.table<TableFieldName> = (<TableClassName>)this.Tables["<TableName>"];
                    //\\ if (this.table<TableFieldName> != null)
                    //\\    this.table<TableFieldName>.InitVars();
                    initVarsMethod.Statements.Add(Assign(fieldTable, Cast(TableClassName(dataSet.Tables[i]), Indexer(Property(This(),"Tables"),Str(dataSet.Tables[i].TableName)))));

                    initVarsMethod.Statements.Add(
                        If(
                            EQ(Variable("initTable"), Primitive(true)),
                            new CodeStatement[] {
                               If(IdNotEQ(fieldTable,Primitive(null)),
                                  Stm(MethodCall(fieldTable,"InitVars")))
                            })
                    );
                }


            //\\ protected override bool ShouldSerializeTables() {
            //\\     return false;
            //\\ }
            CodeMemberMethod shouldSerializeTables = MethodDecl(typeof(System.Boolean), "ShouldSerializeTables", MemberAttributes.Family | MemberAttributes.Override); {
                shouldSerializeTables.Statements.Add(Return(Primitive(false)));
            }
            dataSetClass.Members.Add(shouldSerializeTables);

            //\\ protected override bool ShouldSerializableRelations() {
            //\\     return false;
            //\\ }
            CodeMemberMethod shouldSerializeRelations = MethodDecl(typeof(System.Boolean), "ShouldSerializeRelations", MemberAttributes.Family | MemberAttributes.Override); {
                shouldSerializeRelations.Statements.Add(Return(Primitive(false)));
            }
            dataSetClass.Members.Add(shouldSerializeRelations);

            //\\  sample wsdl generated for TDS, we will just generate for Version 1.0 & 1.1
            //\\  <xs:element minoccurs="0" maxoccurs="1" name="TypedDataSet" nillable="true">
            //\\     <xs:complexType>
            //\\        <xs:sequence>
            //\\           <xs:any namespace="http://TDS's namespace>
            //\\        </xs:sequence>
            //\\     </xs:complexType>            
            //\\  </xs:element>
            //\\
            //\\ public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs) {
            //\\    Authors_DS ds = new Authors_DS();
            //\\    xs.Add(ds.GetSchemaSerializable());
            //\\    XmlSchemaComplexType type = new XmlSchemaComplexType();
            //\\    XmlSchemaSequence sequence = new XmlSchemaSequence();
            //\\    XmlSchemaAny any = new XmlSchemaAny();
            //\\    any.Namespace = <ds.Namespace>
            //\\    sequence.Items.Add(any);
            //\\    type.Particle = sequence;
            //\\    return type;
            //\\ }
            //\\
            
            CodeMemberMethod getTypedDataSetSchema = MethodDecl(typeof(XmlSchemaComplexType), "GetTypedDataSetSchema", MemberAttributes.Static | MemberAttributes.Public); {
                getTypedDataSetSchema.Parameters.Add(ParameterDecl(typeof(XmlSchemaSet), "xs"));
                getTypedDataSetSchema.Statements.Add(VariableDecl(stDataSetClassName,"ds",New(stDataSetClassName,new CodeExpression[] {})));
                getTypedDataSetSchema.Statements.Add(MethodCall(Argument("xs"), "Add", new CodeExpression [] { MethodCall(Variable("ds"), "GetSchemaSerializable", new CodeExpression[] {})}));            
                getTypedDataSetSchema.Statements.Add(VariableDecl(typeof(XmlSchemaComplexType),"type",New(typeof(XmlSchemaComplexType),new CodeExpression[] {})));
                getTypedDataSetSchema.Statements.Add(VariableDecl(typeof(XmlSchemaSequence),"sequence",New(typeof(XmlSchemaSequence),new CodeExpression[] {})));
                getTypedDataSetSchema.Statements.Add(VariableDecl(typeof(XmlSchemaAny),"any",New(typeof(XmlSchemaAny),new CodeExpression[] {})));
                getTypedDataSetSchema.Statements.Add(Assign(Property(Variable("any"),"Namespace"),Property(Variable("ds"),"Namespace")));
                getTypedDataSetSchema.Statements.Add(MethodCall(Property(Variable("sequence"),"Items"), "Add", new CodeExpression [] { Variable("any") }));
                getTypedDataSetSchema.Statements.Add(Assign(Property(Variable("type"),"Particle"),Variable("sequence")));
                getTypedDataSetSchema.Statements.Add(Return(Variable("type")));
            }
            dataSetClass.Members.Add(getTypedDataSetSchema);

            //\\ protected override void ReadXmlSerializable(XmlReader reader) {
            //\\     ReadXml(reader, XmlReadMode.IgnoreSchema);
            //\\ }
            CodeMemberMethod readXmlSerializable = MethodDecl(typeof(void), "ReadXmlSerializable", MemberAttributes.Family | MemberAttributes.Override); {
                readXmlSerializable.Parameters.Add(ParameterDecl(typeof(System.Xml.XmlReader), "reader"));
                readXmlSerializable.Statements.Add(MethodCall(This(), "Reset", new CodeExpression [] {}));
                readXmlSerializable.Statements.Add(VariableDecl(typeof(DataSet),"ds",New(typeof(DataSet),new CodeExpression[] {})));
                readXmlSerializable.Statements.Add(MethodCall(Variable("ds"), "ReadXml", new CodeExpression [] { Argument("reader") }));
//                readXmlSerializable.Statements.Add(MethodCall(Variable("ds"), "ReadXmlSchema", new CodeExpression [] { Argument("reader") }));
                for (int i = 0; i < dataSet.Tables.Count; i++) {
                    //\\ this.Tables.Add(new <TableClassName>("<TableName>"));
                    readXmlSerializable.Statements.Add(
                        If(IdNotEQ(Indexer(Property(Variable("ds"),"Tables"),Str(dataSet.Tables[i].TableName)),Primitive(null)),
                            Stm(MethodCall(Property(This(), "Tables"), "Add", New(TableClassName(dataSet.Tables[i]), new CodeExpression[] {Indexer(Property(Variable("ds"),"Tables"),Str(dataSet.Tables[i].TableName))})))
                        )
                    );
                }
                readXmlSerializable.Statements.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"),"DataSetName")));
                readXmlSerializable.Statements.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"),"Prefix")));
                readXmlSerializable.Statements.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"),"Namespace")));
                readXmlSerializable.Statements.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"),"Locale")));
                readXmlSerializable.Statements.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"),"CaseSensitive")));
                readXmlSerializable.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"),"EnforceConstraints")));
                readXmlSerializable.Statements.Add(MethodCall(This(),"Merge",new CodeExpression[] {Variable("ds"),Primitive(false),Field(TypeExpr(typeof(MissingSchemaAction)),"Add")}));
                readXmlSerializable.Statements.Add(MethodCall(This(),"InitVars"));
//                readXmlSerializable.Statements.Add(MethodCall(This(), "ReadXml", new CodeExpression [] { Argument("reader"), Argument("XmlReadMode.IgnoreSchema") }));
            }
            dataSetClass.Members.Add(readXmlSerializable);

            //\\ protected override System.Xml.Schema.XmlSchema GetSchemaSerializable() {
            //\\     System.IO.MemoryStream stream = new System.IO.MemoryStream();
            //\\     WriteXmlSchema(new XmlTextWriter(stream, null ));
            //\\    stream.Position = 0;
            //\\     return System.Xml.Schema.XmlSchema.Read(new XmlTextReader(stream));
            //\\ }
            CodeMemberMethod getSchemaSerializable = MethodDecl(typeof(System.Xml.Schema.XmlSchema), "GetSchemaSerializable", MemberAttributes.Family | MemberAttributes.Override); {
                getSchemaSerializable.Statements.Add(VariableDecl(typeof(System.IO.MemoryStream), "stream", New(typeof(System.IO.MemoryStream),new CodeExpression[] {})));
                getSchemaSerializable.Statements.Add(MethodCall(This(), "WriteXmlSchema", New(typeof(System.Xml.XmlTextWriter),new CodeExpression[] {Argument("stream"),Primitive(null)})));
                getSchemaSerializable.Statements.Add(Assign(Property(Argument("stream"),"Position"),Primitive(0)));
                getSchemaSerializable.Statements.Add(Return(MethodCall(TypeExpr("System.Xml.Schema.XmlSchema"),"Read",new CodeExpression[] {New(typeof(System.Xml.XmlTextReader),new CodeExpression[] {Argument("stream")}), Primitive(null)})));
            }
            dataSetClass.Members.Add(getSchemaSerializable);

                /************ Add Constraints to the Tables **************************/
                CodeExpression varFkc = null;
                foreach(DataTable table in dataSet.Tables) {
                    foreach(Constraint constraint in table.Constraints) {
                        if (constraint is ForeignKeyConstraint) {
                            // We only initialize the foreign key constraints here.
                            //\\ ForeignKeyConstraint fkc;
                            //\\ fkc = new ForeignKeyConstraint("<ConstrainName>", 
                            //\\     new DataColumn[] {this.table<TableClassName>.<ColumnName>Column}, // parent columns
                            //\\     new DataColumn[] {this.table<TableClassName>.<ColumnName>Column}  // child columns
                            //\\ ));
                            //\\ this.table<TableClassName>.Constraints.Add(fkc);
                            //\\ fkc.AcceptRejectRule = constraint.AcceptRejectRule;
                            //\\ fkc.DeleteRule = constraint.DeleteRule;
                            //\\ fkc.UpdateRule = constraint.UpdateRule;

                            ForeignKeyConstraint fkc = (ForeignKeyConstraint) constraint;

                            CodeArrayCreateExpression childrenColumns = new CodeArrayCreateExpression(typeof(DataColumn), 0); {
                                foreach(DataColumn c in fkc.Columns) {
                                    childrenColumns.Initializers.Add(Property(Field(This(), TableFieldName(c.Table)), TableColumnPropertyName(c)));
                                }
                            }

                            CodeArrayCreateExpression parentColumns = new CodeArrayCreateExpression(typeof(DataColumn), 0); {
                                foreach(DataColumn c in fkc.RelatedColumnsReference) {
                                    parentColumns.Initializers.Add(Property(Field(This(), TableFieldName(c.Table)), TableColumnPropertyName(c)));
                                }
                            }

                            if (varFkc == null) {
                                initClassMethod.Statements.Add(VariableDecl(typeof(ForeignKeyConstraint),"fkc"));
                                varFkc = Variable("fkc");
                            }

                            initClassMethod.Statements.Add(Assign(
                                varFkc,
                                New(typeof(ForeignKeyConstraint), new CodeExpression[]{Str(fkc.ConstraintName), parentColumns, childrenColumns})
                            ));
                            initClassMethod.Statements.Add(MethodCall(
                                Property(Field(This(), TableFieldName(table)), "Constraints"), 
                                "Add", 
                                varFkc
                            ));

                            string acceptRejectRule = fkc.AcceptRejectRule.ToString();
                            string deleteRule = fkc.DeleteRule.ToString();
                            string updateRule = fkc.UpdateRule.ToString();
                            initClassMethod.Statements.Add(Assign(Property(varFkc,"AcceptRejectRule"),Field(TypeExpr(fkc.AcceptRejectRule.GetType()), acceptRejectRule)));
                            initClassMethod.Statements.Add(Assign(Property(varFkc,"DeleteRule"),Field(TypeExpr(fkc.DeleteRule.GetType()), deleteRule)));
                            initClassMethod.Statements.Add(Assign(Property(varFkc,"UpdateRule"),Field(TypeExpr(fkc.UpdateRule.GetType()), updateRule)));
                        }
                    }                
                }

                /************ Add Relations to the Dataset **************************/
                foreach(DataRelation relation in dataSet.Relations) {                    
                    //\\ this.relation<RelationName>= new DataRelation("<RelationName>", 
                    //\\     new DataColumn[] {this.table<TableClassName>.<ColumnName>Column}, // parent columns
                    //\\     new DataColumn[] {this.table<TableClassName>.<ColumnName>Column}, // child columns
                    //\\     false                                                             // createConstraints 
                    //\\ ));
                    CodeArrayCreateExpression parentColCreate =  new CodeArrayCreateExpression(typeof(DataColumn), 0); {
                        string parentTableField = TableFieldName(relation.ParentTable);
                        foreach(DataColumn column in relation.ParentColumnsReference) {
                            parentColCreate.Initializers.Add(Property(Field(This(), parentTableField), TableColumnPropertyName(column)));
                        }
                    }

                    CodeArrayCreateExpression childColCreate =  new CodeArrayCreateExpression(typeof(DataColumn), 0); {
                        string childTableField = TableFieldName(relation.ChildTable);
                        foreach(DataColumn column in relation.ChildColumnsReference) {
                            childColCreate.Initializers.Add(Property(Field(This(), childTableField), TableColumnPropertyName(column)));
                        }
                    }
    
                    initClassMethod.Statements.Add(Assign(
                        Field(This(), RelationFieldName(relation)),
                        New(typeof(DataRelation), new CodeExpression[] {Str(relation.RelationName), parentColCreate, childColCreate,Primitive(false)})
                    ));

                    if (relation.Nested) {
                        //\\ this.relation<RelationName>.Nested = true;
                        initClassMethod.Statements.Add(Assign(Property(Field(This(), RelationFieldName(relation)), "Nested"), Primitive(true)));
                    }
                    //\\ this.Relations.Add(this.relation<RelationName>);
                    initClassMethod.Statements.Add(MethodCall(Property(This(), "Relations"), "Add", Field(This(), RelationFieldName(relation))));

                    //\\ this.relation<RelationName> = this.Relations["<RelationName>"];
                    initVarsMethod.Statements.Add(Assign(Field(This(), RelationFieldName(relation)), Indexer(Property(This(),"Relations"),Str(relation.RelationName))));
                }
            dataSetClass.Members.Add(initVarsMethod);
            }
            dataSetClass.Members.Add(initClassMethod);
            }

            for (int i = 0; i < dataSet.Tables.Count; i++) {
                string TableProperty = TablePropertyName(dataSet.Tables[i]);
                CodeMemberProperty prop = PropertyDecl(TableClassName(dataSet.Tables[i]), TableProperty, MemberAttributes.Public | MemberAttributes.Final); {
                    prop.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable",
                        Primitive(false)
                    ));
                    prop.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerSerializationVisibilityAttribute", 
                        Field(TypeExpr(typeof(DesignerSerializationVisibility)), "Content")
                    ));
                    prop.GetStatements.Add(Return(Field(This(), TableFieldName(dataSet.Tables[i]))));
                }
                dataSetClass.Members.Add(prop);

                CodeMemberMethod shouldSerializeTableProperty = MethodDecl(typeof(System.Boolean), "ShouldSerialize"+TableProperty, MemberAttributes.Private); {
                    shouldSerializeTableProperty.Statements.Add(Return(Primitive(false)));
                }
                dataSetClass.Members.Add(shouldSerializeTableProperty);
            }

            CodeMemberMethod schemaChanged = MethodDecl(typeof(void), "SchemaChanged", MemberAttributes.Private); {
                schemaChanged.Parameters.Add(ParameterDecl(typeof(object), "sender"));            
                schemaChanged.Parameters.Add(ParameterDecl(typeof(CollectionChangeEventArgs), "e"));
                schemaChanged.Statements.Add(
                    If(EQ(Property(Argument("e"),"Action"),Field(TypeExpr(typeof(CollectionChangeAction)),"Remove")),
                        Stm(MethodCall(This(),"InitVars"))
                    )
                );
            }
            dataSetClass.Members.Add(schemaChanged);

            bool bInitExpressions = false;
            //\\  private void initExpressionMethod() {
            //\\  this.table_<TableName>.<ColumnProperty>.Expression = "<ColumnExpression>";
            //\\  }
            CodeMemberMethod initExpressionMethod = MethodDecl(typeof(void), "InitExpressions", MemberAttributes.Private); {
                foreach(DataTable table in dataSet.Tables) {
                   for (int i = 0; i < table.Columns.Count; i++) {
                      DataColumn column = table.Columns[i];
                      CodeExpression codeField = Property(Field(This(), TableFieldName(table)), TableColumnPropertyName(column));
                      if (column.Expression.Length > 0) {
                         bInitExpressions = true;
                         initExpressionMethod.Statements.Add(Assign(Property(codeField, "Expression"), Str(column.Expression)));
                      }
                   }
                }
            }

            if (bInitExpressions) {
                dataSetClass.Members.Add(initExpressionMethod);
                initClassMethod.Statements.Add(MethodCall(This(), "InitExpressions"));
            }

            return dataSetClass;
        }// CreateTypedDataSet

        // CodeGen Helper functions :
        // -------------------- Expressions: ----------------------------
        //\\ this
        private static CodeExpression     This() { return new CodeThisReferenceExpression();}
        //\\ base
        private static CodeExpression     Base() { return new CodeBaseReferenceExpression();}
        //\\ value
        private static CodeExpression     Value() { return new CodePropertySetValueReferenceExpression();}
        //\\ <type>
        private static CodeTypeReference  Type(string type) { return new CodeTypeReference(type); }   
        private static CodeTypeReference  Type(Type type) { return new CodeTypeReference(type); }
        //\\ <type>[<rank>]
        private static CodeTypeReference  Type(string type, Int32 rank) { return new CodeTypeReference(type, rank); }   
        //\\ <type>
        private static CodeTypeReferenceExpression TypeExpr(Type   type) { return new CodeTypeReferenceExpression(type); }   
        private static CodeTypeReferenceExpression TypeExpr(string type) { return new CodeTypeReferenceExpression(type); }   
        //\\ ((<type>)<expr>)
        private static CodeExpression     Cast(string type           , CodeExpression expr) { return new CodeCastExpression(type, expr); }   
        private static CodeExpression     Cast(CodeTypeReference type, CodeExpression expr) { return new CodeCastExpression(type, expr); }   
        //\\ typeof(<type>)
        private static CodeExpression     TypeOf(string type) { return new CodeTypeOfExpression(type); }   
        //\\ <exp>.field
        private static CodeExpression     Field(CodeExpression exp, string field) { return new CodeFieldReferenceExpression(exp, field);}
        //\\ <exp>.property
        private static CodeExpression     Property(CodeExpression exp, string property) { return new CodePropertyReferenceExpression(exp, property);}
        //\\ argument
        private static CodeExpression     Argument(string argument) { return new CodeArgumentReferenceExpression(argument);}
        //\\ variable
        private static CodeExpression     Variable(string variable) { return new CodeVariableReferenceExpression(variable);}
        //\\ this.eventName
        private static CodeExpression     Event(string eventName) { return new CodeEventReferenceExpression(This(), eventName);}
        //\\ new <type>(<parameters>)
        private static CodeExpression     New(string type, CodeExpression[] parameters) { return new CodeObjectCreateExpression(type, parameters);}

        //\\ new <type>(<parameters>)
        private static CodeExpression     New(Type type, CodeExpression[] parameters) { return new CodeObjectCreateExpression(type, parameters);}
        
        //\\ <primitive>
        private static CodeExpression     Primitive(object primitive) { return new CodePrimitiveExpression(primitive);}
        //\\ "<str>"
        private static CodeExpression     Str(string str) { return Primitive(str);}
        //\\ <targetObject>.<methodName>(<parameters>)
        private static CodeExpression     MethodCall(CodeExpression targetObject, String methodName, CodeExpression[] parameters) {
            return new CodeMethodInvokeExpression(targetObject, methodName, parameters);
        }
        //\\ <targetObject>.<methodName>()
        private static CodeExpression     MethodCall(CodeExpression targetObject, String methodName) {
            return new CodeMethodInvokeExpression(targetObject, methodName);
        }
        //\\ <targetObject>.<methodName>(par)
        private static CodeExpression     MethodCall(CodeExpression targetObject, String methodName, CodeExpression par) {
            return new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[] {par});
        }
        //\\ <targetObject>(par)
        private static CodeExpression     DelegateCall(CodeExpression targetObject, CodeExpression par) {
            return new CodeDelegateInvokeExpression(targetObject, new CodeExpression[] {This(), par});
        }
        //\\ <targetObject>[indices]()
        private static CodeExpression     Indexer(CodeExpression targetObject, CodeExpression indices) {return new CodeIndexerExpression(targetObject, indices);}

        // -------------------- Binary Operators: ----------------------------
        private static CodeBinaryOperatorExpression      BinOperator(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right) {
            return new CodeBinaryOperatorExpression(left, op, right);
        }
        //\\ (left) != (right)
        private static CodeBinaryOperatorExpression      IdNotEQ(CodeExpression left, CodeExpression right) {return BinOperator(left, CodeBinaryOperatorType.IdentityInequality, right);}
        //\\ (left) == (right)
        private static CodeBinaryOperatorExpression      EQ(     CodeExpression left, CodeExpression right) {return BinOperator(left, CodeBinaryOperatorType.ValueEquality, right);}

        // -------------------- Statments: ----------------------------
        //\\ <expr>;
        private static CodeStatement      Stm(CodeExpression expr) { return new CodeExpressionStatement(expr);}
        //\\ return(<expr>);
        private static CodeStatement      Return(CodeExpression expr) { return new CodeMethodReturnStatement(expr);}
        //\\ return;
        private static CodeStatement      Return() { return new CodeMethodReturnStatement();}
        //\\ left = right;
        private static CodeStatement      Assign(CodeExpression left, CodeExpression right) { return new CodeAssignStatement(left, right);}

        //\\ throw new <exception>(<arg>, <inner>)
        private static CodeStatement      Throw(Type exception, string arg, string inner) { 
            return new CodeThrowExceptionStatement(New(exception, new CodeExpression[] {Str(Res.GetString(arg)), Variable(inner)}));
        }
        // -------------------- If: ----------------------------
        private static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms, CodeStatement[] falseStms) {
            return new CodeConditionStatement(cond, trueStms, falseStms);
        }
        private static CodeStatement If(   CodeExpression cond, CodeStatement[] trueStms ) {return new CodeConditionStatement(cond, trueStms);}
        private static CodeStatement If(   CodeExpression cond, CodeStatement   trueStm  ) {return If(   cond, new CodeStatement[] {trueStm });}
        // -------------------- Declarations: ----------------------------
        private static CodeMemberField  FieldDecl(String type, String name) {return new CodeMemberField(type, name);}
        private static CodeMemberField  FieldDecl(Type type, String name) {return new CodeMemberField(type, name);}
        private static CodeMemberMethod Method(CodeTypeReference type, String name, MemberAttributes attributes) {
            CodeMemberMethod method = new CodeMemberMethod(); {
                method.ReturnType = type;
                method.Name       = name;
                method.Attributes = attributes;
            }
            return method;
        }
        private static CodeMemberMethod   MethodDecl(Type type, String name, MemberAttributes attributes) {return Method(Type(type), name, attributes);}
        private static CodeMemberMethod   MethodDecl(String type, String name, MemberAttributes attributes) {return Method(Type(type), name, attributes);}
        private static CodeMemberProperty PropertyDecl(String type, String name, MemberAttributes attributes) {
            CodeMemberProperty property = new CodeMemberProperty(); {
                property.Type       = Type(type);
                property.Name       = name;
                property.Attributes = attributes;
            }
            return property;
        }

        private static CodeMemberProperty PropertyDecl(Type type, String name, MemberAttributes attributes) {
            CodeMemberProperty property = new CodeMemberProperty(); {
                property.Type       = Type(type);
                property.Name       = name;
                property.Attributes = attributes;
            }
            return property;
        }
        private static CodeStatement   VariableDecl(Type type, String name) { return new CodeVariableDeclarationStatement(type, name); }
        private static CodeStatement   VariableDecl(String type, String name, CodeExpression initExpr) { return new CodeVariableDeclarationStatement(type, name, initExpr); }
        private static CodeStatement   VariableDecl(Type type, String name, CodeExpression initExpr) { return new CodeVariableDeclarationStatement(type, name, initExpr); }
        private static CodeMemberEvent EventDecl(String type, String name)  {
            CodeMemberEvent anEvent = new CodeMemberEvent(); {
                anEvent.Name       = name;
                anEvent.Type       = Type(type);
                anEvent.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            }
            return anEvent;
        }
        private static CodeParameterDeclarationExpression     ParameterDecl(string type, string name) { return new CodeParameterDeclarationExpression(type, name);}
        private static CodeParameterDeclarationExpression     ParameterDecl(Type type, string name) { return new CodeParameterDeclarationExpression(type, name);}
        private static CodeAttributeDeclaration               AttributeDecl(string name) {
            return new CodeAttributeDeclaration(name);
        }
        private static CodeAttributeDeclaration               AttributeDecl(string name, CodeExpression value) {
            return new CodeAttributeDeclaration(name, new CodeAttributeArgument[] { new CodeAttributeArgument(value) });
        }
	// -------------------- Try/Catch ---------------------------
	//\\ try {<tryStmnt>} <catchClause>
	private static CodeStatement      Try(CodeStatement tryStmnt, CodeCatchClause catchClause) {
			return new CodeTryCatchFinallyStatement(
				new CodeStatement[] {tryStmnt}, 
				new CodeCatchClause[] {catchClause}
			);
	}
	//\\ catch(<type> <name>) {<catchStmnt>}
	private static CodeCatchClause Catch(Type type, string name, CodeStatement catchStmnt) {
            CodeCatchClause ccc = new CodeCatchClause();
            ccc.CatchExceptionType = Type(type);
            ccc.LocalName = name;
            ccc.Statements.Add(catchStmnt);
            return ccc;
	}
    }
}
