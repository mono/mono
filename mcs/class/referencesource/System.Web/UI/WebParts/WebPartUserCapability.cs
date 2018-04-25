//------------------------------------------------------------------------------
// <copyright file="WebPartUserCapability.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Web.Util;

    /// <devdoc>
    /// </devdoc>
    public sealed class WebPartUserCapability {

        private string _name;

        /// <devdoc>
        /// </devdoc>
        public WebPartUserCapability(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            _name = name;
        }

        /// <devdoc>
        /// </devdoc>
        public string Name {
            get {
                return _name;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object o) {
            if (o == this) {
                return true;
            }

            WebPartUserCapability other = o as WebPartUserCapability;
            return (other != null) && (other.Name == Name);
        }

        /// <devdoc>
        /// </devdoc>
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
    }
}
