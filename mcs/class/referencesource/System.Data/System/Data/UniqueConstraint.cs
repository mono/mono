//------------------------------------------------------------------------------
// <copyright file="UniqueConstraint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>
    ///       Represents a restriction on a set of columns in which all values must be unique.
    ///    </para>
    /// </devdoc>
    [
    DefaultProperty("ConstraintName"),
    Editor("Microsoft.VSDesigner.Data.Design.UniqueConstraintEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ]
    public class UniqueConstraint : Constraint {
        private DataKey key;
        private Index _constraintIndex;
        internal bool bPrimaryKey = false;

        // Design time serialization
        internal string constraintName = null;
        internal string[] columnNames = null;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified name and
        /// <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public UniqueConstraint(String name, DataColumn column) {
            DataColumn[] columns = new DataColumn[1];
            columns[0] = column;
            Create(name, columns);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public UniqueConstraint(DataColumn column) {
            DataColumn[] columns = new DataColumn[1];
            columns[0] = column;
            Create(null, columns);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified name and array
        ///    of <see cref='System.Data.DataColumn'/> objects.</para>
        /// </devdoc>
        public UniqueConstraint(String name, DataColumn[] columns) {
            Create(name, columns);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the given array of <see cref='System.Data.DataColumn'/>
        ///       objects.
        ///    </para>
        /// </devdoc>
        public UniqueConstraint(DataColumn[] columns) {
            Create(null, columns);
        }

        // Construct design time object
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false)]
        public UniqueConstraint(String name, string[] columnNames, bool isPrimaryKey) {
            this.constraintName = name;
            this.columnNames = columnNames;
            this.bPrimaryKey = isPrimaryKey;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified name and
        /// <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public UniqueConstraint(String name, DataColumn column, bool isPrimaryKey) {
            DataColumn[] columns = new DataColumn[1];
            columns[0] = column;
            this.bPrimaryKey = isPrimaryKey;
            Create(name, columns);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public UniqueConstraint(DataColumn column, bool isPrimaryKey) {
            DataColumn[] columns = new DataColumn[1];
            columns[0] = column;
            this.bPrimaryKey = isPrimaryKey;
            Create(null, columns);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the specified name and array
        ///    of <see cref='System.Data.DataColumn'/> objects.</para>
        /// </devdoc>
        public UniqueConstraint(String name, DataColumn[] columns, bool isPrimaryKey) {
            this.bPrimaryKey = isPrimaryKey;
            Create(name, columns);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.UniqueConstraint'/> with the given array of <see cref='System.Data.DataColumn'/>
        ///       objects.
        ///    </para>
        /// </devdoc>
        public UniqueConstraint(DataColumn[] columns, bool isPrimaryKey) {
            this.bPrimaryKey = isPrimaryKey;
            Create(null, columns);
        }

        // design time serialization only
        internal string[] ColumnNames {
            get {
                return key.GetColumnNames();
            }
        }

        // VSTFDEVDIV 895693: please note that there are scenarios where ConstraintIndex is not the same as key.GetSortIndex()
        // Use constraint index only for search operations (and use key.GetSortIndex() when enumeration is needed and/or order is important)
        internal Index ConstraintIndex {
            get {
                AssertConstraintAndKeyIndexes();
                return _constraintIndex;
            }
        }

        [Conditional("DEBUG")]
        private void AssertConstraintAndKeyIndexes() {
            Debug.Assert(null != _constraintIndex, "null UniqueConstraint index");

            // ideally, we would like constraintIndex and key.GetSortIndex to share the same index underneath: Debug.Assert(_constraintIndex == key.GetSortIndex)
            // but, due to VSTFDEVDIV #895693 there is a scenario where constraint and key indexes are built from the same list of columns but in a different order
            DataColumn[] sortIndexColumns = new DataColumn[_constraintIndex.IndexFields.Length];
            for (int i = 0; i < sortIndexColumns.Length; i++) {
                sortIndexColumns[i] = _constraintIndex.IndexFields[i].Column;
            }
            Debug.Assert(DataKey.ColumnsEqual(key.ColumnsReference, sortIndexColumns), "UniqueConstraint index columns do not match the key sort index");
        }

        internal void ConstraintIndexClear() {
            if (null != _constraintIndex) {
                _constraintIndex.RemoveRef();
                _constraintIndex = null;
            }
        }
        
        internal void ConstraintIndexInitialize() {
            //Debug.Assert(null == _constraintIndex, "non-null UniqueConstraint index");
            if (null == _constraintIndex) {
                _constraintIndex = key.GetSortIndex();
                _constraintIndex.AddRef();
            }

            AssertConstraintAndKeyIndexes();
        }

        internal override void CheckState() {
           NonVirtualCheckState();
        }

        private  void NonVirtualCheckState() {
            key.CheckState();
        }

        internal override void CheckCanAddToCollection(ConstraintCollection constraints) {
        }

        internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException) {
            if (this.Equals(constraints.Table.primaryKey)) {
                Debug.Assert(constraints.Table.primaryKey == this, "If the primary key and this are 'Equal', they should also be '=='");
                if (!fThrowException)
                    return false;
                else
                    throw ExceptionBuilder.RemovePrimaryKey(constraints.Table);
            }
            for (ParentForeignKeyConstraintEnumerator cs = new ParentForeignKeyConstraintEnumerator(Table.DataSet, Table); cs.GetNext();) {
                ForeignKeyConstraint constraint = cs.GetForeignKeyConstraint();
                if (!key.ColumnsEqual(constraint.ParentKey))
                    continue;

                if (!fThrowException)
                    return false;
                else
                    throw ExceptionBuilder.NeededForForeignKeyConstraint(this, constraint);
            }

            return true;
        }

        internal override bool CanEnableConstraint() {
            if (Table.EnforceConstraints)
                return ConstraintIndex.CheckUnique();

            return true;
        }

        internal override bool IsConstraintViolated() {
            bool result = false;
            Index index = ConstraintIndex;
            if (index.HasDuplicates) {
                // 
                object[] uniqueKeys = index.GetUniqueKeyValues();

                for (int i = 0; i < uniqueKeys.Length; i++) {
                    Range r = index.FindRecords((object[])uniqueKeys[i]);
                    if (1 < r.Count) {
                        DataRow[] rows = index.GetRows(r);
                        string error = ExceptionBuilder.UniqueConstraintViolationText(key.ColumnsReference, (object[])uniqueKeys[i]);
                        for (int j = 0; j < rows.Length; j++) {
                            // 
                            rows[j].RowError = error;
                            foreach(DataColumn dataColumn in key.ColumnsReference){
                                rows[j].SetColumnError(dataColumn, error);
                            }
                        }
                        // SQLBU 20011224: set_RowError for all DataRow with a unique constraint violation
                        result = true; 
                    }
                }
            }
            return result;
        }

        internal override void CheckConstraint(DataRow row, DataRowAction action) {
            if (Table.EnforceConstraints &&
                (action == DataRowAction.Add ||
                 action == DataRowAction.Change ||
                 (action == DataRowAction.Rollback && row.tempRecord != -1))) {
                if (row.HaveValuesChanged(ColumnsReference)) {
                    if (ConstraintIndex.IsKeyRecordInIndex(row.GetDefaultRecord())) {
                        object[] values = row.GetColumnValues(ColumnsReference);
                        throw ExceptionBuilder.ConstraintViolation(ColumnsReference, values);
                    }
                }
            }
        }

        internal override bool ContainsColumn(DataColumn column) {
            return key.ContainsColumn(column);
        }

        internal override Constraint Clone(DataSet destination) {
            return Clone(destination, false);
        }

        internal override Constraint Clone(DataSet destination, bool ignorNSforTableLookup) {
            int iDest;
            if (ignorNSforTableLookup) {
                iDest = destination.Tables.IndexOf(Table.TableName);
            }
            else {
                iDest = destination.Tables.IndexOf(Table.TableName, Table.Namespace, false);// pass false for last param to be backward compatable
            }

            if (iDest < 0)
                return null;
            DataTable table = destination.Tables[iDest];

            int keys = ColumnsReference.Length;
            DataColumn[] columns = new DataColumn[keys];

            for (int i = 0; i < keys; i++) {
                DataColumn src = ColumnsReference[i];
                iDest = table.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                columns[i] = table.Columns[iDest];
            }

            UniqueConstraint clone = new UniqueConstraint(ConstraintName, columns);

            // ...Extended Properties
            foreach(Object key in this.ExtendedProperties.Keys) {
               clone.ExtendedProperties[key]=this.ExtendedProperties[key];
            }

            return clone;
        }

        internal UniqueConstraint Clone(DataTable table) {
            int keys = ColumnsReference.Length;
            DataColumn[] columns = new DataColumn[keys];

            for (int i = 0; i < keys; i++) {
                DataColumn src = ColumnsReference[i];
                int iDest = table.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                columns[i] = table.Columns[iDest];
            }

            UniqueConstraint clone = new UniqueConstraint(ConstraintName, columns);

            // ...Extended Properties
            foreach(Object key in this.ExtendedProperties.Keys) {
               clone.ExtendedProperties[key]=this.ExtendedProperties[key];
            }

            return clone;
        }

        /// <devdoc>
        ///    <para>Gets the array of columns that this constraint affects.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.KeyConstraintColumnsDescr),
        ReadOnly(true)
        ]
        public virtual DataColumn[] Columns {
            get {
                return key.ToArray();
            }
        }

        internal DataColumn[] ColumnsReference {
            get {
                return key.ColumnsReference;
            }
        }

        /// <devdoc>
        ///    <para>Gets
        ///       a value indicating whether or not the constraint is on a primary key.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.KeyConstraintIsPrimaryKeyDescr)
        ]
        public bool IsPrimaryKey {
            get {
                if (Table == null) {
                    return false;
                }
                return(this == Table.primaryKey);
            }
        }

        private void Create(String constraintName, DataColumn[] columns) {
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Computed) {
                    throw ExceptionBuilder.ExpressionInConstraint(columns[i]);
                }
            }
            this.key = new DataKey(columns, true);
            ConstraintName = constraintName;
            NonVirtualCheckState();
        }

        /// <devdoc>
        ///    <para>Compares this constraint to a second to
        ///       determine if both are identical.</para>
        /// </devdoc>
        public override bool Equals(object key2) {
            if (!(key2 is UniqueConstraint))
                return false;

            return Key.ColumnsEqual(((UniqueConstraint)key2).Key);
        }

        public override Int32 GetHashCode() {
            return base.GetHashCode();
        }

        internal override bool InCollection {
            set {
                base.InCollection = value;
                if (key.ColumnsReference.Length == 1) {
                    key.ColumnsReference[0].InternalUnique(value);
                }
            }
        }

        internal DataKey Key {
            get {
                return key;
            }
        }

        /// <devdoc>
        ///    <para>Gets the table to which this constraint belongs.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.ConstraintTableDescr),
        ReadOnly(true)
        ]
        public override DataTable Table {
            get {
                if (key.HasValue) {
                    return key.Table;
                }
                return null;
            }
        }

        // misc
    }
}
