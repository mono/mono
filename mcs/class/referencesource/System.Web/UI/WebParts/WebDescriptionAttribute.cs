//------------------------------------------------------------------------------
// <copyright file="WebDescriptionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class WebDescriptionAttribute : Attribute {
        public static readonly WebDescriptionAttribute Default = new WebDescriptionAttribute();

        private string _description;

        public WebDescriptionAttribute() : this(String.Empty) {
        }

        public WebDescriptionAttribute(string description) {
            _description = description;
        }

        public virtual string Description {
            get {
                return DescriptionValue;
            }
        }

        protected string DescriptionValue {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            WebDescriptionAttribute other = obj as WebDescriptionAttribute;
            return (other != null) && other.Description == Description;
        }

        public override int GetHashCode() {
            return Description.GetHashCode();
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
    }
}


