//------------------------------------------------------------------------------
// <copyright file="DataTableCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    /// <devdoc>
    ///    <para>
    ///       Represents the collection of tables for the <see cref='System.Data.DataSet'/>.
    ///    </para>
    /// </devdoc>
    [
    DefaultEvent("CollectionChanged"),
    Editor("Microsoft.VSDesigner.Data.Design.TablesCollectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ListBindable(false),
    ]
    public sealed class DataTableCollection : InternalDataCollectionBase {

        private readonly DataSet dataSet      = null;
        // private DataTable[] tables   = new DataTable[2];
        // private int tableCount       = 0;
        private readonly ArrayList _list = new ArrayList();
        private int defaultNameIndex = 1;
        private DataTable[] delayedAddRangeTables = null;

        private CollectionChangeEventHandler onCollectionChangedDelegate = null;
        private CollectionChangeEventHandler onCollectionChangingDelegate = null;

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        /// <devdoc>
        /// DataTableCollection constructor.  Used only by DataSet.
        /// </devdoc>
        internal DataTableCollection(DataSet dataSet) {
            Bid.Trace("<ds.DataTableCollection.DataTableCollection|INFO> %d#, dataSet=%d\n", ObjectID, (dataSet != null) ? dataSet.ObjectID : 0);
            this.dataSet = dataSet;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the tables
        ///       in the collection as an object.
        ///    </para>
        /// </devdoc>
        protected override ArrayList List {
            get {
                return _list;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        /// <devdoc>
        ///    <para>Gets the table specified by its index.</para>
        /// </devdoc>
        public DataTable this[int index] {
            get {
                try { // Perf: use the readonly _list field directly and let ArrayList check the range
                    return(DataTable) _list[index];
                }
                catch(ArgumentOutOfRangeException) {
                    throw ExceptionBuilder.TableOutOfRange(index);
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the table in the collection with the given name (not case-sensitive).</para>
        /// </devdoc>
        public DataTable this[string name] {
            get {
                int index = InternalIndexOf(name);
                if (index == -2) {
                    throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                }
                if (index == -3) {
                    throw ExceptionBuilder.NamespaceNameConflict(name);
                }
                return (index < 0) ? null : (DataTable)_list[index];
            }
        }

        public DataTable this[string name, string tableNamespace] {
            get {
                if (tableNamespace == null)
                    throw ExceptionBuilder.ArgumentNull("tableNamespace");
                int index = InternalIndexOf(name, tableNamespace);
                if (index == -2) {
                    throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                }
                return (index < 0) ? null : (DataTable)_list[index];
            }
        }

        // Case-sensitive search in Schema, data and diffgram loading
        internal DataTable GetTable(string name, string ns)
        {
            for (int i = 0; i < _list.Count; i++) {
                DataTable   table = (DataTable) _list[i];
                if (table.TableName == name && table.Namespace == ns)
                    return table;
            }
            return null;
        }

        // Case-sensitive smart search: it will look for a table using the ns only if required to
        // resolve a conflict
        internal DataTable GetTableSmart(string name, string ns){
            int fCount = 0;
            DataTable fTable = null;
            for (int i = 0; i < _list.Count; i++) {
                DataTable   table = (DataTable) _list[i];
                if (table.TableName == name) {
                    if (table.Namespace == ns)
                        return table;
                    fCount++;
                    fTable = table;
                }
            }
            // if we get here we didn't match the namespace
            // so return the table only if fCount==1 (it's the only one)
            return (fCount == 1) ? fTable : null;
        }
        /// <devdoc>
        ///    <para>
        ///       Adds
        ///       the specified table to the collection.
        ///    </para>
        /// </devdoc>
        public void Add(DataTable table) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.Add|API> %d#, table=%d\n", ObjectID, (table!= null) ?  table.ObjectID : 0);
            try {
                OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
                BaseAdd(table);
                ArrayAdd(table);

                if (table.SetLocaleValue(dataSet.Locale, false, false) || 
                    table.SetCaseSensitiveValue(dataSet.CaseSensitive, false, false)) {
                    table.ResetIndexes();
                }
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(DataTable[] tables) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.AddRange|API> %d#\n", ObjectID);
            try {
                if (dataSet.fInitInProgress) {
                    delayedAddRangeTables = tables;
                    return;
                }

                if (tables != null) {
                    foreach(DataTable table in tables) {
                        if (table != null) {
                            Add(table);
                        }
                    }
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a table with the given name and adds it to the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public DataTable Add(string name) {
            DataTable table = new DataTable(name);
            // fxcop: new DataTable should inherit the CaseSensitive, Locale, Namespace from DataSet
            Add(table);
            return table;
        }

        public DataTable Add(string name, string tableNamespace) {
            DataTable table = new DataTable(name, tableNamespace);
            // fxcop: new DataTable should inherit the CaseSensitive, Locale from DataSet
            Add(table);
            return table;
        }
        /// <devdoc>
        ///    <para>
        ///       Creates a new table with a default name and adds it to
        ///       the collection.
        ///    </para>
        /// </devdoc>
        public DataTable Add() {
            DataTable table = new DataTable();
            // fxcop: new DataTable should inherit the CaseSensitive, Locale, Namespace from DataSet
            Add(table);
            return table;
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when the collection is changed.
        ///    </para>
        /// </devdoc>
        [ResDescriptionAttribute(Res.collectionChangedEventDescr)]
        public event CollectionChangeEventHandler CollectionChanged {
            add {
                Bid.Trace("<ds.DataTableCollection.add_CollectionChanged|API> %d#\n", ObjectID);
                onCollectionChangedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTableCollection.remove_CollectionChanged|API> %d#\n", ObjectID);
                onCollectionChangedDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public event CollectionChangeEventHandler CollectionChanging {
            add {
                Bid.Trace("<ds.DataTableCollection.add_CollectionChanging|API> %d#\n", ObjectID);
                onCollectionChangingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTableCollection.remove_CollectionChanging|API> %d#\n", ObjectID);
                onCollectionChangingDelegate -= value;
            }
        }

        /// <devdoc>
        ///  Adds the table to the tables array.
        /// </devdoc>
        private void ArrayAdd(DataTable table) {
            _list.Add(table);
        }

        /// <devdoc>
        /// Creates a new default name.
        /// </devdoc>
        internal string AssignName() {
            string newName = null;
            // RAIDBUG: 91671
            while(this.Contains( newName = MakeName(defaultNameIndex)))
                defaultNameIndex++;
            return newName;
        }

        /// <devdoc>
        /// Does verification on the table and it's name, and points the table at the dataSet that owns this collection.
        /// An ArgumentNullException is thrown if this table is null.  An ArgumentException is thrown if this table
        /// already belongs to this collection, belongs to another collection.
        /// A DuplicateNameException is thrown if this collection already has a table with the same
        /// name (case insensitive).
        /// </devdoc>
        private void BaseAdd(DataTable table) {
            if (table == null)
                throw ExceptionBuilder.ArgumentNull("table");
            if (table.DataSet == dataSet)
                throw ExceptionBuilder.TableAlreadyInTheDataSet();
            if (table.DataSet != null)
                throw ExceptionBuilder.TableAlreadyInOtherDataSet();

            if (table.TableName.Length == 0)
                table.TableName = AssignName();
            else {
                if (NamesEqual(table.TableName, dataSet.DataSetName, false, dataSet.Locale) != 0 && !table.fNestedInDataset)
                   throw ExceptionBuilder.DatasetConflictingName(dataSet.DataSetName);
                RegisterName(table.TableName, table.Namespace);
            }

            table.SetDataSet(dataSet);

            //must run thru the document incorporating the addition of this data table
            //must make sure there is no other schema component which have the same
            // identity as this table (for example, there must not be a table with the
            // same identity as a column in this schema.
        }

        /// <devdoc>
        /// BaseGroupSwitch will intelligently remove and add tables from the collection.
        /// </devdoc>
        private void BaseGroupSwitch(DataTable[] oldArray, int oldLength, DataTable[] newArray, int newLength) {
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
                    if (oldArray[oldCur].DataSet == dataSet) {
                        BaseRemove(oldArray[oldCur]);
                    }
                }
            }

            // Now, let's pass through news and those that don't belong, add them.
            for (int newCur = 0; newCur < newLength; newCur++) {
                if (newArray[newCur].DataSet != dataSet) {
                    BaseAdd(newArray[newCur]);
                    _list.Add(newArray[newCur]);
                }
            }
        }

        /// <devdoc>
        /// Does verification on the table and it's name, and clears the table's dataSet pointer.
        /// An ArgumentNullException is thrown if this table is null.  An ArgumentException is thrown
        /// if this table doesn't belong to this collection or if this table is part of a relationship.
        /// </devdoc>
        private void BaseRemove(DataTable table) {
            if (CanRemove(table, true)) {
                UnregisterName(table.TableName);
                table.SetDataSet(null);

            }
            _list.Remove(table);
            dataSet.OnRemovedTable(table);
        }

        /// <devdoc>
        ///    <para>
        ///       Verifies if a given table can be removed from the collection.
        ///    </para>
        /// </devdoc>
        public bool CanRemove(DataTable table) {
            return CanRemove(table, false);
        }

        internal bool CanRemove(DataTable table, bool fThrowException) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.CanRemove|INFO> %d#, table=%d, fThrowException=%d{bool}\n", ObjectID, (table != null)? table.ObjectID : 0 , fThrowException);
            try {
                if (table == null) {
                    if (!fThrowException)
                        return false;
                    else
                        throw ExceptionBuilder.ArgumentNull("table");
                }
                if (table.DataSet != dataSet) {
                    if (!fThrowException)
                        return false;
                    else
                        throw ExceptionBuilder.TableNotInTheDataSet(table.TableName);
                }

                // allow subclasses to throw.
                dataSet.OnRemoveTable(table);

                if (table.ChildRelations.Count != 0 || table.ParentRelations.Count != 0) {
                    if (!fThrowException)
                        return false;
                    else
                        throw ExceptionBuilder.TableInRelation();
                }

                for (ParentForeignKeyConstraintEnumerator constraints = new ParentForeignKeyConstraintEnumerator(dataSet, table); constraints.GetNext();) {
                    ForeignKeyConstraint constraint = constraints.GetForeignKeyConstraint();
                    if (constraint.Table == table && constraint.RelatedTable == table) // we can go with (constraint.Table ==  constraint.RelatedTable)
                        continue;
                    if (!fThrowException)
                        return false;
                    else
                        throw ExceptionBuilder.TableInConstraint(table, constraint);
                }

                for (ChildForeignKeyConstraintEnumerator constraints = new ChildForeignKeyConstraintEnumerator(dataSet, table); constraints.GetNext();) {
                    ForeignKeyConstraint constraint = constraints.GetForeignKeyConstraint();
                    if (constraint.Table == table && constraint.RelatedTable == table) // bug 97670
                        continue;

                    if (!fThrowException)
                        return false;
                    else
                        throw ExceptionBuilder.TableInConstraint(table, constraint);
                }

                return true;
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Clears the collection of any tables.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.Clear|API> %d#\n", ObjectID);
            try {
                int oldLength = _list.Count;
                DataTable[] tables = new DataTable[_list.Count];
                _list.CopyTo(tables, 0);

                OnCollectionChanging(RefreshEventArgs);

                if (dataSet.fInitInProgress && delayedAddRangeTables != null) {
                    delayedAddRangeTables = null;
                }

                BaseGroupSwitch(tables, oldLength, null, 0);
                _list.Clear();

                OnCollectionChanged(RefreshEventArgs);
                }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Checks if a table, specified by name, exists in the collection.
        ///    </para>
        /// </devdoc>
        public bool Contains(string name) {
            return (InternalIndexOf(name) >= 0);
        }

        public bool Contains(string name, string tableNamespace) {
            if (name == null)
                throw ExceptionBuilder.ArgumentNull("name");

            if (tableNamespace == null)
                throw ExceptionBuilder.ArgumentNull("tableNamespace");

            return (InternalIndexOf(name, tableNamespace) >= 0);
        }

        internal bool Contains(string name, string tableNamespace, bool checkProperty, bool caseSensitive) {
            if (!caseSensitive)
                return (InternalIndexOf(name) >= 0);

            // Case-Sensitive compare
            int count = _list.Count;
            for (int i = 0; i < count; i++) {
                DataTable table = (DataTable) _list[i];
                // this may be needed to check wether the cascading is creating some conflicts
                string ns = checkProperty ? table.Namespace : table.tableNamespace ;
                if (NamesEqual(table.TableName, name, true, dataSet.Locale) == 1 && (ns == tableNamespace))
                    return true;
            }
            return false;
        }

        internal bool Contains(string name, bool caseSensitive) {
            if (!caseSensitive)
                return (InternalIndexOf(name) >= 0);

            // Case-Sensitive compare
            int count = _list.Count;
            for (int i = 0; i < count; i++) {
                DataTable table = (DataTable) _list[i];
                if (NamesEqual(table.TableName, name, true, dataSet.Locale) == 1 )
                    return true;
            }
            return false;
        }

        public void CopyTo(DataTable[] array, int index) {
            if (array==null)
                throw ExceptionBuilder.ArgumentNull("array");
            if (index < 0)
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            if (array.Length - index < _list.Count)
                throw ExceptionBuilder.InvalidOffsetLength();
            for(int i = 0; i < _list.Count; ++i) {
                array[index + i] = (DataTable)_list[i];
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the index of a specified <see cref='System.Data.DataTable'/>.
        ///    </para>
        /// </devdoc>
        public int IndexOf(DataTable table) {
            int tableCount = _list.Count;
            for (int i = 0; i < tableCount; ++i) {
                if (table == (DataTable) _list[i]) {
                    return i;
                }
            }
            return -1;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the index of the
        ///       table with the given name (case insensitive), or -1 if the table
        ///       doesn't exist in the collection.
        ///    </para>
        /// </devdoc>
        public int IndexOf(string tableName) {
            int index = InternalIndexOf(tableName);
            return (index < 0) ? -1 : index;
        }

        public int IndexOf(string tableName, string tableNamespace) {
            return IndexOf( tableName, tableNamespace, true);
        }

        internal int IndexOf(string tableName, string tableNamespace, bool chekforNull) { // this should be public! why it is missing?
            if (chekforNull) {
                if (tableName == null)
                    throw ExceptionBuilder.ArgumentNull("tableName");
                if (tableNamespace == null)
                    throw ExceptionBuilder.ArgumentNull("tableNamespace");
            }
            int index = InternalIndexOf(tableName, tableNamespace);
            return (index < 0) ? -1 : index;
        }

        internal void ReplaceFromInference(System.Collections.Generic.List<DataTable> tableList) {
            Debug.Assert(_list.Count == tableList.Count, "Both lists should have equal numbers of tables");
            _list.Clear();
            _list.AddRange(tableList);            
        }

        // Return value:
        //      >= 0: find the match
        //        -1: No match
        //        -2: At least two matches with different cases
        //        -3: At least two matches with different namespaces
        internal int InternalIndexOf(string tableName) {
            int cachedI = -1;
            if ((null != tableName) && (0 < tableName.Length)) {
                int count = _list.Count;
                int result = 0;
                for (int i = 0; i < count; i++) {
                    DataTable table = (DataTable) _list[i];
                    result = NamesEqual(table.TableName, tableName, false, dataSet.Locale);
                    if (result == 1) {
                        // ok, we have found a table with the same name.
                        // let's see if there are any others with the same name
                        // if any let's return (-3) otherwise...
                        for (int j=i+1;j<count;j++) {
                            DataTable dupTable = (DataTable) _list[j];
                            if (NamesEqual(dupTable.TableName, tableName, false, dataSet.Locale) == 1)
                                return -3;
                        }
                       //... let's just return i
                        return i;
                    }

                    if (result == -1)
                        cachedI = (cachedI == -1) ? i : -2;
                }
            }
            return cachedI;
        }

        // Return value:
        //      >= 0: find the match
        //        -1: No match
        //        -2: At least two matches with different cases
        internal int InternalIndexOf(string tableName, string tableNamespace) {
            int cachedI = -1;
            if ((null != tableName) && (0 < tableName.Length)) {
                int count = _list.Count;
                int result = 0;
                for (int i = 0; i < count; i++) {
                    DataTable table = (DataTable) _list[i];
                    result = NamesEqual(table.TableName, tableName, false, dataSet.Locale);
                    if ((result == 1) && (table.Namespace == tableNamespace))
                        return i;

                    if ((result == -1)  && (table.Namespace == tableNamespace))
                        cachedI = (cachedI == -1) ? i : -2;
                }
            }
            return cachedI;

        }

        internal void FinishInitCollection() {
            if (delayedAddRangeTables != null) {
                foreach(DataTable table in delayedAddRangeTables) {
                    if (table != null) {
                        Add(table);
                    }
                }
                delayedAddRangeTables = null;
            }
        }

        /// <devdoc>
        /// Makes a default name with the given index.  e.g. Table1, Table2, ... Tablei
        /// </devdoc>
        private string MakeName(int index) {
            if (1 == index) {
                return "Table1";
            }
            return "Table" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataTableCollection.OnCollectionChanged'/> event.
        ///    </para>
        /// </devdoc>
        private void OnCollectionChanged(CollectionChangeEventArgs ccevent) {
            if (onCollectionChangedDelegate != null) {
                Bid.Trace("<ds.DataTableCollection.OnCollectionChanged|INFO> %d#\n", ObjectID);
                onCollectionChangedDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnCollectionChanging(CollectionChangeEventArgs ccevent) {
            if (onCollectionChangingDelegate != null) {
                Bid.Trace("<ds.DataTableCollection.OnCollectionChanging|INFO> %d#\n", ObjectID);
                onCollectionChangingDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        /// Registers this name as being used in the collection.  Will throw an ArgumentException
        /// if the name is already being used.  Called by Add, All property, and Table.TableName property.
        /// if the name is equivalent to the next default name to hand out, we increment our defaultNameIndex.
        /// </devdoc>
        internal void RegisterName(string name, string tbNamespace) {
            Bid.Trace("<ds.DataTableCollection.RegisterName|INFO> %d#, name='%ls', tbNamespace='%ls'\n", ObjectID, name, tbNamespace);
            Debug.Assert (name != null);

            CultureInfo locale = dataSet.Locale;
            int tableCount = _list.Count;
            for (int i = 0; i < tableCount; i++) {
                DataTable table = (DataTable) _list[i];
                if (NamesEqual(name, table.TableName, true, locale) != 0 && (tbNamespace == table.Namespace)) {
                    throw ExceptionBuilder.DuplicateTableName(((DataTable) _list[i]).TableName);
                }
            }
            if (NamesEqual(name, MakeName(defaultNameIndex), true, locale) != 0) {
                defaultNameIndex++;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the specified table from the collection.
        ///    </para>
        /// </devdoc>
        public void Remove(DataTable table) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.Remove|API> %d#, table=%d\n", ObjectID, (table != null) ? table.ObjectID : 0);
            try {
                OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
                BaseRemove(table);
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the
        ///       table at the given index from the collection
        ///    </para>
        /// </devdoc>
        public void RemoveAt(int index) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.RemoveAt|API> %d#, index=%d\n", ObjectID, index);
            try {
                DataTable dt = this[index];
                if (dt == null)
                    throw ExceptionBuilder.TableOutOfRange(index);
                Remove(dt);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the table with a specified name from the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public void Remove(string name) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTableCollection.Remove|API> %d#, name='%ls'\n", ObjectID, name);
            try {
                DataTable dt = this[name];
                if (dt == null)
                    throw ExceptionBuilder.TableNotInTheDataSet(name);
                Remove(dt);
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        public void Remove(string name, string tableNamespace) {
            if (name == null)
                throw ExceptionBuilder.ArgumentNull("name");
            if (tableNamespace == null)
                throw ExceptionBuilder.ArgumentNull("tableNamespace");
            DataTable dt = this[name, tableNamespace];
            if (dt == null)
                throw ExceptionBuilder.TableNotInTheDataSet(name);
            Remove(dt);
        }


        /// <devdoc>
        /// Unregisters this name as no longer being used in the collection.  Called by Remove, All property, and
        /// Table.TableName property.  If the name is equivalent to the last proposed default name, we walk backwards
        /// to find the next proper default name to  use.
        /// </devdoc>
        internal void UnregisterName(string name) {
            Bid.Trace("<ds.DataTableCollection.UnregisterName|INFO> %d#, name='%ls'\n", ObjectID, name);
            if (NamesEqual(name, MakeName(defaultNameIndex - 1), true, dataSet.Locale) != 0) {
                do {
                    defaultNameIndex--;
                } while (defaultNameIndex > 1 &&
                         !Contains(MakeName(defaultNameIndex - 1)));
            }
        }
    }
}
