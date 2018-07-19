//------------------------------------------------------------------------------
// <copyright file="DataObjectAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Security.Permissions;

    /// <devdoc>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DataObjectMethodAttribute : Attribute {

        private bool _isDefault;
        private DataObjectMethodType _methodType;

        public DataObjectMethodAttribute(DataObjectMethodType methodType) : this(methodType, false) {
        }

        public DataObjectMethodAttribute(DataObjectMethodType methodType, bool isDefault) {
            _methodType = methodType;
            _isDefault = isDefault;
        }

        public bool IsDefault {
            get {
                return _isDefault;
            }
        }

        public DataObjectMethodType MethodType {
            get {
                return _methodType;
            }
        }

        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            DataObjectMethodAttribute other = obj as DataObjectMethodAttribute;
            return (other != null) && (other.MethodType == MethodType) && (other.IsDefault == IsDefault);
        }

        /// <internalonly/>
        public override int GetHashCode() {
            return ((int)_methodType).GetHashCode() ^ _isDefault.GetHashCode();
        }

        /// <internalonly/>
        public override bool Match(object obj) {
            if (obj == this) {
                return true;
            }

            DataObjectMethodAttribute other = obj as DataObjectMethodAttribute;
            return (other != null) && (other.MethodType == MethodType);
        }
    }
}
