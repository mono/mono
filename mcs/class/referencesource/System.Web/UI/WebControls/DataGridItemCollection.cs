//------------------------------------------------------------------------------
// <copyright file="DataGridItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;

    /// <devdoc>
    /// <para>Represents the collection of <see cref='System.Web.UI.WebControls.DataGridItem'/> objects.</para>
    /// </devdoc>
    public class DataGridItemCollection : ICollection {

        private ArrayList items;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataGridItemCollection'/> class.</para>
        /// </devdoc>
        public DataGridItemCollection(ArrayList items) {
            this.items = items;
        }
        

        /// <devdoc>
        ///    <para>Gets the number of items in the collection. This property is read-only.</para>
        /// </devdoc>
        public int Count {
            get {
                return items.Count;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that specifies whether items in the <see cref='System.Web.UI.WebControls.DataGridItemCollection'/> can be 
        ///    modified. This property is read-only.</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that indicates whether the <see cref='System.Web.UI.WebControls.DataGridItemCollection'/> is 
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
        /// <para>Gets a <see cref='System.Web.UI.WebControls.DataGridItem'/> at the specified index in the 
        ///    collection.</para>
        /// </devdoc>
        public DataGridItem this[int index] {
            get {
                return(DataGridItem)items[index];
            }
        }



        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending 
        ///    at the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// <para>Creates an enumerator for the <see cref='System.Web.UI.WebControls.DataGridItemCollection'/> used to 
        ///    iterate through the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return items.GetEnumerator(); 
        }
    }
}

