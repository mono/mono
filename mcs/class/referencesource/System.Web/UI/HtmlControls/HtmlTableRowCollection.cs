//------------------------------------------------------------------------------
// <copyright file="HtmlTableRowCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlTableRowCollection.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.Runtime.InteropServices;

    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlTableRowCollection'/> contains all
///       of the table rows found within an <see langword='HtmlTable'/>
///       server control.
///    </para>
/// </devdoc>
    public sealed class HtmlTableRowCollection : ICollection {

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        private HtmlTable owner;

        internal HtmlTableRowCollection(HtmlTable owner) {
            this.owner = owner;
        }

        /*
         * The number of cells in the row.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the number of items in the
        ///    <see langword='HtmlTableRow'/>
        ///    collection.
        /// </para>
        /// </devdoc>
        public int Count {
            get {
                if (owner.HasControls())
                    return owner.Controls.Count;

                return 0;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets an <see langword='HtmlTableRow'/> control from an <see langword='HtmlTable'/>
        ///       control thorugh the row's ordinal index value.
        ///    </para>
        /// </devdoc>
        public HtmlTableRow this[int index]
        {
            get {
                return(HtmlTableRow)owner.Controls[index];
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Adds the specified HtmlTableRow control to the end of the collection.
        ///    </para>
        /// </devdoc>
        public void Add(HtmlTableRow row) {
            Insert(-1, row);
        }


        /// <devdoc>
        ///    <para>
        ///       Adds an <see langword='HtmlTableRow'/> control to a specified
        ///       location in the collection.
        ///    </para>
        /// </devdoc>
        public void Insert(int index, HtmlTableRow row) {
            owner.Controls.AddAt(index, row);
        }


        /// <devdoc>
        ///    <para>
        ///       Deletes all <see langword='HtmlTableRow'/> controls from the collection.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            if (owner.HasControls())
                owner.Controls.Clear();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// </devdoc>
        public Object SyncRoot {
            get { return this;}
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsReadOnly {
            get { return false;}
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsSynchronized {
            get { return false;}
        }


        /// <devdoc>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return owner.Controls.GetEnumerator();
        }


        /// <devdoc>
        ///    <para>
        ///       Deletes the specified <see langword='HtmlTableRow'/>
        ///       control
        ///       from the collection.
        ///    </para>
        /// </devdoc>
        public void Remove(HtmlTableRow row) {
            owner.Controls.Remove(row);
        }


        /// <devdoc>
        ///    <para>
        ///       Deletes the <see langword='HtmlTableRow'/> control at the specified index
        ///       location from the collection.
        ///    </para>
        /// </devdoc>
        public void RemoveAt(int index) {
            owner.Controls.RemoveAt(index);
        }
    }
}
