//------------------------------------------------------------------------------
// <copyright file="WebDisplayNameAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class WebDisplayNameAttribute : Attribute {
        public static readonly WebDisplayNameAttribute Default = new WebDisplayNameAttribute();

        private string _displayName;

        public WebDisplayNameAttribute() : this(String.Empty) {
        }

        public WebDisplayNameAttribute(string displayName) {
            _displayName = displayName;
        }

        public virtual string DisplayName {
            get {
                return DisplayNameValue;
            }
        }

        protected string DisplayNameValue {
            get {
                return _displayName;
            }
            set {
                _displayName = value;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            WebDisplayNameAttribute other = obj as WebDisplayNameAttribute;
            return (other != null) && other.DisplayName == DisplayName;
        }

        public override int GetHashCode() {
            return DisplayName.GetHashCode();
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
    }
}

