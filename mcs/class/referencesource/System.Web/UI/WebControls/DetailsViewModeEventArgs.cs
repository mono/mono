//------------------------------------------------------------------------------
// <copyright file="DetailsViewModeEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for the <see langword='DetailsViewMode'/> event.</para>
    /// </devdoc>
    public class DetailsViewModeEventArgs : CancelEventArgs {

        private DetailsViewMode _mode;
        private bool _cancelingEdit;


        /// <devdoc>
        /// <para>Initializes a new instance of <see cref='System.Web.UI.WebControls.DetailsViewModeEventArgs'/> class.</para>
        /// </devdoc>
        public DetailsViewModeEventArgs(DetailsViewMode mode, bool cancelingEdit) : base(false) {
            this._mode = mode;
            this._cancelingEdit = cancelingEdit;
        }


        /// <devdoc>
        /// <para>Gets a bool in the <see cref='System.Web.UI.WebControls.DetailsView'/> indicating whether the mode change is the result of a cancel command.
        ///  This property is read-only.</para>
        /// </devdoc>
        public bool CancelingEdit {
            get {
                return _cancelingEdit;
            }
        }
        

        /// <devdoc>
        /// <para>Gets a DetailsViewMode in the <see cref='System.Web.UI.WebControls.DetailsView'/>. This property is read-only.</para>
        /// </devdoc>
        public DetailsViewMode NewMode {
            get {
                return _mode;
            }
            set {
                _mode = value;
            }
        }
    }
}

