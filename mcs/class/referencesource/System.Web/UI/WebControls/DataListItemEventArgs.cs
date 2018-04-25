//------------------------------------------------------------------------------
// <copyright file="DataListItemEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// <para>Provides data for the <see langword='DataListItem'/> event of a <see cref='System.Web.UI.WebControls.DataList'/> .</para>
    /// </devdoc>
    public class DataListItemEventArgs : EventArgs {

        private DataListItem item;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataListItemEventArgs'/> class.</para>
        /// </devdoc>
        public DataListItemEventArgs(DataListItem item) {
            this.item = item;
        }



        /// <devdoc>
        /// <para>Gets the item form the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        public DataListItem Item {
            get {
                return item;
            }
        }
    }
}

