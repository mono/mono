//------------------------------------------------------------------------------
// <copyright file="ListViewSortEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

    public class ListViewSortEventArgs : CancelEventArgs {
        private string _sortExpression;
        private SortDirection _sortDirection;

        public ListViewSortEventArgs(string sortExpression, SortDirection sortDirection)
            : base(false) {
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
