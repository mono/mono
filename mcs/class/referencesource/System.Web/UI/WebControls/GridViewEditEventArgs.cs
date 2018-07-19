//------------------------------------------------------------------------------
// <copyright file="GridViewEditEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.GridView'/> events.</para>
    /// </devdoc>
    public class GridViewEditEventArgs : CancelEventArgs {

        private int _newEditIndex;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewEditEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public GridViewEditEventArgs(int newEditIndex) {
            this._newEditIndex = newEditIndex;
        }



        /// <devdoc>
        ///    <para>Gets the index of the row to be edited. This property is read-only.</para>
        /// </devdoc>
        public int NewEditIndex {
            get {
                return _newEditIndex;
            }
            set {
                _newEditIndex = value;
            }
        }
    }
}

