//------------------------------------------------------------------------------
// <copyright file="ListViewSelectEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

    public class ListViewSelectEventArgs : CancelEventArgs {
        private int _newSelectedIndex;

        public ListViewSelectEventArgs(int newSelectedIndex) : base(false) {
            this._newSelectedIndex = newSelectedIndex;
        }

        /// <devdoc>
        /// <para>Gets the index of the selected row to be displayed in the <see cref='System.Web.UI.WebControls.ListView'/>. 
        ///    This property is read-only.</para>
        /// </devdoc>
        public int NewSelectedIndex {
            get {
                return _newSelectedIndex;
            }
            set {
                _newSelectedIndex = value;
            }
        }
    }
}
