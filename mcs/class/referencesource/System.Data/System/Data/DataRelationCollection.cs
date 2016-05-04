//------------------------------------------------------------------------------
// <copyright file="DataRelationCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    /// <devdoc>
    ///    <para>
    ///       Represents the collection of relations,
    ///       each of which allows navigation between related parent/child tables.
    ///    </para>
    /// </devdoc>
    [
    DefaultEvent("CollectionChanged"),
    Editor("Microsoft.VSDesigner.Data.Design.DataRelationCollectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    DefaultProperty("Table"),
    ]
    public abstract class DataRelationCollection : InternalDataCollectionBase {

        private DataRelation inTransition = null;

        private int defaultNameIndex = 1;

        private CollectionChangeEventHandler onCollectionChangedDelegate;
        private CollectionChangeEventHandler onCollectionChangingDelegate;

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        /// <devdoc>
        ///    <para>Gets the relation specified by index.</para>
        /// </devdoc>
        public abstract DataRelation this[int index] {
            get;
        }

        /// <devdoc>
        ///    <para>Gets the relation specified by name.</para>
        /// </devdoc>
        public abstract DataRelation this[string name] {
            get;
        }

        /// <devdoc>
        ///    <para>
        ///       Adds the relation to the collection.</para>
        /// </devdoc>
        public void Add(DataRelation relation) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRelationCollection.Add|API> %d#, relation=%d\n", ObjectID, (relation != null) ? relation.ObjectID : 0);
            try {
                if (inTransition == relation)
                    return;
                inTransition = relation;
                try {
                    OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
                    AddCore(relation);
                    OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
                }
                finally {
                    inTransition = null;
                }                
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void AddRange(DataRelation[] relations) {
            if (relations != null) {
                foreach(DataRelation relation in relations) {
                    if (relation != null) {
                        Add(relation);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a <see cref='System.Data.DataRelation'/> with the
        ///       specified name, parent columns,
        ///       child columns, and adds it to the collection.</para>
        /// </devdoc>
        public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns) {
            DataRelation relation = new DataRelation(name, parentColumns, childColumns);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Creates a relation given the parameters and adds it to the collection.  An ArgumentNullException is
        /// thrown if this relation is null.  An ArgumentException is thrown if this relation already belongs to
        /// this collection, belongs to another collection, or if this collection already has a relation with the
        /// same name (case insensitive).
        /// An InvalidRelationException is thrown if the relation can't be created based on the parameters.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints) {
            DataRelation relation = new DataRelation(name, parentColumns, childColumns, createConstraints);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Creates a relation given the parameters and adds it to the collection.  The name is defaulted.
        /// An ArgumentException is thrown if
        /// this relation already belongs to this collection or belongs to another collection.
        /// An InvalidConstraintException is thrown if the relation can't be created based on the parameters.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns) {
            DataRelation relation = new DataRelation(null, parentColumns, childColumns);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Creates a relation given the parameters and adds it to the collection.
        /// An ArgumentException is thrown if this relation already belongs to
        /// this collection or belongs to another collection.
        /// A DuplicateNameException is thrown if this collection already has a relation with the same
        /// name (case insensitive).
        /// An InvalidConstraintException is thrown if the relation can't be created based on the parameters.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn) {
            DataRelation relation = new DataRelation(name, parentColumn, childColumn);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Creates a relation given the parameters and adds it to the collection.
        /// An ArgumentException is thrown if this relation already belongs to
        /// this collection or belongs to another collection.
        /// A DuplicateNameException is thrown if this collection already has a relation with the same
        /// name (case insensitive).
        /// An InvalidConstraintException is thrown if the relation can't be created based on the parameters.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn, bool createConstraints) {
            DataRelation relation = new DataRelation(name, parentColumn, childColumn, createConstraints);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Creates a relation given the parameters and adds it to the collection.  The name is defaulted.
        /// An ArgumentException is thrown if
        /// this relation already belongs to this collection or belongs to another collection.
        /// An InvalidConstraintException is thrown if the relation can't be created based on the parameters.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public virtual DataRelation Add(DataColumn parentColumn, DataColumn childColumn) {
            DataRelation relation = new DataRelation(null, parentColumn, childColumn);
            Add(relation);
            return relation;
        }

        /// <devdoc>
        /// Does verification on the table.
        /// An ArgumentNullException is thrown if this relation is null.  An ArgumentException is thrown if this relation
        ///  already belongs to this collection, belongs to another collection.
        /// A DuplicateNameException is thrown if this collection already has a relation with the same
        /// name (case insensitive).
        /// </devdoc>
        protected virtual void AddCore(DataRelation relation) {
            Bid.Trace("<ds.DataRelationCollection.AddCore|INFO> %d#, relation=%d\n", ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (relation == null)
                throw ExceptionBuilder.ArgumentNull("relation");
            relation.CheckState();
            DataSet dataSet = GetDataSet();
            if (relation.DataSet == dataSet)
                throw ExceptionBuilder.RelationAlreadyInTheDataSet();
            if (relation.DataSet != null)
                throw ExceptionBuilder.RelationAlreadyInOtherDataSet();
            if (relation.ChildTable.Locale.LCID != relation.ParentTable.Locale.LCID ||
                relation.ChildTable.CaseSensitive != relation.ParentTable.CaseSensitive)
                throw ExceptionBuilder.CaseLocaleMismatch();
            if (relation.Nested) {
                relation.CheckNamespaceValidityForNestedRelations(relation.ParentTable.Namespace);
                relation.ValidateMultipleNestedRelations();
                relation.ParentTable.ElementColumnCount++;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResDescriptionAttribute(Res.collectionChangedEventDescr)]
        public event CollectionChangeEventHandler CollectionChanged {
            add {
                Bid.Trace("<ds.DataRelationCollection.add_CollectionChanged|API> %d#\n", ObjectID);
                onCollectionChangedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataRelationCollection.remove_CollectionChanged|API> %d#\n", ObjectID);
                onCollectionChangedDelegate -= value;
            }
        }

        internal event CollectionChangeEventHandler CollectionChanging {
            add {
                Bid.Trace("<ds.DataRelationCollection.add_CollectionChanging|INFO> %d#\n", ObjectID);
                onCollectionChangingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataRelationCollection.remove_CollectionChanging|INFO> %d#\n", ObjectID);
                onCollectionChangingDelegate -= value;
            }
        }

        /// <devdoc>
        /// Creates a new default name.
        /// </devdoc>
        internal string AssignName() {
            string newName = MakeName(defaultNameIndex);
            defaultNameIndex++;
            return newName;
        }

        /// <devdoc>
        /// Clears the collection of any relations.
        /// </devdoc>
        public virtual void Clear() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRelationCollection.Clear|API> %d#\n", ObjectID);
            try {
                int count = Count;
                OnCollectionChanging(RefreshEventArgs);
                for (int i = count - 1; i >= 0; i--) {
                    inTransition = this[i];
                    RemoveCore(inTransition); // [....] : No need to go for try catch here and this will surely not throw any exception
                }
                OnCollectionChanged(RefreshEventArgs);
                inTransition = null;
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///  Returns true if this collection has a relation with the given name (case insensitive), false otherwise.
        /// </devdoc>
        public virtual bool Contains(string name) {
            return(InternalIndexOf(name) >= 0);
        }

        public void CopyTo(DataRelation[] array, int index) {
            if (array==null)
                throw ExceptionBuilder.ArgumentNull("array");
            if (index < 0)
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            ArrayList alist = List;
            if (array.Length - index < alist.Count)
                throw ExceptionBuilder.InvalidOffsetLength();
            for(int i = 0; i < alist.Count; ++i) {
                array[index + i] = (DataRelation)alist[i];
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the index of a specified <see cref='System.Data.DataRelation'/>.
        ///    </para>
        /// </devdoc>
        public virtual int IndexOf(DataRelation relation) {
            int relationCount = List.Count;
            for (int i = 0; i < relationCount; ++i) {
                if (relation == (DataRelation) List[i]) {
                    return i;
                }
            }
            return -1;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the index of the
        ///       relation with the given name (case insensitive), or -1 if the relation
        ///       doesn't exist in the collection.
        ///    </para>
        /// </devdoc>
        public virtual int IndexOf(string relationName) {
            int index = InternalIndexOf(relationName);
            return (index < 0) ? -1 : index;
        }

        internal int InternalIndexOf(string name) {
            int cachedI = -1;
            if ((null != name) && (0 < name.Length)) {
                int count = List.Count;
                int result = 0;
                for (int i = 0; i < count; i++) {
                    DataRelation relation = (DataRelation) List[i];
                    result = NamesEqual(relation.RelationName, name, false, GetDataSet().Locale);
                    if (result == 1)
                        return i;

                    if (result == -1)
                        cachedI = (cachedI == -1) ? i : -2;
                }
            }
            return cachedI;
        }

        /// <devdoc>
        /// Gets the dataSet for this collection.
        /// </devdoc>
        protected abstract DataSet GetDataSet();

        /// <devdoc>
        /// Makes a default name with the given index.  e.g. Relation1, Relation2, ... Relationi
        /// </devdoc>
        private string MakeName(int index) {
            if (1 == index) {
                return "Relation1";
            }
            return "Relation" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <devdoc>
        /// This method is called whenever the collection changes.  Overriders
        /// of this method should call the base implementation of this method.
        /// </devdoc>
        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent) {
            if (onCollectionChangedDelegate != null) {
                Bid.Trace("<ds.DataRelationCollection.OnCollectionChanged|INFO> %d#\n", ObjectID);
                onCollectionChangedDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void OnCollectionChanging(CollectionChangeEventArgs ccevent) {
            if (onCollectionChangingDelegate != null) {
                Bid.Trace("<ds.DataRelationCollection.OnCollectionChanging|INFO> %d#\n", ObjectID);
                onCollectionChangingDelegate(this, ccevent);
            }
        }

        /// <devdoc>
        /// Registers this name as being used in the collection.  Will throw an ArgumentException
        /// if the name is already being used.  Called by Add, All property, and Relation.RelationName property.
        /// if the name is equivalent to the next default name to hand out, we increment our defaultNameIndex.
        /// </devdoc>
        internal void RegisterName(string name) {
            Bid.Trace("<ds.DataRelationCollection.RegisterName|INFO> %d#, name='%ls'\n", ObjectID, name);
            Debug.Assert (name != null);

            CultureInfo locale = GetDataSet().Locale;
            int relationCount = Count;
            for (int i = 0; i < relationCount; i++) {
                if (NamesEqual(name, this[i].RelationName, true, locale) != 0) {
                    throw ExceptionBuilder.DuplicateRelation(this[i].RelationName);
                }
            }
            if (NamesEqual(name, MakeName(defaultNameIndex), true, locale) != 0) {
                defaultNameIndex++;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Verifies if a given relation can be removed from the collection.
        ///    </para>
        /// </devdoc>
        public virtual bool CanRemove(DataRelation relation) {
            if (relation == null)
                return false;

            if (relation.DataSet != GetDataSet())
                return false;

            return true;
        }

        /// <devdoc>
        /// Removes the given relation from the collection.
        /// An ArgumentNullException is thrown if this relation is null.  An ArgumentException is thrown
        /// if this relation doesn't belong to this collection.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public void Remove(DataRelation relation) {
            Bid.Trace("<ds.DataRelationCollection.Remove|API> %d#, relation=%d\n", ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (inTransition == relation)
                return;
            inTransition = relation;
            try {
                OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
                RemoveCore(relation);
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
            }
            finally {
               inTransition = null;
            } 
        }

        /// <devdoc>
        /// Removes the relation at the given index from the collection.  An IndexOutOfRangeException is
        /// thrown if this collection doesn't have a relation at this index.
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public void RemoveAt(int index) {
            DataRelation dr = this[index];
            if (dr == null) {
                throw ExceptionBuilder.RelationOutOfRange(index);
            }
            else {
                Remove(dr);
            }
        }

        /// <devdoc>
        /// Removes the relation with the given name from the collection.  An IndexOutOfRangeException is
        /// thrown if this collection doesn't have a relation with that name
        /// The CollectionChanged event is fired if it succeeds.
        /// </devdoc>
        public void Remove(string name) {
            DataRelation dr = this[name];
            if (dr == null) {
                throw ExceptionBuilder.RelationNotInTheDataSet(name);
            } else {
                Remove(dr);
            }
        }

        /// <devdoc>
        /// Does verification on the relation.
        /// An ArgumentNullException is thrown if this relation is null.  An ArgumentException is thrown
        /// if this relation doesn't belong to this collection.
        /// </devdoc>
        protected virtual void RemoveCore(DataRelation relation) {
            Bid.Trace("<ds.DataRelationCollection.RemoveCore|INFO> %d#, relation=%d\n", ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (relation == null)
                throw ExceptionBuilder.ArgumentNull("relation");
            DataSet dataSet = GetDataSet();
            if (relation.DataSet != dataSet)
                throw ExceptionBuilder.RelationNotInTheDataSet(relation.RelationName);
            if (relation.Nested) {
                relation.ParentTable.ElementColumnCount--;
                // webdata 103905
                // why we were not unregistering the table when removing the relation
                relation.ParentTable.Columns.UnregisterName(relation.ChildTable.TableName);
            }
        }

        /// <devdoc>
        /// Unregisters this name as no longer being used in the collection.  Called by Remove, All property, and
        /// Relation.RelationName property.  If the name is equivalent to the last proposed default name, we walk backwards
        /// to find the next proper default name to use.
        /// </devdoc>
        internal void UnregisterName(string name) {
            Bid.Trace("<ds.DataRelationCollection.UnregisterName|INFO> %d#, name='%ls'\n", ObjectID, name);
            if (NamesEqual(name, MakeName(defaultNameIndex - 1), true, GetDataSet().Locale) != 0) {
                do {
                    defaultNameIndex--;
                } while (defaultNameIndex > 1 &&
                         !Contains(MakeName(defaultNameIndex - 1)));
            }
        }

        internal sealed class DataTableRelationCollection : DataRelationCollection {

            private readonly DataTable table;
            private readonly ArrayList relations; // For caching purpose only to improve performance
            private readonly bool fParentCollection;

            private CollectionChangeEventHandler onRelationPropertyChangedDelegate;

            internal DataTableRelationCollection(DataTable table, bool fParentCollection) {
                if (table == null)
                    throw ExceptionBuilder.RelationTableNull();
                this.table = table;
                this.fParentCollection = fParentCollection;
                relations = new ArrayList();
            }

            protected override ArrayList List {
                get {
                    return relations;
                }
            }

            private void EnsureDataSet() {
                if (table.DataSet == null) {
                    throw ExceptionBuilder.RelationTableWasRemoved();
                }
            }

            protected override DataSet GetDataSet() {
                EnsureDataSet();
                return table.DataSet;
            }

            public override DataRelation this[int index] {
                get {
                    if (index >= 0 && index < relations.Count)
                        return (DataRelation)relations[index];
                    else
                        throw ExceptionBuilder.RelationOutOfRange(index);
                }
            }

            public override DataRelation this[string name] {
                get {
                    int index = InternalIndexOf(name);
                    if (index == -2) {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                    return (index < 0) ? null : (DataRelation)List[index];
                }
            }

            internal event CollectionChangeEventHandler RelationPropertyChanged {
                add {
                    onRelationPropertyChangedDelegate += value;
                }
                remove {
                    onRelationPropertyChangedDelegate -= value;
                }
            }

            internal void OnRelationPropertyChanged(CollectionChangeEventArgs ccevent) {
                if (!fParentCollection) {
                    table.UpdatePropertyDescriptorCollectionCache();
                }
                if (onRelationPropertyChangedDelegate != null) {
                    onRelationPropertyChangedDelegate(this, ccevent);
                }
            }

            private void AddCache(DataRelation relation) {
                relations.Add(relation);
                if (!fParentCollection) {
                    table.UpdatePropertyDescriptorCollectionCache();
                }
            }

            protected override void AddCore(DataRelation relation) {
                if (fParentCollection) {
                    if (relation.ChildTable != table)
                        throw ExceptionBuilder.ChildTableMismatch();
                }
                else {
                    if (relation.ParentTable != table)
                        throw ExceptionBuilder.ParentTableMismatch();
                }

//                base.AddCore(relation); // Will be called from DataSet.Relations.AddCore
                GetDataSet().Relations.Add(relation);
                AddCache(relation);
            }

            public override bool CanRemove(DataRelation relation) {
                if (!base.CanRemove(relation))
                    return false;

                if (fParentCollection) {
                    if (relation.ChildTable != table)
                        return false;
                }
                else {
                    if (relation.ParentTable != table)
                        return false;
                }

                return true;
            }

            private void RemoveCache(DataRelation relation) {
                for (int i = 0; i < relations.Count; i++) {
                    if (relation == relations[i]) {
                        relations.RemoveAt(i);
                        if (!fParentCollection) {
                            table.UpdatePropertyDescriptorCollectionCache();
                        }
                        return;
                    }
                }
                throw ExceptionBuilder.RelationDoesNotExist();
            }

            protected override void RemoveCore(DataRelation relation) {
                if (fParentCollection) {
                    if (relation.ChildTable != table)
                        throw ExceptionBuilder.ChildTableMismatch();
                }
                else {
                    if (relation.ParentTable != table)
                        throw ExceptionBuilder.ParentTableMismatch();
                }

//                base.RemoveCore(relation); // Will be called from DataSet.Relations.RemoveCore
                GetDataSet().Relations.Remove(relation);
                RemoveCache(relation);
            }
        }

        internal sealed class DataSetRelationCollection : DataRelationCollection {

            private readonly DataSet dataSet;
            private readonly ArrayList relations;
            private DataRelation[] delayLoadingRelations = null;

            internal DataSetRelationCollection(DataSet dataSet) {
                if (dataSet == null)
                    throw ExceptionBuilder.RelationDataSetNull();
                this.dataSet = dataSet;
                relations = new ArrayList();
            }

            protected override ArrayList List {
                get {
                    return relations;
                }
            }

            public override void AddRange(DataRelation[] relations) {
                if (dataSet.fInitInProgress) {
                    delayLoadingRelations = relations;
                    return;
                }

                if (relations != null) {
                    foreach(DataRelation relation in relations) {
                        if (relation != null) {
                            Add(relation);
                        }
                    }
                }
            }

            public override void Clear() {
                base.Clear();
                if (dataSet.fInitInProgress && delayLoadingRelations != null) {
                    delayLoadingRelations = null;
                }
            }

            protected override DataSet GetDataSet() {
                return dataSet;
            }

            public override DataRelation this[int index] {
                get {
                    if (index >= 0 && index < relations.Count)
                        return (DataRelation)relations[index];
                    else
                        throw ExceptionBuilder.RelationOutOfRange(index);
                }
            }

            public override DataRelation this[string name] {
                get {
                    int index = InternalIndexOf(name);
                    if (index == -2) {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                    return (index < 0) ? null : (DataRelation)List[index];
                }
            }

            protected override void AddCore(DataRelation relation) {
                base.AddCore(relation);
                if (relation.ChildTable.DataSet != dataSet || relation.ParentTable.DataSet != dataSet)
                    throw ExceptionBuilder.ForeignRelation();

                relation.CheckState();
                if(relation.Nested) {
                    relation.CheckNestedRelations();
                }

                if (relation.relationName.Length == 0)
                    relation.relationName = AssignName();
                else
                    RegisterName(relation.relationName);

                DataKey childKey = relation.ChildKey;

                for (int i = 0; i < relations.Count; i++) {
                    if (childKey.ColumnsEqual(((DataRelation)relations[i]).ChildKey)) {
                        if (relation.ParentKey.ColumnsEqual(((DataRelation)relations[i]).ParentKey))
                            throw ExceptionBuilder.RelationAlreadyExists();
                    }
                }

                relations.Add(relation);
                ((DataRelationCollection.DataTableRelationCollection)(relation.ParentTable.ChildRelations)).Add(relation); // Caching in ParentTable -> ChildRelations
                ((DataRelationCollection.DataTableRelationCollection)(relation.ChildTable.ParentRelations)).Add(relation); // Caching in ChildTable -> ParentRelations

                relation.SetDataSet(dataSet);
                relation.ChildKey.GetSortIndex().AddRef();
                if (relation.Nested) {
                    relation.ChildTable.CacheNestedParent();
                }

                ForeignKeyConstraint foreignKey = relation.ChildTable.Constraints.FindForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference);
                if (relation.createConstraints) {
                    if (foreignKey == null) {
                        relation.ChildTable.Constraints.Add(foreignKey = new ForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference));

                        // try to name the fk constraint the same as the parent relation:
                        try {
                            foreignKey.ConstraintName = relation.RelationName;
                        }
                        catch (Exception e) {
                            // 
                            if (!Common.ADP.IsCatchableExceptionType(e)) {
                                throw;
                            }
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                            // ignore the exception
                        }
                    }
                }
                UniqueConstraint key = relation.ParentTable.Constraints.FindKeyConstraint(relation.ParentColumnsReference);
                relation.SetParentKeyConstraint(key);
                relation.SetChildKeyConstraint(foreignKey);
            }

            protected override void RemoveCore(DataRelation relation) {
                base.RemoveCore(relation);

                dataSet.OnRemoveRelationHack(relation);

                relation.SetDataSet(null);
                relation.ChildKey.GetSortIndex().RemoveRef();
                if (relation.Nested) {
                    relation.ChildTable.CacheNestedParent();
                }

                for (int i = 0; i < relations.Count; i++) {
                    if (relation == relations[i]) {
                        relations.RemoveAt(i);
                        ((DataRelationCollection.DataTableRelationCollection)(relation.ParentTable.ChildRelations)).Remove(relation); // Remove Cache from ParentTable -> ChildRelations
                        ((DataRelationCollection.DataTableRelationCollection)(relation.ChildTable.ParentRelations)).Remove(relation); // Removing Cache from ChildTable -> ParentRelations
                        if (relation.Nested)
                            relation.ChildTable.CacheNestedParent();

                        UnregisterName(relation.RelationName);

                        relation.SetParentKeyConstraint(null);
                        relation.SetChildKeyConstraint(null);

                        return;
                    }
                }
                throw ExceptionBuilder.RelationDoesNotExist();
            }

            internal void FinishInitRelations() {
                if (delayLoadingRelations == null)
                    return;

                DataRelation rel;
                int colCount;
                DataColumn[] parents, childs;
                for (int i = 0; i < delayLoadingRelations.Length; i++) {
                    rel = delayLoadingRelations[i];
                    if (rel.parentColumnNames == null || rel.childColumnNames == null) {
                        this.Add(rel);
                        continue;
                    }

                    colCount = rel.parentColumnNames.Length;
                    parents = new DataColumn[colCount];
                    childs = new DataColumn[colCount];

                    for (int j = 0; j < colCount; j++) {
                        if (rel.parentTableNamespace == null)
                            parents[j] = dataSet.Tables[rel.parentTableName].Columns[rel.parentColumnNames[j]];
                        else
                            parents[j] = dataSet.Tables[rel.parentTableName, rel.parentTableNamespace].Columns[rel.parentColumnNames[j]];

                        if (rel.childTableNamespace == null)
                            childs[j] = dataSet.Tables[rel.childTableName].Columns[rel.childColumnNames[j]];
                        else
                            childs[j] = dataSet.Tables[rel.childTableName, rel.childTableNamespace].Columns[rel.childColumnNames[j]];
                    }

                    DataRelation newRelation = new DataRelation(rel.relationName, parents, childs, false);
                    newRelation.Nested = rel.nested;
                    this.Add(newRelation);
                }

                delayLoadingRelations = null;
            }
        }
    }
}
