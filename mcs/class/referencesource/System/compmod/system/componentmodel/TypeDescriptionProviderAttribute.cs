//------------------------------------------------------------------------------
// <copyright file="TypeDescriptionProviderAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel 
{

    using System;
    using System.Security.Permissions;

    /// <devdoc>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class TypeDescriptionProviderAttribute : Attribute 
    {
        private string _typeName;

        /// <devdoc>
        ///     Creates a new TypeDescriptionProviderAttribute object.
        /// </devdoc>
        public TypeDescriptionProviderAttribute(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            _typeName = typeName;
        }
    
        /// <devdoc>
        ///     Creates a new TypeDescriptionProviderAttribute object.
        /// </devdoc>
        public TypeDescriptionProviderAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            _typeName = type.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///     The TypeName property returns the assembly qualified type name 
        ///     for the type description provider.
        /// </devdoc>
        public string TypeName
        {
            get
            {
                return _typeName;
            }
        }
    }
}

