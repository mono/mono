//------------------------------------------------------------------------------
// <copyright file="DefaultSerializationProviderAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {
    using System.Security.Permissions;

    /// <devdoc>
    ///     The default serialization provider attribute is placed on a serializer 
    ///     to indicate the class to use as a default provider of that type of 
    ///     serializer.  To be a default serialization provider, a class must 
    ///     implement IDesignerSerilaizationProvider and have an empty 
    ///     constructor.  The class itself can be internal to the assembly.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DefaultSerializationProviderAttribute : Attribute {

        private string _providerTypeName;

        /// <devdoc>
        ///     Creates a new DefaultSerializationProviderAttribute
        /// </devdoc>
        public DefaultSerializationProviderAttribute(Type providerType) {

            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            _providerTypeName = providerType.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///     Creates a new DefaultSerializationProviderAttribute
        /// </devdoc>
        public DefaultSerializationProviderAttribute(string providerTypeName) {

            if (providerTypeName == null) {
                throw new ArgumentNullException("providerTypeName");
            }

            _providerTypeName = providerTypeName;
        }

        /// <devdoc>
        ///     Returns the type name for the default serialization provider.
        /// </devdoc>
        public string ProviderTypeName {
            get {
                return _providerTypeName;
            }
        }
    }
}

