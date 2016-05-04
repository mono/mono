//------------------------------------------------------------------------------
// <copyright file="DataKeyPropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Web.Util;
    using System.Diagnostics.CodeAnalysis;
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataKeyPropertyAttribute : Attribute {
        private readonly string _name;

        public DataKeyPropertyAttribute(string name) {
            _name = name;
        }
        
        public string Name {
            get {
                return _name;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool Equals(object obj) {
            DataKeyPropertyAttribute other = obj as DataKeyPropertyAttribute;
            if (other != null) {
                return String.Equals(_name, other.Name, StringComparison.Ordinal);
            }
            return false;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int GetHashCode() {
            return (Name != null) ? Name.GetHashCode() : 0;
        }
    }
}
