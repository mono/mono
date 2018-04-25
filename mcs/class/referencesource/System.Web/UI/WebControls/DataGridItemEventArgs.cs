//------------------------------------------------------------------------------
// <copyright file="DataGridItemEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// <para>Provides data for the <see langword='DataGridItem'/> event.</para>
    /// </devdoc>
    public class DataGridItemEventArgs : EventArgs {

        private DataGridItem item;


        /// <devdoc>
        /// <para>Initializes a new instance of <see cref='System.Web.UI.WebControls.DataGridItemEventArgs'/> class.</para>
        /// </devdoc>
        public DataGridItemEventArgs(DataGridItem item) {
            this.item = item;
        }



        /// <devdoc>
        /// <para>Gets an item in the <see cref='System.Web.UI.WebControls.DataGrid'/>. This property is read-only.</para>
        /// </devdoc>
        public DataGridItem Item {
            get {
                return item;
            }
        }
    }
}

