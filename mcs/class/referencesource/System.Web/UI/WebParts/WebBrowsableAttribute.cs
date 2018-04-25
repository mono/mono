//------------------------------------------------------------------------------
// <copyright file="WebBrowsableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class WebBrowsableAttribute : Attribute {

        /// <devdoc>
        /// </devdoc>
        public static readonly WebBrowsableAttribute Yes = new WebBrowsableAttribute(true);

        /// <devdoc>
        /// </devdoc>
        public static readonly WebBrowsableAttribute No = new WebBrowsableAttribute(false);

        /// <devdoc>
        /// </devdoc>
        public static readonly WebBrowsableAttribute Default = No;

        private bool _browsable;

        /// <devdoc>
        /// </devdoc>
        public WebBrowsableAttribute() : this(true) {
        }

        /// <devdoc>
        /// </devdoc>
        public WebBrowsableAttribute(bool browsable) {
            _browsable = browsable;
        }

        /// <devdoc>
        /// </devdoc>
        public bool Browsable {
            get {
                return _browsable;
            }
        }

        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            WebBrowsableAttribute other = obj as WebBrowsableAttribute;
            return (other != null) && (other.Browsable == Browsable);
        }

        /// <internalonly/>
        public override int GetHashCode() {
            return _browsable.GetHashCode();
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
