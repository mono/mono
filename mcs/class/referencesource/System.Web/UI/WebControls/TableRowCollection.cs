//------------------------------------------------------------------------------
// <copyright file="TableRowCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Encapsulates the collection of <see cref='System.Web.UI.WebControls.TableRow'/> objects within a <see cref='System.Web.UI.WebControls.Table'/> control.</para>
    /// </devdoc>
    [
    Editor("System.Web.UI.Design.WebControls.TableRowsCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))
    ]
    public sealed class TableRowCollection : IList {


        /// <devdoc>
        ///    A protected field of type <see cref='System.Web.UI.WebControls.Table'/>. Represents the <see cref='System.Web.UI.WebControls.TableRow'/> collection internally.
        /// </devdoc>
        private Table owner;


        /// <devdoc>
        /// </devdoc>
        internal TableRowCollection(Table owner) {
            this.owner = owner;
        }
        

        /// <devdoc>
        ///    Gets the
        ///    count of <see cref='System.Web.UI.WebControls.TableRow'/> in the collection.
        /// </devdoc>
        public int Count {
            get {
                if (owner.HasControls()) {
                    return owner.Controls.Count;
                }
                return 0;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a <see cref='System.Web.UI.WebControls.TableRow'/> referenced by the
        ///       specified ordinal index value.
        ///    </para>
        /// </devdoc>
        public TableRow this[int index] {
            get {
                return(TableRow)owner.Controls[index];
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Adds the specified <see cref='System.Web.UI.WebControls.TableRow'/> to the end of the collection.
        ///    </para>
        /// </devdoc>
        public int Add(TableRow row) {
            AddAt(-1, row);
            return owner.Controls.Count - 1;
        }


        /// <devdoc>
        ///    <para>
        ///       Adds the specified <see cref='System.Web.UI.WebControls.TableRow'/> to the collection at the specified
        ///       index location.
        ///    </para>
        /// </devdoc>
        public void AddAt(int index, TableRow row) {
            owner.Controls.AddAt(index, row);
            if (row.TableSection != TableRowSection.TableBody) {
                owner.HasRowSections = true;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public void AddRange(TableRow[] rows) {
            if (rows == null) {
                throw new ArgumentNullException("rows");
            }
            foreach(TableRow row in rows) {
                Add(row);
            }
        }


        /// <devdoc>
        /// <para>Removes all <see cref='System.Web.UI.WebControls.TableRow'/> controls from the collection.</para>
        /// </devdoc>
        public void Clear() {
            if (owner.HasControls()) {
                owner.Controls.Clear();
                owner.HasRowSections = false;
            }
        }


        /// <devdoc>
        ///    <para> Returns an ordinal index value that denotes the position of the specified
        ///    <see cref='System.Web.UI.WebControls.TableRow'/> within the collection. </para>
        /// </devdoc>
        public int GetRowIndex(TableRow row) {
            if (owner.HasControls()) {
                return owner.Controls.IndexOf(row);
            }
            return -1;
        }


        /// <devdoc>
        ///    <para>
        ///       Returns an enumerator of all <see cref='System.Web.UI.WebControls.TableRow'/> controls within the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return owner.Controls.GetEnumerator();
        }


        /// <devdoc>
        /// <para>Copies contents from the collection to the specified <see cref='System.Array' qualify='true'/> with the
        ///    specified starting index.</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the object that can be used to synchronize access to the collection. In
        ///       this case, it is the collection itself.
        ///    </para>
        /// </devdoc>
        public Object SyncRoot {
            get { return this;}
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the collection is read-only.
        ///    </para>
        /// </devdoc>
        public bool IsReadOnly {
            get { return false;}
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether access to the collection is synchronized
        ///       (thread-safe).
        ///    </para>
        /// </devdoc>
        public bool IsSynchronized {
            get { return false;}
        }


        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Web.UI.WebControls.TableRow'/> from the collection.</para>
        /// </devdoc>
        public void Remove(TableRow row) {
            owner.Controls.Remove(row);
        }


        /// <devdoc>
        /// <para>Removes the <see cref='System.Web.UI.WebControls.TableRow'/> from the collection at the specified
        ///    index location.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            owner.Controls.RemoveAt(index);
        }

        // IList implementation, required by collection editor

        /// <internalonly/>
        object IList.this[int index] {
            get {
                return owner.Controls[index];
            }
            set {
                RemoveAt(index);
                AddAt(index, (TableRow)value);
            }
        }


        /// <internalonly/>
        bool IList.IsFixedSize {
            get {
                return false;
            }
        }


        /// <internalonly/>
        int IList.Add(object o) {
            return Add((TableRow) o);            
        }


        /// <internalonly/>
        bool IList.Contains(object o) {
            return owner.Controls.Contains((TableRow)o);
        }


        /// <internalonly/>
        int IList.IndexOf(object o) {
            return owner.Controls.IndexOf((TableRow)o);
        }


        /// <internalonly/>
        void IList.Insert(int index, object o) {
            AddAt(index, (TableRow)o);
        }


        /// <internalonly/>
        void IList.Remove(object o) {
            Remove((TableRow)o);
        }

    }
}

