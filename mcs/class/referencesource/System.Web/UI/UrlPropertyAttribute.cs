//------------------------------------------------------------------------------
// <copyright file="UrlPropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Web.Util;

    // An UrlPropertyAttribute metadata attribute can be applied to string 
    // properties that contain URL values.
    // This can be used to identify URLs which allows design-time functionality and runtime
    // functionality to do interesting things with the property values.
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UrlPropertyAttribute : Attribute {

        private string _filter;
        // Used to mark a property as an URL.
        public UrlPropertyAttribute() : this("*.*") {
        }

        // Used to mark a property as an URL. In addition, the type of files allowed
        // can be specified. This can be used at design-time to customize the URL picker.
        public UrlPropertyAttribute(string filter) {
            if(filter == null) {
                _filter = "*.*";
            }
            else {
                _filter = filter;
            }
        }

        // The file filter associated with the URL property. This takes
        // the form of a file filter string typically used with Open File
        // dialogs. The default is *.*, so all file types can be chosen.
        public string Filter {
            get {
                return _filter;
            }
        }

        public override int GetHashCode() {
            return Filter.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            UrlPropertyAttribute other = obj as UrlPropertyAttribute;
            if (other != null) {
                return Filter.Equals(other.Filter);
            }

            return false;
        }
    }
}
