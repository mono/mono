//------------------------------------------------------------------------------
// <copyright file="DataListItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;

    /// <devdoc>
    /// <para>Represents the collection of <see cref='System.Web.UI.WebControls.DataListItem'/> objects</para>
    /// </devdoc>
    public sealed class DataListItemCollection : ICollection {

        private ArrayList items;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataListItemCollection'/> class.</para>
        /// </devdoc>
        public DataListItemCollection(ArrayList items) {
            this.items = items;
        }


        /// <devdoc>
        ///    <para>Gets the number of items in the collection.</para>
        /// </devdoc>
        public int Count {
            get {
                return items.Count;
            }
        }

        /// <devdoc>
        /// <para>Gets a value that specifies whether items in the <see cref='System.Web.UI.WebControls.DataListItemCollection'/> can be modified.</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <devdoc>
        /// <para>Gets a value that indicates whether the <see cref='System.Web.UI.WebControls.DataListItemCollection'/> is thread-safe.</para>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>Gets the object used to synchronize access to the collection. </para>
        /// </devdoc>
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// <para>Gets a <see cref='System.Web.UI.WebControls.DataListItem'/> at the specified index in the 
        ///    collection.</para>
        /// </devdoc>
        public DataListItem this[int index] {
            get {
                return(DataListItem)items[index];
            }
        }



        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending at 
        ///    the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// <para>Creates an enumerator for the <see cref='System.Web.UI.WebControls.DataListItemCollection'/> used to iterate 
        ///    through the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return items.GetEnumerator(); 
        }
    }
}

