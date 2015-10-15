//------------------------------------------------------------------------------
// <copyright file="DataRelation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

/*****************************************************************************************************
Rules for Multiple Nested Parent, enforce following constraints

1) At all times, only 1(ONE) FK can be NON-Null in a row.
2) NULL FK values are not associated with PARENT(x), even if if PK is NULL in Parent
3) Enforce <rule 1> when
        a) Any FK value is changed
        b) A relation created that result in Multiple Nested Child

WriteXml

1) WriteXml will throw if <rule 1> is violated
2) if NON-Null FK has parentRow (boolean check) print as Nested, else it will get written as normal row

additional notes:
We decided to enforce the rule 1 just if Xml being persisted
******************************************************************************************************/

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Data.Common;
    using System.Collections.Generic;

    /// <devdoc>
    ///    <para>
    ///       Represents a parent/child relationship between two tables.
    ///    </para>
    /// </devdoc>
    [
    DefaultProperty("RelationName"),
    Editor("Microsoft.VSDesigner.Data.Design.DataRelationEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    TypeConverter(typeof(RelationshipConverter)),
    ]
    public class DataRelation {

        // properties
        private DataSet dataSet    = null;
        internal PropertyCollection extendedProperties = null;
        internal string relationName   = "";

        // events
        private PropertyChangedEventHandler onPropertyChangingDelegate = null;

        // state
        private DataKey childKey;
        private DataKey parentKey;
        private UniqueConstraint parentKeyConstraint = null;
        private ForeignKeyConstraint childKeyConstraint = null;

        // Design time serialization
        internal string[] parentColumnNames = null;
        internal string[] childColumnNames = null;
        internal string parentTableName = null;
        internal string childTableName = null;
        internal string parentTableNamespace= null;
        internal string childTableNamespace = null;
        
        /// <devdoc>
        /// this stores whether the  child element appears beneath the parent in the XML persised files.
        /// </devdoc>
        internal bool nested = false;

        /// <devdoc>
        /// this stores whether the the relationship should make sure that KeyConstraints and ForeignKeyConstraints
        /// exist when added to the ConstraintsCollections of the table.
        /// </devdoc>
        internal bool createConstraints;

        private bool _checkMultipleNested = true;

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataRelation'/> class using the specified name,
        ///       parent, and child columns.
        ///    </para>
        /// </devdoc>
        public DataRelation(string relationName, DataColumn parentColumn, DataColumn childColumn)
        : this(relationName, parentColumn, childColumn, true) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataRelation'/> class using the specified name, parent, and child columns, and
        ///       value to create constraints.
        ///    </para>
        /// </devdoc>
        public DataRelation(string relationName, DataColumn parentColumn, DataColumn childColumn, bool createConstraints) {
            Bid.Trace("<ds.DataRelation.DataRelation|API> %d#, relationName='%ls', parentColumn=%d, childColumn=%d, createConstraints=%d{bool}\n",
                            ObjectID, relationName, (parentColumn != null) ? parentColumn.ObjectID : 0, (childColumn != null) ? childColumn.ObjectID : 0,
                            createConstraints);
            
            DataColumn[] parentColumns = new DataColumn[1];
            parentColumns[0] = parentColumn;
            DataColumn[] childColumns = new DataColumn[1];
            childColumns[0] = childColumn;
            Create(relationName, parentColumns, childColumns, createConstraints);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataRelation'/> class using the specified name
        ///       and matched arrays of parent and child columns.
        ///    </para>
        /// </devdoc>
        public DataRelation(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns)
        : this(relationName, parentColumns, childColumns, true) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataRelation'/> class using the specified name, matched arrays of parent
        ///       and child columns, and value to create constraints.
        ///    </para>
        /// </devdoc>
        public DataRelation(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints) {
            Create(relationName, parentColumns, childColumns, createConstraints);
        }

        // Design time constructor
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false)]
        public DataRelation(string relationName, string parentTableName, string childTableName, string[] parentColumnNames, string[] childColumnNames, bool nested) {
            this.relationName = relationName;            
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.childTableName = childTableName;
            this.nested = nested;
            // DataRelation(relationName, parentTableName, null, childTableName, null, parentColumnNames, childColumnNames, nested) 
        }

        [Browsable(false)]
        // Design time constructor
        public DataRelation(string relationName, string parentTableName, string parentTableNamespace, string childTableName, string childTableNamespace, string[] parentColumnNames, string[] childColumnNames, bool nested) {
            this.relationName = relationName;            
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.childTableName = childTableName;
            this.parentTableNamespace = parentTableNamespace;
            this.childTableNamespace = childTableNamespace;
            this.nested = nested;
        }
            
        /// <devdoc>
        ///    <para>
        ///       Gets the child columns of this relation.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataRelationChildColumnsDescr)
        ]
        public virtual DataColumn[] ChildColumns {
            get {
                CheckStateForProperty();
                return childKey.ToArray();
            }
        }

        internal DataColumn[] ChildColumnsReference {
            get {
                CheckStateForProperty();
                return childKey.ColumnsReference;
            }
        }

        /// <devdoc>
        /// The internal Key object for the child table.
        /// </devdoc>
        internal DataKey ChildKey {
            get {
                CheckStateForProperty();
                return childKey;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the child table of this relation.
        ///    </para>
        /// </devdoc>
        public virtual DataTable ChildTable {
            get {
                CheckStateForProperty();
                return childKey.Table;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Data.DataSet'/> to which the relations' collection belongs to.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual DataSet DataSet {
            get {
                CheckStateForProperty();
                return dataSet;
            }
        }

        internal string[] ParentColumnNames {
            get {
                return parentKey.GetColumnNames();
            }
        }

        internal string[] ChildColumnNames {
            get {
                return childKey.GetColumnNames();
            }
        }

        private static bool IsKeyNull(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                if (!DataStorage.IsObjectNull(values[i]))
                    return false;
            }

            return true;
        }

        /// <devdoc>
        /// Gets the child rows for the parent row across the relation using the version given
        /// </devdoc>
        internal static DataRow[] GetChildRows(DataKey parentKey, DataKey childKey, DataRow parentRow, DataRowVersion version) {
            object[] values = parentRow.GetKeyValues(parentKey, version);
            if (IsKeyNull(values)) {
                return childKey.Table.NewRowArray(0);
            }

            Index index = childKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            return index.GetRows(values);
        }

        /// <devdoc>
        /// Gets the parent rows for the given child row across the relation using the version given
        /// </devdoc>
        internal static DataRow[] GetParentRows(DataKey parentKey, DataKey childKey, DataRow childRow, DataRowVersion version) {
            object[] values = childRow.GetKeyValues(childKey, version);
            if (IsKeyNull(values)) {
                return parentKey.Table.NewRowArray(0);
            }

            Index index = parentKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            return index.GetRows(values);
        }

        internal static DataRow GetParentRow(DataKey parentKey, DataKey childKey, DataRow childRow, DataRowVersion version) {
            if (!childRow.HasVersion((version == DataRowVersion.Original) ? DataRowVersion.Original : DataRowVersion.Current))
                if (childRow.tempRecord == -1)
                    return null;

            object[] values = childRow.GetKeyValues(childKey, version);
            if (IsKeyNull(values)) {
                return null;
            }

            Index index = parentKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            Range range = index.FindRecords(values);
            if (range.IsNull) {
                return null;
            }

            if (range.Count > 1) {
                throw ExceptionBuilder.MultipleParents();
            }
            return parentKey.Table.recordManager[index.GetRecord(range.Min)];
        }


        /// <devdoc>
        /// Internally sets the DataSet pointer.
        /// </devdoc>
        internal void SetDataSet(DataSet dataSet) {
            if (this.dataSet != dataSet) {
                this.dataSet = dataSet;
            }
        }

        internal void SetParentRowRecords(DataRow childRow, DataRow parentRow) {
            object[] parentKeyValues = parentRow.GetKeyValues(ParentKey);
            if (childRow.tempRecord != -1) {
                ChildTable.recordManager.SetKeyValues(childRow.tempRecord, ChildKey, parentKeyValues);
            }
            if (childRow.newRecord != -1) {
                ChildTable.recordManager.SetKeyValues(childRow.newRecord, ChildKey, parentKeyValues);
            }
            if (childRow.oldRecord != -1) {
                ChildTable.recordManager.SetKeyValues(childRow.oldRecord, ChildKey, parentKeyValues);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the parent columns of this relation.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataRelationParentColumnsDescr)
        ]
        public virtual DataColumn[] ParentColumns {
            get {
                CheckStateForProperty();
                return parentKey.ToArray();
            }
        }

        internal DataColumn[] ParentColumnsReference {
            get {
                return parentKey.ColumnsReference;
            }
        }

        /// <devdoc>
        /// The internal constraint object for the parent table.
        /// </devdoc>
        internal DataKey ParentKey {
            get {
                CheckStateForProperty();
                return parentKey;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the parent table of this relation.
        ///    </para>
        /// </devdoc>
        public virtual DataTable ParentTable {
            get {
                CheckStateForProperty();
                return parentKey.Table;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the name used to look up this relation in the parent
        ///       data set's <see cref='System.Data.DataRelationCollection'/>.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataRelationRelationNameDescr)
        ]
        public virtual string RelationName {
            get {
                CheckStateForProperty();
                return relationName;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataRelation.set_RelationName|API> %d#, '%ls'\n", ObjectID, value);
                try {
                    if (value == null)
                        value = "";

                    CultureInfo locale = (dataSet != null ? dataSet.Locale : CultureInfo.CurrentCulture);
                    if (String.Compare(relationName, value, true, locale) != 0) {
                        if (dataSet != null) {
                            if (value.Length == 0)
                                throw ExceptionBuilder.NoRelationName();
                            dataSet.Relations.RegisterName(value);
                            if (relationName.Length != 0)
                                dataSet.Relations.UnregisterName(relationName);
                        }
                        this.relationName = value;
                        ((DataRelationCollection.DataTableRelationCollection)(ParentTable.ChildRelations)).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        ((DataRelationCollection.DataTableRelationCollection)(ChildTable.ParentRelations)).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                    }
                    else if (String.Compare(relationName, value, false, locale) != 0) {
                        relationName = value;
                        ((DataRelationCollection.DataTableRelationCollection)(ParentTable.ChildRelations)).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        ((DataRelationCollection.DataTableRelationCollection)(ChildTable.ParentRelations)).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                    }
                }
                finally{
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }
        internal void CheckNamespaceValidityForNestedRelations(string ns) {
            foreach(DataRelation rel in ChildTable.ParentRelations) {
                if (rel == this || rel.Nested) {
                    if (rel.ParentTable.Namespace != ns) {
                        throw ExceptionBuilder.InValidNestedRelation(ChildTable.TableName);
                    }
                }
            }
        }

        internal void CheckNestedRelations() {
            Bid.Trace("<ds.DataRelation.CheckNestedRelations|INFO> %d#\n", ObjectID);
            
            Debug.Assert(DataSet == null || ! nested, "this relation supposed to be not in dataset or not nested");
            // 1. There is no other relation (R) that has this.ChildTable as R.ChildTable
            //  This is not valid for Whidbey anymore so the code has been removed

            // 2. There is no loop in nested relations
#if DEBUG
            int numTables = ParentTable.DataSet.Tables.Count;
#endif
            DataTable dt = ParentTable;
            
            if (ChildTable == ParentTable){
                if (String.Compare(ChildTable.TableName, ChildTable.DataSet.DataSetName, true, ChildTable.DataSet.Locale) == 0)  
                   throw ExceptionBuilder.SelfnestedDatasetConflictingName(ChildTable.TableName);
                return; //allow self join tables.
            }

            List<DataTable> list = new List<DataTable>();
            list.Add(ChildTable);
            
            // We have already checked for nested relaion UP
            for(int i = 0; i < list.Count; ++i) {
                DataRelation[] relations = list[i].NestedParentRelations;
                foreach(DataRelation rel in relations) {
                    if (rel.ParentTable == ChildTable && rel.ChildTable != ChildTable) {
                        throw ExceptionBuilder.LoopInNestedRelations(ChildTable.TableName);
                    }
                    if (!list.Contains (rel.ParentTable)) { // check for self nested
                        list.Add(rel.ParentTable);
                    }
                }
            }
        }
        /********************
          The Namespace of a table nested inside multiple parents can be
          1. Explicitly specified
          2. Inherited from Parent Table
          3. Empty (Form = unqualified case)
          However, Schema does not allow (3) to be a global element and multiple nested child has to be a global element.
          Therefore we'll reduce case (3) to (2) if all parents have same namespace else throw.
         ********************/
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether relations are nested.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataRelationNested)
        ]
        public virtual bool Nested {
            get {
                CheckStateForProperty();
                return nested;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataRelation.set_Nested|API> %d#, %d{bool}\n", ObjectID, value); 
                try {                
                    if (nested != value) {
                        if (dataSet != null) {
                            if (value) {
                                if (ChildTable.IsNamespaceInherited()) { // if not added to collection, don't do this check
                                    CheckNamespaceValidityForNestedRelations(ParentTable.Namespace);
                                }
                                Debug.Assert(ChildTable != null, "On a DataSet, but not on Table. Bad state");
                                ForeignKeyConstraint constraint = ChildTable.Constraints.FindForeignKeyConstraint(ChildKey.ColumnsReference, ParentKey.ColumnsReference);
                                if (constraint != null) {
                                    constraint.CheckConstraint();
                                }
                                ValidateMultipleNestedRelations();
                            }
                        }
                        if (!value && (parentKey.ColumnsReference[0].ColumnMapping == MappingType.Hidden))
                            throw ExceptionBuilder.RelationNestedReadOnly();

                        if (value) {
                          this.ParentTable.Columns.RegisterColumnName(this.ChildTable.TableName, null);
                        } else {
                          this.ParentTable.Columns.UnregisterName(this.ChildTable.TableName);
                        }
                        RaisePropertyChanging("Nested");

                        if(value) {
                            CheckNestedRelations();
                            if (this.DataSet != null)
                                if (ParentTable == ChildTable) {
                                    foreach(DataRow row in ChildTable.Rows)
                                        row.CheckForLoops(this);

                                    if (ChildTable.DataSet != null && ( String.Compare(ChildTable.TableName, ChildTable.DataSet.DataSetName, true, ChildTable.DataSet.Locale) == 0) )
                                        throw ExceptionBuilder.DatasetConflictingName(dataSet.DataSetName);                                    
                                    ChildTable.fNestedInDataset = false;
                                }
                                else {
                                        foreach(DataRow row in ChildTable.Rows)
                                            row.GetParentRow(this);
                                }
                            
                            this.ParentTable.ElementColumnCount++;
                        }
                        else {
                            this.ParentTable.ElementColumnCount--;
                        }

                        this.nested = value;
                        ChildTable.CacheNestedParent();
                        if (value) {
                            if (ADP.IsEmpty(ChildTable.Namespace) && ((ChildTable.NestedParentsCount > 1) || 
                                ((ChildTable.NestedParentsCount > 0) && ! (ChildTable.DataSet.Relations.Contains(this.RelationName))))) {
                                string parentNs = null;
                                foreach(DataRelation rel in ChildTable.ParentRelations) {
                                    if (rel.Nested) {
                                        if (null == parentNs) {
                                            parentNs = rel.ParentTable.Namespace;
                                        }
                                        else {
                                            if (String.Compare(parentNs, rel.ParentTable.Namespace, StringComparison.Ordinal) != 0) {
                                                this.nested = false;
                                                throw ExceptionBuilder.InvalidParentNamespaceinNestedRelation(ChildTable.TableName); 
                                            }
                                        }
                                    }
                                }
                                // if not already in memory , form == unqualified
                                if (CheckMultipleNested && ChildTable.tableNamespace != null && ChildTable.tableNamespace.Length == 0) {
                                    throw ExceptionBuilder.TableCantBeNestedInTwoTables(ChildTable.TableName);
                                }
                                ChildTable.tableNamespace = null; // if we dont throw, then let it inherit the Namespace
                            }
                        }
                    }
                }
                finally{
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the constraint which ensures values in a column are unique.
        ///    </para>
        /// </devdoc>
        public virtual UniqueConstraint ParentKeyConstraint {
            get {
                CheckStateForProperty();
                return parentKeyConstraint;
            }
        }

        internal void SetParentKeyConstraint(UniqueConstraint value) {
            Debug.Assert(parentKeyConstraint == null || value == null, "ParentKeyConstraint should not have been set already.");
            parentKeyConstraint = value;
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Data.ForeignKeyConstraint'/> for the relation.
        ///    </para>
        /// </devdoc>
        public virtual ForeignKeyConstraint ChildKeyConstraint {
            get {
                CheckStateForProperty();
                return childKeyConstraint;
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of custom user information.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data), 
        Browsable(false),
        ResDescriptionAttribute(Res.ExtendedPropertiesDescr)
        ]
        public PropertyCollection ExtendedProperties {
            get {
                if (extendedProperties == null) {
                    extendedProperties = new PropertyCollection();
                }
                return extendedProperties;
            }
        }

        internal bool CheckMultipleNested {
            get {
                return _checkMultipleNested;
            }
            set {
                _checkMultipleNested = value;
            }
        }
        
        internal void SetChildKeyConstraint(ForeignKeyConstraint value) {
            Debug.Assert(childKeyConstraint == null || value == null, "ChildKeyConstraint should not have been set already.");
            childKeyConstraint = value;
        }

        internal event PropertyChangedEventHandler PropertyChanging {
            add {
                onPropertyChangingDelegate += value; 
            }
            remove {
                onPropertyChangingDelegate -= value;
            }
        }
        // If we're not in a dataSet relations collection, we need to verify on every property get that we're
        // still a good relation object.
        internal void CheckState() {
            if (dataSet == null) {
                parentKey.CheckState();
                childKey.CheckState();

                if (parentKey.Table.DataSet != childKey.Table.DataSet) {
                    throw ExceptionBuilder.RelationDataSetMismatch();
                }

                if (childKey.ColumnsEqual(parentKey)) {
                    throw ExceptionBuilder.KeyColumnsIdentical();
                }

                for (int i = 0; i < parentKey.ColumnsReference.Length; i++) {
                    if ((parentKey.ColumnsReference[i].DataType != childKey.ColumnsReference[i].DataType) ||
                        ((parentKey.ColumnsReference[i].DataType ==  typeof(DateTime)) && 
                        (parentKey.ColumnsReference[i].DateTimeMode != childKey.ColumnsReference[i].DateTimeMode) &&
                        ((parentKey.ColumnsReference[i].DateTimeMode & childKey.ColumnsReference[i].DateTimeMode) != DataSetDateTime.Unspecified)))
                        // alow unspecified and unspecifiedlocal
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                }
            }
        }

        /// <devdoc>
        ///    <para>Checks to ensure the DataRelation is a valid object, even if it doesn't
        ///       belong to a <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        protected void CheckStateForProperty() {
            try {
                CheckState();
            }
            catch (Exception e) {
                // 
                if (ADP.IsCatchableExceptionType(e)) {
                    throw ExceptionBuilder.BadObjectPropertyAccess(e.Message);            
                }
                throw;
            }
        }

        private void Create(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRelation.Create|INFO> %d#, relationName='%ls', createConstraints=%d{bool}\n",
                               ObjectID, relationName, createConstraints);
            try {
                this.parentKey = new DataKey(parentColumns, true);
                this.childKey = new DataKey(childColumns, true);

                if (parentColumns.Length != childColumns.Length)
                    throw ExceptionBuilder.KeyLengthMismatch();

                for(int i = 0; i < parentColumns.Length; i++){
                    if ((parentColumns[i].Table.DataSet == null) || (childColumns[i].Table.DataSet == null))
                        throw ExceptionBuilder.ParentOrChildColumnsDoNotHaveDataSet();
                }

                CheckState();

                this.relationName = (relationName == null ? "" : relationName);
                this.createConstraints = createConstraints;
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }


        internal DataRelation Clone(DataSet destination) {
            Bid.Trace("<ds.DataRelation.Clone|INFO> %d#, destination=%d\n", ObjectID, (destination != null) ? destination.ObjectID : 0);
            
            DataTable parent = destination.Tables[ParentTable.TableName, ParentTable.Namespace];
            DataTable child = destination.Tables[ChildTable.TableName, ChildTable.Namespace];
            int keyLength = parentKey.ColumnsReference.Length;

            DataColumn[] parentColumns = new DataColumn[keyLength];
            DataColumn[] childColumns = new DataColumn[keyLength];

            for (int i = 0; i < keyLength; i++) {
                parentColumns[i] = parent.Columns[ParentKey.ColumnsReference[i].ColumnName];
                childColumns[i] = child.Columns[ChildKey.ColumnsReference[i].ColumnName];
            }

            DataRelation clone = new DataRelation(relationName, parentColumns, childColumns, false);

            clone.CheckMultipleNested = false; // disable the check  in clone as it is already created
            clone.Nested = this.Nested;
            clone.CheckMultipleNested = true; // enable the check 

            // ...Extended Properties
            if (this.extendedProperties != null) {
                foreach(Object key in this.extendedProperties.Keys) {
                    clone.ExtendedProperties[key]=this.extendedProperties[key];
                }
            }
            return clone;
        }

        protected internal void OnPropertyChanging(PropertyChangedEventArgs pcevent) {
            if (onPropertyChangingDelegate != null) {
                Bid.Trace("<ds.DataRelation.OnPropertyChanging|INFO> %d#\n", ObjectID);
                onPropertyChangingDelegate(this, pcevent);
            }
        }
        
        protected internal void RaisePropertyChanging(string name) {
            OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        /// <devdoc>
        /// </devdoc>
        public override string ToString() {
            return RelationName;
        }

        internal void ValidateMultipleNestedRelations() {
            // find all nested relations that this child table has
            // if this relation is the only relation it has, then fine, 
            // otherwise check if all relations are created from XSD, without using Key/KeyRef
            // check all keys to see autogenerated

            if (!this.Nested || !CheckMultipleNested) // no need for this verification 
                return;

            if (0 <  ChildTable.NestedParentRelations.Length) {
                DataColumn[] childCols = ChildColumns;
                if (childCols.Length != 1 || !IsAutoGenerated(childCols[0])) {
                    throw ExceptionBuilder.TableCantBeNestedInTwoTables(ChildTable.TableName);
                }
                
                if (!XmlTreeGen.AutoGenerated(this)) {
                    throw ExceptionBuilder.TableCantBeNestedInTwoTables(ChildTable.TableName);
                }
                
                foreach (Constraint cs in ChildTable.Constraints) {
                    if (cs is ForeignKeyConstraint) {
                        ForeignKeyConstraint fk = (ForeignKeyConstraint) cs;
                        if (!XmlTreeGen.AutoGenerated(fk, true)) {
                            throw ExceptionBuilder.TableCantBeNestedInTwoTables(ChildTable.TableName);
                        }
                    }
                    else {
                        UniqueConstraint unique = (UniqueConstraint) cs;
                        if (!XmlTreeGen.AutoGenerated(unique)) {
                            throw ExceptionBuilder.TableCantBeNestedInTwoTables(ChildTable.TableName);
                        }
                    }
                }
                
            }
        }

        private bool IsAutoGenerated(DataColumn col) {
            if (col.ColumnMapping != MappingType.Hidden)
                return false;
            if (col.DataType != typeof(int))
                return false;
            string generatedname = col.Table.TableName+"_Id";

            if ((col.ColumnName == generatedname) || (col.ColumnName == generatedname+"_0"))
                return true;

            generatedname = this.ParentColumnsReference[0].Table.TableName+"_Id";
            if ((col.ColumnName == generatedname) || (col.ColumnName == generatedname+"_0"))
                return true;

            return false;
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }
    }
}
