//------------------------------------------------------------------------------
// <copyright file="ColumnCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Represents the collection of columns to be displayed in 
    ///       a <see cref='System.Web.UI.WebControls.DataGrid'/>
    ///       control.</para>
    /// </devdoc>
    public sealed class DataGridColumnCollection : ICollection, IStateManager {

        private DataGrid owner;
        private ArrayList columns;
        private bool marked;


        /// <devdoc>
        /// <para>Initializes a new instance of <see cref='System.Web.UI.WebControls.DataGridColumnCollection'/> class.</para>
        /// </devdoc>
        public DataGridColumnCollection(DataGrid owner, ArrayList columns) {
            this.owner = owner;
            this.columns = columns;
        }
        

        /// <devdoc>
        ///    <para>Gets the number of columns in the collection. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public int Count {
            get {
                return columns.Count;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that specifies whether items in the <see cref='System.Web.UI.WebControls.DataGridColumnCollection'/> can be 
        ///    modified. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that indicates whether the <see cref='System.Web.UI.WebControls.DataGridColumnCollection'/> is thread-safe. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>Gets the object used to synchronize access to the collection. This property is read-only. </para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public Object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// <para>Gets a <see cref='System.Web.UI.WebControls.DataGridColumn'/> at the specified index in the 
        ///    collection.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public DataGridColumn this[int index] {
            get {
                return (DataGridColumn)columns[index];
            }
        }



        /// <devdoc>
        /// <para>Appends a <see cref='System.Web.UI.WebControls.DataGridColumn'/> to the collection.</para>
        /// </devdoc>
        public void Add(DataGridColumn column) {
            AddAt(-1, column);
        }


        /// <devdoc>
        /// <para>Inserts a <see cref='System.Web.UI.WebControls.DataGridColumn'/> to the collection 
        ///    at the specified index.</para>
        /// </devdoc>
        public void AddAt(int index, DataGridColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }
            if (index == -1) {
                columns.Add(column);
            }
            else {
                columns.Insert(index, column);
            }
            column.SetOwner(owner);
            if (marked)
                ((IStateManager)column).TrackViewState();
            OnColumnsChanged();
        }


        /// <devdoc>
        /// <para>Empties the collection of all <see cref='System.Web.UI.WebControls.DataGridColumn'/> objects.</para>
        /// </devdoc>
        public void Clear() {
            columns.Clear();
            OnColumnsChanged();
        }


        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending at 
        ///    the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// <para>Creates an enumerator for the <see cref='System.Web.UI.WebControls.DataGridColumnCollection'/> used to iterate through the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return columns.GetEnumerator();
        }



        /// <devdoc>
        /// <para>Returns the index of the first occurrence of a value in a <see cref='System.Web.UI.WebControls.DataGridColumn'/>.</para>
        /// </devdoc>
        public int IndexOf(DataGridColumn column) {
            if (column != null) {
                return columns.IndexOf(column);
            }
            return -1;
        }


        /// <devdoc>
        /// </devdoc>
        private void OnColumnsChanged() {
            if (owner != null) {
                owner.OnColumnsChanged();
            }
        }


        /// <devdoc>
        /// <para>Removes a <see cref='System.Web.UI.WebControls.DataGridColumn'/> from the collection at the specified 
        ///    index.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            if ((index >= 0) && (index < Count)) {
                columns.RemoveAt(index);
                OnColumnsChanged();
            }
            else {
                throw new ArgumentOutOfRangeException("index");
            }
        }


        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Web.UI.WebControls.DataGridColumn'/> from the collection.</para>
        /// </devdoc>
        public void Remove(DataGridColumn column) {
            int index = IndexOf(column);
            if (index >= 0) {
                RemoveAt(index);
            }
        }



        /// <internalonly/>
        /// <devdoc>
        /// Return true if tracking state changes.
        /// </devdoc>
        bool IStateManager.IsTrackingViewState {
            get {
                return marked;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Load previously saved state.
        /// </devdoc>
        void IStateManager.LoadViewState(object savedState) {
            if (savedState != null) {
                object[] columnsState = (object[])savedState;

                if (columnsState.Length == columns.Count) {
                    for (int i = 0; i < columnsState.Length; i++) {
                        if (columnsState[i] != null) {
                            ((IStateManager)columns[i]).LoadViewState(columnsState[i]);
                        }
                    }
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Start tracking state changes.
        /// </devdoc>
        void IStateManager.TrackViewState() {
            marked = true;

            int columnCount = columns.Count;
            for (int i = 0; i < columnCount; i++) {
                ((IStateManager)columns[i]).TrackViewState();
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return object containing state changes.
        /// </devdoc>
        object IStateManager.SaveViewState() {
            int columnCount = columns.Count;
            object[] columnsState = new object[columnCount];
            bool savedState = false;

            for (int i = 0; i < columnCount; i++) {
                columnsState[i] = ((IStateManager)columns[i]).SaveViewState();
                if (columnsState[i] != null)
                    savedState = true;
            }

            return savedState ? columnsState : null;
        }
    }
}
