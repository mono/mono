//------------------------------------------------------------------------------
// <copyright file="DataListCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// <para>Provides data for the <see langword='DataListCommand'/> event of a <see cref='System.Web.UI.WebControls.DataList'/>.
    /// </para>
    /// </devdoc>
    public class DataListCommandEventArgs : CommandEventArgs {

        private DataListItem item;
        private object commandSource;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataListCommandEventArgs'/> class.</para>
        /// </devdoc>
        public DataListCommandEventArgs(DataListItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
            this.item = item;
            this.commandSource = commandSource;
        }



        /// <devdoc>
        /// <para>Gets the selected item in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        public DataListItem Item {
            get {
                return item;
            }
        }


        /// <devdoc>
        ///    <para>Gets the source of the command.</para>
        /// </devdoc>
        public object CommandSource {
            get {
                return commandSource;
            }
        }
    }
}

