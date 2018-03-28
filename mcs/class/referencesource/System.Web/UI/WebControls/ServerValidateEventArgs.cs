//------------------------------------------------------------------------------
// <copyright file="ServerValidateEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    ///    <para>Provides data for the
    ///    <see langword='ServerValidate'/> event of the <see cref='System.Web.UI.WebControls.CustomValidator'/> .</para>
    /// </devdoc>
    public class ServerValidateEventArgs : EventArgs {

        private bool isValid;
        private string value;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ServerValidateEventArgs'/> 
        /// class.</para>
        /// </devdoc>
        public ServerValidateEventArgs(string value, bool isValid) : base() {
            this.isValid = isValid;
            this.value = value;
        }


        /// <devdoc>
        /// <para>Gets the string value to validate.</para>
        /// </devdoc>
        public string Value {
            get {
                return value;
            }
        }


        /// <devdoc>
        ///    Gets or sets whether the input is valid.
        /// </devdoc>
        public bool IsValid {
            get {
                return isValid;
            }
            set {
                this.isValid = value;
            }
        }
    }
}

