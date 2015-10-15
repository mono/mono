//------------------------------------------------------------------------------
// <copyright file="ForeignKeyConstraint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Data.Common;

    /// <devdoc>
    ///    <para>Represents an action
    ///       restriction enforced on a set of columns in a primary key/foreign key relationship when
    ///       a value or row is either deleted or updated.</para>
    /// </devdoc>
    [
    DefaultProperty("ConstraintName"),
    Editor("Microsoft.VSDesigner.Data.Design.ForeignKeyConstraintEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ]
    public class ForeignKeyConstraint : Constraint {
        // constants
        internal const Rule                   Rule_Default                   = Rule.Cascade;
        internal const AcceptRejectRule       AcceptRejectRule_Default       = AcceptRejectRule.None;

        // properties
        internal Rule deleteRule = Rule_Default;
        internal Rule updateRule = Rule_Default;
        internal AcceptRejectRule acceptRejectRule = AcceptRejectRule_Default;
        private DataKey childKey;
        private DataKey parentKey;

        // Design time serialization
        internal string constraintName = null;
        internal string[] parentColumnNames = null;
        internal string[] childColumnNames = null;
        internal string parentTableName = null;
        internal string parentTableNamespace = null;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.ForeignKeyConstraint'/> class with the specified parent and
        ///       child <see cref='System.Data.DataColumn'/> objects.
        ///    </para>
        /// </devdoc>
        public ForeignKeyConstraint(DataColumn parentColumn, DataColumn childColumn)
        : this(null, parentColumn, childColumn) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.ForeignKeyConstraint'/> class with the specified name,
        ///       parent and child <see cref='System.Data.DataColumn'/> objects.
        ///    </para>
        /// </devdoc>
        public ForeignKeyConstraint(string constraintName, DataColumn parentColumn, DataColumn childColumn) {
            DataColumn[] parentColumns = new DataColumn[] {parentColumn};
            DataColumn[] childColumns = new DataColumn[] {childColumn};
            Create(constraintName, parentColumns, childColumns);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.ForeignKeyConstraint'/> class with the specified arrays
        ///       of parent and child <see cref='System.Data.DataColumn'/> objects.
        ///    </para>
        /// </devdoc>
        public ForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns)
        : this(null, parentColumns, childColumns) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.ForeignKeyConstraint'/> class with the specified name,
        ///       and arrays of parent and child <see cref='System.Data.DataColumn'/> objects.
        ///    </para>
        /// </devdoc>
        public ForeignKeyConstraint(string constraintName, DataColumn[] parentColumns, DataColumn[] childColumns) {
            Create(constraintName, parentColumns, childColumns);
        }

        // construct design time object
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames,
                                    AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule) {
            this.constraintName = constraintName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.acceptRejectRule = acceptRejectRule;
            this.deleteRule = deleteRule;
            this.updateRule = updateRule;
//            ForeignKeyConstraint(constraintName, parentTableName, null, parentColumnNames, childColumnNames,acceptRejectRule, deleteRule, updateRule)
        }


        // construct design time object
        [Browsable(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string parentTableNamespace, string[] parentColumnNames,
                                    string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule) {
            this.constraintName = constraintName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.parentTableNamespace= parentTableNamespace;
            this.acceptRejectRule = acceptRejectRule;
            this.deleteRule = deleteRule;
            this.updateRule = updateRule;
        }

        /// <devdoc>
        /// The internal constraint object for the child table.
        /// </devdoc>
        internal DataKey ChildKey {
            get {
                CheckStateForProperty();
                return childKey;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the child columns of this constraint.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.ForeignKeyConstraintChildColumnsDescr),
        ReadOnly(true)
        ]
        public virtual DataColumn[] Columns {
            get {
                CheckStateForProperty();
                return childKey.ToArray();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the child table of this constraint.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.ConstraintTableDescr),
        ReadOnly(true)
        ]
        public override DataTable Table {
            get {
                CheckStateForProperty();
                return childKey.Table;
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

        internal override void CheckCanAddToCollection(ConstraintCollection constraints) {
            if (Table != constraints.Table)
                throw ExceptionBuilder.ConstraintAddFailed(constraints.Table);
            if (Table.Locale.LCID != RelatedTable.Locale.LCID || Table.CaseSensitive != RelatedTable.CaseSensitive)
                throw ExceptionBuilder.CaseLocaleMismatch();
        }

        internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException) {
            return true;
        }

        internal bool IsKeyNull( object[] values ) {
            for (int i = 0; i < values.Length; i++) {
            	if (! DataStorage.IsObjectNull(values[i]))
                    return false;
            }

            return true;
        }

        internal override bool IsConstraintViolated() {
            Index childIndex = childKey.GetSortIndex();
            object[] uniqueChildKeys = childIndex.GetUniqueKeyValues();
            bool errors = false;

            Index parentIndex = parentKey.GetSortIndex();
            for (int i = 0; i < uniqueChildKeys.Length; i++) {
                object[] childValues = (object[]) uniqueChildKeys[i];

                if (!IsKeyNull(childValues)) {
                    if (!parentIndex.IsKeyInIndex(childValues)) {
                        DataRow[] rows = childIndex.GetRows(childIndex.FindRecords(childValues));
                        string error = Res.GetString(Res.DataConstraint_ForeignKeyViolation, ConstraintName, ExceptionBuilder.KeysToString(childValues));
                        for (int j = 0; j < rows.Length; j++) {
                            rows[j].RowError = error;
                        }
                        errors = true;
                    }
                }
            }
            return errors;
        }

        internal override bool CanEnableConstraint() {
            if (Table.DataSet == null || !Table.DataSet.EnforceConstraints)
                return true;

            Index childIndex = childKey.GetSortIndex();
            object[] uniqueChildKeys = childIndex.GetUniqueKeyValues();

            Index parentIndex = parentKey.GetSortIndex();
            for (int i = 0; i < uniqueChildKeys.Length; i++) {
                object[] childValues = (object[]) uniqueChildKeys[i];

                if (!IsKeyNull(childValues) && !parentIndex.IsKeyInIndex(childValues)) {
                    return false;
                }
            }
            return true;
        }

        internal void CascadeCommit(DataRow row) {
            if (row.RowState == DataRowState.Detached)
                return;
            if (acceptRejectRule == AcceptRejectRule.Cascade) {
                Index childIndex = childKey.GetSortIndex(      row.RowState == DataRowState.Deleted ? DataViewRowState.Deleted : DataViewRowState.CurrentRows );
                object[] key     = row.GetKeyValues(parentKey, row.RowState == DataRowState.Deleted ? DataRowVersion.Original  : DataRowVersion.Default       );
                if (IsKeyNull(key)) {
                    return;
                }

                Range range      = childIndex.FindRecords(key);
                if (!range.IsNull) {
                    // SQLBU 499726 - DataTable internal index is corrupted: '13'
                    // Self-referencing table has suspendIndexEvents, in the multi-table scenario the child table hasn't
                    // this allows the self-ref table to maintain the index while in the child-table doesn't
                    DataRow[] rows = childIndex.GetRows(range);
                    foreach(DataRow childRow in rows) {
                        if (DataRowState.Detached != childRow.RowState) {
                            if (childRow.inCascade)
                                continue;
                            childRow.AcceptChanges();
                        }
                    }
                }
            }
        }

        internal void CascadeDelete(DataRow row) {
            if (-1 == row.newRecord)
                return;

            object[] currentKey = row.GetKeyValues(parentKey, DataRowVersion.Current);
            if (IsKeyNull(currentKey)) {
                return;
            }

            Index childIndex = childKey.GetSortIndex();
            switch (DeleteRule) {
            case Rule.None: {
                    if (row.Table.DataSet.EnforceConstraints) {
                        // if we're not cascading deletes, we should throw if we're going to strand a child row under enforceConstraints.
                        Range range = childIndex.FindRecords(currentKey);
                        if (!range.IsNull) {
                            if (range.Count == 1 && childIndex.GetRow(range.Min) == row)
                                return;

                            throw ExceptionBuilder.FailedCascadeDelete(ConstraintName);
                        }
                    }
                    break;
                }

            case Rule.Cascade: {
                    object[] key = row.GetKeyValues(parentKey, DataRowVersion.Default);
                    Range range = childIndex.FindRecords(key);
                    if (!range.IsNull) {
                        DataRow[] rows = childIndex.GetRows(range);

                        for (int j = 0; j < rows.Length; j++) {
                            DataRow r = rows[j];
                            if (r.inCascade)
                                continue;
                            r.Table.DeleteRow(r);
                        }
                    }
                    break;
                }

            case Rule.SetNull: {
                    object[] proposedKey = new object[childKey.ColumnsReference.Length];
                    for (int i = 0; i < childKey.ColumnsReference.Length; i++)
                        proposedKey[i] = DBNull.Value;
                    Range range = childIndex.FindRecords(currentKey);
                    if (!range.IsNull) {
                        DataRow[] rows = childIndex.GetRows(range);
                        for (int j = 0; j < rows.Length; j++) {
                            // if (rows[j].inCascade)
                            //    continue;
                            if (row != rows[j])
                                rows[j].SetKeyValues(childKey, proposedKey);
                        }
                    }
                    break;
                }
            case Rule.SetDefault: {
                    object[] proposedKey = new object[childKey.ColumnsReference.Length];
                    for (int i = 0; i < childKey.ColumnsReference.Length; i++)
                        proposedKey[i] = childKey.ColumnsReference[i].DefaultValue;
                    Range range = childIndex.FindRecords(currentKey);
                    if (!range.IsNull) {
                        DataRow[] rows = childIndex.GetRows(range);
                        for (int j = 0; j < rows.Length; j++) {
                            // if (rows[j].inCascade)
                            //    continue;
                            if (row != rows[j])
                                rows[j].SetKeyValues(childKey, proposedKey);
                        }
                    }
                    break;
                }
            default: {
                    Debug.Assert(false, "Unknown Rule value");
			break;
                }
            }
        }

        internal void CascadeRollback(DataRow row) {
            Index childIndex = childKey.GetSortIndex(      row.RowState == DataRowState.Deleted  ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            object[] key     = row.GetKeyValues(parentKey, row.RowState == DataRowState.Modified ? DataRowVersion.Current        : DataRowVersion.Default      );

            // 
            if (IsKeyNull(key)) {
                return;
            }

            Range range      = childIndex.FindRecords(key);
            if (acceptRejectRule == AcceptRejectRule.Cascade) {
                if (!range.IsNull) {
                    DataRow[] rows = childIndex.GetRows(range);
                    for (int j = 0; j < rows.Length; j++) {
                        if (rows[j].inCascade)
                            continue;
                        rows[j].RejectChanges();
                    }
                }
            }
            else {
                // AcceptRejectRule.None
                if (row.RowState != DataRowState.Deleted && row.Table.DataSet.EnforceConstraints) {
                    if (!range.IsNull) {
                        if (range.Count == 1 && childIndex.GetRow(range.Min) == row)
                            return;

                        if (row.HasKeyChanged(parentKey)) {// if key is not changed, this will not cause child to be stranded
                            throw ExceptionBuilder.FailedCascadeUpdate(ConstraintName);
                        }
                    }
                }
            }
        }

        internal void CascadeUpdate(DataRow row) {
            if (-1 == row.newRecord)
                return;

            object[] currentKey = row.GetKeyValues(parentKey, DataRowVersion.Current);
            if (!Table.DataSet.fInReadXml && IsKeyNull(currentKey)) {
                return;
            }

            Index childIndex = childKey.GetSortIndex();
            switch (UpdateRule) {
            case Rule.None: {
                    if (row.Table.DataSet.EnforceConstraints)
                    {
                        // if we're not cascading deletes, we should throw if we're going to strand a child row under enforceConstraints.
                        Range range = childIndex.FindRecords(currentKey);
                        if (!range.IsNull) {
                            throw ExceptionBuilder.FailedCascadeUpdate(ConstraintName);
                        }
                    }
                    break;
                }

            case Rule.Cascade: {
                    Range range = childIndex.FindRecords(currentKey);
                    if (!range.IsNull) {
                        object[] proposedKey = row.GetKeyValues(parentKey, DataRowVersion.Proposed);
                        DataRow[] rows = childIndex.GetRows(range);
                        for (int j = 0; j < rows.Length; j++) {
                            // if (rows[j].inCascade)
                            //    continue;
                            rows[j].SetKeyValues(childKey, proposedKey);
                        }
                    }
                    break;
                }

            case Rule.SetNull: {
                    object[] proposedKey = new object[childKey.ColumnsReference.Length];
                    for (int i = 0; i < childKey.ColumnsReference.Length; i++)
                        proposedKey[i] = DBNull.Value;
                    Range range = childIndex.FindRecords(currentKey);
                    if (!range.IsNull) {
                        DataRow[] rows = childIndex.GetRows(range);
                        for (int j = 0; j < rows.Length; j++) {
                            // if (rows[j].inCascade)
                            //    continue;
                            rows[j].SetKeyValues(childKey, proposedKey);
                        }
                    }
                    break;
                }
            case Rule.SetDefault: {
                    object[] proposedKey = new object[childKey.ColumnsReference.Length];
                    for (int i = 0; i < childKey.ColumnsReference.Length; i++)
                        proposedKey[i] = childKey.ColumnsReference[i].DefaultValue;
                    Range range = childIndex.FindRecords(currentKey);
                    if (!range.IsNull) {
                        DataRow[] rows = childIndex.GetRows(range);
                        for (int j = 0; j < rows.Length; j++) {
                            // if (rows[j].inCascade)
                            //    continue;
                            rows[j].SetKeyValues(childKey, proposedKey);
                        }
                    }
                    break;
                }
            default: {
                    Debug.Assert(false, "Unknown Rule value");
		    break;
                }
            }
        }

        internal void CheckCanClearParentTable(DataTable table) {
            if (Table.DataSet.EnforceConstraints && Table.Rows.Count > 0) {
                throw ExceptionBuilder.FailedClearParentTable(table.TableName, ConstraintName, Table.TableName);
            }
        }

        internal void CheckCanRemoveParentRow(DataRow row) {
            Debug.Assert(Table.DataSet != null, "Relation " + ConstraintName + " isn't part of a DataSet, so this check shouldn't be happening.");
            if (!Table.DataSet.EnforceConstraints)
                return;
            if (DataRelation.GetChildRows(this.ParentKey, this.ChildKey, row, DataRowVersion.Default).Length > 0) {
                throw ExceptionBuilder.RemoveParentRow(this);
            }
        }

        internal void CheckCascade(DataRow row, DataRowAction action) {
            Debug.Assert(Table.DataSet != null, "ForeignKeyConstraint " + ConstraintName + " isn't part of a DataSet, so this check shouldn't be happening.");

            if (row.inCascade)
                return;

            row.inCascade = true;
            try {
                if (action == DataRowAction.Change) {
                    if (row.HasKeyChanged(parentKey)) {
                        CascadeUpdate(row);
                    }
                }
                else if (action == DataRowAction.Delete) {
                    CascadeDelete(row);
                }
                else if (action == DataRowAction.Commit) {
                    CascadeCommit(row);
                }
                else if (action == DataRowAction.Rollback) {
                    CascadeRollback(row);
                 }
                else if (action == DataRowAction.Add) {
                }
                else {
                    Debug.Assert(false, "attempt to cascade unknown action: " + ((Enum) action).ToString());
                }
            }
            finally {
                row.inCascade = false;
            }
        }

        internal override void CheckConstraint(DataRow childRow, DataRowAction action) {
            if ((action == DataRowAction.Change ||
                 action == DataRowAction.Add ||
                 action == DataRowAction.Rollback) &&
                Table.DataSet != null && Table.DataSet.EnforceConstraints &&
                childRow.HasKeyChanged(childKey)) {

                // This branch is for cascading case verification.
                DataRowVersion version = (action == DataRowAction.Rollback) ? DataRowVersion.Original : DataRowVersion.Current;
                object[] childKeyValues = childRow.GetKeyValues(childKey);
                // check to see if this is just a change to my parent's proposed value.
                if (childRow.HasVersion(version)) {
                    // this is the new proposed value for the parent.
                    DataRow parentRow = DataRelation.GetParentRow(this.ParentKey, this.ChildKey, childRow, version);
                    if(parentRow != null && parentRow.inCascade) {
                        object[] parentKeyValues = parentRow.GetKeyValues(parentKey, action == DataRowAction.Rollback ? version : DataRowVersion.Default);

                        int parentKeyValuesRecord = childRow.Table.NewRecord();
                        childRow.Table.SetKeyValues(childKey, parentKeyValues, parentKeyValuesRecord);
                        if (childKey.RecordsEqual(childRow.tempRecord, parentKeyValuesRecord)) {
                            return;
                        }
                    }
                }

                // now check to see if someone exists... it will have to be in a parent row's current, not a proposed.
                object[] childValues = childRow.GetKeyValues(childKey);
                if (!IsKeyNull(childValues)) {
                    Index parentIndex = parentKey.GetSortIndex();
                    if (!parentIndex.IsKeyInIndex(childValues)) {
                        // could be self-join constraint
                        if (childKey.Table == parentKey.Table && childRow.tempRecord != -1) {
                            int lo = 0;
                            for (lo = 0; lo < childValues.Length; lo++) {
                                DataColumn column = parentKey.ColumnsReference[lo];
                                object value = column.ConvertValue(childValues[lo]);
                                if (0 != column.CompareValueTo(childRow.tempRecord, value)) {
                                    break;
                                }
                            }
                            if (lo == childValues.Length) {
                                return;
                            }
                        }
                        throw ExceptionBuilder.ForeignKeyViolation(ConstraintName, childKeyValues);
                    }
                }
            }
        }

        private void NonVirtualCheckState () {
            if (_DataSet == null) {
                // Make sure columns arrays are valid
                parentKey.CheckState();
                childKey.CheckState();

                if (parentKey.Table.DataSet != childKey.Table.DataSet) {
                    throw ExceptionBuilder.TablesInDifferentSets();
                }

                for (int i = 0; i < parentKey.ColumnsReference.Length; i++) {
                    if (parentKey.ColumnsReference[i].DataType != childKey.ColumnsReference[i].DataType ||
                        ((parentKey.ColumnsReference[i].DataType ==  typeof(DateTime)) && (parentKey.ColumnsReference[i].DateTimeMode != childKey.ColumnsReference[i].DateTimeMode) && ((parentKey.ColumnsReference[i].DateTimeMode & childKey.ColumnsReference[i].DateTimeMode) != DataSetDateTime.Unspecified)))
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                }

                if (childKey.ColumnsEqual(parentKey)) {
                    throw ExceptionBuilder.KeyColumnsIdentical();
                }
            }
        }

        // If we're not in a DataSet relations collection, we need to verify on every property get that we're
        // still a good relation object.
        internal override void CheckState() {
            NonVirtualCheckState ();
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates what kind of action should take place across
        ///       this constraint when <see cref='System.Data.DataTable.AcceptChanges'/>
        ///       is invoked.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(AcceptRejectRule_Default),
        ResDescriptionAttribute(Res.ForeignKeyConstraintAcceptRejectRuleDescr)
        ]
        public virtual AcceptRejectRule AcceptRejectRule {
            get {
                CheckStateForProperty();
                return acceptRejectRule;
            }
            set {
                switch(value) { // @perfnote: Enum.IsDefined
                case AcceptRejectRule.None:
                case AcceptRejectRule.Cascade:
                    this.acceptRejectRule = value;
                    break;
                default:
                    throw Common.ADP.InvalidAcceptRejectRule(value);
                }
            }
        }

        internal override bool ContainsColumn(DataColumn column) {
            return parentKey.ContainsColumn(column) || childKey.ContainsColumn(column);
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
                iDest = destination.Tables.IndexOf(Table.TableName, Table.Namespace, false); // pass false for last param 
                // to be backward compatable, otherwise meay cause new exception
            }
            
            if (iDest < 0)
                return null;
            DataTable table = destination.Tables[iDest];
            if (ignorNSforTableLookup) {
                iDest = destination.Tables.IndexOf(RelatedTable.TableName);
            }
            else {
                iDest = destination.Tables.IndexOf(RelatedTable.TableName, RelatedTable.Namespace, false);// pass false for last param 
            }            
            if (iDest < 0)
                return null;
            DataTable relatedTable = destination.Tables[iDest];

            int keys = Columns.Length;
            DataColumn[] columns = new DataColumn[keys];
            DataColumn[] relatedColumns = new DataColumn[keys];

            for (int i = 0; i < keys; i++) {
                DataColumn src = Columns[i];
                iDest = table.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                columns[i] = table.Columns[iDest];

                src = RelatedColumnsReference[i];
                iDest = relatedTable.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                relatedColumns[i] = relatedTable.Columns[iDest];
            }
            ForeignKeyConstraint clone = new ForeignKeyConstraint(ConstraintName,relatedColumns, columns);
            clone.UpdateRule = this.UpdateRule;
            clone.DeleteRule = this.DeleteRule;
            clone.AcceptRejectRule = this.AcceptRejectRule;

            // ...Extended Properties
            foreach(Object key in this.ExtendedProperties.Keys) {
                clone.ExtendedProperties[key]=this.ExtendedProperties[key];
            }

            return clone;
        }


        internal ForeignKeyConstraint Clone(DataTable destination) {
            Debug.Assert(this.Table == this.RelatedTable, "We call this clone just if we have the same datatable as parent and child ");
            int keys = Columns.Length;
            DataColumn[] columns = new DataColumn[keys];
            DataColumn[] relatedColumns = new DataColumn[keys];

            int iDest  =0;

            for (int i = 0; i < keys; i++) {
                DataColumn src = Columns[i];
                iDest = destination.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                columns[i] = destination.Columns[iDest];

                src = RelatedColumnsReference[i];
                iDest = destination.Columns.IndexOf(src.ColumnName);
                if (iDest < 0)
                    return null;
                relatedColumns[i] = destination.Columns[iDest];
            }
            ForeignKeyConstraint clone = new ForeignKeyConstraint(ConstraintName, relatedColumns, columns);
            clone.UpdateRule = this.UpdateRule;
            clone.DeleteRule = this.DeleteRule;
            clone.AcceptRejectRule = this.AcceptRejectRule;

            // ...Extended Properties
            foreach(Object key in this.ExtendedProperties.Keys) {
                clone.ExtendedProperties[key]=this.ExtendedProperties[key];
            }

            return clone;
        }



        private void Create(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns) {
            if (parentColumns.Length == 0 || childColumns.Length == 0)
                throw ExceptionBuilder.KeyLengthZero();

            if (parentColumns.Length != childColumns.Length)
                throw ExceptionBuilder.KeyLengthMismatch();

            for (int i = 0; i < parentColumns.Length; i++) {
                if (parentColumns[i].Computed) {
                    throw ExceptionBuilder.ExpressionInConstraint(parentColumns[i]);
                }
                if (childColumns[i].Computed) {
                    throw ExceptionBuilder.ExpressionInConstraint(childColumns[i]);
                }
            }

            this.parentKey = new DataKey(parentColumns, true);
            this.childKey = new DataKey(childColumns, true);

            ConstraintName = relationName;

            NonVirtualCheckState();
        }

        /// <devdoc>
        ///    <para> Gets
        ///       or sets the action that occurs across this constraint when a row is deleted.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(Rule_Default),
        ResDescriptionAttribute(Res.ForeignKeyConstraintDeleteRuleDescr)
        ]
        public virtual Rule DeleteRule {
            get {
                CheckStateForProperty();
                return deleteRule;
            }
            set {
                switch(value) { // @perfnote: Enum.IsDefined
                case Rule.None:
                case Rule.Cascade:
                case Rule.SetNull:
                case Rule.SetDefault:
                    this.deleteRule = value;
                    break;
                default:
                    throw Common.ADP.InvalidRule(value);
                }
            }
        }

        /// <devdoc>
        /// <para>Gets a value indicating whether the current <see cref='System.Data.ForeignKeyConstraint'/> is identical to the specified object.</para>
        /// </devdoc>
        public override bool Equals(object key) {
            if (!(key is ForeignKeyConstraint))
                return false;
            ForeignKeyConstraint key2 = (ForeignKeyConstraint) key;

            // The ParentKey and ChildKey completely identify the ForeignKeyConstraint
            return this.ParentKey.ColumnsEqual(key2.ParentKey) && this.ChildKey.ColumnsEqual(key2.ChildKey);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override Int32 GetHashCode() {
            return base.GetHashCode();
        }

        /// <devdoc>
        ///    <para>
        ///       The parent columns of this constraint.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.ForeignKeyConstraintParentColumnsDescr),
        ReadOnly(true)
        ]
        public virtual DataColumn[] RelatedColumns {
            get {
                CheckStateForProperty();
                return parentKey.ToArray();
            }
        }

        internal DataColumn[] RelatedColumnsReference {
            get {
                CheckStateForProperty();
                return parentKey.ColumnsReference;
            }
        }

        /// <devdoc>
        /// The internal key object for the parent table.
        /// </devdoc>
        internal DataKey ParentKey {
            get {
                CheckStateForProperty();
                return parentKey;
            }
        }

        internal DataRelation FindParentRelation () {
            DataRelationCollection rels = Table.ParentRelations;

            for (int i = 0; i < rels.Count; i++) {
                if (rels[i].ChildKeyConstraint == this)
                    return rels[i];
            }
            return null;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the parent table of this constraint.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.ForeignKeyRelatedTableDescr),
        ReadOnly(true)
        ]
        public virtual DataTable RelatedTable {
            get {
                CheckStateForProperty();
                return parentKey.Table;
            }
        }

        /// <devdoc>
        ///    <para>Gets or
        ///       sets the action that occurs across this constraint on when a row is
        ///       updated.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(Rule_Default),
        ResDescriptionAttribute(Res.ForeignKeyConstraintUpdateRuleDescr)
        ]
        public virtual Rule UpdateRule {
            get {
                CheckStateForProperty();
                return updateRule;
            }
            set {
                switch(value) { // @perfnote: Enum.IsDefined
                case Rule.None:
                case Rule.Cascade:
                case Rule.SetNull:
                case Rule.SetDefault:
                    this.updateRule = value;
                    break;
                default:
                    throw Common.ADP.InvalidRule(value);
                }
            }
        }
    }
}
