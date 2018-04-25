//------------------------------------------------------------------------------
// <copyright file="HtmlTableCellCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlTableCellCollection.cs
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
///       The <see langword='HtmlTableCellCollection'/> contains all of the table
///       cells, both &lt;td&gt; and &lt;th&gt; elements, found within an <see langword='HtmlTable'/>
///       server control.
///    </para>
/// </devdoc>
    public sealed class HtmlTableCellCollection : ICollection {

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        private HtmlTableRow owner;

        internal HtmlTableCellCollection(HtmlTableRow owner) {
            this.owner = owner;
        }

        /*
         * The number of cells in the row.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the number of items in the <see langword='HtmlTableCell'/>
        ///       collection.
        ///    </para>
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
        ///       Gets an <see langword='HtmlTableCell'/> control from an
        ///    <see langword='HtmlTable'/> control thorugh the cell's ordinal index value. 
        ///    </para>
        /// </devdoc>
        public HtmlTableCell this[int index]
        {
            get {
                return(HtmlTableCell)owner.Controls[index];
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Adds the specified <see langword='HtmlTableCell'/> control to the end of the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public void Add(HtmlTableCell cell) {
            Insert(-1, cell);
        }


        /// <devdoc>
        ///    <para>
        ///       Adds an <see langword='HtmlTableCell'/> control to a specified location in the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public void Insert(int index, HtmlTableCell cell) {
            owner.Controls.AddAt(index, cell);
        }


        /// <devdoc>
        ///    <para>
        ///       Deletes all <see langword='HtmlTableCell'/>
        ///       controls from the collection.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            if (owner.HasControls())
                owner.Controls.Clear();
        }

        /*
         * Returns an enumerator that enumerates over the cells in a table row in order.
         */

        /// <devdoc>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return owner.Controls.GetEnumerator();
        }


        /// <devdoc>
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
        ///    <para>
        ///       Deletes the specified <see langword='HtmlTableCell'/> control from the
        ///       collection.
        ///    </para>
        /// </devdoc>
        public void Remove(HtmlTableCell cell) {
            owner.Controls.Remove(cell);
        }


        /// <devdoc>
        ///    <para>
        ///       Deletes the <see langword='HtmlTableCell'/> control at the specified index
        ///       location from the collection.
        ///    </para>
        /// </devdoc>
        public void RemoveAt(int index) {
            owner.Controls.RemoveAt(index);
        }
    }
}
