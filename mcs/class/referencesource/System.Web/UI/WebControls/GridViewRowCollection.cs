//------------------------------------------------------------------------------
// <copyright file="GridViewRowCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;

    /// <devdoc>
    /// <para>Represents the collection of <see cref='System.Web.UI.WebControls.GridViewRow'/> objects.</para>
    /// </devdoc>
    public class GridViewRowCollection : ICollection {

        private ArrayList _rows;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewRowCollection'/> class.</para>
        /// </devdoc>
        public GridViewRowCollection(ArrayList rows) {
            this._rows = rows;
        }
        

        /// <devdoc>
        ///    <para>Gets the number of rows in the collection. This property is read-only.</para>
        /// </devdoc>
        public int Count {
            get {
                return _rows.Count;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that specifies whether rows in the <see cref='System.Web.UI.WebControls.GridViewRowCollection'/> can be 
        ///    modified. This property is read-only.</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that indicates whether the <see cref='System.Web.UI.WebControls.GridViewRowCollection'/> is 
        ///    thread-safe. This property is read-only.</para>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>Gets the object used to synchronize access to the collection. This property is read-only. </para>
        /// </devdoc>
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// <para>Gets a <see cref='System.Web.UI.WebControls.GridViewRow'/> at the specified index in the 
        ///    collection.</para>
        /// </devdoc>
        public GridViewRow this[int index] {
            get {
                return(GridViewRow)_rows[index];
            }
        }



        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending 
        ///    at the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(GridViewRow[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// <para>Creates an enumerator for the <see cref='System.Web.UI.WebControls.GridViewRowCollection'/> used to 
        ///    iterate through the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return _rows.GetEnumerator(); 
        }
    }
}

