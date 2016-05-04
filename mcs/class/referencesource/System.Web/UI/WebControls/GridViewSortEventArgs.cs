//------------------------------------------------------------------------------
// <copyright file="GridViewSortEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for the <see langword='GridViewSort'/> event of a <see cref='System.Web.UI.WebControls.GridView'/>.
    /// </para>
    /// </devdoc>
    public class GridViewSortEventArgs : CancelEventArgs {

        private string _sortExpression;
        private SortDirection _sortDirection;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewSortEventArgs'/> class.</para>
        /// </devdoc>
        public GridViewSortEventArgs(string sortExpression, SortDirection sortDirection) {
            this._sortExpression = sortExpression;
            this._sortDirection = sortDirection;
        }

        /// <devdoc>
        ///    <para>Gets the direction used to sort.</para>
        /// </devdoc>
        public SortDirection SortDirection {
            get {
                return _sortDirection;
            }
            set {
                _sortDirection = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the expression used to sort.</para>
        /// </devdoc>
        public string SortExpression {
            get {
                return _sortExpression;
            }
            set {
                _sortExpression = value;
            }
        }
    }
}

