//------------------------------------------------------------------------------
// <copyright file="FormViewPageEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>Provides data for 
    ///       the <see langword='FormViewPage'/>
    ///       event.</para>
    /// </devdoc>
    public class FormViewPageEventArgs : CancelEventArgs {

        private int _newPageIndex;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FormViewPageEventArgs'/> class.</para>
        /// </devdoc>
        public FormViewPageEventArgs(int newPageIndex) {
            this._newPageIndex = newPageIndex;
        }


        /// <devdoc>
        /// <para>Gets the index of the first new Page to be displayed in the <see cref='System.Web.UI.WebControls.FormView'/>. 
        ///    This property is read-only.</para>
        /// </devdoc>
        public int NewPageIndex {
            get {
                return _newPageIndex;
            }
            set {
                _newPageIndex = value;
            }
        }
    }
}

