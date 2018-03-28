//------------------------------------------------------------------------------
// <copyright file="DataBindingHandlerAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;


    /// <devdoc>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataBindingHandlerAttribute : Attribute {
        private string _typeName;


        /// <devdoc>
        /// </devdoc>
        public static readonly DataBindingHandlerAttribute Default = new DataBindingHandlerAttribute();


        /// <devdoc>
        /// </devdoc>
        public DataBindingHandlerAttribute() {
            _typeName = String.Empty;
        }
        

        /// <devdoc>
        /// </devdoc>
        public DataBindingHandlerAttribute(Type type) {
            _typeName = type.AssemblyQualifiedName;
        }


        /// <devdoc>
        /// </devdoc>
        public DataBindingHandlerAttribute(string typeName) {
            _typeName = typeName;
        }


        /// <devdoc>
        /// </devdoc>
        public string HandlerTypeName {
            get {
                return (_typeName != null ? _typeName : String.Empty);
            }
        }


        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            
            DataBindingHandlerAttribute other = obj as DataBindingHandlerAttribute; 

            if (other != null) {
                return (String.Compare(HandlerTypeName, other.HandlerTypeName,
                                       StringComparison.Ordinal) == 0);
            }

            return false;
        }


        /// <internalonly/>
        public override int GetHashCode() {
            return HandlerTypeName.GetHashCode();
        }
    }
}

