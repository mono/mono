//------------------------------------------------------------------------------
// <copyright file="DataColumnCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Xml;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Diagnostics;

    /// <devdoc>
    /// <para>Represents a collection of <see cref='System.Data.DataColumn'/>
    /// objects for a <see cref='System.Data.DataTable'/>.</para>
    /// </devdoc>
    [
    DefaultEvent("CollectionChanged"),
    Editor("Microsoft.VSDesigner.Data.Design.ColumnsCollectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ]
    public sealed class DataColumnCollection : InternalDataCollectionBase {

        private readonly DataTable table;
        private readonly ArrayList _list = new ArrayList();
        private int defaultNameIndex = 1;
        private DataColumn[] delayedAddRangeColumns;

        private readonly Dictionary<string, DataColumn> columnFromName;     // Links names to columns
        private CollectionChangeEventHandler onCollectionChangedDelegate;
        private CollectionChangeEventHandler onCollectionChangingDelegate;

        private CollectionChangeEventHandler onColumnPropertyChangedDelegate;

        private bool fInClear;

        private DataColumn[] columnsImplementingIChangeTracking = DataTable.zeroColumns;
        private int nColumnsImplementingIChangeTracking = 0;
        private int nColumnsImplementingIRevertibleChangeTracking = 0;

        /// <devdoc>
        /// DataColumnCollection constructor.  Used only by DataTable.
        /// </devdoc>
        internal DataColumnCollection(DataTable table) {
            this.table = table;
            columnFromName = new Dictionary<string, DataColumn>();
        }

        /// <devdoc>
        ///    <para>Gets the list of the collection items.</para>
        /// </devdoc>
        protected override ArrayList List {
            get {
                return _list;
            }
        }

        internal DataColumn[] ColumnsImplementingIChangeTracking {
            get {
                return columnsImplementingIChangeTracking;
            }
        }
        internal int ColumnsImplementingIChangeTrackingCount{
            get {
                return nColumnsImplementingIChangeTracking;
            }
        }
        internal int ColumnsImplementingIRevertibleChangeTrackingCount {
            get {
                return nColumnsImplementingIRevertibleChangeTracking;
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Data.DataColumn'/>
        ///       from the collection at the specified index.
        ///    </para>
        /// </devdoc>
        public DataColumn this[int index] {
            get {
                try { // Perf: use the readonly _list field directly and let ArrayList check the range
                    return (DataColumn)_list[index];
                }
                catch(ArgumentOutOfRangeException) {
                    throw ExceptionBuilder.ColumnOutOfRange(index);
                }
            }
        }

        /// <devdoc>
        /// <para>Gets the <see cref='System.Data.DataColumn'/> from the collection with the specified name.</para>
        /// </devdoc>
        public DataColumn this[string name] {
            get {
                if (null == name) {
                    throw ExceptionBuilder.ArgumentNull("name");
                }
                DataColumn column;
                if ((!columnFromName.TryGetValue(name, out column)) || (column == null)) {
                    // Case-Insensitive compares
                    int index = IndexOfCaseInsensitive(name);
                    if (0 <= index) {
                        column = (DataColumn)_list[index];
                    }
                    else if (-2 == index) {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                }
                return column;
            }
        }

        internal DataColumn this[string name, string ns] {
            get {
                DataColumn column;
                if ((columnFromName.TryGetValue(name, out column)) && (column != null) && (column.Namespace == ns)) {
                    return column;
                }

                return null;
            }
        }

        internal void EnsureAdditionalCapacity(int capacity) {
            if (_list.Capacity < capacity + _list.Count) {
                _list.Capacity = capacity + _list.Count;
            }
        }

        /// <devdoc>
        /// <para>Adds the specified <see cref='System.Data.DataColumn'/>
        /// to the columns collection.</para>
        /// </devdoc>
        public void Add(DataColumn column) {
            AddAt(-1, column);
        }

        internal void AddAt(int index, DataColumn column) {
            if (column != null && column.ColumnMapping == MappingType.SimpleContent) {
                if (table.XmlText != null && table.XmlText != column)
                    throw ExceptionBuilder.CannotAddColumn3();
                if (table.ElementColumnCount > 0)
                    throw ExceptionBuilder.CannotAddColumn4(column.ColumnName);
                OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
                BaseAdd(column);
                if (index != -1)
                    ArrayAdd(index, column);
                else
                    ArrayAdd(column);

                table.XmlText = column;
            }
            else {
                OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
                BaseAdd(column);
                if (index != -1)
                    ArrayAdd(index, column);
                else
                    ArrayAdd(column);
                // if the column is an element increase the internal dataTable counter
                if (column.ColumnMapping == MappingType.Element)
                    table.ElementColumnCount ++;
            }
            if (!table.fInitInProgress && column != null && column.Computed) {
                column.Expression = column.Expression;
            }
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(DataColumn[] columns) {
            if (table.fInitInProgress) {
                delayedAddRangeColumns = columns;
                return;
            }

            if (columns != null) {
                foreach(DataColumn column in columns) {
                    if (column != null) {
                        Add(column);
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Creates and adds a <see cref='System.Data.DataColumn'/>
        /// with
        /// the specified name, type, and compute expression to the columns collection.</para>
        /// </devdoc>
        public DataColumn Add(string columnName, Type type, string expression) {
            DataColumn column = new DataColumn(columnName, type, expression);
            Add(column);
            return column;
        }

        /// <devdoc>
        /// <para>Creates and adds a <see cref='System.Data.DataColumn'/>
        /// with the
        /// specified name and type to the columns collection.</para>
        /// </devdoc>
        public DataColumn Add(string columnName, Type type) {
            DataColumn column = new DataColumn(columnName, type);
            Add(column);
            return column;
        }

        /// <devdoc>
        /// <para>Creates and adds a <see cref='System.Data.DataColumn'/>
        /// with the specified name to the columns collection.</para>
        /// </devdoc>
        public DataColumn Add(string columnName) {
            DataColumn column = new DataColumn(columnName);
            Add(column);
            return column;
        }

        /// <devdoc>
        /// <para>Creates and adds a <see cref='System.Data.DataColumn'/> to a columns collection.</para>
        /// </devdoc>
        public DataColumn Add() {
            DataColumn column = new DataColumn();
            Add(column);
            return column;
        }


        /// <devdoc>
        ///    <para>Occurs when the columns collection changes, either by adding or removing a column.</para>
        /// </devdoc>
        [ResDescriptionAttribute(Res.collectionChangedEventDescr)]
        public event CollectionChangeEventHandler CollectionChanged {
            add {
                onCollectionChangedDelegate += value;
            }
            remove {
                onCollectionChangedDelegate -= value;
            }
        }

        internal event CollectionChangeEventHandler CollectionChanging {
            add {
                onCollectionChangingDelegate += value;
            }
            remove {
                onCollectionChangingDelegate -= value;
            }
        }

        internal event CollectionChangeEventHandler ColumnPropertyChanged {
            add {
                onColumnPropertyChangedDelegate += value;
            }
            remove {
                onColumnPropertyChangedDelegate -= value;
            }
        }

        /// <devdoc>
        ///  Adds the column to the columns array.
        /// </devdoc>
        private void ArrayAdd(DataColumn column) {
            _list.Add(column);
            column.SetOrdinalInternal(_list.Count - 1);
            CheckIChangeTracking(column);
        }

        private void ArrayAdd(int index, DataColumn column) {
            _list.Insert(index, column);
            CheckIChangeTracking(column);
        }

        private void ArrayRemove(DataColumn column) {
            column.SetOrdinalInternal(-1);
            _list.Remove(column);

            int count = _list.Count;
            for (int i =0; i < count; i++) {
                ((DataColumn) _list[i]).SetOrdinalInternal(i);
            }
            if (column.ImplementsIChangeTracking) {
                RemoveColumnsImplementingIChangeTrackingList(column);
            }
        }

        /// <devdoc>
        /// Creates a new default name.
        /// </devdoc>
        internal string AssignName() {
            string newName = MakeName(defaultNameIndex++);

            while (columnFromName.ContainsKey(newName)) {
                newName = MakeName(defaultNameIndex++);
            }

            return newName;
        }

        /// <devdoc>
        /// Does verification on the column and it's name, and points the column at the dataSet that owns this collection.
        /// An ArgumentNullException is thrown if this column is null.  An ArgumentException is thrown if this column
        /// already belongs to this collection, belongs to another collection.
        /// A DuplicateNameException is thrown if this collection already has a column with the same
        /// name (case insensitive).
        /// </devdoc>
        private void BaseAdd(DataColumn column) {
            if (column == null)
                throw ExceptionBuilder.ArgumentNull("column");
            if (column.table == table)
                throw ExceptionBuilder.CannotAddColumn1(column.ColumnName);
            if (column.table != null)
                throw ExceptionBuilder.CannotAddColumn2(column.ColumnName);
            
            if (column.ColumnName.Length == 0) {
                column.ColumnName = AssignName();
            }
            RegisterColumnName(column.ColumnName, column);
            try {
                column.SetTable(table);
                if (!table.fInitInProgress && column.Computed) {
                    if (column.DataExpression.DependsOn(column)) {
                        throw ExceptionBuilder.ExpressionCircular();
                    }
                }

                if (0 < table.RecordCapacity) {
                    // adding a column to table with existing rows
                    column.SetCapacity(table.RecordCapacity);
                }

                // fill column with default value.
                for (int record = 0; record < table.RecordCapacity; record++) {
                    column.InitializeRecord(record);
                }

                if (table.DataSet != null) {
                    column.OnSetDataSet();
                }
            }
            catch (Exception e) {
                // 
                if (ADP.IsCatchableOrSecurityExceptionType(e)) {
                    UnregisterName(column.ColumnName);
                }
                throw;
            }
        }

        /// <devdoc>
        /// BaseGroupSwitch will intelligently remove and add tables from the collection.
        /// </devdoc>
        private void BaseGroupSwitch(DataColumn[] oldArray, int oldLength, DataColumn[] newArray, int newLength) {
            // We're doing a smart diff of oldArray and newArray to find out what
            // should be removed.  We'll pass through oldArray and see if it exists
            // in newArray, and if not, do remove work.  newBase is an opt. in case
            // the arrays have similar prefixes.
            int newBase = 0;
            for (int oldCur = 0; oldCur < oldLength; oldCur++) {
                bool found = false;
                for (int newCur = newBase; newCur < newLength; newCur++) {
                    if (oldArray[oldCur] == newArray[newCur]) {
                        if (newBase == newCur) {
                            newBase++;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // This means it's in oldArray and not newArray.  Remove it.
                    if (oldArray[oldCur].Table == table) {
                        BaseRemove(oldArray[oldCur]);
                        _list.Remove(oldArray[oldCur]);
                        oldArray[oldCur].SetOrdinalInternal(-1);
                    }
                }
            }

            // Now, let's pass through news and those that don't belong, add them.
            for (int newCur = 0; newCur < newLength; newCur++) {
                if (newArray[newCur].Table != table) {
                    BaseAdd(newArray[newCur]);
                    _list.Add(newArray[newCur]);
                }
                newArray[newCur].SetOrdinalInternal(newCur);
            }
        }

        /// <devdoc>
        /// Does verification on the column and it's name, and clears the column's dataSet pointer.
        /// An ArgumentNullException is thrown if this column is null.  An ArgumentException is thrown
        /// if this column doesn't belong to this collection or if this column is part of a relationship.
        /// An ArgumentException is thrown if another column's compute expression depends on this column.
        /// </devdoc>
        private void BaseRemove(DataColumn column) {
            if (CanRemove(column, true)) {

                // remove
                if (column.errors > 0) {
                    for (int i = 0; i < table.Rows.Count; i++) {
                        table.Rows[i].ClearError(column);
                    }
                }
                UnregisterName(column.ColumnName);
                column.SetTable(null);
            }
        }

        /// <devdoc>
        ///    <para>Checks
        ///       if
        ///       a given column can be removed from the collection.</para>
        /// </devdoc>
        public bool CanRemove(DataColumn column) {
            return CanRemove(column, false);
        }

        internal bool CanRemove(DataColumn column, bool fThrowException) {
            if (column == null) {
                if (!fThrowException)
                    return false;
                else
                throw ExceptionBuilder.ArgumentNull("column");
            }
            if (column.table != table) {
                if (!fThrowException)
                    return false;
                else
                throw ExceptionBuilder.CannotRemoveColumn();
            }

            // allow subclasses to complain first.
            table.OnRemoveColumnInternal(column);

            // We need to make sure the column is not involved in any Relations or Constriants
            if (table.primaryKey != null && table.primaryKey.Key.ContainsColumn(column)) {
                if (!fThrowException)
                    return false;
                else
                throw ExceptionBuilder.CannotRemovePrimaryKey();
            }
            for (int i = 0; i < table.ParentRelations.Count; i++) {
                if (table.ParentRelations[i].ChildKey.ContainsColumn(column)) {
                    if (!fThrowException)
                        return false;
                    else
                    throw ExceptionBuilder.CannotRemoveChildKey(table.ParentRelations[i].RelationName);
                }
            }
            for (int i = 0; i < table.ChildRelations.Count; i++) {
                if (table.ChildRelations[i].ParentKey.ContainsColumn(column)) {
                    if (!fThrowException)
                        return false;
                    else
                    throw ExceptionBuilder.CannotRemoveChildKey(table.ChildRelations[i].RelationName);
                }
            }
            for (int i = 0; i < table.Constraints.Count; i++) {
                if (table.Constraints[i].ContainsColumn(column))
                    if (!fThrowException)
                        return false;
                    else
                    throw ExceptionBuilder.CannotRemoveConstraint(table.Constraints[i].ConstraintName, table.Constraints[i].Table.TableName);
            }
            if (table.DataSet != null) {
                for (ParentForeignKeyConstraintEnumerator en = new ParentForeignKeyConstraintEnumerator(table.DataSet, table); en.GetNext();) {
                    Constraint constraint = en.GetConstraint();
                    if (((ForeignKeyConstraint)constraint).ParentKey.ContainsColumn(column))
                        if (!fThrowException)
                            return false;
                        else
                            throw ExceptionBuilder.CannotRemoveConstraint(constraint.ConstraintName, constraint.Table.TableName);
                }
            }

            if (column.dependentColumns != null) {
                for (int i = 0; i < column.dependentColumns.Count; i++) {
                    DataColumn col = column.dependentColumns[i];
                    if (fInClear && (col.Table == table || col.Table == null))
                        continue;
                    if (col.Table == null)
                        continue;
                    Debug.Assert(col.Computed, "invalid (non an expression) column in the expression dependent columns");
                    DataExpression expr = col.DataExpression;
                    if ((expr!= null) && (expr.DependsOn(column))) {
                        if (!fThrowException)
                            return false;
                        else
                            throw ExceptionBuilder.CannotRemoveExpression(col.ColumnName, col.Expression);
                    }
                }
            }

            // SQLBU 429176: you can't remove a column participating in an index,
            // while index events are suspended else the indexes won't be properly maintained.
            // However, all the above checks should catch those participating columns.
            // except when a column is in a DataView RowFilter or Sort clause
            foreach (Index index in table.LiveIndexes) {
#if false
                if (!Object.ReferenceEquals(index, column.sortIndex)) {
                    foreach (IndexField field in index.IndexFields) {
                        if (Object.ReferenceEquals(field.Column, column)) {
                            if (fThrowException) {
                                throw ExceptionBuilder.CannotRemoveExpression("DataView", column.ColumnName);
                            }
                            return false;
                        }
                    }
                }
#endif
            }

            return true;
        }

        private void CheckIChangeTracking(DataColumn column) {
            if (column.ImplementsIRevertibleChangeTracking) {
                nColumnsImplementingIRevertibleChangeTracking++;
                nColumnsImplementingIChangeTracking++;
                AddColumnsImplementingIChangeTrackingList(column);
            }
            else if (column.ImplementsIChangeTracking) {
                nColumnsImplementingIChangeTracking++;
                AddColumnsImplementingIChangeTrackingList(column);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Clears the collection of any columns.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            int oldLength = _list.Count;

            DataColumn[] columns = new DataColumn[_list.Count];
            _list.CopyTo(columns, 0);

            OnCollectionChanging(RefreshEventArgs);

            if (table.fInitInProgress && delayedAddRangeColumns != null) {
                delayedAddRangeColumns = null;
            }

            try {
                // this will smartly add and remove the appropriate tables.
                fInClear = true;
                BaseGroupSwitch(columns, oldLength, null, 0);
                fInClear = false;
            }
            catch (Exception e) {
                // 
                if (ADP.IsCatchableOrSecurityExceptionType(e)) {
                    // something messed up: restore to old values and throw
                    fInClear = false;
                    BaseGroupSwitch(null, 0, columns, oldLength);
                    _list.Clear();
                    for (int i = 0; i < oldLength; i++)
                        _list.Add(columns[i]);
                }
                throw;
            }
            _list.Clear();
            table.ElementColumnCount  = 0;
            OnCollectionChanged(RefreshEventArgs);
        }

        /// <devdoc>
        ///    <para>Checks whether the collection contains a column with the specified name.</para>
        /// </devdoc>
        public bool Contains(string name) {
            DataColumn column;
            if ((columnFromName.TryGetValue(name, out column)) && (column != null)) {
                return true;
            }

            return (IndexOfCaseInsensitive(name) >= 0);
        }

        internal bool Contains(string name, bool caseSensitive) {
            DataColumn column;
            if ((columnFromName.TryGetValue(name, out column)) && (column != null)) {
                return true;
            }

            if (caseSensitive) { // above check did case sensitive check
                return false;
            }
            else {
                return (IndexOfCaseInsensitive(name) >= 0);
            }
        }

        public void CopyTo(DataColumn[] array, int index) {
            if (array==null)
                throw ExceptionBuilder.ArgumentNull("array");
            if (index < 0)
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            if (array.Length - index < _list.Count)
                throw ExceptionBuilder.InvalidOffsetLength();
            for(int i = 0; i < _list.Count; ++i) {
                array[index + i] = (DataColumn)_list[i];
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the index of a specified <see cref='System.Data.DataColumn'/>.
        ///    </para>
        /// </devdoc>
        public int IndexOf(DataColumn column) {
            int columnCount = _list.Count;
            for (int i = 0; i < columnCount; ++i) {
                if (column == (DataColumn) _list[i]) {
                    return i;
                }
            }
            return -1;
        }

        /// <devdoc>
        ///    <para>Returns the index of
        ///       a column specified by name.</para>
        /// </devdoc>
        public int IndexOf(string columnName) {

            if ((null != columnName) && (0 < columnName.Length)) {
                int count = Count;
                DataColumn column;
                if ((columnFromName.TryGetValue(columnName, out column)) && (column != null)) {
                    for (int j = 0; j < count; j++)
                        if (column == _list[j]) {
                            return j;
                    }
                }
                else {
                    int res = IndexOfCaseInsensitive(columnName);
                    return (res < 0) ? -1 : res;
                }
            }
            return -1;
        }

        internal int IndexOfCaseInsensitive (string name) {
            int hashcode = table.GetSpecialHashCode(name);
            int cachedI = -1;
            DataColumn column = null;
            for (int i = 0; i < Count; i++) {
                column = (DataColumn) _list[i];
                if ( (hashcode == 0 || column._hashCode == 0 || column._hashCode == hashcode) &&
                   NamesEqual(column.ColumnName, name, false, table.Locale) != 0 ) {
                    if (cachedI == -1)
                        cachedI = i;
                    else
                        return -2;
                }
            }
            return cachedI;
        }

        internal void FinishInitCollection() {
            if (delayedAddRangeColumns != null) {
                foreach(DataColumn column in delayedAddRangeColumns) {
                    if (column != null) {
                        Add(column);
                    }
                }

                foreach(DataColumn column in delayedAddRangeColumns) {
                    if (column != null) {
                        column.FinishInitInProgress();
                    }
                }

                delayedAddRangeColumns = null;
            }
        }

        /// <devdoc>
        /// Makes a default name with the given index.  e.g. Column1, Column2, ... Columni
        /// </devdoc>
        private string MakeName(int index) {
            if (1 == index) {
                return "Column1";
            }
            return "Column" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        internal void MoveTo(DataColumn column, int newPosition) {
            if (0 > newPosition || newPosition > Count -1) {
                throw ExceptionBuilder.InvalidOrdinal("ordinal", newPosition);
            }
            if (column.ImplementsIChangeTracking) {
                RemoveColumnsImplementingIChangeTrackingList(column);
            }
            _list.Remove(column);
            _list.Insert(newPosition, column);
            int count = _list.Count;
            for (int i =0; i < count; i++) {
                ((DataColumn) _list[i]).SetOrdinalInternal(i);
            }
            CheckIChangeTracking(column);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, column));
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataColumnCollection.OnCollectionChanged'/> event.
        ///    </para>
        /// </devdoc>
        private void OnCollectionChanged(CollectionChangeEventArgs ccevent) {
            table.UpdatePropertyDescriptorCollectionCache();

            if ((null != ccevent) && !table.SchemaLoading && !table.fInitInProgress) {
                DataColumn column = (DataColumn)ccevent.Element;
            }
            if (onCollectionChangedDelegate != null) {
                onCollectionChangedDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnCollectionChanging(CollectionChangeEventArgs ccevent) {
            if (onCollectionChangingDelegate != null) {
                onCollectionChangingDelegate(this, ccevent);
            }
        }

        internal void OnColumnPropertyChanged(CollectionChangeEventArgs ccevent) {
            table.UpdatePropertyDescriptorCollectionCache();
            if (onColumnPropertyChangedDelegate != null) {
                onColumnPropertyChangedDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        /// Registers this name as being used in the collection.  Will throw an ArgumentException
        /// if the name is already being used.  Called by Add, All property, and Column.ColumnName property.
        /// if the name is equivalent to the next default name to hand out, we increment our defaultNameIndex.
        /// NOTE: To add a child table, pass column as null
        /// </devdoc>
        internal void RegisterColumnName(string name, DataColumn column) {
            Debug.Assert (name != null);

            try {
                columnFromName.Add(name, column);

                if (null != column) {
                    column._hashCode = table.GetSpecialHashCode(name);
                }
            }
            catch (ArgumentException) { // Argument exception means that there is already an existing key
                if (columnFromName[name] != null) {
                    if (column != null) {
                        throw ExceptionBuilder.CannotAddDuplicate(name);
                    }
                    else {
                        throw ExceptionBuilder.CannotAddDuplicate3(name);
                    }
                }
                throw ExceptionBuilder.CannotAddDuplicate2(name);
            }

            // If we're adding a child table, then update defaultNameIndex to avoid colisions between the child table and auto-generated column names
            if ((column == null) && NamesEqual(name, MakeName(defaultNameIndex), true, table.Locale) != 0) {
                do {
                    defaultNameIndex++;
                } while (Contains(MakeName(defaultNameIndex)));
            }
        }

        internal bool CanRegisterName(string name) {
            Debug.Assert (name != null, "Must specify a name");
            return (!columnFromName.ContainsKey(name));
        }

        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Data.DataColumn'/>
        /// from the collection.</para>
        /// </devdoc>
        public void Remove(DataColumn column) {
            OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
            BaseRemove(column);
            ArrayRemove(column);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
            // if the column is an element decrease the internal dataTable counter
            if (column.ColumnMapping == MappingType.Element)
                table.ElementColumnCount --;
        }

        /// <devdoc>
        ///    <para>Removes the
        ///       column at the specified index from the collection.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            DataColumn dc = this[index];
            if (dc == null)
                throw ExceptionBuilder.ColumnOutOfRange(index);
            Remove(dc);
        }

        /// <devdoc>
        ///    <para>Removes the
        ///       column with the specified name from the collection.</para>
        /// </devdoc>
        public void Remove(string name) {
            DataColumn dc = this[name];
            if (dc == null)
                throw ExceptionBuilder.ColumnNotInTheTable(name, table.TableName);
            Remove(dc);
        }

        /// <devdoc>
        /// Unregisters this name as no longer being used in the collection.  Called by Remove, All property, and
        /// Column.ColumnName property.  If the name is equivalent to the last proposed default name, we walk backwards
        /// to find the next proper default name to use.
        /// </devdoc>
        internal void UnregisterName(string name) {
            columnFromName.Remove(name);

            if (NamesEqual(name, MakeName(defaultNameIndex - 1), true, table.Locale) != 0) {
                do {
                    defaultNameIndex--;
                } while (defaultNameIndex > 1 &&
                         !Contains(MakeName(defaultNameIndex - 1)));
            }
        }
                
        private void AddColumnsImplementingIChangeTrackingList(DataColumn dataColumn) {
            DataColumn[] columns = columnsImplementingIChangeTracking;
            DataColumn[] tempColumns = new DataColumn[columns.Length +1];
            columns.CopyTo(tempColumns, 0);
            tempColumns[columns.Length] = dataColumn;
            columnsImplementingIChangeTracking = tempColumns;
        }

        private void RemoveColumnsImplementingIChangeTrackingList(DataColumn dataColumn) {
            DataColumn[] columns = columnsImplementingIChangeTracking;
            DataColumn[] tempColumns = new DataColumn[columns.Length - 1];
            for(int i = 0, j = 0; i < columns.Length; i++) {
                if (columns[i] != dataColumn) {
                    tempColumns[j++] = columns[i];
                }
            }
            columnsImplementingIChangeTracking = tempColumns;
        }
    }
}
