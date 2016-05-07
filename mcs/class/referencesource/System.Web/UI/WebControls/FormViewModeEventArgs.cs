//------------------------------------------------------------------------------
// <copyright file="FormViewModeEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for the <see langword='FormViewMode'/> event.</para>
    /// </devdoc>
    public class FormViewModeEventArgs : CancelEventArgs {

        private FormViewMode _mode;
        private bool _cancelingEdit;


        /// <devdoc>
        /// <para>Initializes a new instance of <see cref='System.Web.UI.WebControls.FormViewModeEventArgs'/> class.</para>
        /// </devdoc>
        public FormViewModeEventArgs(FormViewMode mode, bool cancelingEdit) : base(false) {
            this._mode = mode;
            this._cancelingEdit = cancelingEdit;
        }


        /// <devdoc>
        /// <para>Gets a bool in the <see cref='System.Web.UI.WebControls.FormView'/> indicating whether the mode change is the result of a cancel command.
        ///  This property is read-only.</para>
        /// </devdoc>
        public bool CancelingEdit {
            get {
                return _cancelingEdit;
            }
        }
        

        /// <devdoc>
        /// <para>Gets a FormViewMode in the <see cref='System.Web.UI.WebControls.FormView'/>. This property is read-only.</para>
        /// </devdoc>
        public FormViewMode NewMode {
            get {
                return _mode;
            }
            set {
                _mode = value;
            }
        }
    }
}

